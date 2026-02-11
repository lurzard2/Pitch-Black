using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public class CreatureCycle : Cycle
{
    public CreatureCycle(AbstractCreature creature) : base(creature)
    {
        spacialTracker = new(this);
        modules.Add(spacialTracker);
        idleRippleTracker = new(this);
        modules.Add(idleRippleTracker);
    }
}
