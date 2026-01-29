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

    // Ripple
    public const float rippleNone = 0;
    public readonly float rippleSurface = 0.55f;
    public readonly float rippleSurfaceTension = 0.65f;
    public const float rippleWaters = 1f;
    public const float rippleDepths = 5f;
    public bool reboundFromRipple;
    public bool AboveRippleSurface => pos.v <= rippleSurfaceTension;
    public bool BelowRippleSurface => pos.v >= rippleSurface;
    public bool InRippleWaters => pos.v > rippleSurfaceTension;
    public bool InRippleDepths => pos.v >= rippleDepths;

    // Dream (Affects Ripple)
    public const float dreamNone = 0f;
    public const float dreamInside = 1f;
    public bool AvailableForDream => MiscUtils.IsRegionOutSideCycle(cycle.abstractOwner.world);
    public bool InDream => InRippleDepths && pos.w >= dreamInside;

    // Spiral
    public readonly float spiralNone = 0f;


    public SpacialTracker(Cycle owner) : base(owner)
    {
        // Prevent common syncing ticks to spawn ripples
        pos.v = Random.Range(rippleNone, rippleSurface);
    }

    public override void Realized()
    {
        base.Realized();
        pos.Main = cycle.RealizedOwner.mainBodyChunk.pos;

        // Intentionally ordered
        H_Axis();
        W_Axis();
        V_Axis();

        if (devMode && cycle.CycleCreatureTemplateType == CreatureTemplate.Type.Slugcat)
        {
            logger.LogDebug($"W: {pos.w} | V: {pos.v} - {reboundFromRipple}");
        }
    }

    #region H Axis Influence
    private void H_Axis()
    {

    }

    public void SpiralTick(float target)
    {
        pos.h = Custom.LerpAndTick(pos.h, target, 0.008f, 0.0025f);
    }

    public void SetSpiral(float set)
    {
        pos.h = set;
    }
    #endregion

    #region W Axis Influence
    private void W_Axis()
    {
        // Submerged in dream also influences ripple. InRippleDepths should be true if InsideDream is
        if (AvailableForDream)
        {
            if (pos.w < dreamInside)
            {
                SetDream(dreamInside);
                SetRipple(rippleDepths);
            }
        }
        else if (pos.w > dreamNone)
        {
            SetDream(dreamNone);
            SetRipple(rippleNone);
        }
    }

    public void SetDream(float set)
    {
        pos.w = set;
    }
    #endregion

    #region V Axis Influence
    private void V_Axis()
    {
        // Too far
        if (InRippleDepths)
        {
        }
        // Submerged, but not close enough
        else if (InRippleWaters)
        {

        }
        // Idling submersion only, ticks randomly, and brought down to 0 once the "ripple surface" is reached
        else if (AboveRippleSurface)
        {
            IdleRippleSubmersionTick();
        }
    }

    private void IdleRippleSubmersionTick()
    {
        RippleTick(reboundFromRipple ? 0 : rippleSurfaceTension);
        if (BelowRippleSurface && !reboundFromRipple)
        {
            // Activate rebound once barrier reached, and move it back
            if (InRippleWaters)
            {
                reboundFromRipple = true;
                pos.v = rippleSurfaceTension;
            }
            // While pushing against the surface, there's a chance to activate rebound
            else if (Random.value < 0.008)
            {
                reboundFromRipple = true;
            }
        }
    }

    public void RippleTick(float target)
    {
        if (reboundFromRipple)
        {
            pos.v = Custom.LerpAndTick(pos.v, target, 0.008f, 0.0025f);
            if (pos.v == rippleNone)
            {
                reboundFromRipple = false;
            }
        }
        else if (Random.value < 0.02f)
        {
            pos.v = Custom.LerpAndTick(pos.v, target, 0.008f, 0.0025f);
        }
    }
    public void SetRipple(float set)
    {
        pos.v = set;
    }
    #endregion
}
