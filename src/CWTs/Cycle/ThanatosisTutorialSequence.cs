using RWCustom;
using System;
using System.Linq;
using UnityEngine;
using Watcher;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class ThanatosisTutorialSequence
{
    public string SequenceText => room.world.game.manager.rainWorld.inGameTranslator.Translate("Hold SPECIAL, and don't stop");
    public string CycleDisplayText => room.world.game.manager.rainWorld.inGameTranslator.Translate($"Cycle {cycle.saveState.cycleNumber} ~ {Region.GetRegionFullName(cycle.owner.room.world.region.name, SlugcatStats.Name.White)}");
    public string SequenceCompleteText => room.world.game.manager.rainWorld.inGameTranslator.Translate("Hold SPECIAL to enter and exit a state of thanatosis");

    public BeaconCycle cycle;
    public Room room;
    public Counter sequenceTime = new(Int32.MaxValue, 0, true);
    public Counter sequencePhaseTime = new(Int32.MaxValue, 0, true);
    public Counter timeTilCycleDisplay = new(25, 0, true);
    public CreatureSpasmer spasmer;

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

    float tutorialDeathIntensity = 0;
    float targetDeathIntensity = 0;

    public bool markedAsDead;
    private bool seenSpecialPromptThisCycle;
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

                    // ---

                    //musicEvent.cyclesRest = 5;
                    //musicEvent.stopAtDeath = false;
                    //musicEvent.stopAtGate = false;
                    //musicEvent.songName = "PB_12 - Fated Demise";
                    //// Game saves songs that have played once unfortunately, so it won't play again if you take too long to do this i think
                    //room.game.manager.musicPlayer.GameRequestsSong(musicEvent);
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
                                if (obj.intensity >= 0.8f)
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
                            cycle.owner.room.game.cameras[0].hud.textPrompt.AddMessage(CycleDisplayText, 40, 30, false, true);
                            var prompt = cycle.owner.room.game.cameras[0].hud.textPrompt;
                            if (prompt.messages.Count > 0
                                && prompt.messages[0].text == CycleDisplayText
                                && prompt.messages[0].text != SequenceText)
                            {
                                prompt.messages[0].time = 20;
                            }
                            timeTilCycleDisplay.Reset();
                            obj = null;
                        }
                    }
                }
            }

            // Death effect
            if (targetDeathIntensity < 0f)
            {
                targetDeathIntensity = 0;
            }
            tutorialDeathIntensity = Mathf.Lerp(tutorialDeathIntensity, targetDeathIntensity, 0.006f);
            cycle.owner.rippleDeathIntensity = tutorialDeathIntensity;

            // Drowning
            if (!markedAsDead)
            {
                cycle.owner.airInLungs -= tutorialDeathIntensity;
            }

            logger.LogDebug($"EFFECT:{targetDeathIntensity} - PHASE:{phase.value} - THANATOSISLERP:{cycle.thanatosisLerp}");

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
            if (sequenceTime == (40 * 30))
            {
                ChangePhase(Phase.PendingSuffocation);
            }
        }

        if (phase == Phase.PendingSuffocation)
        {
            if (sequencePhaseTime == (40 * 45))
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
            if (sequencePhaseTime >= (40 * 10))
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
                    var prompt = cycle.owner.room.world.game.cameras[0].hud.textPrompt;
                    STOPSHOWINGCONSISTENTCYCLES = true;
                    prompt.messages.Clear();
                    AddSpecialInputPrompt();
                    if (prompt.messages.Count > 0 && prompt.messages.Count < 2 && prompt.messages[0].text == SequenceText)
                    {
                        prompt.messages[0].time = 180;
                    }
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
            STOPSHOWINGCONSISTENTCYCLES = false;
            if (cycle.thanatosisLerp < 0.5f)
            {
                cycle.thanatosisLerp += 0.004f;
            }
            if (sequencePhaseTime <= 60)
            {
                if (spasmer == null)
                {
                    spasmer = new(cycle.owner, false, sequencePhaseTime + 40);
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
                var prompt = cycle.owner.room.game.cameras[0].hud.textPrompt;
                STOPSHOWINGCONSISTENTCYCLES = true;
                prompt.messages.Clear();
                cycle.owner.room.game.cameras[0].hud.textPrompt.AddMessage(SequenceCompleteText, 40, 200, true, true);
                if (prompt.messages.Count > 0)
                {
                    // Tutorial text
                    if (prompt.messages[0].text == SequenceCompleteText)
                    {
                        prompt.messages[0].time = 180;
                    }
                }
                seenTrueTutorialPromptThisCycle = true;
            }

            BeaconSaveData.SetHasUsedThanatosis(cycle.saveState, true);
            // Do not make them do that whole thing over again
            RainWorldGame.ForceSaveNewDenLocation(cycle.owner.room.world.game, cycle.owner.room.abstractRoom.name, false);
            // Ends sequence
            ChangePhase(Phase.UsedThanatosis);
        }
    }

    private void Fight(int input)
    {
        float demise = 0;
        float fight = 0;
        int time = 0;

        demise = 0.8f;

        time = 40;
        if (input >= time)
        {
            fight = 0.5f;
        }
        if (sequencePhaseTime >= time)
        {
            demise += 0.2f;
        }

        time = 40 * 20;
        if (input >= time)
        {
            fight += 0.2f;
        }
        if (sequencePhaseTime >= time + (40 * 10))
        {
            demise += 0.25f;
        }

        time = 40 * 35;
        if (input >= time)
        {
            fight += 0.1f;
            if (input >= time + (40 * 8))
            {
                fight += 0.2f;
            }
        }
        if (sequencePhaseTime >= time + (40 * 5))
        {
            demise += 0.15f;
        }

        time = 40 * 60;
        if (input >= time)
        {
            fight += 0.3f;
        }
        if (sequencePhaseTime >= time + (40 * 10))
        {
            demise += 0.6f;
        }

        time = 40 * 80;
        if (input >= time)
        {
            fight += 0.3f;
        }

        if (input >= 40 * 85)
        {
            fight += 0.9f;
            demise = 0;
        }

        time = 40 * 90;
        if ((input >= time) && fight > demise)
        {
            fight = 0;
            demise = 0;
            ChangePhase(Phase.PassPersistEventHorizon_NoLongerNeedsInput);
        }

        float fate = demise - fight;
        targetDeathIntensity = fate;
    }

    private void FightDeathUpdate()
    {
        bool holdingInput = false;
        int inputCount = cycle.specInputCounter;
        if (inputCount > 0)
        {
            holdingInput = true;
        }
        Fight(inputCount);
        if (holdingInput)
        {
            targetDeathIntensity -= 0.006f;
            if (cycle.cycle.idleRipplesToSpawn < 15)
            {
                cycle.cycle.idleRipplesToSpawn += UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, 12));
            }
            cycle.cycle.spawnRipples = true;
        }
        if (cycle.cycle.idleRipplesToSpawn >= 8 && cycle.cycle.spawnRipples)
        {
            cycle.cycle.AddRipple(Cycle.CycleRippleSource.Thanatosis);
            cycle.owner.room.PlaySound(Enums.SoundID.Player_Deactivated_Thanatosis, 0.5f, 0.05f * cycle.cycle.idleRipplesToSpawn, 0.85f);
        }
    }

    private void AddSpecialInputPrompt()
    {
        cycle.owner.room.game.cameras[0].hud.textPrompt.AddMessage(SequenceText, 40, 240, true, true);
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
