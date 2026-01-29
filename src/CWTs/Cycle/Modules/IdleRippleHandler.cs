using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RWCustom;
using static PitchBlack.Plugin;
using Watcher;

namespace PitchBlack;

public class IdleRippleHandler : CycleModule
{
    public SpacialTracker spacialTracker => cycle.spacialTracker;

    private readonly string debug = $"{nameof(IdleRippleHandler)}:";
    private Counter delayCounter = new(80, 0, true);
    private readonly int defaultDelay = 80;

    public class RippleRingSource : ExtEnum<RippleRingSource>
    {
        public RippleRingSource(string value, bool register) : base(value, register) { }

        public static readonly RippleRingSource Idle = new(nameof(Idle), true);
        public static readonly RippleRingSource Thanatosis = new(nameof(Thanatosis), true);
        public static readonly RippleRingSource Cache = new(nameof(Cache), true);
    }

    public IdleRippleHandler(Cycle cycle) : base(cycle) { }

    public override void Abstract()
    {
        base.Abstract();
        // If ticked, needs delay
        if (delayCounter > 0)
        {
            delayCounter.Tick();
            if (delayCounter.isFinished)
            {
                delayCounter.Reset();
            }
        }
    }

    public override void Realized()
    {
        base.Realized();
        // If ongoing delay, hold
        if (delayCounter > 0)
        {
            return;
        }

        if (spacialTracker.BelowRippleSurface)
        {
            // Manipulate counter delay for differently timed ripples
            if (spacialTracker.pos.v > spacialTracker.rippleSurface)
            {
                delayCounter.max = Random.Range(40, 100);
            }
            else if (delayCounter.max != defaultDelay)
            {
                delayCounter.max = defaultDelay;
            }

            if (cycle.RealizedOwner.room != null)
            {
                SpawnRippleRing();
            }
            // Start the delay loop and perpetuate it
            delayCounter.Tick();
        }
    }

    public void SpawnRippleRing()
    {
        float v = spacialTracker.pos.v;

        Vector2 pos = spacialTracker.pos.Main;
        int life = Random.Range(20, Random.Range(20, 80));
        float intensity = v;
        float speed = 0.15f + intensity;

        if (spacialTracker.InDream)
        {
            life = Random.Range(20, 120);
            intensity = Random.Range(0.3f, spacialTracker.rippleSurface);
            speed = Random.Range(0.5f, 1f);
        }

        if (devMode)
        {
            if (Input.GetKey("e"))
            {
                intensity = 2f;
            }

            // Can be empty, but shouldn't be if it's a player
            string playerCharString = "";
            string creatureType = cycle.CycleCreatureTemplateType.value;
            if (cycle.RealizedOwner is Player p)
            {
                string playerName = $"{p.slugcatStats.name}";
                string playerIndex = $"{p.room.PlayersInRoom.IndexOf(p)}";
                playerCharString = $"Player[{playerName},{playerIndex}]";
                creatureType = "";
            }
            Custom.LogImportant(
            [
                $"{debug} ",
                "Spawned ripple for ",
                $"{playerCharString}",
                $"{creatureType}",
                $"~ {life}, {intensity}, {speed}",
            ]);
        }

        // RippleRing
        RippleRing ripple = new(pos, life, intensity, speed);
        cycle.RealizedOwner.room.AddObject(ripple);

        // Shockwave
        float intensity2 = intensity - 0.35f;
        cycle.RealizedOwner.room.AddObject(new ShockWave(pos, intensity2, intensity, life));
    }
}
