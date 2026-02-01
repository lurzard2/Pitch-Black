using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public class BeaconDeathHandler : DeathHandler
{
    public BeaconCycle2 cycleRef;
    public bool ToggleDeathFlag {  get; private set; }
    public bool Dead { get; private set; }
    public bool SacrificeAvailable { get; set; }

    public BeaconDeathHandler(BeaconCycle2 cycle) : base(cycle)
    {
        cycleRef = cycle;
    }

    /// <summary>
    /// Represents the value of ToggleDeathFlag. If toggle param is true, then it'll set ToggleDeathFlag as true.
    /// </summary>
    /// <param name="toggle">Override with true</param>
    /// <returns>ToggleDeathFlag determining whether Thanatosis should be toggled</returns>
    public bool ToggleThanatosis(bool toggle = false)
    {
        if (toggle || cycleRef.InputHandler.SpecialHeldConditionMet)
        {
            ToggleDeathFlag = true;
        }
        return ToggleDeathFlag;
    }

    public override void Realized()
    {
        // If meant to toggle, we do that, this change will then affect code checking Dead
        if (ToggleThanatosis())
        {
            Dead = !Dead;
            ToggleDeathFlag = false;
        }
    }

    public override void PlayerDie()
    {
        if (SacrificeAvailable)
        {
            if (cycleRef.SaveState is not null)
            {
                cycleRef.SaveState.SetSpiralLevel(cycleRef.SpiralLevel - 1);
                cycleRef.SaveState.deathPersistentSaveData.deaths++;
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