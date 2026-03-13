using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public class HUDHooks
{
    public static void Apply()
    {
        On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
    }

    private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    {
        if ((self.owner as Player).SlugCatClass == Enums.SlugcatStatsName.Beacon)
        {
            // Remaking this
        }
        orig(self, cam);
    }
}
