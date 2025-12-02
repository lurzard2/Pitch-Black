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

namespace PitchBlack;
public class MenuSceneHooks
{
    private static MenuScene.SceneID EstablishSlugcatPageSceneID(SlugcatSelectMenu self, SlugcatStats.Name owner)
    {
        var progression = Custom.rainWorld.progression;
        bool nullSave = !progression.IsThereASavedGame(owner);
        if (nullSave)
        {
            return Enums.MenuSceneID.Slugcat_Spawn;
        }
        return GetSlugcatPageSceneID(self, owner, progression);
    }

    private static MenuScene.SceneID GetSlugcatPageSceneID(SlugcatSelectMenu self, SlugcatStats.Name owner, PlayerProgression progression)
    {
        MenuScene.SceneID ph = Enums.MenuSceneID.Slugcat_Beacon;
        SaveState currentSaveState = progression.currentSaveState != null ? progression.currentSaveState : null;
        bool conditionMaxSpiral = currentSaveState != null ? BeaconSaveData.GetMaxSpiralLevel(currentSaveState) == 5 : false;
        bool conditionSpiralProgression = currentSaveState != null ? BeaconSaveData.GetMaxSpiralLevel(currentSaveState) >= 2.5 : false;
        if (conditionMaxSpiral)
        {
            return ph;
        }
        if (conditionSpiralProgression)
        {
            return ph;
        }
        return Enums.MenuSceneID.Slugcat_Beacon_Dreamer;
    }

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
        orig(self);
        if (self.menu is SlugcatSelectMenu
            && self.sceneID != null
            && self.owner is SlugcatSelectMenu.SlugcatPage page)
        {
            var owner = page.slugcatNumber;
            var slugcatMenu = self.menu as SlugcatSelectMenu;
            if (owner == Enums.SlugcatStatsName.Beacon)
            {
                // Assign scene
                self.sceneID = EstablishSlugcatPageSceneID(slugcatMenu, owner);

                // Mark stuff
                var markGlow = page.markGlow;
                var markSquare = page.markSquare;
                markGlow?.RemoveFromContainer();
                page.markGlow = null;
                markSquare?.RemoveFromContainer();
                page.markSquare = null;
            }
        }
        //BuildPBScene(self);
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
