using SlugBase.SaveData;
using System;
using System.Collections.Generic;

namespace PitchBlack;

public static class BeaconSaveData
{
    /// <summary>
    /// Gets a SaveState from the game's StoryGameSession properly without breaking other types of sessions. Must Check if null.
    /// </summary>
    /// <param name="rwg">The RainWorldGame instance.</param>
    /// <returns>An instance of SaveState from GetStorySession. Returns null if GameSession is not a StoryGameSession</returns>
    public static bool TryGetSaveState(this RainWorldGame rwg, out SaveState saveState, bool onlyForBeacon = false)
    {
        saveState = rwg.IsStorySession ? rwg.GetStorySession.saveState : null;
        if (onlyForBeacon)
        {
            return saveState is not null && BeaconUtils.IsBeacon(rwg.session);
        }
        return saveState is not null;
    }

    // Called in RWG ctor, to update savedata on cycle 0 depending on remix config
    public static void PBConfigUpdateSaveState(this SaveState beaconSaveState)
    {
        if (ModOptions.DreamerEncounters > 0)
        {
            beaconSaveState.SetDreamerEncountersNumber(ModOptions.DreamerEncounters);
        }

        if (ModOptions.ThanatosisEnabled)
        {
            beaconSaveState.SetCanUseThanatosis(true);
            if (ModOptions.SkipThanatosisSequence)
            {
                beaconSaveState.SetHasUsedThanatosis(true);
            }

            switch (ModOptions.ThanatosisVariant)
            {
                case 1:
                    // Starving
                    beaconSaveState.SetMaxSpiralLevel(1f);
                    break;
                case 2:
                    // Rot
                    beaconSaveState.SetMaxSpiralLevel(2f);
                    break;
                case 3:
                    // Hybrid
                    beaconSaveState.SetMaxSpiralLevel(4f);
                    break;
                default: break;
            }
        }

        if (ModOptions.UsesFlareMechanics)
        {
            beaconSaveState.SetCanCraftFlares(true);
            beaconSaveState.SetCanStoreFlares(true);
        }
    }

    #region Dreamer Encounters Number

    // Int
    // Assigned: +1 from encountering The Dreamer
    // Used: Chooses Dreamer's conversation ID, based on visits instead of place

