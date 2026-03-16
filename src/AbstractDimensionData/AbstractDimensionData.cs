using RWCustom;
using UnityEngine;

namespace PitchBlack.AbstractDimensionData
{
    public class AbstractDimensionData
    {
        public AbstractPhysicalObject owner;
        public bool IsRealized => RealizedOwner is not null && RealizedRoom is not null;
        public PhysicalObject RealizedOwner => owner.realizedObject;
        public Room RealizedRoom => owner.Room.realizedRoom;

        #region Ripple Axis
        // Ripple
        public RippleDimension.Axis rippleAxis = new();
        public bool AllowedToEnterRippleDimension { get; set; }
        public Counter spawningRippleRingDelay = new(80, 0, true);

        // Ripple Exposure
        public RoomRippleExposure rippleExposure => owner.Room.GetRippleExposure();
        public float dynamicRippleExposureFromProximity;
        public bool updateDynamicExposureFlag { get; set; } = false;

        private RippleTravelPhase rippleTravelPhase;
        public enum RippleTravelPhase
        {
            Idle,
            Rebound,
            PassThrough,
        }
        public void SetRippleTravelPhase(RippleTravelPhase newType) => rippleTravelPhase = newType;
        #endregion

        public AbstractDimensionData(AbstractPhysicalObject absOwner)
        {
            owner = absOwner;
            rippleAxis.pos = Random.Range(0, RippleDimension.Axis.ContactPos);
        }

        private void TravelRippleAxis()
        {
            // Find target value
            float targetValue = RippleDimension.Axis.ContactPos;
            bool inRipple = owner.rippleLayer == 1;
            switch (rippleTravelPhase)
            {
                case RippleTravelPhase.Idle:
                    targetValue = RippleDimension.Axis.SurfaceTensionPos;
                    break;
                case RippleTravelPhase.Rebound:
                    targetValue = 0;
                    break;
            }

            // lerp is room exposure
            // tick is personal dynamic exposure: default low value + currently tracked value
            rippleAxis.pos = Custom.LerpAndTick(rippleAxis.pos, targetValue, rippleExposure.globalExposure, 0.0015f + dynamicRippleExposureFromProximity);
        }

        public void Update()
        {
            if (AllowedToEnterRippleDimension)
            {
                RippleTravel_Allowed();
            }
            else
            {
                RippleTravel_Idle();
            }

            TravelRippleAxis();

            // Tick delay before spawning again, otherwise spawn then tick delay
            if (spawningRippleRingDelay > 0)
            {
                spawningRippleRingDelay.Tick();
                if (spawningRippleRingDelay.isFinished)
                {
                    spawningRippleRingDelay.Reset();
                }
            }
            else if (IsRealized && rippleAxis.IsUnderRippleSurface)
            {
                spawningRippleRingDelay.max = Random.Range(20, 100);
                RippleDimension.SpawnRippleRing(RealizedOwner.firstChunk.pos, RealizedRoom, rippleAxis.pos);
                spawningRippleRingDelay.Tick();
            }
        }

        private void RippleTravel_Idle()
        {
            if (rippleAxis.IsAgainstRippleSurfaceTension)
            {
                // value can increase over surface tension, Activate rebound either:
                // A- Randomly if we're getting very close to submerging.
                // B- We're at the limit.

                if (Random.value < 0.008f || rippleAxis.IsUnderRippleSurface)
                {
                    SetRippleTravelPhase(RippleTravelPhase.Rebound);
                }
            }
        }

        private void RippleTravel_Allowed()
        {
            //<Later>
        }
    }
}
