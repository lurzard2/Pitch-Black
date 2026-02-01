using RWCustom;
using System;
using System.Collections.Generic;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class Cycle
{
    public AbstractCreature abstractOwner;
    public AbstractRoom AbstractRoom => abstractOwner.Room;
    public CreatureTemplate.Type CycleCreatureTemplateType => abstractOwner.creatureTemplate.type;
    public bool IsSlugcatType => abstractOwner.creatureTemplate.type == CreatureTemplate.Type.Slugcat;
    public List<AbstractCreature> CreaturesInRoom => abstractOwner.Room.creatures;

    // Can be null when abstract
    public Creature RealizedOwner => abstractOwner.realizedCreature;
    public bool Realized => RealizedOwner is not null;

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

    #region Modules
    public List<CycleModule> modules = [];
    // Holds and manages "spacial effects" like ripple
    public SpacialTracker spacialTracker {  get; set; }
    // Idle ripplering spawner
    public IdleRippleHandler idleRippleHandler { get; set; }
    // Controls dying, optionally overriden
    public DeathHandler deathHandler { get; set; }
    #endregion

    public Cycle(AbstractCreature abstractOwner)
    {
        this.abstractOwner = abstractOwner;
        state = State.Init;

        spacialTracker = new(this);
        modules.Add(spacialTracker);
        idleRippleHandler = new(this);
        modules.Add(idleRippleHandler);
        deathHandler = new(this);
        modules.Add(deathHandler);
    }

    public virtual void Update()
    {
        if (state == State.Init)
        {
            Sync();
            return;
        }

        CycleTick();

        foreach (var mod in modules)
        {
            mod.Abstract();
            if (Realized)
            {
                mod.Realized();
            }
        }
    }

    public void ChangeState(State newState)
    {
        state = newState;
        cycleStateTime.Reset();
    }

    public void CycleTick()
    {
        cycleTime.Tick();
        cycleStateTime.Tick();
    }

    public void OnRealize()
    {
        // We have to wait until player exists, to then create their cycle, this will also result in a key transfer within the object
        // Instead, we'll leave it up to BeaconCWT to handle, since player exists there
    }

    private void Sync()
    {
        // Will include functionality for setting other states later
        ChangeState(State.Alive);
    }
}
