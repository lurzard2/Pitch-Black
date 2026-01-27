using UnityEngine;
using System.Collections.Generic;

namespace PitchBlack;

public interface IManipulator
{
    Cycle CycleToManipulate(Cycle targetCycle);
    void Act();
    void ManipulateOther(Cycle target);
}
