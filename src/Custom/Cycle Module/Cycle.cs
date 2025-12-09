using RWCustom;
using static PitchBlack.Plugin;

namespace PitchBlack;

// - TODO -
// - Caching implementation
// - Cachespace
// - Thanatosis migration
// - Cosmetic visuals
// - (More later)

public class Cycle : AbstractCreatureSpecificCycle
{

    public Cycle(AbstractCreature creature) : base(creature)
    {
        state = State.Init;
    }

    // State tracking and determining
    public void AbstractUpdate()
    {
        if (state == State.Init)
        {
            Sync();
        }



        if (realizedOwner != null)
        {
            RealizedUpdate();
        }
    }
    
    public void Sync()
    {
        if (abstractOwner.state.alive)
        {
            ChangeState(State.Alive);
            return;
        }
        ChangeState(State.Cached);
    }

    // In-room features based on state
    public void RealizedUpdate()
    {
        // This just stuns literally every realized creature (including the player) in the game LOL I'm so proud
        //realizedOwner.Stun(30000);
    }
}
