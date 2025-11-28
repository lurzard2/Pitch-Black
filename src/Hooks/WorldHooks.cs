using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PitchBlack.Plugin;

namespace PitchBlack;

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
                    return;
                }
                logger.LogDebug("DreamerRooms: Assigned dummy presence");
                logger.LogDebug("DreamerRooms: Not an encounter room, moving forward with presence");
                dummyPresence = new DreamerPresence(self.world, abstractRoom);
                presencesToAdd.Add(dummyPresence);
                logger.LogDebug($"DreamerRooms: Added dummy presence to the presence list queue");
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

        return;
    }

    public static void DeactivateDreamerPresence(Room self)
    {
        if (dreamerPresence.TryGetValue(self.world, out var presence))
        {
            for (int i = 0; i < presence.Count; i++)
            {
                if (presence[i].presenceSpawned && presence[i].dreamerRoom == self.abstractRoom)
                {
                    self.world.migrationInfluences.Remove(presence[i]);
                    presence[i].dreamerRoom = null;
                    presence[i].presenceSpawned = false;
                    presence[i].dreamerSpawned = false;
                    presence.Remove(presence[i]);
                    logger.LogDebug($"DreamerPresence: Removed DreamerPresence from CWT");
                }
            } 
        }
    }

    public static int timesToAssignDreamerPresence;
    public static float targetDreamIntensity;
    public static float lastGhostMode;

    public static void Inject()
    {
        On.RoomCamera.Update += RoomCamera_Update;
        On.RoomCamera.UpdateGhostMode += RoomCamera_UpdateGhostMode;
    }

    private static void RoomCamera_UpdateGhostMode(On.RoomCamera.orig_UpdateGhostMode orig, RoomCamera self, Room newRoom, int newCamPos)
    {
        orig(self, newRoom, newCamPos);
        if (Plugin.dreamerPresence.TryGetValue(self.room.world, out _))
        {
            lastGhostMode = self.ghostMode;
        }
    }

    // Adding effects to rooms, using SpinningTop's radial effect
    private static void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera self)
    {
        orig(self);

        Creature creature = (self.followAbstractCreature != null) ? self.followAbstractCreature.realizedCreature : null;
        if (self.room != null && creature != null && creature is Player)
        {
            if (Plugin.dreamerPresence.TryGetValue(self.room.world, out _))
            {
                // SpinningTop's radial room effect is executed like this for Watcher
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
                lastGhostMode = self.ghostMode;
                self.ghostMode = Mathf.Lerp(lastGhostMode, targetDreamIntensity, 0.06f);
            }
            else
            {
                if (self.ghostMode > 0f && targetDreamIntensity > 0f)
                {
                    self.ghostMode -= 0.005f;
                    targetDreamIntensity -= 0.5f;
                }
                else
                {
                    self.ghostMode = 0f;
                    targetDreamIntensity = 0f;
                }
            }
        }
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
        
        On.Region.ctor_string_int_int_RainWorldGame_Timeline += ModifyRegionProperties;
    }

    // Replace rot eye+effect color for Beacon
    private static void ModifyRegionProperties(On.Region.orig_ctor_string_int_int_RainWorldGame_Timeline orig, Region self, string name, int firstRoomIndex, int regionNumber, RainWorldGame game, SlugcatStats.Timeline timelineIndex)
    {
        orig(self, name, firstRoomIndex, regionNumber, game, timelineIndex);

        if (timelineIndex != null && timelineIndex == Enums.Timeline.Beacon)
        {
            self.regionParams.corruptionEffectColor = RainWorld.RippleColor;
            self.regionParams.corruptionEyeColor = RainWorld.RippleColor;

            //Todo: Add conditional for Nightmare Rot when that's being developed.
        }
    }
}