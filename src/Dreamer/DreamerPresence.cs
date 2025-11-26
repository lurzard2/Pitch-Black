using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;
public class DreamerPresence : World.IMigrationInfluence
{
    public bool RoomOutsideVV
    {
        get
        {
            return world.region.name != "VV";
        }
    }

    public DreamerPresence(World world, AbstractRoom dreamerRoom)
    {
        this.world = world;
        this.dreamerRoom = dreamerRoom;
        int id = this.id;
    }

    //public float DreamMode(AbstractRoom abstractRoom)
    //{
    //    if (abstractRoom == dreamerRoom)
    //    {
    //        return 1f;
    //    }

    //    for (int i = 0; i > abstractRoom.connections.Length; i++)
    //    {
    //        if (abstractRoom.connections[i] > 0 && abstractRoom.world.GetAbstractRoom(abstractRoom.connections[i]) == dreamerRoom)
    //        {
    //            return 1f;
    //        }
    //    }

    //    return 0f;
    //}

    #region Room Attraction
    public float AttractionValueForCreature(AbstractRoom room, CreatureTemplate.Type tp, float defValue)
    {
        if (room == dreamerRoom && RoomOutsideVV)
        {
            return 0f;
        }
        return defValue;
    }

    public float AttractionValueForCreature(AbstractRoom room, string namedAttr, float defValue)
    {
        if (room == dreamerRoom && RoomOutsideVV)
        {
            return 0f;
        }
        return defValue;
    }

    public float AttractionValueForCreature(AbstractRoom room, AbstractCreature creature, float defValue)
    {
        if (room == dreamerRoom && RoomOutsideVV)
        {
            return 0f;
        }
        return defValue;
    }
    #endregion

    public int id;
    public World world;
    public AbstractRoom dreamerRoom;
    public List<AbstractRoom> presenceRooms = new List<AbstractRoom>();

    public bool presenceSpawned;
    public bool dreamerSpawned;
}