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
    // Not a UAD yet, probably won't become one
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
            SaveState.SetSpiralLevel(MaxSpiralLevel);
        }

        // for Playtest, for now
        if (SaveState.GetCompletedBeacon())
        {
            string ptText = $"[THIS MARKS THE END OF THE PLAYTEST CURRENTLY] ~ {MOD_VERSION}";
            MiscUtils.AddHUDMessage(owner.room.game.cameras[0].hud, true, ptText, 40 * 30, 120, true, true);
        }
    }

    public void Update()
    {
        if (owner.input[0].spec)
            specInputCounter.Tick();
        else
            specInputCounter.Reset();

        // Stop everything else
        if (MiscUtils.IsRegionOutSideCycle(owner.abstractCreature.world))
        {
            // Indicator for being unable to use Thanatosis if unlocked
            if (MaxSpiralLevel >= 1 && specInputCounter == UnityEngine.Random.Range(60, 140))
            {
                owner.Stun(120);
                specInputCounter.Reset();
                string popupText = "";
                popupText = owner.abstractCreature.world.name switch
                {
                    "ud" => "These tides are sinister",
                    "pbsb" => "These tides rest still",
                    _ => "These tides flow without disturbance"
                };
                MiscUtils.AddHUDMessage(owner.room.game.cameras[0].hud, true, popupText, 60, 120, false, true);
            }
            return;
        }

        // Perpetuate cycle
        cycle.CycleTick();
        cycle.RealizedUpdate();
        if (owner.abstractCreature != null)
        {
            cycle.AbstractUpdate();
        }

        if (specInputCounter > 0 && UnityEngine.Random.value < 0.005f && cycle.idleRipplesToSpawn < 10)
            cycle.idleRipplesToSpawn++;

        if (SaveState.GetCanUseThanatosis())
        {
            ThanatosisUpdate();
        }
        // Runs once per cycle post-3rd encounter saved: If the sequence has not finished (it enables thanatosis)
        else if (SaveState.GetDreamerEncountersNumber() == 3)
        {
            #region Thanatosis Sequence
            if (thanatosisTutorialSequence != null)
            {
                thanatosisTutorialSequence.Update();
                // Finished, it's not a UAD so we just null it
                if (thanatosisTutorialSequence.phase == ThanatosisTutorialSequence.Phase.UsedThanatosis)
                {
                    thanatosisTutorialSequence = null;
                    BeaconSaveData.SetCompletedBeacon(SaveState, true);
                }
            }
            else
            {
                thanatosisTutorialSequence = new(this, owner.room);
            }
            #endregion
        }
    }
    
    private void ThanatosisUpdate()
    {
        //logger.LogDebug($"Thanatosis: Doing input - {specInputCounter}");
        if (specInputCounter == 24)
        {
            ToggleThanatosis(true);
        }

        if (ReachedThanatosisLimit && killMe)
        {
            if (SpiralLevel >= 0f)
            {
                Persist();
            }
            else
            {
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

        //logger.LogDebug($"{unstableness} - {SpiralLevel}");
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

    private void Persist()
    {
        logger.LogDebug("Thanatosis: Persisting!");
        BeaconSaveData.SetSpiralLevel(SaveState, SpiralLevel - 1f);
        ToggleThanatosis(true);
        owner.Stun(80);
        owner.room.PlaySound(Enums.SoundID.Player_Revived, owner.mainBodyChunk);
    }

    private void EndCycle()
    {
        logger.LogDebug("Thanatosis: Die!");
        cycle.ChangeState(Cycle.State.MarkedForCache);
        owner.room.PlaySound(Enums.SoundID.Player_Died_From_Thanatosis, owner.mainBodyChunk);
    }

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
