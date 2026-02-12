using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PitchBlack;

public static class BeaconVisibilityBonus
{
    public static void Apply()
    {
        _ = new Hook(typeof(Player).GetProperty(nameof(Player.VisibilityBonus), BindingFlags.Public | BindingFlags.Instance).GetGetMethod(), Player_VisibilityBonus);
    }

    public static float Player_VisibilityBonus(Func<Player, float> orig, Player self)
    {
        if (self.TryGetBeacon(out var beacon))
        {
            return orig(self);
        }
        else
        {
            return orig(self);
        }
    }
}
