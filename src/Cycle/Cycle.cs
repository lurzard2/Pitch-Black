using RWCustom;
using System;
using System.Collections.Generic;
using static PitchBlack.Plugin;
using UnityEngine;
using Random = UnityEngine.Random;
using Watcher;
using BepInEx.Bootstrap;

namespace PitchBlack;

public abstract class Cycle
{
    public AbstractCreature abstractOwner;
    public Creature RealizedOwner => abstractOwner.realizedCreature;
    public CreatureTemplate.Type CycleCreatureTemplateType => abstractOwner.creatureTemplate.type;

    public SaveState SaveState => abstractOwner.world.game.GetSaveState();

    public State state;
    public class State : ExtEnum<State>
    {
        public State(string value, bool register) : base(value, register) { }

        public static readonly State Init = new(nameof(Init), true);
        public static readonly State Alive = new(nameof(Alive), true);
        public static readonly State Thanatosis = new(nameof(Thanatosis), true);
        public static readonly State ExitThanatosis = new(nameof(ExitThanatosis), true);
        public static readonly State PersistThroughCache = new(nameof(PersistThroughCache), true);
        public static readonly State MarkedForCache = new(nameof(MarkedForCache), true);
        public static readonly State Cached = new(nameof(Cached), true);
    }

    public int idleRipplesToSpawn;
    public bool spawnedPendingRipples;
    public class CycleRippleSource : ExtEnum<CycleRippleSource>
    {
        public CycleRippleSource(string value, bool register) : base(value, register) { }

        public static readonly CycleRippleSource Idle = new(nameof(Idle), true);
        public static readonly CycleRippleSource Thanatosis = new(nameof(Thanatosis), true);
        public static readonly CycleRippleSource Cache = new(nameof(Cache), true);
    }

    public Counter cycleTime = new(Int32.MaxValue, 0, true);
    public Counter cycleStateTime = new(Int32.MaxValue, 0, true);

    public List<CycleModule> modules = [];
    public IdleRippleTracker idleRippleTracker {  get; set; }
    public SpacialTracker spacialTracker {  get; set; }

    public Cycle(AbstractCreature abstractOwner)
    {
        this.abstractOwner = abstractOwner;
        state = State.Init;
    }

    // Back end
    public virtual void AbstractUpdate()
    {
        if (state == State.Init)
        {
            Sync();
            return;
        }

        CycleTick();

        if (RealizedOwner is not null)
            RealizedUpdate();
    }

    // Front end
    public virtual void RealizedUpdate()
    {
        foreach (CycleModule module in modules)
        {
            module.Update();
        }
    }

    public virtual bool KillMe()
    {
        // Default to making this kill me
        return true;
    }

    public void AddRipple(CycleRippleSource source)
    {
        RippleRing ripple = null;
        Vector2 pos = RealizedOwner.bodyChunks[0].pos;
        int life = Random.Range(20, Random.Range(20, 60));
        float intensity = Random.Range(0.1f, Random.Range(0.1f, 1f));

        if (source == CycleRippleSource.Thanatosis)
        {
            life = 80;
            intensity = Random.Range(0.6f, Random.Range(0.6f, 1f));
        }
        // must calculate speed after determining intensity
        float speed = intensity * (life / 20);

        ripple = new RippleRing(pos, life, intensity, speed);
        if (ripple != null && RealizedOwner != null && RealizedOwner.room != null)
        {
            RealizedOwner.room.AddObject(ripple);
            RealizedOwner.room.AddObject(new ShockWave(pos, 0.15f * (intensity / 2f), intensity, life, true));
        }
        if (devMode && RealizedOwner.room.updateList.Contains(ripple))
        {
            logger.LogDebug($"Spawned Idle ripple for {abstractOwner.creatureTemplate.type.value} - {life}, {intensity}, {speed}");
        }
        return;
    }

    public void CycleTick()
    {
        cycleTime.Tick();
        cycleStateTime.Tick();
    }

    public void ChangeState(State newState)
    {
        state = newState;
        cycleStateTime.Reset();
    }

    public void Sync()
    {
        // We'll add more later
        ChangeState(State.Alive);
    }
}
