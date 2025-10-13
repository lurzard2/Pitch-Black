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
        On.Menu.MenuScene.BuildWatcherScene += MenuScene_BuildWatcherScene;
    }

    // I dont know if this works

    private static void MenuScene_BuildWatcherScene(On.Menu.MenuScene.orig_BuildWatcherScene orig, MenuScene self)
    {
        if (self.menu is SlugcatSelectMenu
            && self.sceneID != null
            && self.owner is SlugcatSelectMenu.SlugcatPage page)
        {
            var owner = page.slugcatNumber;

            if (owner == Enums.SlugcatStatsName.Beacon)
            {
                self.sceneID = MenuScene.SceneID.Ghost_White; //Enums.MenuSceneID.Slugcat_Beacon;
            }
        }
        orig(self);
    }
}
