using Watcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlTypes;

namespace PitchBlack;

public class RiftManager : UpdatableAndDeletable
{
    public PlacedObject placedObj;

    public List<CreatureTemplate.Type> whitelistedCreatureTypes;
    public List<AbstractPhysicalObject.AbstractObjectType> whitelistedObjectTypes;

    public Rift placedRift;
    bool jobDone;
    public bool selfSufficient;

    public RiftManager(Room room, PlacedObject placedObj, bool selfSufficient)
    {
        this.room = room;
        this.placedObj = placedObj;
        this.selfSufficient = selfSufficient;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (selfSufficient)
        {
            placedRift = selfSufficient ? GenerateRift() : null;
            if (placedRift != null && !jobDone)
            {
                room.AddObject(placedRift);
                jobDone = true;
            }
        }
        else
        {
            if (placedRift != null)
            {
                room.AddObject(placedRift);
                jobDone = true;
            }
        }
        if (jobDone)
        {
            Destroy();
        }
    }

    // For PlacedObject
    public Rift GenerateRift()
    {
        Rift rift = new(room, placedObj);

        WarpPoint.WarpPointData warpPointData = placedObj.data as WarpPoint.WarpPointData;
        string key = WarpPoint.IdentifyingString(room.game, warpPointData, room.abstractRoom);

        // Find valid destRoom in a current warp in room to fall out
        foreach (WarpPoint warpPoint in room.warpPoints)
        {
            if (warpPoint.MyIdentifyingString() == key)
            {
                string destRoom = warpPoint.Data.destRoom;
                string accessDestRoom = (destRoom != null) ? destRoom.ToLowerInvariant() : null;
                string validDestRoom = warpPointData.destRoom;
                if (accessDestRoom == ((validDestRoom != null) ? validDestRoom.ToLowerInvariant() : null))
                {
                    return warpPoint as Rift;
                }
            }
            string destRoom2 = warpPoint.Data.destRoom;
            string accessDestRoom2 = (destRoom2 != null) ? destRoom2.ToLowerInvariant() : null;
            string validDestRoom2 = warpPointData.destRoom;
            if (accessDestRoom2 == ((validDestRoom2 != null) ? validDestRoom2.ToLowerInvariant() : null))
            {
                return warpPoint as Rift;
            }
        }

        // Check objects
        if ((warpPointData.nonDynamicWarpPoint)
            && warpPointData.destRoom != null
            && !warpPointData.rippleWarp
            && !warpPointData.oneWayExit
            && !warpPointData.UpToDateWithIndexMaps(room.abstractRoom.name.ToLowerInvariant()))
        {
            bool flag = false;
            for (int i = 0; i < room.roomSettings.placedObjects.Count; i++)
            {
                var roomObj = room.roomSettings.placedObjects[i];
                if (roomObj.type == Enums.PlacedObjectType.RiftSpot
                    || roomObj.type == Enums.PlacedObjectType.DreamerSpot
                    || roomObj.type == Enums.PlacedObjectType.StillbornSpot)
                {
                    if ((roomObj.data is WarpPoint.WarpPointData rData && rData.destRoom == warpPointData.destRoom)
                        || (roomObj.data is EntityWarpData dData && dData.destRoom == warpPointData.destRoom))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                return null;
            }
        }

        return rift;
    }

    // For Code Spawning, self-ufficient warp generating and place
    public Rift ScriptedRift(SlugcatStats.Timeline newTimeline, string newRegion, string newRoom)
    {
        Rift rift = GenerateRift();
        rift.Data.destTimeline = newTimeline;
        rift.Data.destRegion = newRegion;
        rift.Data.destRoom = newRoom;

        // Find the pos from an object in the destRoom
        bool foundExit = false;
        foreach (PlacedObject placedObject in new RoomSettings(newRoom, null, false, false, room.world.game.TimelinePoint, room.world.game).placedObjects)
        {
            if (placedObject.type == Enums.PlacedObjectType.RiftExitTarget)
            {
                rift.Data.destPos = placedObject.pos;
                foundExit = true;
                break;
            }
        }
        if (!foundExit)
        {
            rift.Data.destPos = UnityEngine.Vector2.zero;
        }

        rift.Data.destCam = WarpPoint.GetDestCam(rift.Data);

        return rift;
    }
}
