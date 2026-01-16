using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace PitchBlack;

// Credit to Alduris' BetterWorkshopUploader for the json stuff I referenced

internal class PBRegionData
{
    public static string PBPath => Plugin.MOD_PATH;
    public static string FilePath => Path.Combine(PBPath, "pbworld.json");

    #region Data
    public List<Region> Regions { get; set; }
    public List<Region> CycleRegions { get; set; }

    public class Region
    {
        public string ID { get; set; }
        public List<Creature> Creatures { get; set; }
        public List<Dreamer> DreamerEncounters { get; set; }

        public class Creature
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string State { get; set; }
        }

        public class Dreamer
        {
            public int ID { get; set; }
            public string Room { get; set; }
            public bool Encountered { get; set; }
        }
    }
    #endregion


    public PBRegionData()
    {
        if (!File.Exists(FilePath))
        {

            Region region = new();
            region.ID = "TestRegion";
            region.Creatures = [];
            region.Creatures.Add(new()
            {
                ID = "-1",
                Name = "Spider",
                State = "Alive",
            });

            region.DreamerEncounters = [];
            region.DreamerEncounters.Add(new()
            {
                ID = -1,
                Room = "TestRoom",
                Encountered = false,
            });

            Regions = [];
            CycleRegions = [region];
        }
    }

    public static PBRegionData GetData()
    {
        PBRegionData data = JsonConvert.DeserializeObject<PBRegionData>(FilePath);
        return data;
    }

    public void Save()
    {
        string obj = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(FilePath, obj);
    }
}
        