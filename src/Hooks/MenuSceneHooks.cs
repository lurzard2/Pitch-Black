using BepInEx.Logging;
using Menu;
using RWCustom;
using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.IO;
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
        // Todo: inject Beacon conditional > self.manager.nextSlideShow = Enums.SlideshowID.DreamBirth; < before Yellow's check in the else

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
                self.sceneID = Enums.MenuSceneID.Slugcat_Beacon;
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


    // Needs ifs, dont try making a switch (I already did)
    private static void BuildPBScene(MenuScene scene)
    {
        #region Dream - Birth
        if (scene.sceneID == Enums.MenuSceneID.Dream_Birth_1)
        {
            BuildBeaconBirthScene(scene, 1);
            return;
        }
        if (scene.sceneID == Enums.MenuSceneID.Dream_Birth_2)
        {
            BuildBeaconBirthScene(scene, 2);
            return;
        }
        if (scene.sceneID == Enums.MenuSceneID.Dream_Birth_3)
        {
            BuildBeaconBirthScene(scene, 3);
            return;
        }
        if (scene.sceneID == Enums.MenuSceneID.Dream_Birth_4)
        {
            BuildBeaconBirthScene(scene, 4);
            return;
        }
        if (scene.sceneID == Enums.MenuSceneID.Dream_Birth_5)
        {
            BuildBeaconBirthScene(scene, 5);
            return;
        }
        if (scene.sceneID == Enums.MenuSceneID.Dream_Birth_6)
        {
            BuildBeaconBirthScene(scene, 6);
            return;
        }
        if (scene.sceneID == Enums.MenuSceneID.Dream_Birth_7)
        {
            BuildBeaconBirthScene(scene, 7);
            return;
        }
        if (scene.sceneID == Enums.MenuSceneID.Dream_Birth_8)
        {
            BuildBeaconBirthScene(scene, 8);
            return;
        }
        #endregion
    }

    private static void BuildBeaconBirthScene(MenuScene scene, int index)
    {
        scene.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + "dream - birth " + index.ToString();
        string str = "dream birth " + index.ToString();

        if (scene.flatMode)
        {
            scene.useFlatCrossfades = true;
            // Todo: Add flat illustrations - Vector2(683f, 384f) is proper placement, crispPixels:false, anchorCenter:true
            //return;
        }

        // Todo: Add illustrations for each slide
        switch (index)
        {
            case 1: return;
            case 2: return;
            case 3: return;
            case 4: return;
            case 5: return;
            case 6: return;
            case 7: return;
            case 8: break;
            default: return;
        }
    }
}
