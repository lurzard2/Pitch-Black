using RWCustom;
using System;
using System.Collections.Generic;
using static PitchBlack.Plugin;
using UnityEngine;
using Random = UnityEngine.Random;
using Watcher;
using IL.MoreSlugcats;

namespace PitchBlack;

public class Cycle
{
    public AbstractCreature abstractOwner;
    public CreatureTemplate.Type CycleCreatureTemplateType => abstractOwner.creatureTemplate.type;

    // Can be null when abstract
    public Creature RealizedOwner => abstractOwner.realizedCreature;

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

    public Counter cycleTime = new(Int32.MaxValue, 0, true);
    public Counter cycleStateTime = new(Int32.MaxValue, 0, true);

    public List<CycleModule> modules = [];
    public IdleRippleTracker idleRippleTracker {  get; set; }
    public SpacialTracker spacialTracker {  get; set; }
    public ManipulationTracker manipulationTracker { get; set; }
    public ManipulationModule Manipulator { get; set; }

    public Cycle(AbstractCreature abstractOwner)
    {
        this.abstractOwner = abstractOwner;
        state = State.Init;

        spacialTracker = new(this);
        modules.Add(spacialTracker);
        idleRippleTracker = new(this);
        modules.Add(idleRippleTracker);
        manipulationTracker = new(this);
        modules.Add(manipulationTracker);
    }

    // Back end
    public virtual void AbstractUpdate()
    {
        if (state == State.Init)
        {
            ChangeState(State.Alive);
            return;
        }

        CycleTick();

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

    public void CycleTick()
    {
        cycleTime.Tick();
        cycleStateTime.Tick();
        if (RealizedOwner != null)
        {
            RealizedUpdate();
            foreach (CycleModule mod in modules)
            {
                mod.Update();
            }
        }
    }

    // Front end
    public void RealizedUpdate()
    {
        if (Manipulator == null)
        {
            if (RealizedOwner is Player player && MiscUtils.IsBeacon(player))
            {
                Manipulator = new BeaconManipulator(this, player);
            }
            modules.Add(Manipulator);
        }
    }

    private void MarkForCache()
    {
        
    }

    public void ChangeState(State newState)
    {
        state = newState;
        cycleStateTime.Reset();
    }
}
