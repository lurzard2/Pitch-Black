using UnityEngine;

namespace PitchBlack;

public static class MiscUtils
{
    // Beacon checks for various contexts
    public static bool IsBeacon(GameSession session) => (session is StoryGameSession s) && IsBeacon(s.saveStateNumber);
    public static bool IsBeacon(Creature crit) => (crit is Player player) && IsBeacon(player.slugcatStats.name);
    public static bool IsBeacon(SlugcatStats.Name name) => name != null && name == Enums.SlugcatStatsName.Beacon;
    public static bool IsBeacon(SlugcatStats.Timeline time) => time != null && time == Enums.Timeline.Beacon;

    public static bool IsNightTerror(this CreatureTemplate creatureTemplate) => creatureTemplate.type == Enums.CreatureTemplateType.NightTerror;
    // Might be a better way to further check specific creatures a bajillion times
    public static bool IsCreature(this CreatureTemplate creatureTemplate, CreatureTemplate.Type type) => type != null && creatureTemplate.type == type;

    // For NT tracking
    public static bool ValidTrackRoom(this Room room) => room != null && !room.abstractRoom.shelter && !room.abstractRoom.gate;

    public static bool RegionOutSideCycle(this World world) => world != null && world.region.name == "VV" || world.region.name == "UD" || world.region.name == "WRSA";

    // Regions that make Beacon squint regardless of room darkness
    public static bool MakeBeaconCloseEyesHere(Player self, string region, string room)
    {
        bool vhosOverride = room == "vv_e01" || room == "vv_b06";
        bool vhosCondition = region == "vv";
        bool placeIsBright = self.room.Darkness(self.mainBodyChunk.pos) < 0.15f;
        bool presentGhostMode = DreamersHooks.lastGhostMode > 0.40f;

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
}