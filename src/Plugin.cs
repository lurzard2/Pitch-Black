using BepInEx;
using BepInEx.Logging;
using Fisobs.Core;
using IL.Watcher;
using SlugBase.Features;
using System;
using System.Collections.Generic;
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
    public const string MOD_ID = "lurzard.pitchblack";
    public const string MOD_NAME = "Pitch Black";
    public const string MOD_VERSION = "0.1.0";

    private bool init = false;
    public static ManualLogSource logger;

    // Dev bool for testing and/or hardcoding values
    public static bool devMode = true;
    
    // CWTs
    public static readonly ConditionalWeakTable<Player, ScugCWT> scugCWT = new();
    public static readonly ConditionalWeakTable<AbstractCreature, NightTerror> NTAbstractCWT = new();
    public static readonly ConditionalWeakTable<AbstractCreature, StrongBox<int>> KILLIT = new();
    public static readonly ConditionalWeakTable<RainWorldGame, List<NTTracker>> pursuerTracker = new();
    public static readonly ConditionalWeakTable<MouseGraphics, RotData> rotRatData = new();
    public static readonly ConditionalWeakTable<World, List<AbstractRoom>> roomsWithDreamerSpot = new();
    public static readonly ConditionalWeakTable<World, List<DreamerPresence>> dreamerPresence = new();
    public static readonly ConditionalWeakTable<AbstractCreature, Cycle> creatureCycle = new();

    // Colors moved to Colors.cs after I saw Alduris set up his codespace that way -Lur 

    // SlugBase Features for PB:
    // - Names MUST match in both code and .json in order to work, otherwise SlugBase throws a fit.
    // - These fields MUST be in Plugin (according to SlimeCubed).
    // - Implemented in Hooks\Player\PBSlugBaseFeatures.cs
    // [Lur]
    public static readonly PlayerFeature<float> FlipBoost = PlayerFloat("pb/flip_boost");
    
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

        MenuSceneHooks.Apply();
        CreatureCycleHooks.Apply();
        PhysicalObjectHooks.Apply();
        DevToolsHooks.Apply();
        WorldHooks.Apply();
        MusicHooks.Apply();

        PBSlugBaseFeatures.Apply();
        ScugHooks.Apply();
        ScugGraphics.Apply();
        FlareStorage.Apply();
        Crafting.Apply();

        logger.LogDebug("Hooks successfully applied!");
    }

    // Registering
    private void EnableMod(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (!init)
        {
            // Remix Menu
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
                

                Content.Register(new RotRatCritob());
                RotRatHooks.Apply();
                if (!MultiplayerUnlocks.CreatureUnlockList.Contains(Enums.SandboxUnlockID.RotRat))
                    MultiplayerUnlocks.CreatureUnlockList.Add(Enums.SandboxUnlockID.RotRat);

                Content.Register(new CitizenCritob());
                CitizenHooks.Apply();
            }
            catch (Exception err)
            {
                logger.LogDebug($"Error in Critob registry\n{err}");
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

            // DreamSpawn
            self.Shaders["DreamSpawnBody"] = FShader.CreateShader("dreamspawnbody", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/dreamspawnbody")).LoadAsset<Shader>("Assets/Shaders/DreamSpawnBody.shader"));
            self.Shaders["RoseGlow"] = FShader.CreateShader("roseglow", AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/roseglow")).LoadAsset<Shader>("Assets/Shaders/RoseGlow.shader"));

            init = true;
        }
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
                Enums.SandboxUnlockID.UnregisterValues();
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

        if (pursuerTracker.TryGetValue(self, out List<NTTracker> trackers)) foreach (NTTracker tracker in trackers) tracker.Update();
    }
    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        pursuerTracker.Add(self, new List<NTTracker>());
        if ((MiscUtils.IsBeacon(self.session) || ModOptions.universalPursuer.Value) && pursuerTracker.TryGetValue(self, out var trackers))
        {
            trackers.Add(new NTTracker(self));
            logger.LogDebug("ADDING NT TRACKER");
        }
    }

    private void Room_ctor(On.Room.orig_ctor orig, Room self, RainWorldGame game, World world, AbstractRoom abstractRoom, bool devUI)
    {
        orig(self, game, world, abstractRoom, devUI);

        if (game != null && game.session != null & MiscUtils.IsBeacon(game.session))
        {
            // Need this
            self.ripple = true;
        }
    }
}