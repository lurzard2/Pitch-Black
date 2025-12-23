using IL.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.PlayerLoop;

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
        // Assign a barebones instance, which gets re-instantiated when Dreamer gets added to a room
        myDreamer = new(dreamerRoom, false, null);
        Plugin.logger.LogDebug($"DreamerSpawner: ROOM:{myDreamer.abstractRoom.name} - {myDreamer.hasSpawned} - {myDreamer.obj}");
    }

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

    public bool presenceSpawned;
    public bool dreamerSpawned;
    public string songName = "PB_Dreamcatcher";

    // Defined Tuple for Dreamer containing its abstractRoom, spawn bool, and object, now a class
    //public Tuple<AbstractRoom, bool, Dreamer> dreamer;
    public MyDreamer myDreamer;

    public class MyDreamer
    {
        public AbstractRoom abstractRoom;   
        public bool hasSpawned;
        public Dreamer obj;
        public MyDreamer(AbstractRoom abstractRoom, bool hasSpawned, Dreamer obj)
        {
            this.abstractRoom = abstractRoom;
            this.hasSpawned = hasSpawned;
            this.obj = obj;
        }
    }
}