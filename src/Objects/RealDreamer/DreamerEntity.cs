using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public class DreamerEntity : PBEntity
{
    public PlacedObject placedObject;
    public DreamerData SpecialData => placedObject.data as DreamerData;

    public DreamerEntity(Room room, PlacedObject placedObject) : base(room, placedObject)
    {
        this.placedObject = placedObject;
        visibleEntity = new DreamerGraphics(this);
        room.AddObject(visibleEntity);
        behaviorModule = new DreamerBehavior(this);
    }
}
