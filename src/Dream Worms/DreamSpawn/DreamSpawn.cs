using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public class DreamSpawn : VoidSpawn
{
    public DreamSpawn(AbstractPhysicalObject physicalObject, float voidMeltInRoom, bool dayLightMode, SpawnType variant) : base(physicalObject, voidMeltInRoom, dayLightMode, variant)
    {
    }
}
