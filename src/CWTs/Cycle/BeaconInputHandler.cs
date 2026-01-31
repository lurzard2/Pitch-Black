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
    public Counter specialInputCounter = new(Int32.MaxValue, 0, true);

    public BeaconInputHandler(BeaconCycle2 cycle) : base(cycle)
    {
        cycleRef = cycle;
    }

    public void Update(bool dontReset = false)
    {
        // Special input press time tracking
        if (cycleRef.Beacon.input[0].spec)
        {
            specialInputCounter.Tick();
        }
        else if (specialInputCounter > 0 && !dontReset)
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

        if (AllowSpecialOnly)
        {
            newInputs = new(false, Options.ControlSetup.Preset.None, 0, 0, false, false, false, false, false, originalInputs.spec);
        }

        // Set to null inputs if flagged. Or otherwise if controller exists, remove it.
        // Player controller removes usual inputs from the player
        if (AllowNone)
        {
            newInputs = none;
        }

        return newInputs;
    }
}
