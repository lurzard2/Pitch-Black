using BepInEx.Logging;
using EffExt;
using Menu;
using RWCustom;
using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Vector2 = UnityEngine.Vector2;

namespace PitchBlack;

public static class PBDreamScene
{
    // Adding a CrossFade to the list, required for timing crossfades
    private static void AddCrossFadeToPlaylist(MenuScene self, MenuScene.SceneID id, int fadeNow, int fadeDuration = 20)
    {
        // To crossfade, illustraion MUST be a MenuDepthIllustration

        if (self.menu is SlideShow slideShow && slideShow.playList.Count > 0)
        {
            foreach (SlideShow.Scene slideShowScene in slideShow.playList)
            {
                if (slideShowScene.sceneID == id)
                {
                    slideShowScene.AddCrossFade(fadeNow, fadeDuration);
                }
            }
        }
    }

    private static void NewIllustration(MenuScene self, string path, string name, float depth, MenuDepthIllustration.MenuShader menuShader,
        int type = 0,
        int fadeNow = 10,
        bool crispPixels = false)
    {
        Vector2 pos = new Vector2(0f, 0f);
        var sceneID = self.sceneID;

        // 0 - Depth
        // 1 - Crossfade
        // 2 - Flat
        switch (type)
        {
            case 1:
                var illustration = new MenuDepthIllustration(self.menu, self, path, name, pos, depth, menuShader)
                {
                    crossfadeMethod = MenuIllustration.CrossfadeType.Standard
                };
                self.AddCrossfade(illustration);
                return;
            case 2:
                self.useFlatCrossfades = true;
                self.AddIllustration(new MenuIllustration(self.menu, self, path, name, pos, crispPixels, true));
                return;
            default:
                self.AddIllustration(new MenuDepthIllustration(self.menu, self, path, name, pos, depth, menuShader));
                return;
        }
    }

    // Fucky way of assigning a usable value from an enum with a similar value
    private static int GetSceneIndex(MenuScene.SceneID id)
    {
        if (id == Enums.MenuSceneID.Dream_Birth_4)
        {
            return 4;
        }
        if (id == Enums.MenuSceneID.Dream_Birth_5)
        {
            return 5;
        }
        return 0;
    }

    // We work WITH Slugbase scenes, so we match a scene in the json's id to one in the code, (only ones we want to do crossfades for)
    public static void MatchSlideshowIDToSlugbaseScene(MenuScene self)
    {
        int assignableIndex = 0;
        string assignablePrefix = "";
        if ((self.menu as SlideShow)?.slideShowID == Enums.SlideShowID.Dream_Birth)
        {
            assignableIndex = GetSceneIndex(self.sceneID);
            self.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + "dream - birth " + assignableIndex.ToString();
            assignablePrefix = "dream birth " + assignableIndex.ToString() + " - ";
            BuildDreamBirthScene(self, assignableIndex, self.sceneFolder, assignablePrefix);
        }
    }

    public static void BuildDreamBirthScene(MenuScene self, int index, string path, string str)
    {
        if (self.flatMode)
        {
            return;
        }
        int fadeWhen = 0;
        switch (index)
        {
            case 4:
                fadeWhen = 34;
                AddCrossFadeToPlaylist(self, self.sceneID, fadeWhen);

                NewIllustration(self, path, str + "6 - egg", 1.6f, MenuDepthIllustration.MenuShader.Basic);
                NewIllustration(self, path, str + "5 - beacon a", 2.5f, MenuDepthIllustration.MenuShader.Basic);
                NewIllustration(self, path, str + "4 - beacon glow", 2.4f, MenuDepthIllustration.MenuShader.Basic);
                // Eye
                NewIllustration(self, path, "empty eye", 2.4f, MenuDepthIllustration.MenuShader.Basic);
                NewIllustration(self, path, str + "3 - beacon eye a", 2.4f, MenuDepthIllustration.MenuShader.Basic, 1, fadeWhen);
                // Membrane glow
                NewIllustration(self, path, "empty highlight", 0.4f, MenuDepthIllustration.MenuShader.Basic);
                NewIllustration(self, path, str + "1 - beacon membrane highlight", 0.4f, MenuDepthIllustration.MenuShader.Basic, 1, fadeWhen);
                // Hand
                NewIllustration(self, path, "empty hand", 0.2f, MenuDepthIllustration.MenuShader.Basic);
                NewIllustration(self, path, str + "0 - beacon hand", 0.2f, MenuDepthIllustration.MenuShader.Basic, 1, fadeWhen);
                break;
            case 5:
                fadeWhen = 45;
                AddCrossFadeToPlaylist(self, self.sceneID, fadeWhen);

                NewIllustration(self, path, str + "6 - egg", 1.6f, MenuDepthIllustration.MenuShader.Basic);

                // Beacon
                NewIllustration(self, path, str + "5 - beacon a", 2.5f, MenuDepthIllustration.MenuShader.Basic);
                NewIllustration(self, path, str + "5 - beacon b", 2.7f, MenuDepthIllustration.MenuShader.Basic, 1, fadeWhen);
                // Glow
                NewIllustration(self, path, str + "4 - beacon glow", 1.6f, MenuDepthIllustration.MenuShader.Basic);
                NewIllustration(self, path, "empty glow", 1.6f, MenuDepthIllustration.MenuShader.Basic, 1, fadeWhen);
                // Eye b
                NewIllustration(self, path, str + "3 - beacon eye b", 2.4f, MenuDepthIllustration.MenuShader.Basic);
                NewIllustration(self, path, "empty eye", 1.6f, MenuDepthIllustration.MenuShader.Basic, 1, fadeWhen);
                // Bubbles
                NewIllustration(self, path, str + "2 - beacon albumen bubbles", 2.2f, MenuDepthIllustration.MenuShader.Basic);
                NewIllustration(self, path, "empty bubbles", 2.2f, MenuDepthIllustration.MenuShader.Basic, 1, fadeWhen);

                NewIllustration(self, path, str + "1 - beacon membrane highlight", 0.4f, MenuDepthIllustration.MenuShader.Basic);
                NewIllustration(self, path, str + "0 - beacon hand", 0.2f, MenuDepthIllustration.MenuShader.Basic);
                break;
            default:
                return;
        }
    }
}

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
        // Evil parsing
        SaveState currentSaveState = progression.currentSaveState != null ? progression.currentSaveState : null;
        bool saveStateExists = currentSaveState != null ? true : false;

        bool conditionMaxSpiralProgression = saveStateExists 
            ? BeaconSaveData.GetMaxSpiralLevel(currentSaveState) == 5
            : false;
        bool conditionSpiralProgression = saveStateExists
            ? BeaconSaveData.GetMaxSpiralLevel(currentSaveState) >= 2.5
            : false;

        if (conditionMaxSpiralProgression)
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
    }

    private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
    {
        orig(self);

        // Slideshows
        if ((self.menu as SlideShow)?.slideShowID == Enums.SlideShowID.Dream_Birth)
        {
            PBDreamScene.MatchSlideshowIDToSlugbaseScene(self);
            return;
        }

        // Slugcat Menu Scenes
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
    }
}
