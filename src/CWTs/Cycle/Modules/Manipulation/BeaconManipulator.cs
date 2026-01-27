using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public class BeaconManipulator : ManipulationModule, IManipulator
{
    public Player Beacon { get; set; }

    public BeaconManipulator(Cycle cycle, Player player) : base(cycle)
    {
        Beacon = player;
    }

    public override void Update()
    {

    }

    public override void BeingManipulated(Cycle reference)
    {

    }

    public void Act()
    {
        throw new NotImplementedException();
    }

    public Cycle CycleToManipulate(Cycle targetCycle)
    {
        throw new NotImplementedException();
    }

    public void ManipulateOther(Cycle target)
    {
        throw new NotImplementedException();
    }
}
