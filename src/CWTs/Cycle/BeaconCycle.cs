using RWCustom;
using System;
using UnityEngine;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class BeaconCycle
{
    public Cycle cycle;
    public Player owner;
    public SaveState SaveState => owner.abstractCreature.world.game.GetSaveState();
    // Not a UAD yet
    public ThanatosisTutorialSequence thanatosisTutorialSequence;

    // Thanatosis
    public bool deathToggle;
    public bool isDead;
    public Counter specInputCounter = new(Int32.MaxValue, 0, true);
    public float SpiralLevel => SaveState.GetSpiralLevel();
    public float MinSpiralLevel => SaveState.GetMinSpiralLevel();
    public float MaxSpiralLevel => SaveState.GetMaxSpiralLevel();
    public float ThanatosisLimit => (40 * 8) * SpiralLevel;
    public bool ReachedThanatosisLimit => cycle.state == Cycle.State.Thanatosis && cycle.cycleStateTime > ThanatosisLimit;
    public float thanatosisLerp;
    public bool killMe = false;

    private float unstableness = 0f;

    public BeaconCycle(Cycle cycle, Player owner)
    {
        this.cycle = cycle;
        cycle.Sync();

        this.owner = owner;
        
        if (SaveState == null)
        {
            return;
        }

        // New cycle, catch up to max revives?
        if (MaxSpiralLevel > 1 && SpiralLevel < MaxSpiralLevel)
        {
            BeaconSaveData.SetSpiralLevel(SaveState, MaxSpiralLevel);
        }

        // for Playtest, for now
        if (BeaconSaveData.GetCompletedBeacon(SaveState))
        {
            string ptText = $"[THIS MARKS THE END OF THE PLAYTEST CURRENTLY] ~ {MOD_VERSION}";
            MiscUtils.AddHUDMessage(owner.room.game.cameras[0].hud, true, ptText, 40 * 30, 120, true, true);
        }
    }

    public void Update()
    {
        // Stopsplayer ripples and thanatosis
        if (MiscUtils.IsRegionOutSideCycle(owner.abstractCreature.world))
        {
            // Indicator for being unable to use Thanatosis
            if (MaxSpiralLevel >= 1 && owner.input[0].spec)
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

        cycle.CycleTick();
        if (owner.abstractCreature != null)
        {
            cycle.AbstractUpdate();
        }
        if (owner != null)
        {
            cycle.RealizedUpdate();
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
            && !BeaconSaveData.GetHasUsedThanatosis(SaveState)
            && BeaconSaveData.GetDreamerEncountersNumber(SaveState) == 3)
        {
            if (thanatosisTutorialSequence != null)
            {
                thanatosisTutorialSequence.Update();

                if (thanatosisTutorialSequence.phase == ThanatosisTutorialSequence.Phase.UsedThanatosis)
                {
                    thanatosisTutorialSequence = null;
                    BeaconSaveData.SetCompletedBeacon(SaveState, true);
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

        // Unstable effects from using Thanatosis too much
        if (unstableness > 8f)
        {
            if (UnityEngine.Random.value < 0.01f)
            {
                ToggleThanatosis(true);
                owner.Stun(120);
            }
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

        if (BeaconSaveData.GetCanUseThanatosis(SaveState))
        {
            ThanatosisUpdate();
        }
    }
    
    private void ThanatosisUpdate()
    {
        //logger.LogDebug($"Thanatosis: Doing input - {specInputCounter}");
        if (specInputCounter == 24)
        {
            ToggleThanatosis(true);
            if (unstableness > 4f)
            {
                owner.Stun(80);
            }
        }

        if (cycle.idleRipplesToSpawn == 0)
        {
            cycle.idleRipplesToSpawn++;
        }

        if (ReachedThanatosisLimit && killMe)
        {
            if (SpiralLevel >= 0f)
            {
                unstableness += 2f;
                logger.LogDebug("Thanatosis: Persisting!");
                BeaconSaveData.SetSpiralLevel(SaveState, SpiralLevel - 1f);
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
            LeavingThanatosis();
        }

        if (isDead)
        {
            // 0 if maxLvl=4 or 0.5 if maxLvl>=2
            float mult = MaxSpiralLevel == 4 ? 0 : MaxSpiralLevel >= 2 ? 0.5f : 1;
            unstableness += 0.005f * mult;
        }
        else if (!isDead && unstableness > 0)
        {
            unstableness -= 0.01f;
        }

        logger.LogDebug($"{unstableness} - {SpiralLevel}");
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
        float minSafeTime = 10 * 40f; //tc
        float maxSafeTime = 40 * 40f; // Tc
        float beginningIntensity = 0.3f; //l
        float endIntensity = 0.45f; //m
        float windUpTime = 3 * 40f; //wc
        float rampUpTime = 3 * 40f; //Wc
        float plateauDuration = SpiralLevel * (maxSafeTime - (windUpTime + rampUpTime) * 2) / 4 + minSafeTime - windUpTime - rampUpTime; //c
        float targetIntensity = 0.3f;
                         
        // Starting plateau
        if (thanatosisTime < windUpTime)
        {
            targetIntensity = Mathf.Sqrt(thanatosisTime) * beginningIntensity / Mathf.Sqrt(windUpTime);
        }
        // Middle of plateau
        else if ((thanatosisTime < windUpTime + plateauDuration) && thanatosisTime >= windUpTime)
        {
            targetIntensity = (thanatosisTime - windUpTime) * (endIntensity - beginningIntensity) / plateauDuration + beginningIntensity;
        }
        // End
        if (thanatosisTime >= windUpTime + plateauDuration + (rampUpTime / 2))
        {
            targetIntensity = 1f;
        }
        owner.rippleDeathIntensity = Mathf.Lerp(owner.rippleDeathIntensity, targetIntensity, 0.06f);
    }

    // Decreasing values that linger from Thanatosis
    private void LeavingThanatosis()
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

        // Switch back to alive if effects are done being removed
        if (!isDead && thanatosisLerp < 0f && owner.rippleDeathIntensity < 0f)
        {
            cycle.ChangeState(Cycle.State.Alive);
        }
    }

    #region Dying & Revivng

    private void Persist()
    {
        ToggleThanatosis(true);
        owner.Stun(80);
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
