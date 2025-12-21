using System;
using System.Collections.Generic;
using UnityEngine;
using Watcher;
using static PitchBlack.Plugin;

namespace PitchBlack;

public static class WorldLoaderHooks
{
    private static void LogRooms(WorldLoader self)
    {
        // List of rooms I store
        List<AbstractRoom> listOfRoomsForDreamer = new List<AbstractRoom>();
        List<string> listOfRoomNames = new List<string>();

        // Checking rooms in the current world
        for (int i = 0; i < self.abstractRooms.Count; i++)
        {
            // We need room to add RoomSettings here
            Room room = new Room(null, self.world, self.abstractRooms[i], false);
            RoomSettings roomSettings = new RoomSettings(room, WorldLoader.RoomNameManipulator(room.abstractRoom.FileName, self.game), self.world.region, false, false, self.game?.TimelinePoint, self.game);
            // Actually check the objects in those rooms, to then add rooms with our object to a list
            for (int j = 0; j < roomSettings.placedObjects.Count; j++)
            {
                if (roomSettings.placedObjects[j].type == Enums.PlacedObjectType.DreamerSpot)
                {
                    listOfRoomsForDreamer.Add(self.abstractRooms[i]);
                    listOfRoomNames.Add(self.abstractRooms[i].name);
                    break;
                }
            }
        }
        // Once the loop is finished, add every room from the list to the CWT
        roomsWithDreamerSpot.Add(self.world, listOfRoomsForDreamer);
        DreamerPresence_Functions.timesToAssignDreamerPresence = listOfRoomsForDreamer.Count;
        string joinedRoomNameString = String.Join(",", listOfRoomNames);
        logger.LogDebug($"DreamerRooms: DreamerSpot rooms in world - {joinedRoomNameString}");
    }

    public static void Inject()
    {
        On.WorldLoader.CreatingWorld += WorldLoader_CreatingWorld;
        On.Room.TrySpawnWarpPoint_PlacedObject_bool += CheckSpawnWarpForDreamer;
    }

    private static WarpPoint CheckSpawnWarpForDreamer(On.Room.orig_TrySpawnWarpPoint_PlacedObject_bool orig, Room self, PlacedObject po, bool saveInRegionState)
    {
        WarpPoint.WarpPointData warpPointData = po.data as WarpPoint.WarpPointData;
        string text = WarpPoint.IdentifyingString(self.game, warpPointData, self.abstractRoom);
        foreach (WarpPoint warpPoint in self.warpPoints)
        {
            if (warpPoint.MyIdentifyingString() == text && warpPoint.Data.destRoom?.ToLowerInvariant() == warpPointData.destRoom?.ToLowerInvariant())
            {
                return warpPoint;
            }

            if (warpPoint.Data.destRoom?.ToLowerInvariant() == warpPointData.destRoom?.ToLowerInvariant())
            {
                return warpPoint;
            }
        }

        if ((warpPointData.nonDynamicWarpPoint || warpPointData.wasNonDynamicWarpBeforeWeaverTriggered) && warpPointData.destRoom != null && !warpPointData.rippleWarp && !warpPointData.oneWayExit && !warpPointData.UpToDateWithIndexMaps(self.abstractRoom.name))
        {
            bool flag = false;
            for (int i = 0; i < self.roomSettings.placedObjects.Count; i++)
            {
                if (self.roomSettings.placedObjects[i].type == PlacedObject.Type.WarpPoint && (self.roomSettings.placedObjects[i].data as WarpPoint.WarpPointData).destRoom == warpPointData.destRoom)
                {
                    flag = true;
                    break;
                }

                if (self.roomSettings.placedObjects[i].type == WatcherEnums.PlacedObjectType.SpinningTopSpot && (self.roomSettings.placedObjects[i].data as SpinningTopData).destRoom == warpPointData.destRoom)
                {
                    flag = true;
                    break;
                }
                // Inject DreamerSpot check
                bool isDreamerSpot = self.roomSettings.placedObjects[i].type == Enums.PlacedObjectType.DreamerSpot && (self.roomSettings.placedObjects[i].data as DreamerData).destRoom == warpPointData?.destRoom;
                if (isDreamerSpot)
                {
                    flag = true;
                }
            }

            if (!flag)
            {
                return null;
            }
        }

        return self.ForceSpawnWarpPoint(po, saveInRegionState);
    }

    /// <summary>
    /// Tracking Dreamer presence rooms dynamically based on if the roomsettings of a room contains their placed object type
    /// </summary>
    private static void WorldLoader_CreatingWorld(On.WorldLoader.orig_CreatingWorld orig, WorldLoader self)
    {
        orig(self);
        LogRooms(self);
    }
}

public static class WorldHooks
{

    public static void Apply()
    {
        WorldLoaderHooks.Inject();

        On.Region.ctor_string_int_int_RainWorldGame_Timeline += ModifyRegionProperties;
        On.Region.HasWarpFatigueResistance += ModifyHasWarpFatigueResistence;
    }

    private static bool ModifyHasWarpFatigueResistence(On.Region.orig_HasWarpFatigueResistance orig, string name)
    {
        return Region.IsAncientUrbanRegion(name) || Region.IsDaemonRegion(name) || MiscUtils.IsVhosRegion(name);
    }

    // Replace rot eye+effect color for Beacon
    private static void ModifyRegionProperties(On.Region.orig_ctor_string_int_int_RainWorldGame_Timeline orig, Region self, string name, int firstRoomIndex, int regionNumber, RainWorldGame game, SlugcatStats.Timeline timelineIndex)
    {
        orig(self, name, firstRoomIndex, regionNumber, game, timelineIndex);

        if (MiscUtils.IsBeacon(timelineIndex))
        {
            self.regionParams.corruptionEffectColor = RainWorld.RippleColor;
            self.regionParams.corruptionEyeColor = RainWorld.RippleColor;

            //Todo: Add conditional for Nightmare Rot when that's being developed.
        }
    }
}