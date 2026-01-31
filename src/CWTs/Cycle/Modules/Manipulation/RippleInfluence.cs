using UnityEngine;
using System.Collections.Generic;
using RWCustom;

namespace PitchBlack;

public class RippleManipulation : InfluenceHandler
{
    public RippleManipulation(Cycle cycle) : base(cycle) { }

    public override void Realized()
    {
        base.Realized();
        foreach (var absCrit in cycle.CreaturesInRoom)
        {
            if (!IsThisMe(absCrit) && Plugin.creatureCycle.TryGetValue(absCrit, out var otherCycle))
            {
                // If other cycle is affecting ripple then it should affect me, but only if close enough
                float distance = Vector2.Distance(cycle.spacialTracker.pos.Main, otherCycle.spacialTracker.pos.Main);
                if (!otherCycle.spacialTracker.InDream && otherCycle.spacialTracker.BelowRippleSurface && distance < radiusForInfluence)
                {
                    BeingInfluenced(otherCycle);
                    if (TooManyCreaturesAvailable)
                    {
                        break;
                    }
                }
            }
        }
    }

    public override void BeingInfluenced(Cycle oC)
    {
        cycle.spacialTracker.RippleTick(oC.spacialTracker.pos.v);
    }
}