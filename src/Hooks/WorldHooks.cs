using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watcher;
using static PitchBlack.Plugin;

namespace PitchBlack;

public static class RoomSpecificScriptHooks
{
    private static void NewUAD(Room room, UpdatableAndDeletable scriptObj)
    {
        room.AddObject(scriptObj);
    }

    public static void Inject()
    {
        On.RoomSpecificScript.AddRoomSpecificScript += AddScriptToRoom;
    }

    private static void AddScriptToRoom(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
    {
        orig(room);
        if (room.game.session is StoryGameSession storyCheck && !MiscUtils.IsBeacon(storyCheck.saveStateNumber))
        {
            return;
        }

        if (room.abstractRoom.name == "VV_E01" && room.world.game.GetStorySession.saveState.cycleNumber == 0)
        {
            for (int i = 0; i < room.game.Players.Count; i++)
            {
                NewUAD(room, new VV_E01(room, i));
            }
        }

    }
}

public static class DreamersHooks
{
    // New Dreamer Presence Thing
    // > Assign presence somewhere outside the actual room itself is being loaded, so presence is global to the loaded region
    // > for each room in region that includes a dreamerspot, create a presence with dreamer's room being that room
    // > Have a list of these presences then attach it to the plugin value that stores presences
    // > for each presence, and the room is the dreamer room, and they havent been encountered, spawn dreamer
    // > if already encountered, and can, spawn warp

    public static void InitDreamerRoomsToPresences(Room self)
    {
        List<DreamerPresence> presencesToAdd = new List<DreamerPresence>();

        // Queue of dummy presences
        if (roomsWithDreamerSpot.TryGetValue(self.world, out var roomsForDreamer))
        {
            foreach (var abstractRoom in roomsForDreamer)
            {
                logger.LogDebug($"DreamerRooms: Room in list's name is " + abstractRoom.name);
                DreamerPresence dummyPresence = null;
                AbstractRoom currentRoom = self.abstractRoom;
                string encounterRoomName = abstractRoom.name;
                bool roomMarkedEncountered = BeaconSaveData.GetDreamerEncounteredRooms(self.world.game.GetStorySession.saveState).Contains(encounterRoomName);
                if (roomMarkedEncountered)
                {
                    logger.LogDebug("DreamerPresence: Dreamer was marked encountered in this room, aborting process!");
                    continue;
                }
                else
                {
                    logger.LogDebug("DreamerRooms: Assigned dummy presence");
                    logger.LogDebug("DreamerRooms: Not an encounter room, moving forward with presence");
                    dummyPresence = new DreamerPresence(self.world, abstractRoom);
                    presencesToAdd.Add(dummyPresence);
                    logger.LogDebug($"DreamerRooms: Added dummy presence to the presence list queue");
                }
            }
        }

        // Assigning presences from the queue to the presence cwt
        if (dreamerPresence.TryGetValue(self.world, out var dreamerPresences))
        {
            for (int i = 0; i < presencesToAdd.Count; i++)
            {
                logger.LogDebug("DreamerPresence: Spawning");
                presencesToAdd[i].presenceSpawned = true;
                dreamerPresences.Add(presencesToAdd[i]);
                self.world.migrationInfluences.Add(dreamerPresences[i]);
                logger.LogDebug($"DreamerPresence: Added queue of presences to the presence CWT");
                logger.LogDebug($"DreamerPresence Contains:");
                logger.LogDebug($"> Dreamer's room - {presencesToAdd[i].dreamerRoom.name}");
                logger.LogDebug($"> Presence active - {presencesToAdd[i].presenceSpawned}");
                logger.LogDebug($"> Dreamer active - {presencesToAdd[i].dreamerSpawned}");
                timesToAssignDreamerPresence--;
            }
        }

        // Create the list first otherwise NOTHING works
        else
        {
            dreamerPresence.Add(self.world, new List<DreamerPresence>());
        }
    }

