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
    public string SequenceText => Translator.Translate("Hold SPECIAL to maintain stasis, but if it gets too close...");
    public string SequenceInvertedText => Translator.Translate("The tides are shifting. And If it gets too far...");
    public string CycleDisplayText => Translator.Translate($"Cycle {cycle.saveState.cycleNumber} ~ {Region.GetRegionFullName(cycle.owner.room.world.region.name, SlugcatStats.Name.White)}");
    public string SequenceCompleteText => Translator.Translate("Hold SPECIAL to enter and exit a state of thanatosis");
    #endregion

    public BeaconCycle cycle;   
    public Room room;
    public Counter sequenceTime = new(Int32.MaxValue, 0, true);
    public Counter sequencePhaseTime = new(Int32.MaxValue, 0, true);
    public Counter timeTilCycleDisplay = new(25, 0, true);
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
        public static readonly Phase Drowning_TimeSlows = new(nameof(Drowning_TimeSlows), true);
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

    private bool STOPSHOWINGCONSISTENTCYCLES;

    public ThanatosisTutorialSequence(BeaconCycle cycle, Room room)
    {
        this.cycle = cycle;
        this.room = room;
        phase = Phase.Init;
        //saveGameFramesDefault = cycle.owner.room.world.game.framesPerSecond;
        markedAsDead = false;
        seenSpecialPromptThisCycle = false;
        seenPromptForTidesShifting = false;
        seenTrueTutorialPromptThisCycle = false;
        thanatosisSong = new SequenceSong(room.game.manager.musicPlayer, songName);
        songPlayed = false;
        STOPSHOWINGCONSISTENTCYCLES = false;
    }

    public void Update()
    {
        if (phase == Phase.Init)
        {
            ChangePhase(Phase.JustExitedFromEncounter);
            return;
        }
        else
        {
            // Music Track
            if (!songPlayed && phase == Phase.StartSuffocation)
            {
                MusicEvent musicEvent = new MusicEvent();
                var musicPlayer = room.game.manager.musicPlayer;
                if (musicPlayer != null && room.game.world.rainCycle.MusicAllowed && thanatosisSong.ConditionToPlay(songName))
                {
                    thanatosisSong.StopCurrentSong();
                    musicPlayer.song = thanatosisSong;
                    musicPlayer.song.playWhenReady = true;
                }
                songPlayed = true;
            }

            SequenceTick();
            timeTilCycleDisplay.Tick();

            // Tracking RippleRings in room
            if (targetDeathIntensity > 0 && !STOPSHOWINGCONSISTENTCYCLES)
            {
                if (cycle.owner.room.updateList.FirstOrDefault(x => x is RippleRing) is RippleRing obj)
                {
                    for (int i = 0; i < cycle.owner.room.updateList.Count; i++)
                    {
                        if (cycle.owner.room.updateList[i] is RippleRing)
                        {
                            obj = cycle.owner.room.updateList[i] as RippleRing;
                            if (obj != null && obj.intensity > 0.6f)
                            {
                                cycle.saveState.cycleNumber--;
                                if (obj.intensity >= 0.85f)
                                {
                                    cycle.saveState.cycleNumber -= UnityEngine.Random.Range(1, 4);
                                }
                            }
                        }
                    }
                    if (obj != null)
                    {
                        if (timeTilCycleDisplay.isFinished)
                        {
                            MiscUtils.AddHUDMessage(cycle.owner.room.game.cameras[0].hud, false, CycleDisplayText, 40, 20, false, true);
                            //cycle.owner.room.game.cameras[0].hud.textPrompt.AddMessage(CycleDisplayText, 40, 30, false, true);
                            //var prompt = cycle.owner.room.game.cameras[0].hud.textPrompt;
                            //if (prompt.messages.Count > 0
                            //    && prompt.messages[0].text == CycleDisplayText)
                            //{
                            //    prompt.messages[0].time = 20;
                            //}
                            timeTilCycleDisplay.Reset();
                            obj = null;
                        }
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
            if (!markedAsDead)
            {
                cycle.owner.airInLungs -= tutorialDeathIntensity;
            }

            //logger.LogDebug($"EFFECT:{targetDeathIntensity} - PHASE:{phase.value} - THANATOSISLERP:{cycle.thanatosisLerp}");

            if (markedAsDead)
            {
                cycle.owner.animation = Player.AnimationIndex.Dead;
                cycle.owner.bodyMode = Player.BodyModeIndex.Dead;
            }
            else if (!cycle.owner.Stunned && !cycle.owner.dead && !markedAsDead && cycle.owner.bodyMode == Player.BodyModeIndex.Dead && phase == Phase.UsedThanatosis)
            {
                cycle.owner.animation = Player.AnimationIndex.DownOnFours;
                cycle.owner.bodyMode = Player.BodyModeIndex.Crawl;
                cycle.owner.Blink(40);
            }
        }

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
            || phase == Phase.Drowning_TimeSlows)
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
            if (sequencePhaseTime <= (40 * 5))
            {
                if (!seenSpecialPromptThisCycle)
                {
                    MiscUtils.AddHUDMessage(cycle.owner.room.game.cameras[0].hud, true, SequenceText, 40, 180, true, true);
                    //var prompt = cycle.owner.room.world.game.cameras[0].hud.textPrompt;
                    STOPSHOWINGCONSISTENTCYCLES = true;
                    //prompt.messages.Clear();
                    //AddSpecialInputPrompt();
                    //if (prompt.messages.Count > 0 && prompt.messages.Count < 2 && prompt.messages[0].text == SequenceText)
                    //{
                    //    prompt.messages[0].time = 180;
                    //}
                    seenSpecialPromptThisCycle = true;
                }
            }
            else
            {
                FightDeathUpdate();
                return;
            }
        }

        if (phase == Phase.PassPersistEventHorizon_NoLongerNeedsInput)
        {
            targetDeathIntensity = 0.3f;
            STOPSHOWINGCONSISTENTCYCLES = false;
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
            cycle.cycle.AddRipple(Cycle.CycleRippleSource.Thanatosis);
            cycle.owner.room.PlaySound(Enums.SoundID.Player_Deactivated_Thanatosis, 0.5f, 1f, 0.9f);
            cycle.owner.room.PlaySound(Enums.SoundID.Player_Activated_Thanatosis, 0.5f, 1f, 0.9f);
            BeaconSaveData.SetCanUseThanatosis(cycle.saveState, true);
            cycle.ToggleThanatosis();
            markedAsDead = false;
            cycle.owner.Stun(80);
            ChangePhase(Phase.Thanatosis);
        }

        if (phase == Phase.Thanatosis)
        {
            if (!seenTrueTutorialPromptThisCycle)
            {
                MiscUtils.AddHUDMessage(cycle.owner.room.game.cameras[0].hud, true, SequenceCompleteText, 120, 220, true, true);
                //var prompt = cycle.owner.room.game.cameras[0].hud.textPrompt;
                STOPSHOWINGCONSISTENTCYCLES = true;
                //prompt.messages.Clear();
                //cycle.owner.room.game.cameras[0].hud.textPrompt.AddMessage(SequenceCompleteText, 40, 200, true, true);
                //if (prompt.messages.Count > 0)
                //{
                //    // Tutorial text
                //    if (prompt.messages[0].text == SequenceCompleteText)
                //    {
                //        prompt.messages[0].time = 180;
                //    }
                //}
                seenTrueTutorialPromptThisCycle = true;
            }

            BeaconSaveData.SetHasUsedThanatosis(cycle.saveState, true);
            // Do not make them do that whole thing over again
            RainWorldGame.ForceSaveNewDenLocation(cycle.owner.room.world.game, cycle.owner.room.abstractRoom.name, false);
            cycle.cycle.spawnRipples = false;
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
            MiscUtils.AddHUDMessage(cycle.owner.room.game.cameras[0].hud, true, SequenceInvertedText, 40, 220, true, true);
            STOPSHOWINGCONSISTENTCYCLES = true;
            seenPromptForTidesShifting = true;
        }
        // Turn back on cycle counting
        if (prompt.messages.Count == 0 && STOPSHOWINGCONSISTENTCYCLES && seenPromptForTidesShifting)
        {
            STOPSHOWINGCONSISTENTCYCLES = false;
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

        if (time >= 40 * 98)
        {
            ChangePhase(Phase.PassPersistEventHorizon_NoLongerNeedsInput);
        }

        if (lastIntensity < 0.1f || lastIntensity > 0.8f)
        {
            // For now, before I rewrite it.
            cycle.owner.Die();
        }
    }

    private void FightDeathUpdate()
    {
        bool holdingInput = false;

        int inputCount = cycle.specInputCounter;
        if (inputCount > 0)
        {
            holdingInput = true;
        }

        Fight(sequencePhaseTime, holdingInput);

        if (holdingInput)
        {
            if (cycle.cycle.idleRipplesToSpawn < 15)
            {
                cycle.cycle.idleRipplesToSpawn += UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, 12));
            }
            cycle.cycle.spawnRipples = true;
        }
        if (cycle.cycle.idleRipplesToSpawn >= 8 && cycle.cycle.spawnRipples)
        {
            cycle.cycle.AddRipple(Cycle.CycleRippleSource.Thanatosis);
            cycle.owner.room.PlaySound(Enums.SoundID.Player_Deactivated_Thanatosis, 0.5f, 0.03f * cycle.cycle.idleRipplesToSpawn, 0.85f);
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
                ChangePhase(Phase.Drowning_TimeSlows);
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
        if (phase == Phase.Drowning_TimeSlows)
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
        if (cycle.cycle.idleRipplesToSpawn == 0)
        {
            cycle.cycle.idleRipplesToSpawn++;
            if (cycle.owner.abstractCreature.rippleLayer == 1)
            {
                cycle.cycle.idleRipplesToSpawn += UnityEngine.Random.Range(2, 5);
            }
        }
        else
        {
            cycle.cycle.spawnRipples = true;
        }
    }

    private void SequenceTick()
    {
        sequenceTime.Tick();
        sequencePhaseTime.Tick();
    }
}
