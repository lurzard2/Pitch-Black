using UnityEngine;
using System.Collections.Generic;
using RWCustom;

namespace PitchBlack;

public class ManipulationTracker : ManipulationModule
{
    public ManipulationTracker(Cycle cycle) : base(cycle) { }

    public override void Update()
    {
        foreach (var absCrit in cycle.abstractOwner.Room.creatures)
        {
            // I hate needing to check this constantly within the loop but don't know what would be better than continue or goto -Lur
            if (absCrit != cycle.abstractOwner && Plugin.creatureCycle.TryGetValue(absCrit, out var otherCycle))
            {
                // If other cycle is affecting ripple then it should affect me, but only if close enough
                float distance = Vector2.Distance(cycle.spacialTracker.pos.Main, otherCycle.spacialTracker.pos.Main);
                if (!otherCycle.spacialTracker.InDream && otherCycle.spacialTracker.BelowRippleSurface && distance < radiusForInfluence)
                {
                    BeingManipulated(otherCycle);
                    if (TooManyCreaturesAvailable)
                    {
                        break;
                    }
                }
            }
        }
    }

    public override void BeingManipulated(Cycle oC)
    {
        cycle.spacialTracker.RippleTick(oC.spacialTracker.pos.v);
    }
}
