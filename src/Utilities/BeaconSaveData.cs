using SlugBase.SaveData;
using System;
using System.Collections.Generic;

namespace PitchBlack;
public static class BeaconSaveData
{
    // Dreamer needs this to spawn
    public static string dreamerEncounters = "DreamerEncounters";
    public static List<int> GetDreamerEncounters(this SaveState save) => save.deathPersistentSaveData.GetSlugBaseData().TryGet(dreamerEncounters, out List<int> encounters) ? encounters : null;
    //public static void SetDreamerEncounters(this SaveState save, List<int> value) => save.deathPersistentSaveData.GetSlugBaseData().Set(dreamerEncounters, value);

    // ThanatosisUpdate() ability check
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