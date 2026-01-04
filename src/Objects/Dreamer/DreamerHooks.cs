using RWCustom;
using System;
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
    public static float lastGhostMode;
    public static float targetGhostMode;
    public static DreamerPresence currentTarget = null;

	public static Texture2D dreamerFadeTex = null;

    public static void Inject()
    {
        On.RoomCamera.Update += RoomCamera_Update;
        On.RoomCamera.ctor += On_RoomCamera_ctor;
        On.RoomCamera.ClearAllSprites += On_RoomCamera_ClearAllSprites;
        On.RoomCamera.ApplyFade += On_RoomCamera_ApplyFade;
    }

	// I apologize in advance for the horrendous indents but how about you take a crack at it if you think you can do better
	private static void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera self)
    {
        orig(self);

        if (dreamerPresence.TryGetValue(self.room.world, out var presences) && MiscUtils.IsBeacon(self.room.world.game.session))
        {
            //logger.LogDebug($"{s} Presence CWT accessed and campaign is Beacon, proceeding to determine ghostMode of room");
            foreach (var presence in presences)
            {
                var currentPresence = presence;

                if (currentPresence != null && currentPresence.presenceSpawned)
                {
                    lastGhostMode = self.ghostMode;
                    self.ghostMode = Mathf.Lerp(lastGhostMode, targetGhostMode, 0.06f);

                    if (currentTarget != null)
                    {
                        currentPresence = currentTarget;
                    }

                    /* presence.dreamer is a Tuple, blame Alduris for teaching me
                    * they're neat and I'm just using it because we have a lot of things to associate at once,
                    * and individually checking them without direct references is really convoluted.
                    * It's also because more than 1 Dreamer can exist in the same region, dialogue just depends on progression, but we want 1 Dreamer to be placed per presence
                    * And I've banged my head against a wall for a days now trying to write this damn feature
                    */

                    // Dreamer in room mode
                    if (currentPresence.myDreamer.hasSpawned && currentPresence.myDreamer.obj != null && self.room.abstractRoom == currentPresence.myDreamer.abstractRoom)
                    {
                        if (currentPresence.myDreamer.obj.behaviorModule.conversation != null)
                        {
                            targetGhostMode = 1f;
                            //logger.LogDebug($"{s} MODE:Dreamer Conversation");
                            return;
                        }

                        Creature creature = self.followAbstractCreature?.realizedCreature as Player;
                        float distance = Vector2.Distance(currentPresence.myDreamer.obj.placedObject.pos, creature.mainBodyChunk.pos);
                        targetGhostMode = Mathf.Lerp(0.11f, 1f, Mathf.InverseLerp(1500f, 0f, distance));
                        //logger.LogDebug($"{s} MODE:Dreamer Proximity");
                        return;
                    }
                    // Adjacent connections mode
                    else
                    {
                        for (int i = 0; i < self.room.abstractRoom.connections.Length; i++)
                        {
                            if (self.room.abstractRoom.connections[i] >= 0 && self.room.world.GetAbstractRoom(self.room.abstractRoom.connections[i]) == currentPresence.myDreamer.abstractRoom)
                            {
                                // We only assign this once, here, because it's also true for the next room
                                // We also only return if it's the same target, to prevent unnecessary checks
                                currentTarget = presence;
                                targetGhostMode = 0.25f;
                                //logger.LogDebug($"{s} MODE:Connection to Dreamer Room");
                                return;
                            }
                            else
                            {
                                currentTarget = null;
                                if (targetGhostMode > 0)
                                {
                                    targetGhostMode -= 0.006f;
                                }
                                //logger.LogDebug($"{s} MODE:NONE");
                            }
                        }
                    }
                    //logger.LogDebug($"{s} Determined effects - {self.ghostMode}/{targetGhostMode} - {currentPresence.dreamer.Item1}, {currentPresence.dreamer.Item2}, {currentPresence.dreamer.Item3}");
                }
            }
        }
    }

	private static void On_RoomCamera_ctor(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, int cameraNumber)
	{
		orig(self, game, cameraNumber);
		self.LoadPalette(1004, ref dreamerFadeTex);
	}

	private static void On_RoomCamera_ClearAllSprites(On.RoomCamera.orig_ClearAllSprites orig, RoomCamera self)
	{
		orig(self);
		UnityEngine.Object.Destroy(dreamerFadeTex);
	}

	private static void On_RoomCamera_ApplyFade(On.RoomCamera.orig_ApplyFade orig, RoomCamera self)
	{
		Texture2D ghostFadeTex = self.ghostFadeTex;
		if (currentTarget != null || MiscUtils.IsBeacon(self.game.GetStorySession)) {
			self.ghostFadeTex = dreamerFadeTex;
		}

		orig(self);

		self.ghostFadeTex = ghostFadeTex;
	}
}