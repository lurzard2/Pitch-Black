using System;
using RWCustom;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class BeaconCycle : Cycle
{
    public Cycle cycle;
    public Player owner;
    public SaveState SaveState => owner.room.world.game.GetStorySession.saveState;

    public BeaconCycle(Cycle cycle, Player owner) : base(owner.abstractCreature)
    {
        this.cycle = cycle;
        this.owner = owner;
    }

    public override void AbstractUpdate()
    {
        base.AbstractUpdate();
    }

    public override void RealizedUpdate()
    {
        base.RealizedUpdate();
    }
}
