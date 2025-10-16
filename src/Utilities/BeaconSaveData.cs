using SlugBase.SaveData;
using System;
using System.Collections.Generic;

namespace PitchBlack;

// I'm unsure if this entirely works
public static class BeaconSaveData
{
    private static SlugBaseSaveData saveData;

    private const string _beatenBeaconString = "BeatenBeacon";
    private static bool _beatenBeacon = false;
    public static bool BeatenBeacon
    {
        get => _beatenBeacon;
        set
        {
            _beatenBeacon = value;
            saveData.Set(_beatenBeaconString, _beatenBeacon);
        }
    }

    public static void Initialize(RainWorld rW)
    {
        saveData = rW.progression.miscProgressionData.GetSlugBaseData();
        // Ending tracking
        saveData.TryGet(_beatenBeaconString, out _beatenBeacon);
    }

    public static bool MetDreamer(this SaveState save)
    {
        return save.deathPersistentSaveData.GetSlugBaseData().TryGet("MetDreamer", out bool metDreamer) && metDreamer;
    }
    
    public static List<int> DreamerEncounters(this SaveState save)
    {
        return save.deathPersistentSaveData.GetSlugBaseData().TryGet("DreamerEncounters", out List<int> dreamerEncounters) ? dreamerEncounters : [];
    }

    //public static bool CanUseThanatosis(this SaveState save)
    //{
    //    return save.deathPersistentSaveData.GetSlugBaseData().TryGet("CanUseThanatosis", out bool _canUseThanatosis) && _canUseThanatosis;
    //}
}