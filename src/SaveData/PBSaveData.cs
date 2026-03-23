using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PitchBlack;

public static partial class PBSaveData
{
    /// <summary>
    /// Gets a SaveState from the game's StoryGameSession properly without breaking other types of sessions. Must Check if null.
    /// </summary>
    /// <param name="rwg">The RainWorldGame instance.</param>
    /// <returns>An instance of SaveState from GetStorySession. Returns null if GameSession is not a StoryGameSession</returns>
    public static bool TryGetSaveState(this RainWorldGame rwg, out SaveState saveState)
    {
        saveState = rwg.IsStorySession ? rwg.GetStorySession.saveState : null;
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

    // -------- SAVEDATA --------

    // Bool
    // Restrict spawning thanatosis' tutorial past completion.
    #region Completed Thanatosis Tutorial
    private static string CompletedThanatosisTutorial = nameof(CompletedThanatosisTutorial);
    public static bool GetCompletedThanatosisTutorial(this SaveState s) => s.deathPersistentSaveData.GetSlugBaseData().TryGet(CompletedThanatosisTutorial, out bool x) ? x : false;
    public static void SetCompletedThanatosisTutorial(this SaveState s, bool x) => s.deathPersistentSaveData.GetSlugBaseData().Set(CompletedThanatosisTutorial, x);
    #endregion

    // Int
    // Assigned: +1 from encountering The Dreamer
    // Used: Chooses Dreamer's conversation ID, based on visits instead of place
    #region Dreamer Encounters Number
    public static string dreamerEncountersNumber = "DreamerEncountersNumber";
    public static int GetDreamerEncountersNumber(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(dreamerEncountersNumber, out int encounters) ? encounters : 0;
    public static void SetDreamerEncountersNumber(this SaveState save, int value) => save.deathPersistentSaveData.GetSlugBaseData().Set(dreamerEncountersNumber, value);
    #endregion

    // List<string>
    // Assigned: Entries added from encountering The Dreamer
    // Data: Name of the room they were encountered in
    // Used: Determining whether Dreamer should spawn in the room, or something else.
    #region Dreamer Encounters Room
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

    // Bool
    // Assigned: From finishing the Thanatosis Sequence, which is when the player unlocks Thanatosis
    // Used: Checking if the player has unlocked Thanatosis
    #region Can Use Thanatosis
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

    // Bool
    // Assigned: From finishing the Thanatosis Sequence
    // Used: Checking if false, which is before the Thanatosis Sequence has ever completed. Used for creating the Thanatosis Sequence conditionally
    #region Has Used Thanatosis
    public static string hasUsedThanatosis = "HasUsedThanatosis";
    public static bool GetHasUsedThanatosis(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(hasUsedThanatosis, out bool usedThanatosis) && usedThanatosis;
    public static void SetHasUsedThanatosis(this SaveState save, bool value) => save.deathPersistentSaveData.GetSlugBaseData().Set(hasUsedThanatosis, value);
    #endregion

    // General purpose is to influence beacon's mechanics and such.
    // Assigned: Incremented by encountering The Dreamer (0.25 before 0.5, then 0.5 each subsequent encounter)
    // Used: Max amount of cycles/lives available (floors to int)
    #region Spiral Level
    public static float maximumSpiralLimit = 5;
    public static string maxSpiralLevel = "MaxSpiralLevel";
    public static float GetMaxSpiralLevel(this SaveState save)
    {
        // Arena fallback value
        if (save is null)
        {
            return 1;
        }
        return save.deathPersistentSaveData.GetSlugBaseData().TryGet(maxSpiralLevel, out float value) ? value : 0f;
    }
    public static void SetMaxSpiralLevel(this SaveState save, float value) => save.deathPersistentSaveData.GetSlugBaseData().Set(maxSpiralLevel, value);
    #endregion

    // Assigned: Unused for now
    // Used: Enables flare crafting for Beacon
    #region Can Craft Flares
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

    // Assigned: Unused for now
    // Used: Enabled flare storage for Beacon
    #region Can Store Flares
    public static readonly string canStoreFlares = "CanStoreFlares";
    public static bool GetCanStoreFlares(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(canStoreFlares, out bool store) && store;
    public static void SetCanStoreFlares(this SaveState save, bool value) => save.deathPersistentSaveData.GetSlugBaseData().Set(canStoreFlares, value);
    #endregion

    // Bool
    // Assigned: For the playtest completion currently
    // Used: Spawns hud popup text saying you've completed the content in the playtest
    // PBv0.6: Assigned after thanatosis sequence completion
    #region Campaign Completion
    public static readonly string completedBeacon = "CompletedBeacon";
    public static bool GetCompletedBeacon(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(completedBeacon, out bool completion) ? completion : false;
    public static void SetCompletedBeacon(this SaveState save, bool value) => save.deathPersistentSaveData.GetSlugBaseData().Set(completedBeacon, value);
    #endregion
}