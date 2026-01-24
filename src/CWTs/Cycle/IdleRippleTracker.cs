using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RWCustom;
using static PitchBlack.Plugin;
using Watcher;

namespace PitchBlack;

public class IdleRippleTracker : CycleModule
{
    public SpacialTracker spacialTracker => cycle.spacialTracker;

    public string s = "IdleRippleTracker:";
    public float rippleSpawnChance = 0.0008f;
    public int rippleLimit = 10;
    public bool SpawnIdleRipples;
    int defaultDelay = 80;
    public Counter delayCounter = new(80, 0, true);

    public IdleRippleTracker(Cycle cycle) : base(cycle) { }

    public override void Update()
    {
        // If ticked, needs delay, then we wait until the delay is finished to start over
        if (delayCounter > 0)
        {
            delayCounter.Tick();
            if (delayCounter.isFinished)
            {
                delayCounter.Reset();
            }
            return;
        }

        if (spacialTracker.BelowRippleSurface)
        {
            // Manipulate counter delay for more frequent ripples
            if (spacialTracker.pos.w > spacialTracker.rippleSurface)
            {
                delayCounter.max = Random.Range(40, 100);
            }
            else if (delayCounter.max != defaultDelay)
            {
                delayCounter.max = defaultDelay;
            }

            SpawnRippleRing();
            delayCounter.Tick();
        }
    }

    public void SpawnRippleRing()
    {
        float w = spacialTracker.pos.w;
        Vector2 pos = new(spacialTracker.pos.x, spacialTracker.pos.y);
        int life = Random.Range(20, Random.Range(20, 80));
        float intensity = w;
        float speed = 0.15f + intensity;

        if (cycle.RealizedOwner == null)
        {
            return;
        }

        // RippleRing
        RippleRing ripple = new(pos, life, intensity, speed);
        cycle.RealizedOwner.room.AddObject(ripple);

        // Shockwave
        float intensity2 = intensity - 0.45f;
        cycle.RealizedOwner.room.AddObject(new ShockWave(pos, intensity2, intensity, life));

        if (devMode)
        {
            logger.LogDebug($"{s} Spawned ripple for {cycle.CycleCreatureTemplateType.value} - {life}, {intensity}, {speed} - {intensity2}");
        }
    }
}
