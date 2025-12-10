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

        if (Random.value < 0.1f)
        {
            idleRipples++;
        }
    }

    // In-room features based on state
    public virtual void RealizedUpdate()
    {
        if (Random.value < 0.1f && idleRipples > 0)
        {
            for (int i = 0; i < idleRipples; i++)
            {
                if (state == State.MarkedForCache || state == State.Cached)
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
    }

    public void AddRipple(CycleRippleSource source)
    {
        RippleRing ripple = null;
        Vector2 pos = RealizedOwner.bodyChunks[0].pos;
        // Life
        int life = 0;
        if (source == CycleRippleSource.Cache)
        {
            Random.Range(-40, Random.Range(-40, -120));
        }
        else
        {
            life = Random.Range(40, Random.Range(40, 120));
        }
        // Intensity
        float intensity = 0;
        if (source == CycleRippleSource.Idle)
        {
            intensity = Random.Range(0.1f, Random.Range(0.1f, 1f));
            RealizedOwner.room.PlaySound(SoundID.MENU_Karma_Ladder_Increase_Bump, pos, intensity - 0.85f, life);
        }
        else if (source == CycleRippleSource.Thanatosis)
        {
            intensity = 1f;
        }
        else if (source == CycleRippleSource.Cache)
        {
            intensity = 0.8f;
        }
        float speed = intensity * (life / 20);

        ripple = new RippleRing(pos, life, intensity, speed);
        if (ripple != null)
        {
            RealizedOwner.room.AddObject(ripple);
        }
        if (devMode && RealizedOwner.room.updateList.Contains(ripple))
        {
            logger.LogDebug($"Spawned Idle ripple for {abstractOwner.creatureTemplate.type.value} - {life}, {intensity}, {speed}");
        }
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
        public static readonly CycleRippleSource Oscillation = new(nameof(Oscillation), true);
        public static readonly CycleRippleSource Cache = new(nameof(Cache), true);
    }
}
