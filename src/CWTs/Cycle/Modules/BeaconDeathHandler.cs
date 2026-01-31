using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public class BeaconDeathHandler : DeathHandler
{
    public BeaconCycle2 cycleRef;

    public BeaconDeathHandler(BeaconCycle2 cycle) : base(cycle)
    {
        cycleRef = cycle;
    }

    public override void PlayerDie()
    {
        if (cycleRef.SpiralLevel > cycleRef.MinSpiralLevel)
        {
            if (cycleRef.SaveState is not null)
            {
                cycleRef.SaveState.SetSpiralLevel(cycleRef.SpiralLevel - 1);
            }
            else
            {
                cycleRef.SpiralLevel--;
            }
            return;
        }
        else
        {
            base.PlayerDie();
        }
    }
}