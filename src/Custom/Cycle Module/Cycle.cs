using RWCustom;
using System;
using System.Collections.Generic;
using static PitchBlack.Plugin;
using UnityEngine;
using Random = UnityEngine.Random;
using Watcher;

namespace PitchBlack;

// - TODO -
// - Caching implementation
// - Cachespace
// - Thanatosis migration
// - Cosmetic visuals
// - (More later)

public class Cycle
{
    public AbstractCreature abstractOwner;

    // Representation of abstract creature in room
    public Creature RealizedOwner
    {
        get
        {
            return abstractOwner.realizedCreature;
        }
    }

    public State state;
    // Time existing
    public Counter cycleTime = new(Int32.MaxValue, 0, true);
    // Time per state
    public Counter cycleStateTime = new(Int32.MaxValue, 0, true);
    public bool active => cycleTime > 0;
    public int idleRipples;
    private bool spawnRipples;

    public Cycle(AbstractCreature abstractOwner)
    {
        this.abstractOwner = abstractOwner;
        state = State.Init;
    }

    // State tracking and determining
    public virtual void AbstractUpdate()
    {
        if (state == State.Init)
        {
            Sync();
            return;
        }
        else
        {
            CycleTick();
        }

        if (abstractOwner.state.dead)
        {
            ChangeState(State.MarkedForCache);
        }

        if (Random.value < 0.5f)
        {
            idleRipples++;
        }
    }

    // In-room features based on state
    public virtual void RealizedUpdate()
    {
        if (state == State.MarkedForCache)
        {
            AddRipple(CycleRippleSource.Cache);
            ChangeState(State.Cached);
        }

        if (Random.value < 0.0003f && !spawnRipples)
        {
            // I just rippled everywhere
            spawnRipples = true;
        }

        if (idleRipples > 0 && spawnRipples)
        {
            for (int i = 0; i < idleRipples; i++)
            {
                if (state == State.Cached)
                {
                    AddRipple(CycleRippleSource.Cache);
                }
                else 
                {
                    AddRipple(CycleRippleSource.Idle);
                }
                idleRipples--;
            }
        }
        if (idleRipples == 0)
        {
            spawnRipples = false;
        }
    }

    public void AddRipple(CycleRippleSource source)
    {
        RippleRing ripple = null;
        Vector2 pos = RealizedOwner.bodyChunks[0].pos;
        int life = Random.Range(20, Random.Range(20, 60));
        float intensity = 0;

        if (source == CycleRippleSource.Idle)
        {
            intensity = Random.Range(0.1f, Random.Range(0.1f, 1f));
        }
        else if (source == CycleRippleSource.Thanatosis)
        {
            intensity = 0.9f;
            life = 80;
        }
        else if (source == CycleRippleSource.Cache)
        {
            life = Random.Range(-40, Random.Range(-40, -80));
            intensity = -0.9f;
        }
        float speed = intensity * (life / 20);

        ripple = new RippleRing(pos, life, intensity, speed);
        if (ripple != null && RealizedOwner != null && RealizedOwner.room != null)
        {
            RealizedOwner.room.AddObject(ripple);
            RealizedOwner.room.AddObject(new ShockWave(pos, 0.15f * (intensity / 2f), intensity, life, true));
            RealizedOwner.room.PlaySound(SoundID.Small_Object_Into_Water_Slow, pos, intensity - 0.5f, intensity - 0.2f);
        }
        //if (devMode && RealizedOwner.room.updateList.Contains(ripple))
        //{
        //    logger.LogDebug($"Spawned Idle ripple for {abstractOwner.creatureTemplate.type.value} - {life}, {intensity}, {speed}");
        //}
    }

    #region Internal
    public void CycleTick()
    {
        cycleTime.Tick();
        cycleStateTime.Tick();
    }

    public void Sync()
    {
        // We only have to check if it's alive, cause this is when the abstractcreature is first updated
        if (abstractOwner.state.alive)
        {
            ChangeState(State.Alive);
            return;
        }
        // Otherwise, it's dead
        ChangeState(State.Cached);
    }
    #endregion

    #region State
    public void ChangeState(State newState)
    {
        state = newState;
        cycleStateTime.Reset();
    }

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
    #endregion

    public class CycleRippleSource : ExtEnum<CycleRippleSource>
    {
        public CycleRippleSource(string value, bool register) : base(value, register) { }

        public static readonly CycleRippleSource Idle = new(nameof(Idle), true);
        public static readonly CycleRippleSource Thanatosis = new(nameof(Thanatosis), true);
        public static readonly CycleRippleSource Cache = new(nameof(Cache), true);
    }
}
