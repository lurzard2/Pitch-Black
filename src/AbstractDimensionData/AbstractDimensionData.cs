using RWCustom;
using UnityEngine;

namespace PitchBlack.AbstractDimensionData
{
    public class AbstractDimensionData
    {
        public AbstractDimensionData(AbstractPhysicalObject absOwner)
        {
            owner = absOwner;
            rippleAxisPos = Random.Range(0, RippleDimension.SurfacePos);
        }

        public void Update()
        {
            IdleRippleSubmersion();
        }

        public void IdleRippleSubmersion()
        {
            if (reboundFromRipple)
            {
                rippleAxisPos = Custom.LerpAndTick(rippleAxisPos, 0, globalLerpRate, globalTickRate);
                bool closeToNone = rippleAxisPos <= Random.Range(0, 0.2f);
                reboundFromRipple = closeToNone;
            }
            // Tick up with rebound edge case
            else if (IsAboveRippleSurface && Random.value < 0.5f)
            {
                rippleAxisPos = Custom.LerpAndTick(rippleAxisPos, RippleDimension.SurfaceTensionPos, globalLerpRate, globalTickRate);

                // value can increase over surface tension, Activate rebound either:
                // A- Randomly if we're getting very close to submerging.
                // B- We're at the limit.

                if (IsAgainstRippleSurfaceTension && Random.value < 0.008f)
                {
                    reboundFromRipple = true;
                }
                else if (InsideRippleWater)
                {
                    rippleAxisPos = RippleDimension.SurfacePos;
                    reboundFromRipple = true;
                }
            }

            // Tick delay before spawning again
            if (spawningRippleRingDelay > 0)
            {
                spawningRippleRingDelay.Tick();
                if (spawningRippleRingDelay.isFinished)
                {
                    spawningRippleRingDelay.Reset();
                }
            }
            // Spawn then begin delay
            else if (owner.TryGetRealizedObj(out var obj) && obj.room is not null && IsAgainstRippleSurfaceTension)
            {
                spawningRippleRingDelay.max = Random.Range(40, 100);
                RippleDimension.SpawnRippleRing(obj.firstChunk.pos, obj.room, rippleAxisPos);
                spawningRippleRingDelay.Tick();
            }
        }

        public AbstractPhysicalObject owner;

        public float rippleAxisPos;
        public bool reboundFromRipple;
        public Counter spawningRippleRingDelay = new(80, 0, true);

        // Todo: move these to be associated with the current room's exposure to ripplespace and made more descriptive
        public static float globalTickRate = 0.0025f;
        public static float globalLerpRate = 0.008f;

        public bool IsAboveRippleSurface => rippleAxisPos <= RippleDimension.SurfaceTensionPos;
        public bool IsAgainstRippleSurfaceTension => rippleAxisPos >= RippleDimension.SurfacePos && !InsideRippleWater;
        public bool InsideRippleWater => rippleAxisPos > RippleDimension.SurfaceTensionPos;
    }
}
