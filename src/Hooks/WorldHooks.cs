using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PitchBlack;

public static class WorldHooks
{
    public static float targetDreamIntensity;
    public static float lastGhostMode;

    #region Dreamer stuff
    /// <summary>
    /// For DevToolsHooks.RoomLoaded
    /// </summary>
    public static void LoadDreamer(Room self, int objects, DreamerData dreamerData, List<string> dreamerRooms)
    {
        // CWT setup
        if (!Plugin.dreamerPresence.TryGetValue(self.world, out var _))
        {
            Plugin.dreamerPresence.Add(self.world, new List<DreamerPresence>());
        }

        var GotDreamerPresence = Plugin.dreamerPresence.TryGetValue(self.world, out var dreamerPresences);
        var GotRoomsWithDreamer = Plugin.roomsWithDreamer.TryGetValue(self.world, out var abstractRoomsWithDreamer);
        if (!GotRoomsWithDreamer && !GotDreamerPresence)
        {
            return;
        }

        // using a "dummy" instance of the presence class we can then assign the CWT to what we assign to it
        DreamerPresence dummyDreamerPresence = null;
        for (int i = 0; i < dreamerPresences.Count; i++)
        {
            // Assign the current presence if it already exists
            if (dreamerPresences[i].dreamerRoom == self.abstractRoom)
            {
                dummyDreamerPresence = dreamerPresences[i];
                break;
            }
        }
        // Assigning DreamerPresence
        if (dummyDreamerPresence == null)
        {
            bool checkForValidRoom = abstractRoomsWithDreamer.Contains(self.abstractRoom);
            AbstractRoom dreamersRoom = checkForValidRoom ? self.abstractRoom : null;
            dummyDreamerPresence = new DreamerPresence(self.world, dreamersRoom, dreamerData.spawnIdentifier);
            if (!dreamerRooms.Contains(self.abstractRoom.name))
            {
                dreamerPresences.Add(dummyDreamerPresence);
            }
        }
        // Adding Dreamer to the room
        if (!dreamerRooms.Contains(self.abstractRoom.name))
        {
            self.AddObject(new Dreamer(self, self.roomSettings.placedObjects[objects]));
        }
        // Spawning a warp instead, if you've already met them
        else
        {
            AddWarpInstead(self, objects, dreamerData);
        }
    }

    /// <summary>
    /// Adds a warp to the room if Dreamer is intended to spawn one
    /// </summary>
    private static void AddWarpInstead(Room self, int objects, DreamerData dreamerData)
    {
        if (dreamerData.destRoom != null)
        {
            Dreamer.SpawnBackupWarpPoint(self, self.roomSettings.placedObjects[objects]);
        }
    }
    #endregion

    /// <summary>
    /// We can use this to add rooms to a list for tracking if they have something we want to track in them!
    /// </summary>
    private static void LogRooms(WorldLoader self)
    {
        // List of rooms I store
        List<AbstractRoom> listOfRoomsForDreamer = new List<AbstractRoom>();

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
                    break;
                }
            }
        }
        // Once the loop is finished, add every room from the list to the CWT
        Plugin.roomsWithDreamer.Add(self.world, listOfRoomsForDreamer);
    }

    public static void Apply()
    {
        On.Region.ctor_string_int_int_RainWorldGame_Timeline += Region_ctor_string_int_int_RainWorldGame_Timeline;
        On.RoomCamera.Update += RoomCamera_Update;
        On.RoomCamera.UpdateGhostMode += RoomCamera_UpdateGhostMode;
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

    // Replace rot eye+effect color for Beacon
    private static void Region_ctor_string_int_int_RainWorldGame_Timeline(On.Region.orig_ctor_string_int_int_RainWorldGame_Timeline orig, Region self, string name, int firstRoomIndex, int regionNumber, RainWorldGame game, SlugcatStats.Timeline timelineIndex)
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