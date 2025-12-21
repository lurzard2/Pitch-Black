using System.Collections.Generic;
using UnityEngine;
using static PitchBlack.Plugin;

namespace PitchBlack;

public static class DreamerHooks
{
    public static void Apply()
    {
        DreamerMode_Hooks.Inject();
    }
}

public static class DreamerPresence_Functions
{
    // New Dreamer Presence Thing
    // > Assign presence somewhere outside the actual room itself is being loaded, so presence is global to the loaded region
    // > for each room in region that includes a dreamerspot, create a presence with dreamer's room being that room
    // > Have a list of these presences then attach it to the plugin value that stores presences
    // > for each presence, and the room is the dreamer room, and they havent been encountered, spawn dreamer
    // > if already encountered, and can, spawn warp

    // Tracking presences to assign so we have no duplicates
    public static int timesToAssignDreamerPresence;

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
}

public static class DreamerMode_Hooks
{
    // Highest ghostMode should be
    public static float targetIntensity;
    // tracking ghostMode
    public static float lastGhostMode;
    private static float distanceFromDreamer;

    public static void Inject()
    {
        On.RoomCamera.Update += RoomCamera_Update;
    }

    // Adding effects to rooms, using SpinningTop's radial effect
    private static void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera self)
    {
        orig(self);
        //UpdateDreamerMode(self);
    }

    public static void UpdateDreamerMode(RoomCamera self)
    {
        lastGhostMode = self.ghostMode;
        self.ghostMode = Mathf.Lerp(self.ghostMode, targetIntensity, 0.006f);

        if (dreamerPresence.TryGetValue(self.room.world, out var dreamerPresences))
        {
            for (int i = 0; i < dreamerPresences.Count; i++)
            {
                if (dreamerPresences[i].presenceSpawned)
                {
                    if (self.ghostMode == 0)
                    {
                        targetIntensity = 0.001f;
                        return;
                    }

                    if (self.room.abstractRoom != dreamerPresences[i].dreamerRoom)
                    {
                        for (int j = 0; j < self.room.abstractRoom.connections.Length; j++)
                        {
                            if (self.room.abstractRoom.connections[j] >= 0 && self.room.world.GetAbstractRoom(self.room.abstractRoom.connections[j]) == dreamerPresences[j].dreamerRoom)
                            {
                                targetIntensity = 0.25f;
                            }
                            // Isnt a connection to dreamer's room
                            else
                            {
                                if (targetIntensity > 0.005f)
                                {
                                    targetIntensity -= 0.002f;
                                }
                            }
                        }
                    }
                    else if (dreamerPresences[i].dreamerSpawned)
                    {
                        for (int k = 0; k < self.room.updateList.Count; k++)
                        {
                            if (self.room.updateList[k] is Dreamer)
                            {
                                var them = (self.room.updateList[k] as Dreamer).placedObject.pos;
                                var you = (self.followAbstractCreature?.realizedCreature).mainBodyChunk.pos;
                                distanceFromDreamer = Vector2.Distance(them, you);
                                if ((self.room.updateList[k] as Dreamer).conversation != null)
                                {
                                    targetIntensity = 1f;
                                }
                                else
                                {
                                    targetIntensity = Mathf.Lerp(0.11f, 1f, Mathf.InverseLerp(1500f, 0f, distanceFromDreamer));
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (self.ghostMode > 0)
                    {
                        targetIntensity -= 0.0008f;
                    }
                    else
                    {
                        targetIntensity = 0f;
                    }
                }
            }
        }
    }
}