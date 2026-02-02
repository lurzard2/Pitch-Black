using IL.Menu;
using RWCustom;
using System;
using System.Linq;
using UnityEngine;
using Watcher;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class ThanatosisTutorialSequence
{
    #region Prompts
    public InGameTranslator Translator => room.world.game.manager.rainWorld.inGameTranslator;
    public string SequenceText => Translator.Translate("Hold SPECIAL to maintain stasis, but only if it gets too close...");
    public string SequenceInvertedText => Translator.Translate("The tides are shifting...");
    public string CycleDisplayText => Translator.Translate($"Cycle {cycle.SaveState.cycleNumber} ~ {Region.GetRegionFullName(cycle.owner.room.world.region.name, SlugcatStats.Name.White)}");
    public string SequenceCompleteText => Translator.Translate("Hold SPECIAL to enter and exit a state of thanatosis");
    #endregion

    public BeaconCycle cycle;   
    public Room room;
    public RoomCamera rCam;

    public Counter sequenceTime = new(Int32.MaxValue, 0, true);
    public Counter sequencePhaseTime = new(Int32.MaxValue, 0, true);
    public Counter timeTilCycleDisplay = new(25, 0, true);
    public Counter deathTime = new(80, 0, true);
    public CreatureSpasmer spasmer;

    #region Phases
    public class Phase : ExtEnum<Phase>
    {
        public Phase(string value, bool register) : base(value, register) { }

        public static readonly Phase Init = new(nameof(Init), true);
        public static readonly Phase JustExitedFromEncounter = new(nameof(JustExitedFromEncounter), true);
        public static readonly Phase PendingSuffocation = new(nameof(PendingSuffocation), true);
        public static readonly Phase StartSuffocation = new(nameof(StartSuffocation), true);
        public static readonly Phase SlowlyCloseIn = new(nameof(SlowlyCloseIn), true);
        public static readonly Phase PassEventHorizon_StayInSpotInCurrentRoom = new(nameof(PassEventHorizon_StayInSpotInCurrentRoom), true);
        public static readonly Phase InitiateDrown = new(nameof(InitiateDrown), true);
        public static readonly Phase ReadyForDie = new(nameof(ReadyForDie), true);
        public static readonly Phase InitiateDie = new(nameof(InitiateDie), true);
        public static readonly Phase Dead = new(nameof(Dead), true);
        public static readonly Phase Limbo = new(nameof(Limbo), true);
        public static readonly Phase AttemptingPersist = new(nameof(AttemptingPersist), true);
        public static readonly Phase FailedToPersist = new(nameof(FailedToPersist), true);
        public static readonly Phase PassPersistEventHorizon_NoLongerNeedsInput = new(nameof(PassPersistEventHorizon_NoLongerNeedsInput), true);
        public static readonly Phase InitiatePersist = new(nameof(InitiatePersist), true);
        public static readonly Phase Thanatosis = new(nameof(Thanatosis), true);
        public static readonly Phase UsedThanatosis = new(nameof(UsedThanatosis), true);
    }
    public Phase phase;
    private void ChangePhase(Phase phase)
    {
        this.phase = phase;
        sequencePhaseTime.Reset();
    }
    #endregion

    float tutorialDeathIntensity = 0;
    float targetDeathIntensity = 0;

    public bool markedAsDead;
    private bool seenSpecialPromptThisCycle;
    private bool seenPromptForTidesShifting;
    private bool seenTrueTutorialPromptThisCycle;

    private SequenceSong thanatosisSong;
    private string songName = "PB_12 - Fated Demise";
    private bool songPlayed;

    private bool stopShowingCycleCount;

    public ThanatosisTutorialSequence(BeaconCycle cycle, Room room)
    {
        this.cycle = cycle;
        this.room = room;
        rCam = cycle.owner.room.game.cameras[0];
        phase = Phase.Init;
        //saveGameFramesDefault = cycle.owner.room.world.game.framesPerSecond;
        markedAsDead = false;
        seenSpecialPromptThisCycle = false;
        seenPromptForTidesShifting = false;
        seenTrueTutorialPromptThisCycle = false;
        thanatosisSong = new SequenceSong(room.game.manager.musicPlayer, songName);
        songPlayed = false;
        stopShowingCycleCount = false;
    }

    public void Update()
    {
        if (phase == Phase.Init)
        {
            ChangePhase(Phase.JustExitedFromEncounter);
            return;
        }

        SequenceTick();

        // Tracking RippleRings in room
        if (!stopShowingCycleCount || rCam.hud.textPrompt.messages.Count == 0)
        {
            foreach (var ring in cycle.owner.room.updateList.OfType<RippleRing>())
            {
                if (ring.intensity > 0.6f)
                {
                    cycle.SaveState.cycleNumber -= ring.intensity >= 0.85f ? 1 : UnityEngine.Random.Range(1, 4);
                }
                if (timeTilCycleDisplay.isFinished)
                {
                    MiscUtils.AddHUDMessage(rCam.hud, false, CycleDisplayText, 40, 20, false, true);
                    timeTilCycleDisplay.Reset();
                } 
            }
        }

        // Death effect
        if (targetDeathIntensity < 0)
        {
            targetDeathIntensity = 0;
        }
        tutorialDeathIntensity = Mathf.Lerp(tutorialDeathIntensity, targetDeathIntensity, 0.01f);
        cycle.owner.rippleDeathIntensity = tutorialDeathIntensity;

        // Drowning
        if (markedAsDead)
        {
            cycle.owner.animation = Player.AnimationIndex.Dead;
            cycle.owner.bodyMode = Player.BodyModeIndex.Dead;
            (cycle.owner.graphicsModule as PlayerGraphics).LookAtNothing();

        }
        else if (!cycle.owner.Stunned && !cycle.owner.dead && cycle.owner.bodyMode == Player.BodyModeIndex.Dead && phase == Phase.UsedThanatosis)
        {
            cycle.owner.animation = Player.AnimationIndex.DownOnFours;
            cycle.owner.bodyMode = Player.BodyModeIndex.Crawl;
            cycle.owner.Blink(40);
        }
        else
        {
            cycle.owner.airInLungs -= tutorialDeathIntensity;
        }

        //logger.LogDebug($"EFFECT:{targetDeathIntensity} - PHASE:{phase.value} - THANATOSISLERP:{cycle.thanatosisLerp}");

        if (phase == Phase.JustExitedFromEncounter)
        {
            if (sequenceTime == (40 * 20))
            {
                ChangePhase(Phase.PendingSuffocation);
            }
        }

        if (phase == Phase.PendingSuffocation)
        {
            if (sequencePhaseTime == (40 * 35))
            {
                ChangePhase(Phase.StartSuffocation);
            }
            else if (sequencePhaseTime >= (40 * 30) && targetDeathIntensity < 0.4f)
            {
                targetDeathIntensity += 0.006f;
            }
            else if (targetDeathIntensity < 0.2f)
            {
                targetDeathIntensity += 0.002f;
            }
        }

        if (phase == Phase.StartSuffocation)
        {
            // Start playing a scripted music track which lasts until the sequence ends
            var musicPlayer = room.game.manager.musicPlayer;
            if (!songPlayed && musicPlayer != null && room.game.world.rainCycle.MusicAllowed && thanatosisSong.ConditionToPlay(songName))
            {
                thanatosisSong.StopCurrentSong();
                musicPlayer.song = thanatosisSong;
                musicPlayer.song.playWhenReady = true;
                songPlayed = true;
            }

            if (sequencePhaseTime >= (40 * 15) && targetDeathIntensity < 0.1f)
            {
                ChangePhase(Phase.SlowlyCloseIn);
            }
            else
            {
                if (targetDeathIntensity > 0)
                {
                    targetDeathIntensity -= 0.002f;
                }
            }
        }

        if (phase == Phase.SlowlyCloseIn)
        {
            if (sequencePhaseTime >= (40 * 8))
            {
                ChangePhase(Phase.PassEventHorizon_StayInSpotInCurrentRoom);
            }
            else if (sequencePhaseTime >= (40 * 5) && targetDeathIntensity >= 0.2f)
            {
                targetDeathIntensity += 0.001f;
            }
            else
            {
                targetDeathIntensity += 0.002f;
            }
        }

        if (phase == Phase.PassEventHorizon_StayInSpotInCurrentRoom
            || phase == Phase.InitiateDrown
            || phase == Phase.ReadyForDie)
        {
            PassedEventHorizonUpdate();
            return;
        }

        if (phase == Phase.InitiateDie
            || phase == Phase.Dead)
        {
            if (cycle.owner.abstractCreature.rippleLayer == 0)
            {
                cycle.owner.abstractCreature.rippleLayer = 1;
            }
            KillingUpdate();
            return;
        }

        if (phase == Phase.Limbo)
        {
            if (sequencePhaseTime <= (40 * 5) && !seenSpecialPromptThisCycle)
            {
                MiscUtils.AddHUDMessage(rCam.hud, true, SequenceText, 40, 180, true, true);
                stopShowingCycleCount = true;
                seenSpecialPromptThisCycle = true;
                targetDeathIntensity = 0.2f;
            }
            else
            {
                if (seenSpecialPromptThisCycle && rCam.hud.textPrompt.messages.Count == 0 && stopShowingCycleCount)
                {
                    stopShowingCycleCount = false;
                }

                FightDeathUpdate();
                return;
            }
        }

        if (phase == Phase.PassPersistEventHorizon_NoLongerNeedsInput)
        {
            targetDeathIntensity = 0.3f;
            stopShowingCycleCount = false;
            if (cycle.thanatosisLerp < 0.5f)
            {
                cycle.thanatosisLerp += 0.004f;
            }
            if (sequencePhaseTime <= 40)
            {
                if (!cycle.owner.Malnourished)
                {
                    cycle.owner.SetMalnourished(true, false);
                }
                return;
            }
            cycle.AddRipple(Cycle.CycleRippleSource.Thanatosis);
            cycle.owner.room.PlaySound(Enums.SoundID.Player_Deactivated_Thanatosis, 0.5f, 1f, 0.9f);
            cycle.owner.room.PlaySound(Enums.SoundID.Player_Activated_Thanatosis, 0.5f, 1f, 0.9f);
            BeaconSaveData.SetCanUseThanatosis(cycle.SaveState, true);
            cycle.ToggleThanatosis(false);
            markedAsDead = false;
            cycle.owner.Stun(80);
            ChangePhase(Phase.Thanatosis);
        }

        if (phase == Phase.Thanatosis)
        {
            if (!seenTrueTutorialPromptThisCycle)
            {
                MiscUtils.AddHUDMessage(rCam.hud, true, SequenceCompleteText, 120, 220, true, true);
                stopShowingCycleCount = true;
                seenTrueTutorialPromptThisCycle = true;
            }

            BeaconSaveData.SetHasUsedThanatosis(cycle.SaveState, true);
            // Do not make them do that whole thing over again
            RainWorldGame.ForceSaveNewDenLocation(cycle.owner.room.world.game, cycle.owner.room.abstractRoom.name, true);
            cycle.spawnedPendingRipples = false;
            cycle.AddRipple(Cycle.CycleRippleSource.Thanatosis);
            // Ends sequence
            ChangePhase(Phase.UsedThanatosis);
        }
    }

    private void Fight(int time, bool inputDown)
    {
        float lastIntensity = cycle.owner.rippleDeathIntensity;
        bool inverted = time >= (40 * 45);
        float increment = 0.006f;

        // Show prompt when inverted!
        var prompt = cycle.owner.room.game.cameras[0].hud.textPrompt;
        if (inverted && !seenPromptForTidesShifting)
        {
            MiscUtils.AddHUDMessage(rCam.hud, true, SequenceInvertedText, 40, 220, true, true);
            stopShowingCycleCount = true;
            seenPromptForTidesShifting = true;
        }
        // Turn back on cycle counting
        if (prompt.messages.Count == 0 && stopShowingCycleCount && seenPromptForTidesShifting)
        {
            stopShowingCycleCount = false;
        }

        // Increasing and decreasing intensity, then inverting it
        if (inputDown)
        {
            targetDeathIntensity = inverted ? targetDeathIntensity += increment : targetDeathIntensity -= increment;
        }
        else
        {
            targetDeathIntensity = inverted ? targetDeathIntensity -= increment : targetDeathIntensity += increment;
        }

        if (time >= 40 * 100)
        {
            ChangePhase(Phase.PassPersistEventHorizon_NoLongerNeedsInput);
        }
    }

    private void TickDeath()
    {
        deathTime.Tick();

        if (deathTime.isFinished)
        {
            // For now, before I rewrite it.
            cycle.owner.Die();
        }
    }

    private void FightDeathUpdate()
    {
        if (cycle.owner.rippleDeathIntensity <= 0.11f || cycle.owner.rippleDeathIntensity >= 0.79f)
        {
            TickDeath();
        }
        else
        {
            deathTime.Reset();
        }

        bool holdingInput = false;

        int inputCount = cycle.specInputCounter;
        if (inputCount > 0)
        {
            holdingInput = true;
        }

        Fight(sequencePhaseTime, holdingInput);

        if (holdingInput)
        {
            if (cycle.idleRipplesToSpawn < 15)
            {
                cycle.idleRipplesToSpawn += UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, 12));
            }
            cycle.spawnedPendingRipples = true;
        }
        if (cycle.idleRipplesToSpawn >= 8 && cycle.spawnedPendingRipples)
        {
            cycle.AddRipple(Cycle.CycleRippleSource.Thanatosis);
            cycle.owner.room.PlaySound(Enums.SoundID.Player_Deactivated_Thanatosis, 0.5f, 0.03f * cycle.idleRipplesToSpawn, 0.85f);
        }
    }

    private void KillingUpdate()
    {
        if (sequencePhaseTime >= 40 && sequencePhaseTime <= 80)
        {
            targetDeathIntensity += 0.004f;
        }
        if (phase == Phase.InitiateDie)
        {
            if (!markedAsDead)
            {
                cycle.owner.animation = Player.AnimationIndex.Dead;
                cycle.owner.bodyMode = Player.BodyModeIndex.Dead;
                markedAsDead = true;
            }
            else
            {
                ChangePhase(Phase.Dead);
            }
        }

        if (phase == Phase.Dead)
        {
            if (sequencePhaseTime <= (40 * 8))
            {
                targetDeathIntensity -= 0.001f;
            }
            else
            {
                ChangePhase(Phase.Limbo);
            }
        }
    }

    private void PassedEventHorizonUpdate()
    {
        if (phase == Phase.PassEventHorizon_StayInSpotInCurrentRoom)
        {
            if (sequencePhaseTime >= (40 * 10))
            {
                ChangePhase(Phase.InitiateDrown);
            }
        }

        if (phase == Phase.InitiateDrown)
        {
            if (sequencePhaseTime >= (40 * 45))
            {
                ChangePhase(Phase.ReadyForDie);
            }
            if (sequencePhaseTime >= (40 * 20))
            {
                if (cycle.thanatosisLerp <= 0.25f)
                {
                    cycle.thanatosisLerp += 0.002f;
                    targetDeathIntensity += 0.0005f;
                }
            }
        }

        // This will be figured out later
        if (phase == Phase.ReadyForDie)
        {
            ChangePhase(Phase.InitiateDie);
        }

        cycle.owner.Stun(40);
        if (spasmer != null)
        {
            SpasmerUpdate();
        }
        else
        {
            spasmer = new CreatureSpasmer(cycle.owner, true, sequenceTime);
        }
    }

    private void SpasmerUpdate()
    {
        if (cycle.idleRipplesToSpawn == 0)
        {
            cycle.idleRipplesToSpawn++;
            if (cycle.owner.abstractCreature.rippleLayer == 1)
            {
                cycle.idleRipplesToSpawn += UnityEngine.Random.Range(2, 5);
            }
        }
        else
        {
            cycle.spawnedPendingRipples = true;
        }
    }

    private void SequenceTick()
    {
        sequenceTime.Tick();
        sequencePhaseTime.Tick();
        timeTilCycleDisplay.Tick();
    }
}
