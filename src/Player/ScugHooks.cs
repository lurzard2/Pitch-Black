using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;
using static PitchBlack.Plugin;

namespace PitchBlack;

public static class ScugHooks
{
    public static void Apply()
    {
        On.SlugcatStats.SlugcatToTimeline += SlugcatToTimeline_MODIFY;
        On.Player.ctor += Player_ctor;
        On.Player.Update += Player_Update;
        On.SlugcatHand.EngageInMovement += SlugcatHand_EngageInMovement;
        IL.Player.checkInput += IL_Player_checkInput_SPECIALONLY;
    }

    // Allowing for special input without any others in certain circumstances
    private static void IL_Player_checkInput_SPECIALONLY(ILContext il)
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
                if (Plugin.scugCWT.TryGetValue(self, out ScugCWT c) && c is Beacon beaconCWT)
                {
                    //var state = (self.room.game.session as StoryGameSession).saveState;
                    if (beaconCWT.cycle.thanatosisTutorialSequence != null && beaconCWT.cycle.thanatosisTutorialSequence.markedAsDead)
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
            Plugin.logger.LogDebug($"PB {nameof(IL_Player_checkInput_SPECIALONLY)} applied successfully");
        }
        catch (Exception err)
        {
            Plugin.logger.LogDebug($"PB {nameof(IL_Player_checkInput_SPECIALONLY)} could not match IL.\n{err}");
        }
    }

    /// <summary>
    /// Moves hand above head when squinting if a room is too bright
    /// [WW]
    /// </summary>
    private static bool SlugcatHand_EngageInMovement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand self)
    {
        var player = self.owner.owner as Player;
        
        if (scugCWT.TryGetValue(player, out ScugCWT c) && c is Beacon beacon && beacon.squinter.squintTick > 1)
        {
            PlayerGraphics pGraphics = player.graphicsModule as PlayerGraphics;

            // OKAY WE HAVE NO ACCESS TO EYE POSITION SO WE GOTTA DO THIS...
            // NEVERMIND IT'D BE WAY LESS WORK TO JUST TRANSFER THE EYE POS
            Vector2 shieldDir = pGraphics.lookDirection;
            if (Mathf.Abs(shieldDir.x) <= 0.3 || player.input[0].x != 0)
                shieldDir.x = player.flipDirection;
            shieldDir.y = Mathf.Clamp(shieldDir.y, 0.35f, 0.75f) - 0.2f;

            int touchingHand = shieldDir.x <= 0 ? 0 : 1;
            if (self.limbNumber == touchingHand)
            {
                self.mode = Limb.Mode.HuntAbsolutePosition;
                self.huntSpeed = 15f;
                Vector2 targetPos = (player.graphicsModule as PlayerGraphics).head.pos + (shieldDir * 15) + (player.graphicsModule as PlayerGraphics).head.vel;
                self.absoluteHuntPos = targetPos - Custom.DirVec(player.bodyChunks[0].pos, targetPos) * 3f;
                return false;
            }
        }

        return orig(self);
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        if (scugCWT.TryGetValue(self, out var c) && c is Beacon beacon)
        {
            beacon.Update();
        }
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
                scugCWT.Add(self, new Beacon(self));
            }
            
            // Adding back flares
            if (self.room.abstractRoom.shelter 
                && scugCWT.TryGetValue(self, out ScugCWT c) && c is Beacon beacon)
            {
                foreach (List<PhysicalObject> thingQuar in self.room.physicalObjects) {
                    foreach (PhysicalObject item in thingQuar) {
                        if (item is FlareBomb flare && beacon.storage.storedFlares.Count < beacon.storage.capacity) {
                            foreach (var player in self.room.PlayersInRoom) {
                                if (player != null && scugCWT.TryGetValue(player, out var op) && op is Beacon otherBeacon && otherBeacon.storage!= null && otherBeacon.storage.storedFlares.Contains(flare)) {
                                    goto SkipAddingFlare;
                                }
                            }
                            beacon.storage.FlarebombtoStorage(flare);
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
    private static SlugcatStats.Timeline SlugcatToTimeline_MODIFY(On.SlugcatStats.orig_SlugcatToTimeline orig, SlugcatStats.Name slugcat)
    {
        orig(slugcat);
        
        if (slugcat == Enums.SlugcatStatsName.Beacon)
        {
            return Enums.Timeline.Beacon;
        }
        return orig(slugcat);
    }
}