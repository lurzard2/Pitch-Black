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
    public float rippleNone = 0;
    public float rippleSurface = 0.55f;
    public float rippleSurfaceTension = 0.65f;
    public bool AboveRippleSurface => pos.w <= rippleSurfaceTension;
    public bool BelowRippleSurface => pos.w >= rippleSurface;
    public bool BrokeRippleSurfaceTension => pos.w > rippleSurfaceTension;
    public bool reboundFromRipple;

    public SpacialTracker(Cycle owner) : base(owner) { }

    public override void Update()
    {
        pos.x = cycle.RealizedOwner.mainBodyChunk.pos.x;
        pos.y = cycle.RealizedOwner.mainBodyChunk.pos.y;

        W_Axis();
    }

    #region W Axis Influence
    private void W_Axis()
    {
        // Idling submersion only, ticks randomly, and brought down to 0 once the "ripple surface" is reached
        if (AboveRippleSurface)
        {
            IdleRippleSubmersionTick();
        }
        else if (BrokeRippleSurfaceTension)
        {

        }
    }

    private void IdleRippleSubmersionTick()
    {
        RippleTick(reboundFromRipple ? 0 : rippleSurfaceTension);
        if (BelowRippleSurface && !reboundFromRipple)
        {
            if (BrokeRippleSurfaceTension)
            {
                reboundFromRipple = true;
                pos.w = rippleSurfaceTension;
            }
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
            pos.w = Custom.LerpAndTick(pos.w, target, 0.008f, 0.0025f);
            if (pos.w == rippleNone)
            {
                reboundFromRipple = false;
            }
        }
        else if (Random.value < 0.05f)
        {
            pos.w = Custom.LerpAndTick(pos.w, target, 0.008f, 0.0025f);
        }

        if (devMode && cycle.CycleCreatureTemplateType == CreatureTemplate.Type.Slugcat)
        {
            logger.LogDebug($"W: {pos.w} : {reboundFromRipple}");
        }
    }
    #endregion
}
