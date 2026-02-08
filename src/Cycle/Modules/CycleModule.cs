using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public class CycleModule
{
    public Cycle cycle;
    public CycleModule(Cycle cycle)
    {
        this.cycle = cycle;
    }

    public virtual void Update()
    {

    }
}
