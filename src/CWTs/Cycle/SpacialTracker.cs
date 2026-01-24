using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class SpacialTracker : CycleModule
{
    public MDVector pos = new();
    public float RippleSurfaceTension => 0.1f;
    public bool AboveRippleSurface => pos.w <= RippleSurfaceTension;
    public bool BelowRippleSurface => pos.w >= RippleSurfaceTension;
    public bool reboundFromRipple;

    public SpacialTracker(Cycle owner) : base(owner) { }

    public override void Update()
    {
        pos.x = cycle.RealizedOwner.mainBodyChunk.pos.x;
        pos.y = cycle.RealizedOwner.mainBodyChunk.pos.y;

        // Idling submersion only, ticks randomly, and brought down to 0 once the "ripple surface" is reached
        if (AboveRippleSurface)
        {
            IdleRippleSubmersionTick();
        }
        else if (BelowRippleSurface)
        {
        }
    }

    private void IdleRippleSubmersionTick()
    {
        RippleTick();
        if (pos.w >= RippleSurfaceTension && !reboundFromRipple)
        {
            reboundFromRipple = true;
            pos.w = RippleSurfaceTension;
        }
    }

    public void RippleTick()
    {
        if (reboundFromRipple)
        {
            pos.w = Custom.LerpAndTick(pos.w, 0f, 0.008f, 0.0025f);
            if (pos.w == 0)
            {
                reboundFromRipple = false;
            }
        }
        else if (Random.value < 0.01f)
        {
            if (AboveRippleSurface)
            {
                pos.w = Custom.LerpAndTick(pos.w, 0.1f, 0.008f, 0.0025f);
            }
        }

        if (cycle.CycleCreatureTemplateType == Enums.CreatureTemplateType.Rotrat)
        {
            logger.LogDebug($"W: {pos.w} : {reboundFromRipple}");
        }
    }
}
