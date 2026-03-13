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
            beacon.graphics.Setup(self);
            
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

        if (self.player.TryGetBeacon(out var beacon))
        {
            beacon.graphics.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        //if (self.player.TryGetBeacon(out var beacon))
        //{
        //    beacon.graphics.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        //    // "gets" slugcat color stuff to then be assigned
        //    Color color = PlayerGraphics.SlugcatColor(self.CharacterForColor);
        //    Color skinColor = new Color(color.r, color.g, color.b);
        //    Color eyeColor = new Color(color.r, color.g, color.b);

        //    int flares = 0;
        //    if (beacon.storage != null)
        //    {
        //        flares = beacon.storage.storedFlares.Count;
        //    }
        //    skinColor = Color.Lerp(Colors.BeaconDefaultColor, Colors.BeaconFullColor, flares / (float)4);
        //    eyeColor = Colors.BeaconEyeColor;

        //    if (beacon.cycle != null
        //        && (beacon.cycle.state == Cycle.State.Thanatosis
        //            || beacon.cycle.state == Cycle.State.ExitThanatosis
        //            || beacon.cycle.thanatosisLerp > 0f))
        //    {
        //        beacon.currentSkinColor = Color.Lerp(skinColor, SpriteColors[0], beacon.cycle.thanatosisLerp);
        //        beacon.currentEyeColor = Color.Lerp(eyeColor, SpriteColors[1], beacon.cycle.thanatosisLerp);
        //    }
        //    else
        //    {
        //        beacon.currentSkinColor = skinColor;
        //        beacon.currentEyeColor = eyeColor;
        //    }
        //    for (int i = 0; i < sLeaser.sprites.Length; i++)
        //    {
        //        // eyes
        //        if (i != 9)
        //        {
        //            sLeaser.sprites[i].color = beacon.currentSkinColor;
        //        }
        //        else
        //        {
        //            if (beacon.cycle.isDead || (beacon.cycle.thanatosisTutorialSequence != null && beacon.cycle.thanatosisTutorialSequence.markedAsDead))
        //            {
        //                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("FaceDead");
        //            }
        //            sLeaser.sprites[i].color = beacon.currentEyeColor;
        //        }
        //    }
        //}
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