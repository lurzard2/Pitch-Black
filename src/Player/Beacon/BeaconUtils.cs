using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

internal static class BeaconUtils
{
    public static readonly ConditionalWeakTable<Player, Beacon> BeaconCWT = new();

    #region Checks
    public static bool IsBeacon(GameSession session) => (session is StoryGameSession s) && IsBeacon(s.saveStateNumber);
    public static bool IsBeacon(this Creature crit) => (crit is Player player) && IsBeacon(player.slugcatStats.name);
    public static bool IsBeacon(SlugcatStats.Name name) => name != null && name == Enums.SlugcatStatsName.Beacon;
    public static bool IsBeacon(SlugcatStats.Timeline time) => time != null && time == Enums.Timeline.Beacon;
    #endregion

    public static bool TryGetBeacon(this Player p, out Beacon beacon)
    {
        beacon = null;
        // assigns out with out
        if (!BeaconCWT.TryGetValue(p, out beacon))
        {
            BeaconCWT.Add(p, new(p));
        }
        return p.IsBeacon() && beacon is not null;
    }
    public static bool TryGetBeaconSaveState(this RainWorldGame rwg, out SaveState beaconSaveState)
    {
        beaconSaveState = rwg.GetSaveState(true);
        return beaconSaveState is not null;
    }
}