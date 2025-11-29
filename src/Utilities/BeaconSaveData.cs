using SlugBase.SaveData;
using System;
using System.Collections.Generic;

namespace PitchBlack;
public static class BeaconSaveData
{
    public static string dreamerEncountersNumber = "DreamerEncountersNumber";
    public static int GetDreamerEncountersNumber(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(dreamerEncountersNumber, out int encounters) ? encounters : 0;
    public static void SetDreamerEncountersNumber(this SaveState save, int value) => save.deathPersistentSaveData.GetSlugBaseData().Set(dreamerEncountersNumber, value);

    public static string dreamerEncountersRoom = "DreamerEncountersRoom";
    public static List<string> GetDreamerEncounteredRooms(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(dreamerEncountersRoom, out List<string> encounters) ? encounters : new List<string>();
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

    // ThanatosisUpdate() ability check, true if your max spiral is 0.5, don't check if you want to track the float though
    public static string canUseThanatosis = "CanUseThanatosis";
    public static bool GetCanUseThanatosis(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(canUseThanatosis, out bool thanatosis) && thanatosis;
    public static void SetCanUseThanatosis(this SaveState save, bool value) => save.deathPersistentSaveData.GetSlugBaseData().Set(canUseThanatosis, value);

    public static string hasUsedThanatosis = "HasUsedThanatosis";

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
}