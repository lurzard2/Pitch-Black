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

    // Regions that make Beacon squint regardless of room darkness
    public static bool MakeBeaconCloseEyesHere(Player self, string region, string room)
    {
        bool vhosDarkRooms = room == "vv_e01";
        bool vhosCondition = region == "vv";
        bool placeIsBright = self.room.Darkness(self.mainBodyChunk.pos) < 0.15f;
        bool presentGhostMode = DreamersHooks.lastGhostMode > 0.40f;

        if (vhosDarkRooms)
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
}