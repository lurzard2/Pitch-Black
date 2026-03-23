using BepInEx;
using BepInEx.Logging;
using Fisobs.Core;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
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
    #region modinfo dont edit
    public const string MOD_ID = "lurzard.pitchblack";
    public const string MOD_NAME = "Pitch Black";
    public const string MOD_VERSION = "0.6.4";
    public static string MOD_PATH = "";
    #endregion

    private bool init = false;
    public static ManualLogSource logger;

    // Dev bool for testing and/or hardcoding values
    public static bool devMode = true;
    public static bool remixUpdatedSaveData = false;
    
    // CWTs
    public static readonly ConditionalWeakTable<AbstractCreature, NightTerror> NTAbstractCWT = new();
    public static readonly ConditionalWeakTable<AbstractCreature, StrongBox<int>> KILLIT = new();
    public static readonly ConditionalWeakTable<RainWorldGame, List<NTTracker>> pursuerTracker = new();

    public static readonly ConditionalWeakTable<MouseGraphics, RotRatData> rotRatData = new();

    public static readonly ConditionalWeakTable<World, List<AbstractRoom>> roomsWithDreamerSpot = new();
    public static readonly ConditionalWeakTable<World, List<DreamerPresence>> dreamerPresence = new();

    // Colors moved to Colors.cs after I saw Alduris set up his codespace that way -Lur 

    // SlugBase Features for PB:
    // - Names MUST match in both code and .json in order to work, otherwise SlugBase throws a fit.
    // - These fields MUST be in Plugin (according to SlimeCubed).
    // - Implemented in Hooks\Player\PBSlugBaseFeatures.cs
    // [Lur]
    public static readonly PlayerFeature<float> FlipBoost = PlayerFloat("pb/flip_boost");

    public static readonly int ShadPropGhostSkinColor = Shader.PropertyToID("_GhostSkinColor");
    public static readonly int ShadPropGhostSkinHighlightColor = Shader.PropertyToID("_GhostSkinHighlightColor");
    public static readonly int ShadPropGhostDistortionColor = Shader.PropertyToID("_GhostDistortionColor");

    // Rotund World stuff [WW]
    internal static bool RotundWorldEnabled => _rotundWorldEnabled;
    private static bool _rotundWorldEnabled;
    public static bool individualFoodEnabled;

    // Applies all hooks
    public void OnEnable()
    {
        logger = Logger;
        logger.LogDebug("Applying hooks...");

        On.RainWorld.OnModsInit += EnableMod;
        On.RainWorld.OnModsDisabled += DisableMod;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
        On.RainWorld.UnloadResources += (orig, self) =>
        {
            orig(self);
            if (Futile.atlasManager.DoesContainAtlas("lmllspr"))
                Futile.atlasManager.UnloadAtlas("lmllspr");
        };
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RainWorldGame.Update += RainWorldGame_Update;

        On.Room.ctor += Room_ctor;

        DreamerHooks.Apply();
        WarpPointHooks_ForRift.Apply();
        CreatureHooks.Apply();
        Dimensions._Implement.Apply();

        MenuSceneHooks.Apply();
        PhysicalObjectHooks.Apply();
        DevToolsHooks.Apply();
        HUDHooks.Apply();
        WorldHooks.Apply();
        RoomHooks.Apply();
        MusicHooks.Apply();

        PBSlugBaseFeatures.Apply();
        ScugHooks.Apply();
        PlayerGraphicsHooks.Apply();
        FlareStorageHooks.Apply();
        Crafting.Apply();

        logger.LogDebug("Hooks successfully applied!");
    }

    // Registering
    private void EnableMod(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (init)
        {
            return;
        }
        init = true;

        //const BindingFlags methodFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        //Assembly assembly = Assembly.GetAssembly(typeof(Plugin));
        //var methods = assembly.GetTypes().SelectMany(type => type.GetMethods(methodFlags));

        //foreach (MethodInfo method in methods.Where(type => type.GetCustomAttribute<Utilities.ImplicitModHookAttribute>() is not null))
        //{
        //    try
        //    {
        //        method.Invoke(null, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError("Failed to invoke hook apply func " +
        //                            $"for method {method.Name} " +
        //                            $"from class {(method.DeclaringType is not null
        //                                ? method.DeclaringType.FullName
        //                                : "not specified by method")}\n" +
        //                            $"Exception: {ex}");
        //    }
        //}

        // Always gets the correct path, whether it be workshop or mods directly
        MOD_PATH = ModManager.ActiveMods.First(x => x.id == MOD_ID).path + Path.DirectorySeparatorChar;

        // Required Remix Menu initialization
        MachineConnector.SetRegisteredOI(MOD_ID, ModOptions.Instance);

        Enums.SoundID.RegisterValues();

        // Because Fisobs may break with game updates
        try
        {
            Content.Register(new NTCritob());
            NTHooks.Apply();
            ScareEverything.Apply();

            Content.Register(new LMLLCritob());
            LMLLHooks.Apply();
            if (!MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.LMiniLongLegs))
                MultiplayerUnlocks.CreatureUnlockList.Add(Enums.SandboxUnlockID.LMiniLongLegs);

            Content.Register(new RotDeerCritob());
            RotDeerHooks.Apply();

            Content.Register(new RotRatCritob());
            RotRatHooks.Apply();
            if (!MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.RotRat))
                MultiplayerUnlocks.CreatureUnlockList.Add(Enums.SandboxUnlockID.RotRat);

            Content.Register(new CitizenCritob());
            CitizenHooks.Apply();
            
            Objects.RotPuff._Meta.Apply();
        }
        catch (Exception err)
        {
            logger.LogDebug($"Error in Critob registry\n{err}");
        }

        Futile.atlasManager.LoadAtlas("atlases/PBHat");
        if (!Futile.atlasManager.DoesContainAtlas("lmllspr"))
            Futile.atlasManager.LoadAtlas("atlases/lmllspr");
        Futile.atlasManager.LoadAtlas("atlases/nightTerroratlas");
        Futile.atlasManager.LoadAtlas("atlases/FaceThanatosis");

        // Ghost shaders
        self.Shaders["EtherealRag"] = FShader.CreateShader("etherealrag", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/etherealrag")).LoadAsset<Shader>("Assets/Shaders/EtherealRag.shader"), new string[]
        {
                "ripple_both_sides"
        });
        self.Shaders["EtherealSkin"] = FShader.CreateShader("etherealskin", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/etherealskin")).LoadAsset<Shader>("Assets/Shaders/EtherealSkin.shader"), new string[]
        {
                "ripple_both_sides"
        });
        self.Shaders["EtherealDistortion"] = FShader.CreateShader("etherealdistortion", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/etherealdistortion")).LoadAsset<Shader>("Assets/Shaders/EtherealDistortion.shader"), new string[]
        {
                "ripple_both_sides"
        });

        // DreamSpawn
        self.Shaders["DreamSpawnBody"] = FShader.CreateShader("dreamspawnbody", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/dreamspawnbody")).LoadAsset<Shader>("Assets/Shaders/DreamSpawnBody.shader"));
        self.Shaders["RoseGlow"] = FShader.CreateShader("roseglow", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/roseglow")).LoadAsset<Shader>("Assets/Shaders/RoseGlow.shader"));

        // Haizlbliek Pitch Black Assets
        self.Shaders["PitchBlackBackgroundBuildings"] = FShader.CreateShader("PitchBlackBackgroundBuildings", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/haizlbliekpitchblack")).LoadAsset<Shader>("Assets/Shaders/PBBackgroundBuildings.shader"));

        // Rift Assets
        self.Shaders["DreamWarpTear"] = FShader.CreateShader("DreamWarpTear", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/dreamwarptear")).LoadAsset<Shader>("Assets/Shaders/DreamWarpTear.shader"));
        self.Shaders["IntoDreamWarpTear"] = FShader.CreateShader("IntoDreamWarpTear", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/intodreamwarptear")).LoadAsset<Shader>("Assets/Shaders/IntoDreamWarpTear.shader"));
    }

    // Unregistering
    private void DisableMod(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);

        foreach (var mod in newlyDisabledMods)
        {
            if (mod.id == MOD_ID)
            {
                Enums.MenuSceneID.UnregisterValues();
                Enums.CreatureTemplateType.UnregisterValues();
                Enums.RoomEffectType.UnregisterValues();
                Enums.PlacedObjectType.UnregisterValues();
                Enums.SoundID.UnregisterValues();

                // Remove creatures from CreatureUnlockList
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.LMiniLongLegs))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(Enums.SandboxUnlockID.LMiniLongLegs);

                if (MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.RotRat))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(Enums.SandboxUnlockID.RotRat);

                break;
            }
        }
    }
    
    // Other mods
    private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);
        
        foreach (var mod in ModManager.ActiveMods)
        {
            if (mod.id == "willowwisp.bellyplus")
                _rotundWorldEnabled = true;

            //else if (mod.id == "dressmyslugcat")
            //{
            //    DMSPatch.AddSpritesToDMS();
            //}

            else if (mod.id == "sprobgik.individualfoodbars")
                individualFoodEnabled = true;
            
        }
    }
    
    private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);
        if (pursuerTracker.TryGetValue(self, out List<NTTracker> trackers)) 
            foreach (NTTracker tracker in trackers)
                tracker.Update();
    }

    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        if (self.TryGetSaveState(out var save))
        {
            if (devMode)
            {
                save.PBConfigUpdateSaveState();
            }

            // Pursuer allowed globally / specifically beacon's campaign
            if (ModOptions.UniversalPursuer || BeaconUtils.IsBeacon(self.session))
            {
                pursuerTracker.Add(self, new List<NTTracker>());
                pursuerTracker.TryGetValue(self, out var trackers);
                trackers.Add(new NTTracker(self));
                logger.LogDebug("NightTerror Tracker: Adding tracker!");
            }
        }
    }

    private void Room_ctor(On.Room.orig_ctor orig, Room self, RainWorldGame game, World world, AbstractRoom abstractRoom, bool devUI)
    {
        orig(self, game, world, abstractRoom, devUI);

        if (game != null && game.session != null & BeaconUtils.IsBeacon(game.session))
        {
            // Need this
            self.ripple = true;
        }
    }
}