using System;
using RWCustom;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class BeaconCycle : Cycle
{
    public Cycle cycle;
    public Player owner;
    public SaveState SaveState => owner.room.world.game.GetStorySession.saveState;

    // Thanatosis
    public bool deathToggle;
    public bool isDead;
    public Counter specInputCounter = new(Int32.MaxValue, 0, true);
    public float SpiralLevel => BeaconSaveData.GetSpiralLevel(SaveState);
    public float MinSpiralLevel => BeaconSaveData.GetMinSpiralLevel(SaveState);
    public float MaxSpiralLevel => BeaconSaveData.GetMaxSpiralLevel(SaveState);
    public float ThanatosisLimit => (40 * 12) * SpiralLevel;
    public bool ReachedThanatosisLimit => state == State.Thanatosis && specInputCounter > cycleStateTime && owner.rippleDeathTime == 80;
    public float thanatosisLerp;

    public BeaconCycle(Cycle cycle, Player owner) : base(owner.abstractCreature)
    {
        this.cycle = cycle;
        this.owner = owner;
    }

    public override void AbstractUpdate()
    {
        base.AbstractUpdate();
    }

    public override void RealizedUpdate()
    {
        base.RealizedUpdate();

        if (BeaconSaveData.GetCanUseThanatosis(SaveState) && owner.input[0].spec)
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
            if (SpiralLevel > MinSpiralLevel)
            {
                BeaconSaveData.SetSpiralLevel(SaveState, SpiralLevel - 1f);
            }

            if (SpiralLevel < 1f)
            {
                logger.LogDebug("Thanatosis: Die!");
                EndCycle();
            }
            else
            {
                Persist();
            }
        }

        if (state == State.Thanatosis)
        {
            InThanatosis();
        }
        else
        {
            OutsideThanatosis();
        }
    }

    private void InThanatosis()
    {
        if (cycleStateTime > ThanatosisLimit)
        {
            return;
        }

        if (thanatosisLerp < 0.92f)
        {
            thanatosisLerp += 0.01f;
        }
    }

    private void OutsideThanatosis()
    {
        if (thanatosisLerp > 0f)
        {
            thanatosisLerp -= 0.01f;
        }

        if (thanatosisLerp <= 0.1f)
        {
            abstractOwner.rippleLayer = 0;
        }
    }

    #region Dying
    public void Persist()
    {
        owner.room.PlaySound(Enums.SoundID.Player_Revived, owner.mainBodyChunk);
    }

    public void EndCycle()
    {
        owner.room.PlaySound(Enums.SoundID.Player_Died_From_Thanatosis, owner.mainBodyChunk);
    }
    #endregion

    private void ToggleThanatosis()
    {
        deathToggle = isDead;
        isDead = !isDead;
        if (deathToggle != isDead)
        {
            AddRipple(CycleRippleSource.Thanatosis);
            ChangeState(isDead ? State.Thanatosis : State.Alive);
            logger.LogDebug($"Thanatosis: Reached toggle for Thanatosis - {isDead}");
            abstractOwner.rippleLayer = isDead ? 1 : 0;
            SoundID soundEffect = isDead ? Enums.SoundID.Player_Activated_Thanatosis : Enums.SoundID.Player_Deactivated_Thanatosis;
            owner.room.PlaySound(soundEffect, owner.mainBodyChunk);
        }
    }
}
