using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public class DeathHandler : CycleModule
{
    public DeathHandler(Cycle cycle) : base(cycle) { }

    public virtual void AbstractDie()
    {
        cycle.abstractOwner.Die();
    }

    public virtual void RealizedDie()
    {
        cycle.RealizedOwner.Die();
    }

    public virtual void PlayerDie()
    {
        (cycle.RealizedOwner as Player).Die();
    }
}
