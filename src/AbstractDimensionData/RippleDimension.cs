using DevInterface;
using RWCustom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Watcher;

namespace PitchBlack.AbstractDimensionData
{
    public static class RippleDimension
    {
        public class Axis
        {
            public float pos;

            // Outside
            public const float OriginPos = 0f;
            public const float ContactPos = 0.45f;
            public const float SurfaceTensionPos = 0.5f;
            // Median
            public const float OuterZonePos = 0.65f;
#if false
            public const float IntersticePos = 0.7f;
            public const float TwilightZonePos = 1f;
            // Inside
            public const float MidnightZonePos = 2f;
            public const float AbyssalZonePos = 3f;
            public const float HadalZonePos = 4f;
#endif

            // Coordinate tracking value progression
            public bool IsAboveRippleSurface => pos <= SurfaceTensionPos;
            public bool IsAgainstRippleSurfaceTension => pos >= ContactPos && pos < OuterZonePos;
            public bool IsUnderRippleSurface => pos > SurfaceTensionPos;
            public bool InOuterZone => pos >= OuterZonePos;
        }

        public static Vector2 GetReflectedPos(Vector2 pos, float intensity)
        {
            // Scatter pos in a radius randomly based on rippleAxisPos
            float maxRadius = 30f * (intensity * 10);
            return pos + Custom.RNV() * Random.Range(1f, maxRadius);
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
                intensity = Random.Range(0.3f, Axis.ContactPos);
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
    }
}
