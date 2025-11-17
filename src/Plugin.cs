using BepInEx;
using BepInEx.Logging;
using Fisobs.Core;
using RWCustom;
using SlugBase.Features;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using UnityEngine;
using static SlugBase.Features.FeatureTypes;

// Allows access to private members
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete

namespace PitchBlack;
[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]

class  Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "lurzard.pitchblack";
    public const string MOD_NAME = "Pitch Black";
    public const string MOD_VERSION = "0.1.0";

    private bool init = false;
    public static ManualLogSource logger;
    
    // CWTs
    public static readonly ConditionalWeakTable<Player, ScugCWT> scugCWT = new();
    public static readonly ConditionalWeakTable<AbstractCreature, NightTerror> NTAbstractCWT = new();
    public static readonly ConditionalWeakTable<AbstractCreature, StrongBox<int>> KILLIT = new();
    public static readonly ConditionalWeakTable<RainWorldGame, List<NTTracker>> pursuerTracker = new();
    public static readonly ConditionalWeakTable<MouseGraphics, RotData> rotRatData = new();

    // Colors moved to Colors.cs after I saw Alduris set up his codespace that way -Lur 

    /// <summary>
    /// SlugBase Features for PB:
    /// - Names MUST match in both code and .json in order to work, otherwise SlugBase throws a fit.
    /// - These fields MUST be in Plugin (according to SlimeCubed).
    /// - Implemented in Hooks\Player\PBSlugBaseFeatures.cs
    /// [Lur]
    /// </summary>
    public static readonly PlayerFeature<float> FlipBoost = PlayerFloat("pb/flip_boost");
    
    // Rotund World stuff -WW
    internal static bool RotundWorldEnabled => _rotundWorldEnabled;
    private static bool _rotundWorldEnabled;
    public static bool individualFoodEnabled;
    
    /// <summary>
    /// Applies all hooks.
    /// </summary>
    public void OnEnable()
    {
        logger = Logger;
        
        logger.LogDebug("Applying PitchBlack hooks...");
        On.RainWorld.OnModsInit += OnModsInit;
        On.RainWorld.OnModsDisabled += DisableMod;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
        //On.RainWorld.BuildTokenCache += RainWorld_BuildTokenCache;
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RainWorldGame.Update += RainWorldGame_Update;
        On.RainWorld.UnloadResources += (orig, self) =>
        {
            orig(self);
            if (Futile.atlasManager.DoesContainAtlas("lmllspr"))
                Futile.atlasManager.UnloadAtlas("lmllspr");
        };
        On.SaveState.LoadGame += SaveState_LoadGame;

        WorldHooks.Apply();
        DevToolsHooks.Apply();
        PBSlugBaseFeatures.Apply();
        ScugHooks.Apply();
        ScugGraphics.Apply();
        FlareStorage.Apply();
        Crafting.Apply();
        FlareBombHooks.Apply();
        
        logger.LogDebug("PitchBlack's hooks successfully applied!");
    }

    private void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
    {
        orig(self, str, game);
        if (self.saveStateNumber == Enums.SlugcatStatsName.Beacon && self.cycleNumber == 0)
        {
            BeaconSaveData.GetDreamerEncountersRoom(self).Clear();
            BeaconSaveData.SetDreamerEncountersNumber(self, 0);
            BeaconSaveData.SetCanUseThanatosis(self, false);
            BeaconSaveData.SetSpiralLevel(self, 0);
            BeaconSaveData.SetMaxSpiralLevel(self, 0);
            BeaconSaveData.SetMinSpiralLevel(self, 0);
        }
    }

    //private void RainWorld_BuildTokenCache(On.RainWorld.orig_BuildTokenCache orig, RainWorld self, bool modded, string region)
    //{
    //    orig(self, modded, region);
    //    string fileName = region.ToLowerInvariant();
    //    regionDreamerRooms[fileName] = new List<string>();

    //    string[] array = AssetManager.ListDirectory("World" + Path.DirectorySeparatorChar.ToString() + region + "-Rooms", false, false, false);
    //    List<string> list = new List<string>();
    //    List<string> list2 = new List<string>();
    //    for (int i = 0; i < array.Length; i++)
    //    {
    //        string fileName3 = Path.GetFileName(array[i]);
    //        if (fileName3.Contains("settings"))
    //        {
    //            list.Add(array[i]);
    //            if (fileName3.Contains("settings-"))
    //            {
    //                list2.Add(array[i]);
    //            }
    //        }
    //    }
    //    for (int j = 0; j < list.Count; j++)
    //    {
    //        string[] array2 = File.ReadAllLines(list[j]);
    //        List<string[]> list5 = new List<string[]>();
    //        for (int k = 0; k < array2.Length; k++)
    //        {
    //            string[] array3 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[k], ":"), ": ");
    //            if (array3.Length == 2)
    //            {
    //                list5.Add(array3);
    //            }
    //        }
    //        for (int l = 0; l < list5.Count; l++)
    //        {
    //            if (list5[l][0] == "PlacedObjects")
    //            {
    //                string[] array4 = Regex.Split(Custom.ValidateSpacedDelimiter(list5[l][1], ","), ", ");
    //                for (int m = 0; m < array4.Length; m++)
    //                {
    //                    string[] array5 = Regex.Split(array4[m].Trim(), "><");
    //                    string fileName2 = Path.GetFileName(list[j].Substring(0, list[j].ToLowerInvariant().IndexOf("_settings")));
    //                    if (array5.Length > 1)
    //                    {
    //                        // The important bit
    //                        PlacedObject placedObject = new PlacedObject(PlacedObject.Type.None, null);
    //                        if (placedObject.type == Enums.PlacedObjectType.DreamerSpot)
    //                        {
    //                            if (!regionDreamerRooms[fileName].Contains(fileName2))
    //                            {
    //                                regionDreamerRooms[fileName].Add(fileName2);
    //                            }
    //                        }
    //                    }
    //                }
    //                break;
    //            }
    //        }
    //    }
    //}

    /// <summary>
    /// Load any resources
    /// Enum Registering
    /// </summary>
    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        MachineConnector.SetRegisteredOI(MOD_ID, ModOptions.Instance);
        if (!init)
        {
            Enums.SoundID.RegisterValues();
            MenuSceneHooks.Apply();

            try
            {
                Content.Register(new LMLLCritob());
                Content.Register(new NTCritob());
                Content.Register(new RotRatCritob());
                Content.Register(new CitizenCritob());
                
                LMLLHooks.Apply();
                NTHooks.Apply();
                ScareEverything.Apply();
                RotRatHooks.Apply();
                CitizenHooks.Apply();
                
                // Add creatures to CreatureUnlockList
                if (!MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.LMiniLongLegs))
                {
                    MultiplayerUnlocks.CreatureUnlockList.Add(Enums.SandboxUnlockID.LMiniLongLegs);
                }
                // if (!MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.NightTerror))
                // {
                //     MultiplayerUnlocks.CreatureUnlockList.Add(Enums.SandboxUnlockID.NightTerror);
                // }
                if (!MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.RotRat))
                {
                    MultiplayerUnlocks.CreatureUnlockList.Add(Enums.SandboxUnlockID.RotRat);
                }
            }
            catch (Exception err)
            {
                logger.LogDebug($"PitchBlack error\n{err}");
            }
            
            Futile.atlasManager.LoadAtlas("atlases/PBHat");
            if (!Futile.atlasManager.DoesContainAtlas("lmllspr"))
                Futile.atlasManager.LoadAtlas("atlases/lmllspr");
            Futile.atlasManager.LoadAtlas("atlases/nightTerroratlas");

            // Dreamer
            self.Shaders["DreamerRag"] = FShader.CreateShader("dreamerrag", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/dreamerrag")).LoadAsset<Shader>("Assets/Shaders/DreamerRag.shader"), new string[]
            {
                "ripple_both_sides"
            });
            self.Shaders["DreamerSkin"] = FShader.CreateShader("dreamerskin", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/dreamerskin")).LoadAsset<Shader>("Assets/Shaders/DreamerSkin.shader"), new string[]
            {
                "ripple_both_sides"
            });
            self.Shaders["DreamerDistortion"] = FShader.CreateShader("dreamerdistortion", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/dreamerdistortion")).LoadAsset<Shader>("Assets/Shaders/DreamerDistortion.shader"), new string[]
            {
                "ripple_both_sides"
            });
            //self.Shaders["DreamerRagRipple"] = FShader.CreateShader("dreamerrag", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/dreamerrag")).LoadAsset<Shader>("Assets/Shaders/DreamerRag.shader"), [other]);
            //self.Shaders["DreamerSkinRipple"] = FShader.CreateShader("dreamerskin", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/dreamerskin")).LoadAsset<Shader>("Assets/Shaders/DreamerSkin.shader"), [other]);
            //self.Shaders["DreamerDistortionRipple"] = FShader.CreateShader("dreamerdistortion", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/dreamerdistortion")).LoadAsset<Shader>("Assets/Shaders/DreamerDistortion.shader"), [other]);

            init = true;
        }
    }

    /// <summary>
    /// Unload any resources
    /// Enum Unregistering
    /// </summary>
    private void DisableMod(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);

        foreach (var mod in newlyDisabledMods)
        {
            if (mod.id == MOD_ID)
            {
                Enums.SlideShowID.UnregisterValues();
                Enums.MenuSceneID.UnregisterValues();
                Enums.CreatureTemplateType.UnregisterValues();
                Enums.SandboxUnlockID.UnregisterValues();
                Enums.RoomEffectType.UnregisterValues();
                Enums.PlacedObjectType.UnregisterValues();
                Enums.SoundID.UnregisterValues();
                
                // Remove creatures from CreatureUnlockList
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.LMiniLongLegs))
                {
                    MultiplayerUnlocks.CreatureUnlockList.Remove(Enums.SandboxUnlockID.LMiniLongLegs);
                }
                // if (MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.NightTerror))
                // {
                //     MultiplayerUnlocks.CreatureUnlockList.Remove(Enums.SandboxUnlockID.NightTerror);
                // }
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.RotRat))
                {
                    MultiplayerUnlocks.CreatureUnlockList.Remove(Enums.SandboxUnlockID.RotRat);
                }
                
                break;
            }
        }
    }
    
    /// <summary>
    /// Utilized for conditional code when other mods are enabled.
    /// </summary>
    private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);
        
        foreach (var mod in ModManager.ActiveMods)
        {
            if (mod.id == "willowwisp.bellyplus")
            {
                _rotundWorldEnabled = true;
            }
            else if (mod.id == "dressmyslugcat")
            {
                //DMSPatch.AddSpritesToDMS();
            }
            else if (mod.id == "sprobgik.individualfoodbars")
            {
                individualFoodEnabled = true;
            }
        }
    }
    
    private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);
        if (pursuerTracker.TryGetValue(self, out List<NTTracker> trackers)) foreach (NTTracker tracker in trackers) tracker.Update();
    }
    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);
        pursuerTracker.Add(self, new List<NTTracker>());
        //riftCWT.Add(self, new List<RiftWorldPrecence>());
        if ((MiscUtils.IsBeacon(self.session) || ModOptions.universalPursuer.Value) && pursuerTracker.TryGetValue(self, out var trackers))
        {
            trackers.Add(new NTTracker(self));
            logger.LogDebug("ADDING NT TRACKER");
        }

    }
}