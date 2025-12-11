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

    // Thanatosis
    public bool deathToggle;
    public bool isDead;
    public Counter specInputCounter = new(Int32.MaxValue, 0, true);
    public float SpiralLevel => BeaconSaveData.GetSpiralLevel(saveState);
    public float MinSpiralLevel => BeaconSaveData.GetMinSpiralLevel(saveState);
    public float MaxSpiralLevel => BeaconSaveData.GetMaxSpiralLevel(saveState);
    public float ThanatosisLimit => (40 * 12) * SpiralLevel;
    public bool ReachedThanatosisLimit => cycle.state == Cycle.State.Thanatosis && cycle.cycleStateTime == ThanatosisLimit && owner.rippleDeathTime == 80;
    public float thanatosisLerp;
    public bool kinLeftBody;

    public BeaconCycle(Cycle cycle, Player owner)
    {
        this.cycle = cycle;
        this.owner = owner;
        saveState = owner.room.world.game.GetStorySession.saveState;
    }

    public void Update()
    {
        if (MiscUtils.RegionOutSideCycle(owner.room.world))
        {
            return;
        }

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

        if (BeaconSaveData.GetCanUseThanatosis(saveState) && owner.input[0].spec)
        {
            logger.LogDebug($"Thanatosis: Doing input for Thanatosis - {specInputCounter}");
            if (specInputCounter == 24)
            {
                ToggleThanatosis();
            }
            specInputCounter.Tick();
        }
        else
        {
            specInputCounter.Reset();
        }

        if (ReachedThanatosisLimit)
        {
            if (SpiralLevel >= 1f)
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
        if (cycle.cycleStateTime > ThanatosisLimit)
        {
            return;
        }
        if (thanatosisLerp < 0.92f)
        {
            thanatosisLerp += 0.01f;
        }

        float thanatosisTime = cycle.cycleStateTime; //x
        float minSafeTime = 12 * 40f; //tc
        float maxSafeTime = 60 * 40f; // Tc
        float beginningIntensity = 0.4f; //l
        float endIntensity = 0.45f; //m
        float windUpTime = 3 * 40f; //wc
        float rampUpTime = 3 * 40f; //Wc
        float plateauDuration = (SpiralLevel - 1) * (maxSafeTime - (windUpTime + rampUpTime) * 2) / 4 + minSafeTime - windUpTime - rampUpTime; //c
                         
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
        else
        {
            owner.rippleDeathIntensity = Mathf.Lerp(owner.rippleDeathIntensity, 1f, 0.006f);
        }
        //if (thanatosisTime >= windUpTime + plateauDuration + (rampUpTime / 2))
        //{
        //    owner.rippleDeathIntensity = Mathf.Lerp(owner.rippleDeathIntensity, 1f, 0.006f);
        //}
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
        if (!isDead && thanatosisLerp < 0f && thanatosisLerp < 0f && owner.rippleDeathIntensity < 0f)
        {
            cycle.ChangeState(Cycle.State.Alive);
        }
        kinLeftBody = false;
    }

    #region Dying
    public void Persist()
    {
        cycle.ChangeState(Cycle.State.PersistThroughCache);
        owner.room.PlaySound(Enums.SoundID.Player_Revived, owner.mainBodyChunk);
    }

    public void EndCycle()
    {
        cycle.ChangeState(Cycle.State.MarkedForCache);
        owner.room.PlaySound(Enums.SoundID.Player_Died_From_Thanatosis, owner.mainBodyChunk);
    }
    #endregion

    private void ToggleThanatosis()
    {
        deathToggle = isDead;
        isDead = !isDead;
        if (deathToggle != isDead)
        {
            if (isDead && !kinLeftBody)
            {
                MiscUtils.MaterializeDreamSpawn(owner.room, owner.mainBodyChunk.pos, Enums.DreamSpawnSource.Jetsam);
                kinLeftBody = true;
            }
            cycle.AddRipple(isDead ? Cycle.CycleRippleSource.Thanatosis : Cycle.CycleRippleSource.Cache);
            cycle.ChangeState(isDead ? Cycle.State.Thanatosis : Cycle.State.ExitThanatosis);
            logger.LogDebug($"Thanatosis: Reached toggle for Thanatosis - {isDead}");
            cycle.abstractOwner.rippleLayer = isDead ? 1 : 0;
            SoundID soundEffect = isDead ? Enums.SoundID.Player_Activated_Thanatosis : Enums.SoundID.Player_Deactivated_Thanatosis;
            owner.room.PlaySound(soundEffect, owner.mainBodyChunk);
        }
    }
}
