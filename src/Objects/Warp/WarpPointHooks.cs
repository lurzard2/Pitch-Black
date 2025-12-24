using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

// Eventually we'll have our own object
public static class WarpPointHooks
{
    public static void Apply()
    {
        On.Player.ApplyWarpFatigue += Player_ApplyWarpFatigue;
        On.Region.HasWarpFatigueResistance += ModifyHasWarpFatigueResistence;
    }

    private static bool ModifyHasWarpFatigueResistence(On.Region.orig_HasWarpFatigueResistance orig, string name)
    {
        return Region.IsAncientUrbanRegion(name) || Region.IsDaemonRegion(name) || MiscUtils.IsVhosRegion(name);
    }

    private static void Player_ApplyWarpFatigue(On.Player.orig_ApplyWarpFatigue orig, Player self, RainWorldGame game)
    {
        if (MiscUtils.IsBeacon(self))
        {
            self.warpExhausionTime = 0;
        }
        else
        {
            orig(self, game);
        }
    }
}
