using RWCustom;
using UnityEngine;

namespace PitchBlack.Dimensions
{
    public class AbstractDimensionData
    {
        public AbstractPhysicalObject owner;
        public bool IsRealized => RealizedOwner is not null && RealizedRoom is not null;
        public PhysicalObject RealizedOwner => owner.realizedObject;
        public Room RealizedRoom => owner.Room.realizedRoom;


        public RippleDimension.PersonalRippleData rippleData = new();
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
            SwitchSide,
        }
        public void SetRippleTravelPhase(RippleTravelPhase newType) => rippleTravelPhase = newType;

        public AbstractDimensionData(AbstractPhysicalObject absOwner)
        {
            owner = absOwner;
            rippleData.currentValue = Random.Range(0, RippleDimension.PersonalRippleData.RippleSurfaceContactPos);
        }

        public void Update()
        {
            RippleUpdate();
        }

        private void RippleUpdate()
        {
            if (AllowedToEnterRippleDimension)
            {
                //<later>
            }
            else
            {
                if (rippleData.AgainstRippleSurfaceTension && rippleTravelPhase != RippleTravelPhase.Rebound)
                {
                    // value can increase over surface tension, Activate rebound either:
                    // A- Randomly if we're getting very close to submerging.
                    // B- We're at the limit.
                    if (rippleData.IsUnderRippleSurface || Random.value < 0.008f)
                    {
                        SetRippleTravelPhase(RippleTravelPhase.Rebound);
                    }
                }
            }

            // Tick delay before spawning again, otherwise spawn then tick delay
            if (spawningRippleRingDelay > 0)
            {
                spawningRippleRingDelay.Tick();
                if (spawningRippleRingDelay.isFinished)
                {
                    spawningRippleRingDelay.Reset();
                }
            }
            else if (IsRealized)
            {
                spawningRippleRingDelay.max = Random.Range(10, 50);
                SpawnRippleRing();
                spawningRippleRingDelay.Tick();
            }
            TravelRippleAxis();
        }

        public void SpawnRippleRing()
        {
            RippleDimension.SpawnRippleRing(RealizedOwner.firstChunk.pos, RealizedRoom, rippleData.currentValue);
        }

        private void TravelRippleAxis()
        {
            // Find target value
            float targetValue = RippleDimension.PersonalRippleData.RippleSurfaceContactPos;
            bool inRipple = owner.rippleLayer == 1;
            switch (rippleTravelPhase)
            {
                case RippleTravelPhase.Idle:
                    targetValue = rippleData.SurfaceTensionEndPos;
                    break;
                case RippleTravelPhase.Rebound:
                    targetValue = 0;
                    break;
            }

            // lerp is room exposure
            // tick is personal dynamic exposure: default low value + currently tracked value
            rippleData.currentValue = Custom.LerpAndTick(rippleData.currentValue, targetValue, rippleExposure.globalExposure, 0.0015f + dynamicRippleExposureFromProximity);

            //    if (owner is AbstractCreature c && c.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
            //    Plugin.logger.LogDebug($"RipplePos:[{rippleAxis.pos}, {targetValue}, {rippleExposure.globalExposure}, {dynamicRippleExposureFromProximity}]");
        }
    }
}
