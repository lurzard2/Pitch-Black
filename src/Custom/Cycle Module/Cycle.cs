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
    public string region => abstractOwner.world.region.name;
    public bool regionIsOutsideCycle => region == "VV" || region == "UD";

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

        if (Random.value < 0.1f && !regionIsOutsideCycle)
        {
            idleRipples++;
        }
    }

    // In-room features based on state
    public virtual void RealizedUpdate()
    {
        if (Random.value < 0.1f && idleRipples > 0 && !regionIsOutsideCycle)
        {
            for (int i = 0; i < idleRipples; i++)
            {
                AddRipple();
                idleRipples--;
            }
        }
    }

    private void AddRipple()
    {
        RippleRing ripple = null;
        Vector2 pos = RealizedOwner.bodyChunks[0].pos;
        int life = state == State.Alive ? Random.Range(40, Random.Range(40, 120)) : Random.Range(-40, Random.Range(-40, -120));
        float intensity = Random.Range(0.1f, Random.Range(0.1f, 1f));
        float speed = intensity * (life / 20);

        // I was gonna stop it from spawning invisible ones but I like the implication
        //if (intensity < 0.6f)
        //{
        //    return;
        //}

        ripple = new RippleRing(pos, life, intensity, speed);
        if (ripple != null)
        {
            RealizedOwner.room.AddObject(ripple);
            if (intensity > 0.7f)
            {
                RealizedOwner.room.PlaySound(SoundID.MENU_Karma_Ladder_Increase_Bump, pos, intensity - 0.75f, life);
                RealizedOwner.room.PlaySound(WatcherEnums.WatcherSoundID.Warp_Point_Ripple_Hint, pos, intensity - 0.75f, life);
            }
        }
        if (devMode && RealizedOwner.room.updateList.Contains(ripple))
        {
            logger.LogDebug($"Spawned Idle ripple for {abstractOwner.creatureTemplate.type.value} - {life}, {intensity}, {speed}");
        }
    }

    #region Internal
    private void CycleTick()
    {
        cycleTime.Tick();
        cycleStateTime.Tick();
    }

    private void Sync()
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
        public static readonly State MarkedForCache = new(nameof(MarkedForCache), true);
        public static readonly State Cached = new(nameof(Cached), true);
    }
    #endregion
}
