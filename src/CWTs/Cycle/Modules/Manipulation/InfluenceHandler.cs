namespace PitchBlack;

public class InfluenceHandler : CycleModule
{
    public bool IsThisMe(AbstractCreature absCrit) => absCrit == cycle.abstractOwner;
    public bool TooManyCreaturesAvailable => cycle.abstractOwner.Room.creatures.Count > 10;
    public float radiusForInfluence = 40f;

    public InfluenceHandler(Cycle cycle) : base(cycle) { }

    public override void Abstract() => base.Abstract();

    public override void Realized() => base.Realized();

    public virtual void BeingInfluenced(Cycle reference) { }
}