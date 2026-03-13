using DevInterface;
using RWCustom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Watcher;

namespace PitchBlack.CreatureCWT
{
    public static class RippleInterfacer
    {
        public static void RippleInteract(this AbstractCreature absCrit)
        {
            if (!absCrit.TryGetCreatureCWT(out var data))
            {
                return;
            }

            // Subtract instantly until satisfied
            if (data.reboundFromRipple)
            {
                data.rippleAxisPoint = Custom.LerpAndTick(data.rippleAxisPoint, 0, globalLerpRate, globalTickRate);
                bool closeToNone = data.rippleAxisPoint <= Random.Range(0, 0.2f);
                data.reboundFromRipple = closeToNone;
            }
            // Tick up with rebound edge case
            else if (AboveRippleSurface(data.rippleAxisPoint) && ChanceForTick())
            {
                data.rippleAxisPoint = Custom.LerpAndTick(data.rippleAxisPoint, rippleSurfaceTension, globalLerpRate, globalTickRate);

                // value can increase over surface tension, Activate rebound either:
                // A- Randomly if we're getting very close to submerging.
                // B- We're at the limit.

                if (BelowRippleSurface(data.rippleAxisPoint) && Random.value < 0.008f)
                {
                    data.reboundFromRipple = true;
                }
                else if (InRippleWaters(data.rippleAxisPoint))
                {
                    data.rippleAxisPoint = rippleSurfaceTension;
                    data.reboundFromRipple = true;
                }
            }

            // Tick delay before spawning again
            if (data.rippleSpawnDelay > 0)
            {
                data.rippleSpawnDelay.Tick();
                if (data.rippleSpawnDelay.isFinished)
                {
                    data.rippleSpawnDelay.Reset();
                }
                return;
            }
            // Spawn then begin delay
            if (absCrit.TryGetRealized(out var crit) && BelowRippleSurface(data.rippleAxisPoint))
            {
                data.rippleSpawnDelay.max = Random.Range(40, 100);
                SpawnRippleRing(crit, data.rippleAxisPoint);
                data.rippleSpawnDelay.Tick();
            }
        }

        public static void SpawnRippleRing(this Creature crit, float intensity)
        {
            Vector2 pos = new(crit.mainBodyChunk.pos.x, crit.mainBodyChunk.pos.y);
            int life = Random.Range(20, Random.Range(20, 80));
            float speed = 0.15f + intensity;

            //if (cycle.RealizedOwner.room == null)
            //{
            //    return;
            //}

            //if (spacialTracker.InDream)
            //{
            //    life = Random.Range(20, 120);
            //    intensity = Random.Range(0.3f, spacialTracker.rippleSurface);
            //    speed = Random.Range(0.5f, 1f);
            //}

            if (Plugin.devMode)
            {
                if (Input.GetKey("e"))
                {
                    intensity = 2f;
                }

                // Can be empty, but shouldn't be if it's a player
                string playerCharString = "";
                string creatureType = crit.abstractCreature.creatureTemplate.type.value;
                if (crit is Player p)
                {
                    string playerName = $"{p.slugcatStats.name}";
                    string playerIndex = $"{p.room.PlayersInRoom.IndexOf(p)}";
                    playerCharString = $"Player[{playerName},{playerIndex}]";
                    creatureType = "";
                }
                Custom.LogImportant(
                [
                    $"{nameof(RippleInterfacer)} ",
                "Spawned ripple for ",
                $"{playerCharString}",
                $"{creatureType}",
                $"~ {life}, {intensity}, {speed}",
            ]);
            }

            // RippleRing
            RippleRing ripple = new(pos, life, intensity, speed);
            crit.room.AddObject(ripple);

            // Shockwave
            float intensity2 = intensity - 0.35f;
            crit.room.AddObject(new ShockWave(pos, intensity2, intensity, life));
        }

        public static float globalTickRate = 0.0025f;
        public static float globalLerpRate = 0.008f;
        public static bool ChanceForTick() => Random.value < 0.5f;

        public const float rippleSurface = 0.45f;
        public const float rippleSurfaceTension = 0.65f;
        public static bool AboveRippleSurface(float r) => r <= rippleSurfaceTension;
        public static bool BelowRippleSurface(float r) => r >= rippleSurface && !InRippleWaters(r);
        public static bool InRippleWaters(float r) => r > rippleSurfaceTension;
    }
}
