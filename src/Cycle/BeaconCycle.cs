using RWCustom;
using System;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class BeaconCycle : Cycle
{
    public Player playerObj;
    public Beacon beacon;

    // Not a UAD yet, probably won't become one
    public ThanatosisTutorialSequence thanatosisTutorialSequence;

    public bool deathToggle;
    public bool isDead;
    public Counter specInputCounter = new(Int32.MaxValue, 0, true);
    public float ThanatosisLimit => (40 * 6) * beacon.SpiralLevel;
    public bool ReachedThanatosisLimit => state == State.Thanatosis && cycleStateTime > ThanatosisLimit;
    public float thanatosisLerp;
    public bool killMe = false;

    public float targetRippleDeathIntensity;
    public Counter thanatosisDeathCounter = new(80, 0);

    public BeaconCycle(Beacon beacon, Player playerRef) : base(playerRef.abstractCreature)
    {
        this.beacon = beacon;
        playerObj = playerRef;

        spacialTracker = new(this);
        modules.Add(spacialTracker);
        idleRippleTracker = new(this);
        modules.Add(idleRippleTracker);
    }

    public void Update()
    {
        if (playerObj.input[0].spec)
            specInputCounter.Tick();
        else
            specInputCounter.Reset();

        // Stop everything else
        if (MiscUtils.IsRegionOutSideCycle(playerObj.abstractCreature.world))
        {
            if (!MiscUtils.IsNightmareRegion(playerObj.abstractCreature.world.name) && state == State.Thanatosis)
            {
                ToggleThanatosis();
            }

            // Indicator for being unable to use Thanatosis if unlocked
            if (beacon.SaveState.GetMaxSpiralLevel_CurrentOrArenaDefault() >= 1 && specInputCounter == UnityEngine.Random.Range(60, 140))
            {
                playerObj.Stun(120);
                specInputCounter.Reset();
                string popupText = "";
                if (MiscUtils.IsNightmareRegion(playerObj.abstractCreature.world.name))
                    popupText = "These tides are sinister";
                else if (MiscUtils.IsPBSB(playerObj.abstractCreature.world.name))
                    popupText = "These tides rest still";
                else
                    popupText = "These tides flow without disturbance";
                MiscUtils.AddHUDMessage(playerObj.room.game.cameras[0].hud, true, popupText, 60, 120, false, true);
            }
            return;
        }

        // Perpetuate cycle
        //cycle.CycleTick();
        //cycle.RealizedUpdate();
        //if (owner.abstractCreature != null)
        //    cycle.AbstractUpdate();

        if (beacon.SaveState.GetCanUseThanatosis())
        {
            ThanatosisUpdate();
        }
        // Runs once per cycle post-3rd encounter saved: If the sequence has not finished (it enables thanatosis)
        else if (beacon.SaveState.GetDreamerEncountersNumber() == 3)
        {
            #region Thanatosis Sequence
            if (thanatosisTutorialSequence != null)
            {
                thanatosisTutorialSequence.Update();
                // Finished, it's not a UAD so we just null it
                if (thanatosisTutorialSequence.phase == ThanatosisTutorialSequence.Phase.UsedThanatosis)
                {
                    thanatosisTutorialSequence = null;
                    BeaconSaveData.SetCompletedBeacon(beacon.SaveState, true);
                }
            }
            else
            {
                thanatosisTutorialSequence = new(this, playerObj.room);
            }
            #endregion
        }
    }

    public bool SpiralDie()
    {
        if (beacon.SpiralLevel > beacon.AvailableCycles)
        {
            beacon.SpiralLevel--;
            return false;
        }
        else
        {
            return true;
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
            if (beacon.SpiralLevel >= 0f)
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
        playerObj.rippleDeathIntensity = Custom.LerpAndTick(playerObj.rippleDeathIntensity, targetRippleDeathIntensity, 0.006f, 0.0025f);

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
        else if (playerObj.rippleDeathIntensity > 0.3f)
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
        if (!isDead && thanatosisLerp < 0f && playerObj.rippleDeathIntensity < 0f)
        {
            ChangeState(State.Alive);
        }
    }

    private void Persist()
    {
        logger.LogDebug("Thanatosis: Persisting!");
        beacon.SpiralLevel -= 1f;
        ToggleThanatosis();
        playerObj.Stun(80);
        playerObj.room.PlaySound(Enums.SoundID.Player_Revived, playerObj.mainBodyChunk);
        MiscUtils.MaterializeDreamSpawn(playerObj.room, playerObj.mainBodyChunk.pos, Enums.DreamSpawnSource.Jetsam);
    }

    private void EndCycle()
    {
        logger.LogDebug("Thanatosis: Die!");
        ChangeState(State.MarkedForCache);
        playerObj.room.PlaySound(Enums.SoundID.Player_Died_From_Thanatosis, playerObj.mainBodyChunk);
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
            playerObj.room.PlaySound(soundEffect, playerObj.mainBodyChunk);
            if (layerSwitches)
            {
                abstractOwner.rippleLayer = isDead ? 1 : 0;
            }

            // Rot immunity
            abstractOwner.tentacleImmune = isDead;
        }
    }
}
