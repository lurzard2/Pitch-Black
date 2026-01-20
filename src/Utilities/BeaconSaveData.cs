using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using static PitchBlack.Plugin;

namespace PitchBlack;

public static class BeaconSaveData
{
    /// <summary>
    /// Gets a SaveState from the game's StoryGameSession properly without breaking other types of sessions. Must Check if null.
    /// </summary>
    /// <param name="rwg">The RainWorldGame instance.</param>
    /// <returns>An instance of SaveState from GetStorySession. Returns null if GameSession is not a StoryGameSession</returns>
    public static SaveState GetSaveState(this RainWorldGame rwg, bool onlyForBeacon = false)
    {
        var storySession = rwg.GetStorySession;
        if (storySession != null)
        {
            if (onlyForBeacon && !MiscUtils.IsBeacon(storySession))
            {
                return null;
            }
            return storySession.saveState;
        }
        return null;
    }

    // Called in RWG ctor, to update savedata on cycle 0 depending on remix config
    public static void RemixUpdateSaveState(this SaveState beaconState)
    {
        if (ModOptions.DreamerEncounters > 0)
        {
            SetDreamerEncountersNumber(beaconState, ModOptions.DreamerEncounters);
        }

        if (ModOptions.ThanatosisEnabled)
        {
            SetCanUseThanatosis(beaconState, true);
            if (ModOptions.SkipThanatosisSequence)
            {
                SetHasUsedThanatosis(beaconState, true);
            }

            switch (ModOptions.ThanatosisVariant)
            {
                case 1:
                    // Starving
                    SetMaxSpiralLevel(beaconState, 1f);
                    SetSpiralLevel(beaconState, 1f);
                    break;
                case 2:
                    // Rot
                    SetMaxSpiralLevel(beaconState, 2f);
                    SetSpiralLevel(beaconState, 2f);
                    break;
                case 3:
                    // Hybrid
                    SetMaxSpiralLevel(beaconState, 4f);
                    SetMaxSpiralLevel(beaconState, 4f);
                    break;
                default: break;
            }
        }

        if (ModOptions.UsesFlareMechanics)
        {
            GetOrSetBool(beaconState, canCraftFlares, true);
            GetOrSetBool(beaconState, canStoreFlares, true);
        }
    }

    public static bool GetOrSetBool(this SaveState save, string key, bool? setValue = null)
    {
        var data = save.deathPersistentSaveData.GetSlugBaseData();
        if (setValue != null)
        {
            // Assigns new value to the key
            data.Set(key, setValue);
        }
        return data.TryGet(key, out bool value) ? value : false;
    }

    // For the playtest currently
    public static readonly string completedBeacon = "CompletedBeacon";
    public static bool GetCompletedBeacon(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(completedBeacon, out bool completion) ? completion : false;
    public static void SetCompletedBeacon(this SaveState save, bool value) => save.deathPersistentSaveData.GetSlugBaseData().Set(completedBeacon, value);

    #region Dreamer Encounters
    public static string dreamerEncountersNumber = "DreamerEncountersNumber";
    public static int GetDreamerEncountersNumber(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(dreamerEncountersNumber, out int encounters) ? encounters : 0;
    public static void SetDreamerEncountersNumber(this SaveState save, int value) => save.deathPersistentSaveData.GetSlugBaseData().Set(dreamerEncountersNumber, value);

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

    #region Thanatosis
    // ThanatosisUpdate() ability check, true if your max spiral is 0.5, don't check if you want to track the float though
    public static string canUseThanatosis = "CanUseThanatosis";
    public static bool GetCanUseThanatosis(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(canUseThanatosis, out bool thanatosis) && thanatosis;
    public static void SetCanUseThanatosis(this SaveState save, bool value) => save.deathPersistentSaveData.GetSlugBaseData().Set(canUseThanatosis, value);

    public static string hasUsedThanatosis = "HasUsedThanatosis";
    public static bool GetHasUsedThanatosis(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(hasUsedThanatosis, out bool usedThanatosis) && usedThanatosis;
    public static void SetHasUsedThanatosis(this SaveState save, bool value) => save.deathPersistentSaveData.GetSlugBaseData().Set(hasUsedThanatosis, value);
    #endregion

    #region Spiral Level
    // Spiral
    public static string spiralLevel = "SpiralLevel";
    public static float GetSpiralLevel(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(spiralLevel, out float value) ? value : 0f;
    public static void SetSpiralLevel(this SaveState save, float value) => save.deathPersistentSaveData.GetSlugBaseData().Set(spiralLevel, value);

    public static string minSpiralLevel = "MinSpiralLevel";
    public static float GetMinSpiralLevel(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(minSpiralLevel, out float value) ? value : 0f;
    public static void SetMinSpiralLevel(this SaveState save, float value) => save.deathPersistentSaveData.GetSlugBaseData().Set(minSpiralLevel, value);

    public static string maxSpiralLevel = "MaxSpiralLevel";
    public static float GetMaxSpiralLevel(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(maxSpiralLevel, out float value) ? value : 0f;
    public static void SetMaxSpiralLevel(this SaveState save, float value) => save.deathPersistentSaveData.GetSlugBaseData().Set(maxSpiralLevel, value);
    #endregion

    // Bools for flares
    public static readonly string canCraftFlares = "CanCraftFlares";
    public static readonly string canStoreFlares = "CanStoreFlares";
}