    public static string dreamerEncountersNumber = "DreamerEncountersNumber";
    public static int GetDreamerEncountersNumber(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(dreamerEncountersNumber, out int encounters) ? encounters : 0;
    public static void SetDreamerEncountersNumber(this SaveState save, int value) => save.deathPersistentSaveData.GetSlugBaseData().Set(dreamerEncountersNumber, value);
    #endregion

    #region Dreamer Encounters Room

    // List<string>
    // Assigned: Entries added from encountering The Dreamer
    // Data: Name of the room they were encountered in
    // Used: Determining whether Dreamer should spawn in the room, or something else.

    public static string dreamerEncountersRoom = "DreamerEncountersRoom";
    public static List<string> GetDreamerEncounteredRooms(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(dreamerEncountersRoom, out List<string> encounters) ? encounters : [];
    public static void SetDreamerEncounteredRooms(this SaveState save, string value)
    {
        if (!save.deathPersistentSaveData.GetSlugBaseData().TryGet(dreamerEncountersRoom, out List<string> encounters))
        {
            encounters = new List<string>();
            save.deathPersistentSaveData.GetSlugBaseData().Set(dreamerEncountersRoom, encounters);
        }
        bool hasEncounter = encounters.Contains(value);
        if (!hasEncounter)
        {
            encounters.Add(value);
        }
    }
    #endregion

    #region Can Use Thanatosis

    // Bool
    // Assigned: From finishing the Thanatosis Sequence, which is when the player unlocks Thanatosis
    // Used: Checking if the player has unlocked Thanatosis

    public static string canUseThanatosis = "CanUseThanatosis";
    public static bool GetCanUseThanatosis(this SaveState save)
    {
        // Arena fallback
        if (save is null)
        {
            return true;
        }
        return save.deathPersistentSaveData.GetSlugBaseData().TryGet(canUseThanatosis, out bool thanatosis) && thanatosis;
    }
    public static void SetCanUseThanatosis(this SaveState save, bool value) => save.deathPersistentSaveData.GetSlugBaseData().Set(canUseThanatosis, value);
    #endregion

    #region Has Used Thanatosis

    // Bool
    // Assigned: From finishing the Thanatosis Sequence
    // Used: Checking if false, which is before the Thanatosis Sequence has ever completed. Used for creating the Thanatosis Sequence conditionally

    public static string hasUsedThanatosis = "HasUsedThanatosis";
    public static bool GetHasUsedThanatosis(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(hasUsedThanatosis, out bool usedThanatosis) && usedThanatosis;
    public static void SetHasUsedThanatosis(this SaveState save, bool value) => save.deathPersistentSaveData.GetSlugBaseData().Set(hasUsedThanatosis, value);
    #endregion

    #region Spiral Level

    // General purpose is to influence beacon's mechanics and such.

    public static bool SpiralLevelWillGameOver(float currentLevel) => currentLevel < 1;
    public static float minimumSpiralForAbility = 1;
    public static float maximumSpiralLimit = 5;

    #region Current Level
    // "Current level" system Supporting multiple players (inlcuding 4+) and prevents unwanted resets
    // Assigned: To max at start, and subtracted from on each individual player death (saves to player index)
    // Used: Tracked to allow revives
    public static string PlayerSpiralLevels = "PlayerSpiralLevels";
    public static List<float> GetPlayerSpiralLevels(this SaveState save, bool updateToMax = false)
    {
        // create default based on the max float value (which can fall back in arena to its default)
        float maxLvl = save.GetMaxSpiralLevel();
        List<float> defaultLvls = [maxLvl, maxLvl, maxLvl, maxLvl];
        // grab from savedata
        List<float> lvls = save.deathPersistentSaveData.GetSlugBaseData().TryGet(PlayerSpiralLevels, out List<float> levels) ? levels : defaultLvls;
        if (updateToMax)
        {
            for (int i = 0; i < lvls.Count; i++)
            {
                // Increase to max level but only if needed
                lvls[i] = save.GetMaxSpiralLevel() > lvls[i] ? save.GetMaxSpiralLevel() : lvls[i];
            }
        }
        return lvls;
    }
    public static float GetPlayerSpiralLevelFromIndex(this SaveState save, int index)
    {
        List<float> lvls = save.GetPlayerSpiralLevels();
        if (index > -1)
        {
            // Add usable indeces at end of list
            if (index >= lvls.Count)
            {
                for (int i = lvls.Count; i < index; i++)
                {
                    lvls.Add(lvls[0]);
                }
            }
            return lvls[index];
        }
        return 0;
    }
    public static void SetPlayerSpiralLevelFromIndex(this SaveState save, int index, float value)
    {
        List<float> lvls = save.GetPlayerSpiralLevels();
        if (index > lvls.Count)
        {
            for (int i = lvls.Count; i < index; i++)
            {
                lvls.Add(lvls[0]);
            }
        }
        lvls[index] = value;
    }
    #endregion

    // Assigned: Incremented by encountering The Dreamer (0.25 before 0.5, then 0.5 each subsequent encounter)
    // Used: Max amount of cycles/lives available (floors to int)
    public static string maxSpiralLevel = "MaxSpiralLevel";
    public static float GetMaxSpiralLevel(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(maxSpiralLevel, out float value) ? value : 0f;
    public static void SetMaxSpiralLevel(this SaveState save, float value) => save.deathPersistentSaveData.GetSlugBaseData().Set(maxSpiralLevel, value);
    public static float GetMaxSpiralLevel_CurrentOrArenaDefault(this SaveState save)
    {
        if (save is not null)
        {
            return save.GetMaxSpiralLevel();
        }
        return 1;
    }
    #endregion

    #region Death Stage

    // Death Stage Enum
    // Assigned: Based on MaxSpiralLevel
    // Used: Thanatosis cosmetics and misc progressions

    public static readonly string deathStage = "DeathStage";
    public enum DeathStage
    {
        None, // No progress
        Demised, // Thanatosis Sequence condition
        Dreaming, // Post-Thanatosis effects
        Rotting, // Chapter 2 Effects
        Hybrid // Epilogue Effects
    };
    public static DeathStage GetDeathStage(this SaveState save)
    {
        // Arena fallback
        if (save is null)
        {
            return DeathStage.Dreaming;
        }
        return save.deathPersistentSaveData.GetSlugBaseData().TryGet(deathStage, out DeathStage stage) ? stage : DeathStage.None;
    } 
    public static void SetDeathStage(this SaveState save, DeathStage stage) => save.deathPersistentSaveData.GetSlugBaseData().Set(deathStage, stage);
    #endregion

    #region Can Craft Flares

    // Assigned: Unused for now
    // Used: Enables flare crafting for Beacon

    public static readonly string canCraftFlares = "CanCraftFlares";
    public static bool GetCanCraftFlares(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(canCraftFlares, out bool craft) && craft;
    public static void SetCanCraftFlares(this SaveState save, bool value) => save.deathPersistentSaveData.GetSlugBaseData().Set(canCraftFlares, value);
    public static bool GetCanCraftFlares_CurrentOrArenaDefault(this SaveState save)
    {
        if (save is not null)
        {
            return save.GetCanCraftFlares();
        }
        return false;
    }
    #endregion

    #region Can Store Flares

    // Assigned: Unused for now
    // Used: Enabled flare storage for Beacon

    public static readonly string canStoreFlares = "CanStoreFlares";
    public static bool GetCanStoreFlares(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(canStoreFlares, out bool store) && store;
    public static void SetCanStoreFlares(this SaveState save, bool value) => save.deathPersistentSaveData.GetSlugBaseData().Set(canStoreFlares, value);
    #endregion

    #region Story Progress

    // Story Progress Enum
    // Assigned: Retroactively based on events in the campaign, should only progress
    // Used: Misc things in the campaign

    public const string storyProgress = "StoryProgress";
    public enum StoryProgress
    {
        Start,
        Prologue,
        Prologue_Intermission,
        Chapter1,
        Chapter1_Intermission,
        Chapter2,
        Chapter2_Intermission,
        Chapter3,
        Chapter3_Intermission,
        Epilogue,
        End1,
        End2
    };
    public static StoryProgress GetStoryProgress(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(storyProgress, out StoryProgress stage) ? stage : StoryProgress.Start;
    public static void SetStoryProgress(this SaveState save, StoryProgress stage) => save.deathPersistentSaveData.GetSlugBaseData().Set(storyProgress, stage);
    #endregion

    #region Campaign Completion

    // Bool
    // Assigned: For the playtest completion currently
    // Used: Spawns hud popup text saying you've completed the content in the playtest

    // PBv0.6: Assigned after thanatosis sequence completion

    public static readonly string completedBeacon = "CompletedBeacon";
    public static bool GetCompletedBeacon(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(completedBeacon, out bool completion) ? completion : false;
    public static void SetCompletedBeacon(this SaveState save, bool value) => save.deathPersistentSaveData.GetSlugBaseData().Set(completedBeacon, value);
    #endregion
}