    public static void DeactivateDreamerPresence(Room self)
    {
        if (dreamerPresence.TryGetValue(self.world, out var presence))
        {
            for (int i = 0; i < presence.Count; i++)
            {
                var players = self.abstractRoom.world.game.Players;
                string roomNameOfPlayer = players[0].realizedCreature.room.abstractRoom.name;
                logger.LogDebug($"DreamerPresence: THIS ROOM:{self.abstractRoom.name}");
                if (presence[i].presenceSpawned && presence[i].dreamerRoom.name == roomNameOfPlayer && presence[i].dreamerSpawned)
                {
                    logger.LogDebug($"DreamerPresence: PRESENCE ROOM:{presence[i].dreamerRoom.name} - PLAYER ROOM:{roomNameOfPlayer}");
                    self.world.migrationInfluences.Remove(presence[i]);
                    presence[i].dreamerRoom = null;
                    presence[i].presenceSpawned = false;
                    presence[i].dreamerSpawned = false;
                    presence.Remove(presence[i]);
                    logger.LogDebug($"DreamerPresence: Removed DreamerPresence from CWT");
                }
                else
                {
                    logger.LogDebug("DreamerPresence: Couldn't remove presence");
                    logger.LogDebug($"DreamerPresence Contains:");
                    logger.LogDebug($"> Dreamer's room - {presence[i].dreamerRoom.name}");
                    logger.LogDebug($"> Presence active - {presence[i].presenceSpawned}");
                    logger.LogDebug($"> Dreamer active - {presence[i].dreamerSpawned}");
                }
            }
        }
    }

    public static void DreamerGhostModeUpdate(RoomCamera self)
    {
        // Exiting
        Creature creature = self.followAbstractCreature?.realizedCreature;
        if (!dreamerPresence.TryGetValue(self.room.world, out var dreamerPresences))
        {
            return;
        }
        if (creature is not Player || creature is null)
        {
            return;
        }

        foreach (var presence in dreamerPresences)
        {
            self.ghostMode = lastGhostMode;
            if (presence.presenceSpawned)
            {
                // Set value to be greater than 0 so ghostMode can be modified by us
                if (self.ghostMode == 0)
                {
                    self.ghostMode = 0.001f;
                }

                // Room is Dreamer's room
                if (presence.dreamerSpawned && self.room.abstractRoom == presence.dreamerRoom)
                {
                    // Checking for conversation to set correctly
                    if (targetDreamIntensity < 0.75f)
                    {
                        targetDreamIntensity = 0.25f;
                    }
                    self.ghostMode = lastGhostMode;
                    DreamerInRoomMode(self, creature);
                    break;
                }

                // Adjacent or other rooms in region effects
                ForConnectionsMode(self, presence);
                break;
            }
            else
            {
                if (targetDreamIntensity > 0f)
                {
                    targetDreamIntensity -= 0.0005f;
                }
            }
        }

        // Modifying effect intensity if it exists
        if (self.ghostMode > 0f)
        {
            self.ghostMode = Mathf.Lerp(lastGhostMode, targetDreamIntensity, 0.06f);
            self.lightBloomAlpha = lastGhostMode * 0.8f;
            lastGhostMode = self.ghostMode;
        }
    }

    private static void ForConnectionsMode(RoomCamera self, DreamerPresence presence)
    {
        for (int i = 0; i < self.room.abstractRoom.connections.Length; i++)
        {
            if (self.room.abstractRoom.connections[i] >= 0 && self.room.world.GetAbstractRoom(self.room.abstractRoom.connections[i]) == presence.dreamerRoom)
            {
                targetDreamIntensity = 0.25f;
            }
            // Isnt a connection to dreamer's room
            else
            {
                if (targetDreamIntensity > 0.005f)
                {
                    targetDreamIntensity -= 0.002f;
                }
            }
        }
    }

    private static void DreamerInRoomMode(RoomCamera self, Creature creature)
    {
        int i = 0;
        while (i < self.room.updateList.Count)
        {
            if (self.room.updateList[i] is Dreamer)
            {
                float distanceFromDreamer = Vector2.Distance((self.room.updateList[i] as Dreamer).placedObject.pos, creature.mainBodyChunk.pos);
                targetDreamIntensity = Mathf.Lerp(0.11f, 1f, Mathf.InverseLerp(1500f, 0f, distanceFromDreamer));
                if ((self.room.updateList[i] as Dreamer).conversation != null)
                {
                    targetDreamIntensity = 1f;
                    break;
                }
                break;
            }
            else
            {
                i++;
            }
        }
    }

    // Tracking presences to assign so we have no duplicates
    public static int timesToAssignDreamerPresence;
    // Highest ghostMode should be
    public static float targetDreamIntensity;
    // tracking ghostMode
    public static float lastGhostMode;

    public static void Inject()
    {
        On.RoomCamera.Update += RoomCamera_Update;
    }

    // Adding effects to rooms, using SpinningTop's radial effect
    private static void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera self)
    {
        orig(self);

        DreamerGhostModeUpdate(self);
    }
}

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
        DreamersHooks.timesToAssignDreamerPresence = listOfRoomsForDreamer.Count;
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
        DreamersHooks.Inject();
        RoomSpecificScriptHooks.Inject();

        On.Region.ctor_string_int_int_RainWorldGame_Timeline += ModifyRegionProperties;
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