using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PitchBlack;

public partial class BeaconAbilityHandler
{
    public Beacon owner;

    public Thanatosis theta;
    public bool ThanatosisToggleConditionMet => owner.inputs.specialInputCounter == 40;
    // This can be static because it will be true for all players
    public static bool CanUseThanatosis { get; set; } = false;

    public CycleController cycleController;

    public BeaconAbilityHandler(Beacon owner)
    {
        this.owner = owner;
        theta = new();
        cycleController = new((int)owner.MaxSpiralLevel);
    }

    public void Update()
    {
        // Only have to check savestate once
        if (CanUseThanatosis)
        {
            ThanatosisUpdate();
        }
        else if (owner.SaveState.GetCanUseThanatosis())
        {
            CanUseThanatosis = true;
            theta.ChangeState(Thanatosis.State.Outside);
        }
    }

    private void ThanatosisUpdate()
    {
        theta.timeInState.Tick();

        theta.ManualToggleConditionMet = ThanatosisToggleConditionMet;
        if (theta.ManualToggleConditionMet)
        {
            theta.Toggle(this);
        }

        if (theta.instability > 0)
        {
            theta.instability -= 0.02f;

            if (Random.Range(0, 1000) < theta.instability)
            {
                theta.instability += 5f;
            }
            if (theta.instability > 50f && Random.value < 0.05f)
            {
                theta.Toggle(this, Thanatosis.Type.Involuntary);
            }
        }

        if (theta.IsInbetween)
        {
            Switching();
        }
        
        if (theta.Dead)
        {
            Inside();
        }
        else
        {
            Outside();
        }

        Plugin.logger.LogDebug($"{nameof(Thanatosis)}: State={theta.GetState} - Time={theta.timeInState} - Input={owner.inputs.specialInputCounter}");
    }

    private void Switching()
    {
        // past 1.5s there's an opportunity to "scum" thanatosis or fully proceed. Thanatosis Scumming contributes to instability.
        bool STOP = false;
        if (theta.timeInState > 40 * 1.25)
        {
            if (theta.timeInState >= 40 * 2.5)
            {
                theta.ChangeSide();
            }
            else if (owner.inputs.noSpecInput)
            {
                STOP = true;
                theta.instability += 20f;
            }
        }
        else if (owner.inputs.noSpecInput)
        {
            STOP = true;
        }

        if (STOP)
        {
            theta.Toggle(this, Thanatosis.Type.Involuntary);
        }
    }

    private void Inside()
    {
        switch (theta.GetState)
        {
            case Thanatosis.State.Inside:
                theta.ChangeState(owner.SpiralLevel > 0 ? Thanatosis.State.Safe : Thanatosis.State.Drowning);
                break;

            case Thanatosis.State.Safe:
                if (theta.timeInState >= theta.MaxAvailableSafeTime)
                {
                    theta.ChangeState(Thanatosis.State.Drowning);
                }
                break;

            case Thanatosis.State.Drowning:
                if (theta.timeInState >= 40 * 3)
                {
                    theta.ChangeState(Thanatosis.State.Drowned);
                }
                break;

            case Thanatosis.State.Drowned:
                break;
        }
    }

    private void Outside()
    {

    }
}
