using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using SlugBase.SaveData;
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
        ThanatosisUpdate(self);

        // For debugging pre-Dreamer implementation
        bool dev = true;
        if (dev)
        {
            // Max: <=1f cant move in thanatosis
            // Max: >=2f rot apearance
            // Whole numbers will represent # of revives
            var state = (self.room.game.session as StoryGameSession).saveState;
            if (!BeaconSaveData.GetCanUseThanatosis(state))
            {
                BeaconSaveData.SetCanUseThanatosis(state, true);
            }
            if (BeaconSaveData.GetMinSpiralLevel(state) == 0f)
            {
                BeaconSaveData.SetMinSpiralLevel(state, 1f);
            }
            if (BeaconSaveData.GetSpiralLevel(state) == 0f)
            {
                BeaconSaveData.SetSpiralLevel(state, 2f);
            }
            if (BeaconSaveData.GetMaxSpiralLevel(state) == 0f)
            {
                BeaconSaveData.SetMaxSpiralLevel(state, 2f);
            }
        }

        // Check here if it's Beacon
        if (scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt)
        {
            if (cwt.dontThrowTimer > 0)
            {
                cwt.dontThrowTimer--;
            }
            
            // Detect darkness for beacon squinting if room is too bright -WW
            // A little bit of the code for squinting is also in Player\Graphics\ScugGraphics.cs -Lur
            if (self.room != null)
            {
                if (self.room.Darkness(self.mainBodyChunk.pos) < 0.15f || MiscUtils.RegionBlindsBeacon(self.room))
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

    private static void ThanatosisUpdate(Player self)
    {
        ThanatosisDeathIntensity(self);

        var state = (self.room.game.session as StoryGameSession).saveState;

        if (scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt)
        {
            // inject ability and input check here specifically
            if (BeaconSaveData.GetCanUseThanatosis(state) && self.input[0].spec)
            {
                logger.LogDebug($"[THANATOSIS] Doing input for Thanatosis, input time: {cwt.inputForThanatosisCounter}.");
                ToggleThanatosis(self);
            }
            if (!self.input[0].spec)
            {
                cwt.inputForThanatosisCounter = 0;
            }

            // determining actual death
            if (cwt.thanatosisCounter >= cwt.thanatosisLimit || (cwt.isDead && self.dead) && !cwt.diedInThanatosis)
            {
                logger.LogDebug($"[THANATOSIS] Time limit reached. Time: {cwt.thanatosisCounter}/{cwt.thanatosisLimit}.");
                cwt.diedInThanatosis = true;
            }
            else if (!cwt.diedInThanatosis)
            {
                cwt.thanatosisDeathBumpNeedsToPlay = false;
            }
            if (cwt.diedInThanatosis && !cwt.thanatosisDeathBumpNeedsToPlay && self.rippleDeathTime == 80)
            {
                self.room.PlaySound(Enums.SoundID.Player_Died_From_Thanatosis);
                if (BeaconSaveData.GetSpiralLevel(state) > BeaconSaveData.GetMinSpiralLevel(state))
                {
                    BeaconSaveData.SetSpiralLevel(state, BeaconSaveData.GetSpiralLevel(state) - 1f);
                }
                    //self.room.PlaySound(SoundID.Gate_Rails_Collide);
                    cwt.thanatosisDeathBumpNeedsToPlay = true;
            }

            switch (cwt.isDead)
            {
                case true:
                    InThanatosis(self);
                    break;
                case false:
                    OutsideThanatosis(self);
                    break;
            }
        }
    }

    private static void ToggleThanatosis(Player self)
    {
        var GotCWTData = scugCWT.TryGetValue(self, out ScugCWT c);
        if (GotCWTData && c is BeaconCWT bCWT)
        {
            bCWT.inputForThanatosisCounter++;
            if (bCWT.inputForThanatosisCounter == 24)
            {
                bCWT.deathToggle = bCWT.isDead;
                bCWT.isDead = !bCWT.isDead;
                // Toggling
                if (bCWT.deathToggle != bCWT.isDead)
                {
                    logger.LogDebug($"[THANATOSIS] Toggle reached! Toggling Thanatosis: {bCWT.isDead}. Ripple Layer: {self.abstractCreature.rippleLayer}.");
                    self.abstractCreature.rippleLayer = bCWT.isDead ? 1 : 0;
                    self.room.PlaySound(
                        bCWT.isDead
                            ? Enums.SoundID.Player_Activated_Thanatosis
                            : Enums.SoundID.Player_Deactivated_Thanatosis, self.mainBodyChunk);
                }
            }

        }
    }

    private static void InThanatosis(Player self)
    {
        var GotCWTData = scugCWT.TryGetValue(self, out ScugCWT c);
        if (GotCWTData && c is BeaconCWT cwt)
        {
            // Spawn a DreamSpawn
            if (!cwt.spawnLeftBody)
            {
                //MiscUtils.MaterializeDreamSpawn(self.room, self.mainBodyChunk.pos, PBEnums.VoidSpawn.SpawnSource.Death);
                cwt.spawnLeftBody = true;
            }

            // Input removing is done in IL_Player_checkInput

            // Increase time
            if (cwt.thanatosisCounter <= cwt.thanatosisLimit)
            {
                cwt.thanatosisCounter++;
                if (cwt.thanatosisLerp < 0.92f)
                {
                    cwt.thanatosisLerp += 0.01f;
                }
                if (!cwt.graspsNeedToBeReleased)
                {
                    self.LoseAllGrasps();
                    //DropAllFlares(self);
                    cwt.graspsNeedToBeReleased = true;
                }
            }
        }
    }

    private static void OutsideThanatosis(Player self)
    {
        var GotCWTData = scugCWT.TryGetValue(self, out ScugCWT c);
        if (GotCWTData && c is BeaconCWT cwt)
        {
            cwt.graspsNeedToBeReleased = false;
            cwt.spawnLeftBody = false;
            if (cwt.thanatosisCounter > 0)
            {
                cwt.thanatosisCounter--;
                if (cwt.thanatosisLerp > 0f)
                {
                    cwt.thanatosisLerp -= 0.01f;
                }
            }
            if (cwt.thanatosisLerp < 0.01f)
            {
                cwt.thanatosisCounter = 0;
                self.abstractCreature.rippleLayer = 0;
            }
        }
    }

    /// <summary>
    /// Camera effect for Thanatosis using Watcher's RippleDeath shader
    /// </summary>
    private static void ThanatosisDeathIntensity(Player self)
    {
        if (scugCWT.TryGetValue(self, out ScugCWT c) && c is BeaconCWT cwt)
        {
            if (cwt.isDead)
            {
                // Calculation made by MaxDubstep <3
                float timeCounter = cwt.thanatosisCounter; //x
                float minKarmaSafeTime = 12 * 40f; //tc
                float maxKarmaSafeTime = 40 * 40f; // Tc
                float beginningIntensity = 0.4f; //l
                float endIntensity = 0.45f; //m
                float windUpTime = 3 * 40f; //wc
                float rampUpTime = 3 * 40f; //Wc
                var state = (self.room.game.session as StoryGameSession).saveState;
                float plateauDuration = (BeaconSaveData.GetSpiralLevel(state) - 1) * (maxKarmaSafeTime - (windUpTime + rampUpTime) * 2) / 4 + minKarmaSafeTime - windUpTime - rampUpTime; //c
                // Starting plateau
                if (timeCounter < windUpTime)
                {
                    self.rippleDeathIntensity = Mathf.Sqrt(timeCounter) * beginningIntensity / Mathf.Sqrt(windUpTime);
                }
                // Middle of plateau
                if ((timeCounter < windUpTime + plateauDuration) && timeCounter >= windUpTime)
                {
                    self.rippleDeathIntensity = (timeCounter - windUpTime) * (endIntensity - beginningIntensity) / plateauDuration + beginningIntensity;
                }
                // Ending DIE INTENSITY!!!!
                if (timeCounter >= windUpTime + plateauDuration + (rampUpTime / 2))
                {
                    float increment = 0.008f;
                    int mult = 4;
                    self.rippleDeathIntensity += increment;
                    increment += 0.008f * mult;
                    mult += 4;
                }
            }
            if ((cwt.diedInThanatosis || self.dead) && self.rippleDeathIntensity < 0.12f)
            {
                self.rippleDeathIntensity += 0.004f;
            }
            if (self.rippleDeathIntensity > 0 && !cwt.isDead)
            {
                self.rippleDeathIntensity -= 0.004f;
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
                    var state = (self.room.game.session as StoryGameSession).saveState;
                    if (beaconCWT.isDead && BeaconSaveData.GetMaxSpiralLevel(state) <= 1f)
                    {
                        // Create new inputs
                        Player.InputPackage newInputs = new Player.InputPackage(self.room.game.rainWorld.options.controls[num].gamePad, self.room.game.rainWorld.options.controls[num].GetActivePreset(), 0, 0, false, false, false, false, false, originalInputs.spec);
                        newInputs.downDiagonal = 0;
                        newInputs.analogueDir = Vector2.zero;

                        // Set animation and body mode
                        self.animation = Player.AnimationIndex.Dead;
                        self.bodyMode = Player.BodyModeIndex.Dead;

                        // Put new values on the stack
                        return newInputs;
                    }
                    else if (beaconCWT.thanatosisLerp > 0 && !beaconCWT.diedInThanatosis && !self.dead && self.bodyMode == Player.BodyModeIndex.Dead)
                    {
                        //self.animation = Player.AnimationIndex.DownOnFours;
                        self.bodyMode = Player.BodyModeIndex.Crawl;
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
        /* Called without a scugcwt-beaconcwt check because Update doesn't like moving classes within cwt code
        slug check is inside the function -Lur */
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