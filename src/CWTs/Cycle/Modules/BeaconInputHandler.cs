using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Diagnostics;
using RWCustom;

namespace PitchBlack;

public class BeaconInputHandler : CycleModule
{
    public BeaconCycle2 cycleRef;
    public bool AllowNone {  get; set; }
    public bool AllowSpecialOnly { get; set; }
    public bool UseSpecial {  get; set; }
    public Counter specialInputCounter = new(Int32.MaxValue, 0, true);
    public bool SpecialHeldConditionMet => specialInputCounter == 24;

    public BeaconInputHandler(BeaconCycle2 cycle) : base(cycle)
    {
        cycleRef = cycle;
    }

    public override void Realized()
    {
        // Special input press time tracking
        if (cycleRef.Beacon.input[0].spec)
        {
            specialInputCounter.Tick();
        }
        else if (specialInputCounter > 0)
        {
            specialInputCounter.Reset();
        }
    }

    public Player.InputPackage InputPackage(Player.InputPackage originalInputs)
    {
        var beacon = cycleRef.Beacon;
        var controls = beacon.room.game.rainWorld.options.controls[beacon.playerState.playerNumber];
        Player.InputPackage newInputs = originalInputs;
        Player.InputPackage none = new (controls.gamePad, controls.GetActivePreset(), 0, 0, false, false, false, false, false);

        // Only special input is passed through
        if (AllowSpecialOnly)
        {
            newInputs = none;
            newInputs.spec = originalInputs.spec;
        }

        // Trigger special regardless of input
        if (UseSpecial)
        {
            newInputs.spec = true;
        }

        if (AllowNone)
        {
            newInputs = none;
        }

        return newInputs;
    }
}
