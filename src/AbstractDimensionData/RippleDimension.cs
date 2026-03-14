using DevInterface;
using RWCustom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Watcher;

namespace PitchBlack.AbstractDimensionData
{
    public static class RippleDimension
    {
        public static Vector2 ReflectedPos(Vector2 pos)
        {
            //<Generate rnv, randomly scatters within the radius and returns that new pos>
            return pos;
        }

        public static void SpawnRippleRing(Vector2 passedPos, Room room, float intensity)
        {
            if (room is null)
            {
                return;
            }

            Vector2 pos = ReflectedPos(passedPos);
            int life = Random.Range(20, Random.Range(20, 80));
            float speed = 0.15f + intensity;

            if (MiscUtils.IsRegionOutSideCycle(room.world))
            {
                life = Random.Range(20, 120);
                intensity = Random.Range(0.3f, SurfacePos);
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
                $"~ {passedPos}, {life}, {intensity}, {speed}",
            ]);
            #endregion

            // RippleRing
            RippleRing ripple = new(passedPos, life, intensity, speed);
            room.AddObject(ripple);

            // Shockwave
            float intensity2 = intensity - 0.35f;
            room.AddObject(new ShockWave(passedPos, intensity2, intensity, life));
        }

        public const float SurfacePos = 0.45f;
        public const float SurfaceTensionPos = 0.65f;
    }
}
