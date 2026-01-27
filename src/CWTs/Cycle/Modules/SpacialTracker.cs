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



    public SpacialTracker(Cycle owner) : base(owner)
    {
        // Prevent common syncing ticks to spawn ripples
        pos.v = Random.Range(rippleNone, rippleSurface);
    }

    public override void Update()
    {
        pos.Main = cycle.RealizedOwner.mainBodyChunk.pos;

        // Intentionally ordered
        W_Axis();
        V_Axis();

        if (devMode && cycle.CycleCreatureTemplateType == CreatureTemplate.Type.Slugcat)
        {
            logger.LogDebug($"W: {pos.w} | V: {pos.v} - {reboundFromRipple}");
        }
    }

    #region W Axis Influence
    public void W_Axis()
    {
        // Submerged in dream also influences ripple. InRippleDepths should be true if InsideDream is
        if (AvailableForDream)
        {
            if (pos.w < dreamInside)
            {
                pos.w = dreamInside;
                pos.v = rippleDepths;
            }
        }
        else if (pos.w > dreamNone)
        {
            pos.w = dreamNone;
            pos.v = rippleNone;
        }
    }
    #endregion

    #region V Axis Influence
    private void V_Axis()
    {
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
    #endregion
}
