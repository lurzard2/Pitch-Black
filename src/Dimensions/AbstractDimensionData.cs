using IL.Menu;
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


        public RippleDimension.PersonalRippleAxis rippleData = new();
        public Counter spawningRippleRingDelay = new(80, 0, true);

        // Ripple Exposure
        public RoomRippleExposure rippleExposure => owner.Room.GetRippleExposure();
        public float dynamicRippleExposureFromProximity;
        public bool hasUpdatecDynamicExposure { get; set; } = false;

        public RippleTravelPhase rippleTravelPhase;
        public enum RippleTravelPhase
        {
            Idle,
            FlowIn,
            Rebound,
            SwitchSide,
        }
        public void SetRippleTravelPhase(RippleTravelPhase newType) => rippleTravelPhase = newType;

        public AbstractDimensionData(AbstractPhysicalObject absOwner)
        {
            owner = absOwner;
            rippleData.currentValue = Random.Range(0, RippleDimension.PersonalRippleAxis.RippleSurfaceContactPos);
        }

        public void Update()
        {
            RippleUpdate();
        }

        private void RippleUpdate()
        {
            if (rippleData.AllowedInsideRippleTemporarily)
            {
                bool lastRippleSideTag = rippleData.RippleSideTag;
                rippleData.RippleSideTag = rippleData.IsUnderRippleSurface;
                // Moment where side was switched so we tag it
                if (lastRippleSideTag != rippleData.RippleSideTag)
                {
                    SetRippleTravelPhase(RippleTravelPhase.SwitchSide);
                }
            }
            else
            {
                if (rippleTravelPhase != RippleTravelPhase.Rebound)
                {
                    if (rippleData.AgainstRippleSurfaceTension)
                    {
                        // value can increase over surface tension, Activate rebound either:
                        // A- Randomly if we're getting very close to submerging.
                        // B- We're at the limit.
                        if (rippleData.IsUnderRippleSurface || Random.value < 0.008f)
                        {
                            SetRippleTravelPhase(RippleTravelPhase.Rebound);
                        }
                    }
                    // And if you're completely inside, GET OUT.
                    else if (rippleData.IsUnderRippleSurface)
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
            float targetValue = RippleDimension.PersonalRippleAxis.RippleSurfaceContactPos;
            switch (rippleTravelPhase)
            {
                // Go to 0
                case RippleTravelPhase.Rebound:
                    targetValue = 0;
                    if (rippleData.currentValue <= 0 || Random.value < 0.008f)
                        SetRippleTravelPhase(RippleTravelPhase.Idle);
                    break;
                // Go a little above or below threshold to properly switch sides
                case RippleTravelPhase.SwitchSide:
                    targetValue = rippleData.SwitchSideEndTargetPos;
                    if (rippleData.SwitchedRippleSides)
                        SetRippleTravelPhase(RippleTravelPhase.Idle);
                    break;
                // Target inside zone while in ripples, randomly abandon
                case RippleTravelPhase.FlowIn:
                    targetValue = RippleDimension.PersonalRippleAxis.OuterZonePos;
                    if (rippleData.IsInOuterZone || Random.value < 0.008f)
                        SetRippleTravelPhase(RippleTravelPhase.Idle);
                    break;

                default:
                    targetValue = rippleData.SurfaceTensionEndTargetPos;
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
