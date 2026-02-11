using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Diagnostics;
using RWCustom;

namespace PitchBlack;

public class BeaconInputs
{
    public Beacon owner;
    public bool AllowNone { get; set; }
    public bool AllowSpecialOnly { get; set; }
    public bool UseSpecial { get; set; }
    public Counter specialInputCounter = new(Int32.MaxValue, 0, true);

    public BeaconInputs(Beacon beacon)
    {
        this.owner = beacon;
    }

    public void Update()
    {
        // Special input press time tracking
        if (owner.player.input[0].spec)
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
        var p = owner.player;
        var controls = p.room.game.rainWorld.options.controls[p.playerState.playerNumber];
        Player.InputPackage newInputs = originalInputs;
        Player.InputPackage none = new(controls.gamePad, controls.GetActivePreset(), 0, 0, false, false, false, false, false);

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
