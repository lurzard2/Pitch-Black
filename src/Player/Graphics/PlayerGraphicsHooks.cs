using System;
using Random = UnityEngine.Random;
using static PitchBlack.Plugin;
using UnityEngine;
using RWCustom;

namespace PitchBlack;

public static class PlayerGraphicsHooks
{
    /// <summary>
    /// PlayerGraphics code in RW is a [trash heap]. Here's my general understanding so we can keep this tidy.
    /// - CTOR : Where everything is "set up". Inject classes of (certain/complex?) sprites in order to use them (not many player sprites need a class, but Whiskers does)
    /// - Update : Update anything of sprites, in real time
    /// - InitiateSprites : Adding sprites (internal/backend stuff) calling AddToContainer
    /// - AddToContainer : Add sprite to correct container
    /// - DrawSprites : Rendering the added sprites onto the screen (frontend stuff)
    /// - ApplyPalette : Function dedicated to coloring the sprites when drawn by DrawSprites
    ///
    /// Beacon coloring is done in DrawSprites.
    /// [Lur]
    /// </summary>
    
    /// <summary>
    /// Assigns the correct lerp color to be used for Thanatosis sprite color lerping, because their colors change with progression!
    /// </summary>
    public static Color[] ColorsForBeaconSprites(PlayerGraphics self)
    {
        bool pickedSkinColor = false;
        bool pickedEyeColor = false;
        Color[] colors = new Color[2];

        var saveState = self.player.abstractCreature.world.game.GetStorySession.saveState;
        bool usesThanatosis = BeaconSaveData.GetCanUseThanatosis(saveState);
        // Target 2f
        bool rotMode = BeaconSaveData.GetMaxSpiralLevel(saveState) > 1.5f;
        // Target 4f
        bool hybridMode = BeaconSaveData.GetMaxSpiralLevel(saveState) > 3.5f;
        bool tooWeakToMaintainNourishment = BeaconSaveData.GetDreamerEncountersNumber(saveState) < 4;

        if (!pickedSkinColor)
        {
            if (rotMode || hybridMode)
            {
                colors[0] = Colors.PlayerPaletteBlack;
            }
            else if (tooWeakToMaintainNourishment)
            {
                colors[0] = Colors.BeaconStarveColor;
            }
            else
            {
                colors[0] = Colors.BeaconDefaultColor;
            }
        }
        if (!pickedEyeColor)
        {
            if (hybridMode)
            {
                colors[1] = Colors.NightmareColor;
            }
            else if (rotMode)
            {
                colors[1] = RainWorld.RippleColor;
            }
            else if (tooWeakToMaintainNourishment)
            {
                colors[1] = Color.Lerp(Colors.BeaconEyeColor, RainWorld.RippleColor, 0.25f);
            }
            else
            {
                colors[1] = Colors.BeaconEyeColor;
            }
            pickedEyeColor = true;
        }

        SpriteColors = colors;
        return colors;
    }

    // Assign placeholders to dynamic values
    public static Color DecidedSkinColor = Colors.BeaconFullColor;
    public static Color DecidedEyeColor = Colors.BeaconEyeColor;
    public static Color[] SpriteColors = [];

    public static void Apply()
    {
        On.PlayerGraphics.ctor += PlayerGraphics_ctor;
        On.PlayerGraphics.Update += PlayerGraphics_Update;
        On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
        On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
    }
    
    private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
    {
        orig(self, ow);

        if (self.player.TryGetBeacon(out var beacon))
        {
            beacon.graphics.Set(self);
            
        }
    }

    private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);

        if (self.player.TryGetBeacon(out var beacon))
        {
            beacon.graphics.Update();
        }
    }
    
    private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        if (self.player.TryGetBeacon(out var beacon) && !beacon.graphics.init)
        {
            beacon.graphics.init = true;
            beacon.graphics.InitiateSprites(sLeaser, rCam);
            self.AddToContainer(sLeaser, rCam, null);
        }
    }
    
    private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);

        if (self.player.TryGetBeacon(out var beacon) && beacon.graphics.init)
        {
            beacon.graphics.init = false;
            beacon.graphics.AddToContainer(sLeaser, rCam, newContatiner);
        }
    }
    
    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        
        if (BeaconUtils.IsBeacon(self.player.abstractCreature.world.game.GetStorySession))
        {
            // Assign DecidedSkinColor OUTSIDE of CWT.
            SpriteColors = ColorsForBeaconSprites(self);
        }

        if (self.player.TryGetBeacon(out var beacon))
        {
            beacon.graphics.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            // "gets" slugcat color stuff to then be assigned
            Color color = PlayerGraphics.SlugcatColor(self.CharacterForColor);
            Color skinColor = new Color(color.r, color.g, color.b);
            Color eyeColor = new Color(color.r, color.g, color.b);

            int flares = 0;
            if (beacon.storage != null)
            {
                flares = beacon.storage.storedFlares.Count;
            }
            skinColor = Color.Lerp(Colors.BeaconDefaultColor, Colors.BeaconFullColor, flares / (float)4);
            eyeColor = Colors.BeaconEyeColor;
            
            if (beacon.cycle != null
                && (beacon.cycle.state == Cycle.State.Thanatosis
                    || beacon.cycle.state == Cycle.State.ExitThanatosis
                    || beacon.cycle.thanatosisLerp > 0f))
            {
                beacon.currentSkinColor = Color.Lerp(skinColor, SpriteColors[0], beacon.cycle.thanatosisLerp);
                beacon.currentEyeColor = Color.Lerp(eyeColor, SpriteColors[1], beacon.cycle.thanatosisLerp);
            }
            else
            {
                beacon.currentSkinColor = skinColor;
                beacon.currentEyeColor = eyeColor;
            }
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                // eyes
                if (i != 9)
                {
                    sLeaser.sprites[i].color = beacon.currentSkinColor;
                }
                else
                {
                    if (beacon.cycle.isDead || (beacon.cycle.thanatosisTutorialSequence != null && beacon.cycle.thanatosisTutorialSequence.markedAsDead))
                    {
                        sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("FaceDead");
                    }
                    sLeaser.sprites[i].color = beacon.currentEyeColor;
                }
            }
        }
    }
    
    private static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        
        if (self.player.TryGetBeacon(out var beacon))
        {
            beacon.graphics.ApplyPalette(sLeaser, rCam, palette);
        }
    }
}