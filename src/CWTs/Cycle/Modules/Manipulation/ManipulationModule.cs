using IL.RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public class ManipulationModule : CycleModule
{
    public bool TooManyCreaturesAvailable => cycle.abstractOwner.Room.creatures.Count > 10;
    public float radiusForInfluence = 40f;

    public ManipulationModule(Cycle cycle) : base(cycle) { }

    public virtual void BeingManipulated(Cycle reference) { }
}