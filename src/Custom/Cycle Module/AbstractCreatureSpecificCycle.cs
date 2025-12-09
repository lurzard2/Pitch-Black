namespace PitchBlack;

public abstract class AbstractCreatureSpecificCycle
{
    public AbstractCreature abstractOwner;

    // Representation of abstract creature in room
    public Creature realizedOwner
    {
        get
        {
            return abstractOwner.realizedCreature;
        }
    }
    public State state;

    public AbstractCreatureSpecificCycle(AbstractCreature abstractOwner)
    {
        this.abstractOwner = abstractOwner;
    }

    public void ChangeState(State state)
    {
        this.state = state;
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
}