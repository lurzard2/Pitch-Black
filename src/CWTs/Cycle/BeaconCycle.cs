using RWCustom;
using System;
using UnityEngine;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class BeaconCycle
{
    public Cycle cycle;
    public Player owner;
    public SaveState saveState;
    // Not a UAD yet
    public ThanatosisTutorialSequence thanatosisTutorialSequence;

    // Thanatosis
    public bool deathToggle;
    public bool isDead;
    public Counter specInputCounter = new(Int32.MaxValue, 0, true);
    public float SpiralLevel => BeaconSaveData.GetSpiralLevel(saveState);
    public float MinSpiralLevel => BeaconSaveData.GetMinSpiralLevel(saveState);
    public float MaxSpiralLevel => BeaconSaveData.GetMaxSpiralLevel(saveState);
    public float ThanatosisLimit => (40 * 12) * SpiralLevel;
    public bool ReachedThanatosisLimit => cycle.state == Cycle.State.Thanatosis && cycle.cycleStateTime > ThanatosisLimit;
    public float thanatosisLerp;
    public bool killMe = false;

    public BeaconCycle(Cycle cycle, Player owner)
    {
        this.cycle = cycle;
        this.owner = owner;
        saveState = owner.abstractCreature.world.game.GetStorySession.saveState;
    }

    public void Update()
    {
        // Stopsplayer ripples and thanatosis
        if (MiscUtils.IsRegionOutSideCycle(owner.abstractCreature.world))
        {
            // Indicator for being unable to use Thanatosis
            if (MaxSpiralLevel > 1 && owner.input[0].spec)
            {
                specInputCounter.Tick();
            }
            if (specInputCounter == UnityEngine.Random.Range(60, 140))
            {
                owner.Stun(120);
                specInputCounter.Reset();
                string popupText = "";
                if (MiscUtils.IsNightmareRegion(owner.abstractCreature.world.name))
                {
                    popupText = "These tides are sinister";
                }
                else
                {
                    popupText = "These tides flow without disturbance";
                }
                MiscUtils.AddHUDMessage(owner.room.game.cameras[0].hud, true, popupText, 60, 120, false, true);
            }
            return;
        }

        // Self-sustainable cycle
        if (cycle.state == Cycle.State.Init)
        {
            cycle.Sync();
        }
        else
        {
            cycle.CycleTick();
            if (owner.abstractCreature != null)
            {
                cycle.AbstractUpdate();
            }
            if (owner != null)
            {
                cycle.RealizedUpdate();
            }
        }

        if (BeaconSaveData.GetCompletedBeacon(saveState) && cycle.cycleTime == 40*12)
        {
            string ptText = $"[THIS MARKS THE END OF THE PLAYTEST CURRENTLY] ~ {MOD_VERSION}";
            MiscUtils.AddHUDMessage(owner.room.game.cameras[0].hud, true, ptText, 0, 120, true, true);
        }

        // New cycle, catch up to max revives
        if (cycle.cycleTime == 1 && SpiralLevel < MaxSpiralLevel)
        {
            BeaconSaveData.SetSpiralLevel(saveState, MaxSpiralLevel);
        }

        // Save cycle to save data on end
        //var manager = owner.abstractCreature.world.game.manager;
        //if (manager.upcomingProcess == ProcessManager.ProcessID.SleepScreen
        //    || manager.upcomingProcess == ProcessManager.ProcessID.DeathScreen
        //    || manager.upcomingProcess == ProcessManager.ProcessID.StarveScreen)
        //{
        //    BeaconSaveData.SetSavedCycle(saveState, new SavedPlayerCycle(this, saveState.cycleNumber));
        //}

        #region Thanatosis Sequence
        // Not VV, hasnt used thanatosis. specifically encounter 3
        if (!MiscUtils.IsVhosRegion(owner.room.world.name)
            && !BeaconSaveData.GetHasUsedThanatosis(saveState)
            && BeaconSaveData.GetDreamerEncountersNumber(saveState) == 3)
        {
            if (thanatosisTutorialSequence != null)
            {
                thanatosisTutorialSequence.Update();

                if (thanatosisTutorialSequence.phase == ThanatosisTutorialSequence.Phase.UsedThanatosis)
                {
                    thanatosisTutorialSequence = null;
                    BeaconSaveData.SetCompletedBeacon(saveState, true);
                    return;
                }
            }
            else
            {
                thanatosisTutorialSequence = new(this, owner.room);
                return;
            }
        }
        #endregion

        if (cycle.state == Cycle.State.PersistThroughCache)
        {
            ToggleThanatosis(true);
            cycle.ChangeState(Cycle.State.Alive);
        }

        if (owner.input[0].spec)
        {
            specInputCounter.Tick();
            if (UnityEngine.Random.value < 0.005f && cycle.idleRipplesToSpawn < 10)
            {
                cycle.idleRipplesToSpawn++;
            }
        }
        else
        {
            specInputCounter.Reset();
        }

        if (BeaconSaveData.GetCanUseThanatosis(saveState))
        {
            //logger.LogDebug($"Thanatosis: Doing input - {specInputCounter}");
            if (specInputCounter == 24)
            {
                ToggleThanatosis(true);
            }
            if (cycle.idleRipplesToSpawn == 0)
            {
                cycle.idleRipplesToSpawn++;
            }
            cycle.spawnRipples = true;
        }

        if (ReachedThanatosisLimit && killMe)
        {
            if (SpiralLevel >= 0f)
            {
                logger.LogDebug("Thanatosis: Persisting!");
                BeaconSaveData.SetSpiralLevel(saveState, SpiralLevel - 1f);
                Persist();
            }
            else
            {
                logger.LogDebug("Thanatosis: Die!");
                EndCycle();
            }
            killMe = false;
        }

        if (cycle.state == Cycle.State.Thanatosis)
        {
            InThanatosis();
        }
        else if (cycle.state == Cycle.State.ExitThanatosis)
        {
            OutsideThanatosis();
        }
    }

    // Increasing values while in Thanatosis
    private void InThanatosis()
    {
        if (thanatosisLerp < 0.92f)
        {
            thanatosisLerp += 0.01f;
        }

        // this will have to be modified later to be actually uh, good

        float thanatosisTime = cycle.cycleStateTime; //x
        float minSafeTime = 12 * 40f; //tc
        float maxSafeTime = 60 * 40f; // Tc
        float beginningIntensity = 0.4f; //l
        float endIntensity = 0.45f; //m
        float windUpTime = 3 * 40f; //wc
        float rampUpTime = 3 * 40f; //Wc
        float plateauDuration = (SpiralLevel) * (maxSafeTime - (windUpTime + rampUpTime) * 2) / 4 + minSafeTime - windUpTime - rampUpTime; //c
                         
        // Starting plateau
        if (thanatosisTime < windUpTime)
        {
            owner.rippleDeathIntensity = Mathf.Sqrt(thanatosisTime) * beginningIntensity / Mathf.Sqrt(windUpTime);
        }
        // Middle of plateau
        else if ((thanatosisTime < windUpTime + plateauDuration) && thanatosisTime >= windUpTime)
        {
            owner.rippleDeathIntensity = (thanatosisTime - windUpTime) * (endIntensity - beginningIntensity) / plateauDuration + beginningIntensity;
        }
        // End
        if (thanatosisTime >= windUpTime + plateauDuration + (rampUpTime / 2))
        {
            float increment = 0.008f;
            int mult = 4;
            owner.rippleDeathIntensity += increment;
            increment += 0.008f * mult;
            mult += 4;
        }
    }

    // Decreasing values that linger from Thanatosis
    private void OutsideThanatosis()
    {
        if (thanatosisLerp > 0f)
        {
            thanatosisLerp -= 0.01f;
        }
        if (thanatosisLerp <= 0.1f)
        {
            cycle.abstractOwner.rippleLayer = 0;
        }
        if (owner.rippleDeathIntensity > 0f)
        {
            owner.rippleDeathIntensity -= 0.004f;
        }
        if (!isDead && thanatosisLerp < 0f && owner.rippleDeathIntensity < 0f)
        {
            cycle.ChangeState(Cycle.State.Alive);
        }
    }

    #region Dying & Revivng

    private void Persist()
    {
        cycle.ChangeState(Cycle.State.PersistThroughCache);
        ToggleThanatosis(true);
        owner.room.PlaySound(Enums.SoundID.Player_Revived, owner.mainBodyChunk);

    }

    private void EndCycle()
    {
        cycle.ChangeState(Cycle.State.MarkedForCache);
        owner.room.PlaySound(Enums.SoundID.Player_Died_From_Thanatosis, owner.mainBodyChunk);
    }

    #endregion

    public void ToggleThanatosis(bool layerSwitches)
    {
        deathToggle = isDead;
        isDead = !isDead;
        if (deathToggle != isDead)
        {

            logger.LogDebug($"Thanatosis: Reached toggle for Thanatosis - {isDead} - {cycle.state.value}");

            // Enum determining
            var rippleSource = isDead
                ? Cycle.CycleRippleSource.Thanatosis
                : Cycle.CycleRippleSource.Cache;
            var cycleState = isDead
                ? Cycle.State.Thanatosis
                : Cycle.State.ExitThanatosis;
            var soundEffect = isDead
                ? Enums.SoundID.Player_Activated_Thanatosis
                : Enums.SoundID.Player_Deactivated_Thanatosis;

            MiscUtils.MaterializeDreamSpawn(owner.room, owner.mainBodyChunk.pos, Enums.DreamSpawnSource.Jetsam);
            cycle.AddRipple(rippleSource);
            cycle.ChangeState(cycleState);
            owner.room.PlaySound(soundEffect, owner.mainBodyChunk);
            if (layerSwitches)
            {
                cycle.abstractOwner.rippleLayer = isDead ? 1 : 0;
            }

            // Rot immunity
            cycle.abstractOwner.tentacleImmune = isDead;
        }
    }
}
