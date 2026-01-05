using RWCustom;
using System;
using System.Collections.Generic;
using static PitchBlack.Plugin;
using UnityEngine;
using Random = UnityEngine.Random;
using Watcher;

namespace PitchBlack;

public class Cycle
{
    public AbstractCreature abstractOwner;
    public Creature RealizedOwner => abstractOwner.realizedCreature;

    public State state;
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

    public int idleRipplesToSpawn;
    public bool spawnRipples;
    public class CycleRippleSource : ExtEnum<CycleRippleSource>
    {
        public CycleRippleSource(string value, bool register) : base(value, register) { }

        public static readonly CycleRippleSource Idle = new(nameof(Idle), true);
        public static readonly CycleRippleSource Thanatosis = new(nameof(Thanatosis), true);
        public static readonly CycleRippleSource Cache = new(nameof(Cache), true);
    }

    // Time existing
    public Counter cycleTime = new(Int32.MaxValue, 0, true);
    // Time per state
    public Counter cycleStateTime = new(Int32.MaxValue, 0, true);
    public bool active => cycleTime > 0;

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

        // Mark for cache without ovewriting, on death
        if (abstractOwner.state.dead
            && state != State.MarkedForCache
            && state != State.Cached)
        {
            ChangeState(State.MarkedForCache);
            // Skip to end of process if abstract
            if (RealizedOwner == null)
            {
                ChangeState(State.Cached);
            }
        }
        
        // Dying anim done
        if (state == State.MarkedForCache && cycleStateTime == 80)
        {
            ChangeState(State.Cached);
        }

        if (Random.value < 0.5f && idleRipplesToSpawn <= 15)
        {
            idleRipplesToSpawn++;
        }
    }

    // In-room features based on state
    public virtual void RealizedUpdate()
    {
        if (state == State.MarkedForCache)
        {
            AddRipple(CycleRippleSource.Cache);
        }

        if (Random.value < 0.0003f && !spawnRipples)
        {
            // I just rippled everywhere
            spawnRipples = true;
        }

        if (idleRipplesToSpawn > 0 && spawnRipples)
        {
            for (int i = 0; i < idleRipplesToSpawn; i++)
            {
                if (state == State.Cached)
                {
                    AddRipple(CycleRippleSource.Cache);
                }
                else 
                {
                    AddRipple(CycleRippleSource.Idle);
                }
                idleRipplesToSpawn--;
            }
        }
        if (idleRipplesToSpawn == 0)
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
            life = 80;
            intensity = Random.Range(0.6f, Random.Range(0.6f, 1f));
        }
        else if (source == CycleRippleSource.Cache)
        {
            if (state == State.MarkedForCache)
            {
                life = 60;
                life -= cycleStateTime;
                intensity = Random.Range(0.4f, Random.Range(0.4f, 1f));
            }
            else
            {
                intensity = Random.Range(0.2f, Random.Range(0.2f, 1f));
            }
        }
        float speed = intensity * (life / 20);

        ripple = new RippleRing(pos, life, intensity, speed);
        if (ripple != null && RealizedOwner != null && RealizedOwner.room != null)
        {
            RealizedOwner.room.AddObject(ripple);
            RealizedOwner.room.AddObject(new ShockWave(pos, 0.15f * (intensity / 2f), intensity, life, true));
            // We need a better sound
            //RealizedOwner.room.PlaySound(SoundID.Small_Object_Into_Water_Slow, pos, intensity - 0.5f, intensity - 0.2f);
        }
        //if (devMode && RealizedOwner.room.updateList.Contains(ripple))
        //{
        //    logger.LogDebug($"Spawned Idle ripple for {abstractOwner.creatureTemplate.type.value} - {life}, {intensity}, {speed}");
        //}
        return;
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
}
