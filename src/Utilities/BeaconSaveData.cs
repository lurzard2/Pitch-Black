#if false
using SlugBase.SaveData;
using System;
using System.Collections.Generic;

namespace PitchBlack;

// I'm unsure if this entirely works
public static class BeaconSaveData
{
    public static SlugBaseSaveData slugBaseSaveData;
    private static SaveState save;

    private const string _beatenBeaconString = "BeatenBeacon";
    private static bool _beatenBeacon = false;
    public static bool BeatenBeacon
    {
        get => _beatenBeacon;
        set
        {
            _beatenBeacon = value;
            slugBaseSaveData.Set(_beatenBeaconString, _beatenBeacon);
        }
    }
    private const string _canUseThanatosisString = "CanUseThanatosis";
    private static bool _canUseThanatosis = false;
    public static bool CanUseThanatosis
    {
        get => _canUseThanatosis;
        set
        {
            _canUseThanatosis = value;
            slugBaseSaveData.Set(_canUseThanatosisString, _canUseThanatosis);
        }
    }

    public static void Initialize(RainWorld rW)
    {
        slugBaseSaveData = rW.progression.miscProgressionData.GetSlugBaseData();

        slugBaseSaveData.TryGet(_beatenBeaconString, out _beatenBeacon);

        save.deathPersistentSaveData.GetSlugBaseData().TryGet(_canUseThanatosisString, out _canUseThanatosis);

    }

    public static bool MetDreamer(this SaveState save)
    {
        return save.deathPersistentSaveData.GetSlugBaseData().TryGet("MetDreamer", out bool metDreamer) && metDreamer;
    }
    
    public static List<int> DreamerEncounters(this SaveState save)
    {
        return save.deathPersistentSaveData.GetSlugBaseData().TryGet("DreamerEncounters", out List<int> dreamerEncounters) ? dreamerEncounters : [];
    }
}
#endif