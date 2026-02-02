using RWCustom;
using System;
using UnityEngine;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class BeaconCycle : Cycle
{
    public Player owner;
    public BeaconCWT cwt;

    public SaveState SaveState => owner.abstractCreature.world.game.GetSaveState();
    // Not a UAD yet, probably won't become one
    public ThanatosisTutorialSequence thanatosisTutorialSequence;

    public bool deathToggle;
    public bool isDead;
    public Counter specInputCounter = new(Int32.MaxValue, 0, true);
    public float ThanatosisLimit => (40 * 6) * cwt.SpiralLevel();
    public bool ReachedThanatosisLimit => state == State.Thanatosis && cycleStateTime > ThanatosisLimit;
    public float thanatosisLerp;
    public bool killMe = false;

    public float targetRippleDeathIntensity;
    public Counter thanatosisDeathCounter = new(80, 0);

    public BeaconCycle(Player owner, BeaconCWT cwt) : base(owner.abstractCreature)
    {
        this.owner = owner;
        this.cwt = cwt;
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
            if (!MiscUtils.IsNightmareRegion(owner.abstractCreature.world.name) && state == State.Thanatosis)
            {
                ToggleThanatosis();
            }

            // Indicator for being unable to use Thanatosis if unlocked
            if (cwt.MaxSpiralLevel() >= 1 && specInputCounter == UnityEngine.Random.Range(60, 140))
            {
                owner.Stun(120);
                specInputCounter.Reset();
                string popupText = "";
                if (MiscUtils.IsNightmareRegion(owner.abstractCreature.world.name))
                    popupText = "These tides are sinister";
                else if (MiscUtils.IsPBSB(owner.abstractCreature.world.name))
                    popupText = "These tides rest still";
                else
                    popupText = "These tides flow without disturbance";
                MiscUtils.AddHUDMessage(owner.room.game.cameras[0].hud, true, popupText, 60, 120, false, true);
            }
            return;
        }

        // Perpetuate cycle
        //cycle.CycleTick();
        //cycle.RealizedUpdate();
        //if (owner.abstractCreature != null)
        //    cycle.AbstractUpdate();

        if (cwt.CanUseThanatosis())
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
        if (specInputCounter == 80)
        {
            ToggleThanatosis();
        }

        if (ReachedThanatosisLimit && killMe)
        {
            if (cwt.SpiralLevel() >= 0f)
            {
                Persist();
            }
            else
            {
                EndCycle();
            }
            killMe = false;
        }

        if (state == State.Thanatosis)
        {
            InThanatosis();
        }
        else if (state == State.ExitThanatosis)
        {
            LeavingThanatosis();
        }

        //owner.rippleDeathIntensity = Mathf.Lerp(owner.rippleDeathIntensity, targetRippleDeathIntensity, 0.04f);
        owner.rippleDeathIntensity = Custom.LerpAndTick(owner.rippleDeathIntensity, targetRippleDeathIntensity, 0.006f, 0.0025f);

        //logger.LogDebug($"{unstableness} - {SpiralLevel}");
    }

    // Increasing values while in Thanatosis
    private void InThanatosis()
    {
        if (thanatosisLerp < 0.92f)
        {
            thanatosisLerp += 0.006f;
        }

        if (thanatosisDeathCounter.isFinished)
        {
            thanatosisDeathCounter.Reset();
        }
        else if (owner.rippleDeathIntensity > 0.3f)
        {
            thanatosisDeathCounter.Tick();
        }

        if (ReachedThanatosisLimit)
        {
            targetRippleDeathIntensity = 0.35f;
        }
        else
        {
            targetRippleDeathIntensity = 0.08f;
        }
    }

    // Decreasing values that linger from Thanatosis
    private void LeavingThanatosis()
    {
        if (thanatosisLerp > 0f)
        {
            thanatosisLerp -= 0.04f;
        }
        if (thanatosisLerp <= 0.1f)
        {
            abstractOwner.rippleLayer = 0;
        }
        if (targetRippleDeathIntensity > 0f)
        {
            targetRippleDeathIntensity -= 0.02f;
        }

        // Switch back to alive if effects are done being removed
        if (!isDead && thanatosisLerp < 0f && owner.rippleDeathIntensity < 0f)
        {
            ChangeState(State.Alive);
        }
    }

    private void Persist()
    {
        logger.LogDebug("Thanatosis: Persisting!");
        SaveState.SetSpiralLevel(cwt.SpiralLevel() - 1f);
        ToggleThanatosis();
        owner.Stun(80);
        owner.room.PlaySound(Enums.SoundID.Player_Revived, owner.mainBodyChunk);
        MiscUtils.MaterializeDreamSpawn(owner.room, owner.mainBodyChunk.pos, Enums.DreamSpawnSource.Jetsam);
    }

    private void EndCycle()
    {
        logger.LogDebug("Thanatosis: Die!");
        ChangeState(State.MarkedForCache);
        owner.room.PlaySound(Enums.SoundID.Player_Died_From_Thanatosis, owner.mainBodyChunk);
    }

    public void ToggleThanatosis(bool layerSwitches = true)
    {
        deathToggle = isDead;
        isDead = !isDead;
        if (deathToggle != isDead)
        {
            logger.LogDebug($"Thanatosis: Reached toggle for Thanatosis - {isDead} - {state.value}");

            // Enum determining
            var cycleState = isDead
                ? State.Thanatosis
                : State.ExitThanatosis;
            var soundEffect = isDead
                ? Enums.SoundID.Player_Activated_Thanatosis
                : Enums.SoundID.Player_Deactivated_Thanatosis;

            AddRipple(CycleRippleSource.Thanatosis);
            ChangeState(cycleState);
            owner.room.PlaySound(soundEffect, owner.mainBodyChunk);
            if (layerSwitches)
            {
                abstractOwner.rippleLayer = isDead ? 1 : 0;
            }

            // Rot immunity
            abstractOwner.tentacleImmune = isDead;
        }
    }
}
