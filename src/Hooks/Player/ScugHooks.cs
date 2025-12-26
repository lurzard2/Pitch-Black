using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using UnityEngine;
using static PitchBlack.Plugin;

namespace PitchBlack;

public static class ScugHooks
{
    /// <summary>
    /// Beacon's own update function, put things here instead of directly into a Player.Update hook, because counting inside update impacts performance.
    /// </summary>
    private static void BeaconUpdate(Player self)
    {
        if (devMode)
        {
            var saveState = self.room.world.game.GetStorySession.saveState;
            if (devMode)
            {
                BeaconSaveData.SetCanUseThanatosis(saveState, true);
                BeaconSaveData.SetHasUsedThanatosis(saveState, true);
                BeaconSaveData.SetMaxSpiralLevel(saveState, 1f);
                BeaconSaveData.SetSpiralLevel(saveState, 1f);
                BeaconSaveData.SetDreamerEncountersNumber(saveState, 3);
            }
        }

        // Check here if it's Beacon
        if (scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt)
        {
            if (cwt.playerCycle != null)
            {
                cwt.playerCycle.Update();
            }

            if (cwt.dontThrowTimer > 0)
            {
                cwt.dontThrowTimer--;
            }
            
            // Detect darkness for beacon squinting if room is too bright -WW
            // A little bit of the code for squinting is also in Player\Graphics\ScugGraphics.cs -Lur
            if (self.room != null)
            {
                if (MiscUtils.MakeBeaconCloseEyesHere(self, self.room.world.region.name.ToLowerInvariant(), self.room.abstractRoom.name.ToLowerInvariant()))
                {
                    if (cwt.brightSquint == 0)
                    {
                        cwt.brightSquint = 40 * 6;
                        self.Blink(8);
                    }

                    // Tick down, but not all the way
                    if (cwt.brightSquint > 1)
                    {
                        cwt.brightSquint--;   
                    }
                    else if (cwt.brightSquint == 1)
                    {
                        self.Blink(5);   
                    }
                }
                // Otherwise, tick down
                else if (cwt.brightSquint > 0)
                {
                    cwt.brightSquint--;
                }
            }
        }
    }

    public static void Apply()
    {
        On.SlugcatStats.SlugcatToTimeline += SlugcatStats_SlugcatToTimeline;
        On.Player.ctor += Player_ctor;
        On.Player.Update += Player_Update;
        On.SlugcatHand.EngageInMovement += SlugcatHand_EngageInMovement;
        IL.Player.checkInput += IL_Player_checkInput;
    }

    private static void IL_Player_checkInput(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        try
        {
            // This matches to line 104 (IL_00C8) in IL view, or in the middle of line 26 in C# view, and puts the cursor after the call instruction.
            cursor.GotoNext(MoveType.After, i => i.MatchCall(typeof(RWInput), nameof(RWInput.PlayerInput)));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_0);

            cursor.EmitDelegate((Player.InputPackage originalInputs, Player self, int num) =>
            {
                // This needs a proper check for if the player is in thanatosis
                if (Plugin.scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT beaconCWT)
                {
                    //var state = (self.room.game.session as StoryGameSession).saveState;
                    if (beaconCWT.playerCycle.thanatosisTutorialSequence != null && beaconCWT.playerCycle.thanatosisTutorialSequence.markedAsDead)
                    {
                        // Create new inputs
                        Player.InputPackage newInputs = new Player.InputPackage(self.room.game.rainWorld.options.controls[num].gamePad, self.room.game.rainWorld.options.controls[num].GetActivePreset(), 0, 0, false, false, false, false, false, originalInputs.spec);
                        newInputs.downDiagonal = 0;
                        newInputs.analogueDir = Vector2.zero;

                        // Put new values on the stack
                        return newInputs;
                    }
                }
                // If the prior condition is not met, just return the original inputs to the stack.
                    return originalInputs;
            });
            Plugin.logger.LogDebug($"PB {nameof(IL_Player_checkInput)} applied successfully");
        }
        catch (Exception err)
        {
            Plugin.logger.LogDebug($"PB {nameof(IL_Player_checkInput)} could not match IL.\n{err}");
        }
    }


    /// <summary>
    /// Moves hand above head when squinting if a room is too bright
    /// [WW]
    /// </summary>
    private static bool SlugcatHand_EngageInMovement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand self)
    {
        Player player = self.owner.owner as Player;
        
        if (scugCWT.TryGetValue(player, out ScugCWT c) && c is BeaconCWT cwt && cwt.brightSquint > 1)
        {
            PlayerGraphics graf = player.graphicsModule as PlayerGraphics;

            // OKAY WE HAVE NO ACCESS TO EYE POSITION SO WE GOTTA DO THIS...
            // NEVERMIND IT'D BE WAY LESS WORK TO JUST TRANSFER THE EYE POS
            Vector2 shieldDir = graf.lookDirection;
            if (Mathf.Abs(shieldDir.x) <= 0.3 || player.input[0].x != 0)
                shieldDir.x = player.flipDirection;
            shieldDir.y = Mathf.Clamp(shieldDir.y, 0.35f, 0.75f) - 0.2f;

            int touchingHand = shieldDir.x <= 0 ? 0 : 1;
            if (self.limbNumber == touchingHand)
            {
                self.mode = Limb.Mode.HuntAbsolutePosition;
                self.huntSpeed = 15f;
                Vector2 targPos = (player.graphicsModule as PlayerGraphics).head.pos + (shieldDir * 15) + (player.graphicsModule as PlayerGraphics).head.vel;
                self.absoluteHuntPos = targPos - Custom.DirVec(player.bodyChunks[0].pos, targPos) * 3f;
                return false;
            }

        }

        return orig(self);
    }

    /// <summary>
    /// Injects BeaconUpdate function into Player.Update before the original code (which maintains performance).
    /// </summary>
    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        BeaconUpdate(self);
        
        orig(self, eu);
    }
    
    /// <summary>
    /// Adding BeaconCWT to Beacon, which allows checking one/multiple instances of Beacon.
    /// ^SUPER IMPORTANT! Because otherwise Whiskers and stuff don't work.
    /// Adding/Skipping adding flare to storage code.
    /// </summary>
    private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        
        if (MiscUtils.IsBeacon(self.slugcatStats.name))
        {
            if (!scugCWT.TryGetValue(self, out _))
            { 
                scugCWT.Add(self, new BeaconCWT(self));
            }
            
            if (self.room.abstractRoom.shelter 
                && scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt) {
                foreach (List<PhysicalObject> thingQuar in self.room.physicalObjects) {
                    foreach (PhysicalObject item in thingQuar) {
                        if (item is FlareBomb flare && cwt.storage.storedFlares.Count < cwt.storage.capacity) {
                            foreach (var player in self.room.PlayersInRoom) {
                                if (player != null && scugCWT.TryGetValue(player, out var op) && op is BeaconCWT otherPlayer && otherPlayer.storage.storedFlares.Contains(flare)) {
                                    goto SkipAddingFlare;
                                }
                            }
                            cwt.storage.FlarebombtoStorage(flare);
                            SkipAddingFlare:;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Beacon slugcat set to correspond with the Beacon timeline.
    /// </summary>
    private static SlugcatStats.Timeline SlugcatStats_SlugcatToTimeline(On.SlugcatStats.orig_SlugcatToTimeline orig, SlugcatStats.Name slugcat)
    {
        orig(slugcat);
        
        if (slugcat == Enums.SlugcatStatsName.Beacon)
        {
            return Enums.Timeline.Beacon;
        }
        return orig(slugcat);
    }
}