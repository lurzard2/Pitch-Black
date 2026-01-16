using RWCustom;
using System;
using System.Collections.Generic;
using static PitchBlack.Plugin;
using UnityEngine;
using Random = UnityEngine.Random;
using Watcher;
using BepInEx.Bootstrap;

namespace PitchBlack;

public class Cycle
{
    public AbstractCreature abstractOwner;
    public Creature RealizedOwner => abstractOwner.realizedCreature;
    public CreatureTemplate.Type CycleCreatureTemplateType => abstractOwner.creatureTemplate.type;

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
    private Counter rippleCooldown = new(20, 0, false);

    public class CycleRippleSource : ExtEnum<CycleRippleSource>
    {
        public CycleRippleSource(string value, bool register) : base(value, register) { }

        public static readonly CycleRippleSource Idle = new(nameof(Idle), true);
        public static readonly CycleRippleSource Thanatosis = new(nameof(Thanatosis), true);
        public static readonly CycleRippleSource Cache = new(nameof(Cache), true);
    }
    private int GetRippleSpawnLimitFromCreature()
    {
        // Hopefully reducing lag from coalescipedes
        if (CycleCreatureTemplateType == CreatureTemplate.Type.Spider)
        {
            return 3;
        }
        return 15;
    }

    public Counter cycleTime = new(Int32.MaxValue, 0, true);
    public Counter cycleStateTime = new(Int32.MaxValue, 0, true);

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

        #region Idle Ripples
        if (Random.value < 0.1f && idleRipplesToSpawn <= GetRippleSpawnLimitFromCreature())
        {
            idleRipplesToSpawn++;
        }

        spawnedPendingRipples = Random.value < 0.0008f;

        // Clear if unable to spawn in room
        if (spawnedPendingRipples && RealizedOwner == null)
        {
            for (int i = 0; i < idleRipplesToSpawn; i++)
            {
                idleRipplesToSpawn--;
            }
        }
        #endregion

        if (state == State.MarkedForCache)
        {
            switch (cycleStateTime)
            {
                case 1:
                    MarkForCache();
                    break;
                case 80:
                    ChangeState(State.Cached);
                    break;
                default: break;
            }
        }
        else if (abstractOwner.state.dead && state == State.Alive)
        {
            ChangeState(State.MarkedForCache);
        }
    }

    // Front end
    public virtual void RealizedUpdate()
    {
        // Spawn ripples with a half-second cooldown
        if (spawnedPendingRipples && rippleCooldown.isFinished)
        {
            for (int i = 0; i < idleRipplesToSpawn; i++)
            {
                AddRipple(CycleRippleSource.Idle);
                idleRipplesToSpawn--;
                rippleCooldown.Reset();
            }
        }
        else
        {
            rippleCooldown.Tick();
        }
    }

    private void MarkForCache()
    {
        
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

    public bool TimeInState(State stateToCheck, float timeToCheck)
    {
        if (state == stateToCheck && cycleStateTime == timeToCheck)
        {
            return true;
        }
        return false;
    }

    public void Sync()
    {
        // We only have to check if it's alive, cause this is when the abstractcreature is first updated, otherwise it is guaranteed dead
        if (abstractOwner.state.alive)
        {
            ChangeState(State.Alive);
            return;
        }
        ChangeState(State.Cached);
    }
}
