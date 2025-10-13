using BepInEx.Logging;
using Menu;
using RWCustom;
using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PitchBlack.BeaconSaveData;

namespace PitchBlack;
public class MenuSceneHooks
{
    public static void Apply()
    {
        On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
    }

    private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
    {
        if (self.menu is SlugcatSelectMenu
            && self.sceneID != null
            && self.owner is SlugcatSelectMenu.SlugcatPage page)
        {
            var owner = page.slugcatNumber;
            if (owner == Enums.SlugcatStatsName.Beacon)
            {
                self.sceneID = Enums.MenuSceneID.Slugcat_Beacon;
                var markGlow = page.markGlow;
                var markSquare = page.markSquare;
                markGlow?.RemoveFromContainer();
                page.markGlow = null;
                markSquare?.RemoveFromContainer();
                page.markSquare = null;
            }
        }
        orig(self);
    }
}
