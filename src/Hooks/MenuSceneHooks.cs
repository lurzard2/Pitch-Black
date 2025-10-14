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
        //IL.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGame;
    }

    private static void SlugcatSelectMenu_StartGame(MonoMod.Cil.ILContext il)
    {
        // Todo: inject beacon conditional > self.manager.nextSlideShow = Enums.SlideshowID.DreamBirth; < before Yellow's check

        throw new NotImplementedException();
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
                self.sceneID = Enums.MenuSceneID.Slugcat_Beacon_Dreamer;
                //self.sceneID = Enums.MenuSceneID.Slugcat_Beacon;
                var markGlow = page.markGlow;
                var markSquare = page.markSquare;
                markGlow?.RemoveFromContainer();
                page.markGlow = null;
                markSquare?.RemoveFromContainer();
                page.markSquare = null;
            }
        }
        //BuildPBScene(self);
        orig(self);
    }

    // WIPstuff

    private static void BuildPBScene(MenuScene scene)
    {
        if (scene.sceneID == Enums.MenuSceneID.Dream_Birth_1)
        {
            BuildBirthScene(scene, 1);
            return;
        }
    }

    private static void BuildBirthScene(MenuScene scene, int id)
    {
        return;
    }
}
