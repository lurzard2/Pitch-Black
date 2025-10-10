using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public static class BeaconSaveData
{
    private static SlugBaseSaveData saveData;

    #region BeatenBeacon
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
    #endregion

    public static void Initialize(RainWorld rW)
    {
        saveData = rW.progression.miscProgressionData.GetSlugBaseData();
        

        saveData.TryGet(_beatenBeaconString, out _beatenBeacon);

    }
}

