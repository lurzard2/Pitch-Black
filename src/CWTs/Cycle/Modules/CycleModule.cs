using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public abstract class CycleModule
{
    public Cycle cycle;
    public SaveState SaveState => cycle.abstractOwner.world.game.GetSaveState();

    public CycleModule(Cycle cycle)
    {
        this.cycle = cycle;
    }

    public virtual void Abstract() { }
    public virtual void Realized() { }
}
