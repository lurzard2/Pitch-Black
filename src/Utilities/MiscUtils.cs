using UnityEngine;

namespace PitchBlack;

public static class MiscUtils
{
    #region Beacon Checks
    public static bool IsBeacon(GameSession session) => (session is StoryGameSession s) && IsBeacon(s.saveStateNumber);
    public static bool IsBeacon(Creature crit) => (crit is Player player) && IsBeacon(player.slugcatStats.name);
    public static bool IsBeacon(SlugcatStats.Name name) => name != null && name == Enums.SlugcatStatsName.Beacon;
    public static bool IsBeacon(SlugcatStats.Timeline time) => time != null && time == Enums.Timeline.Beacon;
    #endregion

    #region Creature Checks
    public static bool IsNightTerror(this CreatureTemplate creatureTemplate) => creatureTemplate.type == Enums.CreatureTemplateType.NightTerror;
    // Might be a better way to further check specific creatures a bajillion times
    public static bool IsCreature(this CreatureTemplate creatureTemplate, CreatureTemplate.Type type) => type != null && creatureTemplate.type == type;
    #endregion

    #region Region Checks
    public static bool IsRegionOutSideCycle(this World world)
    {
        bool isPBSBRegion = world.name.ToLowerInvariant() == "pbsb";
        bool watcherCondition = Region.IsAncientUrbanRegion(world.name) || Region.IsDaemonRegion(world.name);

        if (IsVhosRegion(world.name) || isPBSBRegion || watcherCondition)
        {
            return true;
        }
        //for (int i = 0; i < world.abstractRooms.Length; i++)
        //{
        //}
        return false;
    }

    public static bool IsVhosRegion(string name) => name.ToLowerInvariant() == "vv";

    public static int RiftAssociatedWithDreamscape(Room room, Rift rift)
    {
        // Currently In VV
        if (IsVhosRegion(room.world.name.ToLowerInvariant()))
        {
            return 1;
        }
        // Rift leads to VV
        if (IsVhosRegion(rift.Data.destRegion.ToLowerInvariant()))
        {
            return 2;
        }
        return 0;
    }

    #endregion

    #region Room Checks
    // Regions that make Beacon squint regardless of room darkness
    public static bool MakeBeaconCloseEyesHere(Player self, string region, string roomName)
    {
        bool vhosOverride = roomName == "vv_e01"
            || roomName == "vv_b06"
            || roomName == "vv_b08"
            || roomName == "vv_c02"
            || roomName == "vv_c01";
        bool vhosCondition = region == "vv";
        bool placeIsBright = self.room.Darkness(self.mainBodyChunk.pos) < 0.15f;
        bool presentGhostMode = self.room.game.cameras[0].ghostMode > 0.40f;

        if (vhosOverride)
        {
            return false;
        }
        else if (presentGhostMode)
        {
            return false;
        }
        else if (vhosCondition)
        {
            return true;
        }
        else if (placeIsBright)
        {
            return true;
        }
        return false;
    }

    // For NT tracking
    public static bool ValidTrackRoom(this Room room) => room != null && !room.abstractRoom.shelter && !room.abstractRoom.gate;
    #endregion

    #region DreamSpawn
    public static bool IsVariant(VoidSpawn self, VoidSpawn.SpawnType variant)
    {
        if (self.variant == variant)
        {
            return true;
        }
        return false;
    }
    public static void MaterializeDreamSpawn(Room room, Vector2 spawnPos, Room.RippleSpawnSource spawnSource,
        int overrideRippleLayer = 0, bool overrideFadeOut = false)
    {
        // Defaults
        int amountToSpawn = 0;
        int rippleLayer = 0;
        int fadeOut = Random.Range(400, 1200);
        AbstractPhysicalObject obj = null;
        VoidSpawn.SpawnType spawnType = null;
        VoidSpawn spawn = null;
        VoidSpawn.Behavior spawnBehavior = null;

        // Override
        rippleLayer = overrideRippleLayer > 0 ? overrideRippleLayer : 0;

        spawnType = Enums.DreamSpawnType.DreamSpawn;
        if (spawnSource == Enums.DreamSpawnSource.Dreamcatcher || spawnSource == Enums.DreamSpawnSource.Jetsam)
        {
            //spawnBehavior = new DreamSpawnBehavior.Caught(spawn, room);
            spawnType = Enums.DreamSpawnType.DreamKin;
            amountToSpawn = BeaconSaveData.GetDreamerEncountersNumber(room.world.game.GetStorySession.saveState);
        }

        float getVoidMelt = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt);
        obj = new AbstractPhysicalObject(room.world, Enums.AbstractObjectType.DreamSpawn, null, room.GetWorldCoordinate(spawnPos), room.game.GetNewID());
        spawn = new DreamSpawn(obj, getVoidMelt, room.Darkness(spawnPos) > 0.4f ? false : true, spawnType);

        if (spawnSource == Enums.DreamSpawnSource.Dreamcatcher)
        {
            spawnBehavior = new DreamSpawnBehavior.Caught(spawn, room);
        }
        else
        {
            spawnBehavior = new VoidSpawn.BezierSwarm(spawn, room);
        }

        int count = room.voidSpawns.Count;
        //Stopping spawning if the room has too many
        if (count >= count + amountToSpawn)
        {
            return;
        }

        // Setting up everything to spawn one
        spawn.behavior = spawnBehavior;
        if (overrideFadeOut)
        {
            spawn.canBeDestroyed = false;
        }
        else
        {
            spawn.timeUntilFadeout = fadeOut;
        }

        spawn.PlaceInRoom(room);
        spawn.ChangeRippleLayer(rippleLayer, true);
    }
    #endregion

    public static void AddHUDMessage(HUD.HUD hud, bool clear, string text, int wait, int time, bool darken, bool hideHUD)
    {
        var prompt = hud.textPrompt;
        // Clear previous, to force it to 0
        if (clear)
        {
            prompt.messages.Clear();
        }

        // Add message to list
        prompt.AddMessage(text, wait, time, darken, hideHUD);
        if (prompt.messages.Count > 0 && prompt.messages[0].text == text)
        {
            prompt.messages[0].time = time;
        }
    }

    public static Rift PlaceRift(RiftManager riftManager, Rift replacementRift, bool triggerNow)
    {
        riftManager.room.AddObject(riftManager);
        if (replacementRift != null)
        {
            riftManager.placedRift = replacementRift;
        }
        // The object takes care of adding a rift to the room, but there are cases where it shouldn't and be given new values
        else if (!riftManager.selfSufficient)
        {
            riftManager.placedRift = new(riftManager.room, riftManager.placedObj, triggerNow);
        }
        return riftManager.placedRift;
    }
}