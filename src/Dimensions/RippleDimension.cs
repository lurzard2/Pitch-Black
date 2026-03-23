using DevInterface;
using RWCustom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Watcher;

namespace PitchBlack.Dimensions
{
    public static class RippleDimension
    {

        public class PersonalRippleAxis
        {
            // Defined values
            public const float OriginPos = 0f;
            public float SwitchSideEndTargetPos => RippleSideTag ? OuterZonePos : SurfaceTensionEndTargetPos - 0.25f;
            /// Surface of water
            public const float RippleSurfaceContactPos = 0.4f;
            const float surfaceTension = 0.15f;
            const float InsideSurfaceTensionEndPos = RippleSurfaceContactPos + surfaceTension;
            public float SurfaceTensionEndTargetPos => RippleSideTag ? RippleSurfaceContactPos - surfaceTension : InsideSurfaceTensionEndPos;
            /// Inside water
            public const float OuterZonePos = InsideSurfaceTensionEndPos + 0.25f;
            public const float TwilightZonePos = OuterZonePos + 0.25f;
            public const float MidnightZonePos = TwilightZonePos + 0.25f;
            public const float AbyssalZonePos = MidnightZonePos + 0.25f;
            public const float HadalZonePos = AbyssalZonePos + 0.25f;
            public bool IsUnderRippleSurface => currentValue >= InsideSurfaceTensionEndPos;
            public bool AgainstRippleSurfaceTension
            {
                get
                {
                    // Value between range of contact and max/min depending on the side you're on
                    return RippleSideTag ?

                        currentValue < RippleSurfaceContactPos
                        && currentValue >= SurfaceTensionEndTargetPos

                        : currentValue > RippleSurfaceContactPos
                        && currentValue <= SurfaceTensionEndTargetPos;
                }
            }
            public bool SwitchedRippleSides => currentValue == SwitchSideEndTargetPos;
            public bool IsInOuterZone => currentValue >= OuterZonePos && currentValue < TwilightZonePos;  

            public float currentValue;
            public bool AllowedInsideRippleTemporarily { get; set; }
            public bool RippleSideTag { get; set; }
            // Value 0-1 for camo effect
            public float GraphicsMaskProgress => Mathf.InverseLerp(InsideSurfaceTensionEndPos, OuterZonePos, currentValue);
        }

        public static void SpawnRippleRing(Vector2 objPos, Room room, float intensity)
        {
            if (room is null)
            {
                return;
            }
                
            Vector2 pos = GetReflectedPos(objPos, intensity);
            int life = Random.Range(20, Random.Range(20, 80));
            float speed = 0.15f + intensity;

            if (MiscUtils.IsRegionOutSideCycle(room.world))
            {
                life = Random.Range(20, 120);
                intensity = Random.Range(0.3f, PersonalRippleAxis.RippleSurfaceContactPos);
                speed = Random.Range(0.5f, 1f);
            }

            #region Debugging

            if (Plugin.devMode && Input.GetKey("e"))
            {
                intensity = 2f;
                Plugin.logger.LogDebug($"{nameof(RippleDimension)}: Ripple intensity modified for debugging.");
            }

            Custom.LogImportant(
            [
                $"{nameof(RippleDimension)} ",
                "Spawned RippleRing for ",
                $"~ {objPos}, {life}, {intensity}, {speed}",
            ]);
            #endregion

            // RippleRing
            RippleRing ripple = new(objPos, life, intensity, speed);
            room.AddObject(ripple);

            // Shockwave
            float intensity2 = intensity - 0.35f;
            room.AddObject(new ShockWave(objPos, intensity2, intensity, life));
        }

        public static Vector2 GetReflectedPos(Vector2 pos, float intensity)
        {
            // Scatter pos in a radius randomly based on rippleAxisPos
            float maxRadius = 15f * (intensity * 10);
            return pos + Custom.RNV() * Random.Range(1f, maxRadius);
        }
    }
}
