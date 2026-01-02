using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public class DreamerBehavior : PBEntity.BehaviorModule
{
    public DreamerEntity Dreamer => owner as DreamerEntity;

    public DreamerBehavior(DreamerEntity owner) : base(owner)
    {
        this.owner = owner;
    }
}
