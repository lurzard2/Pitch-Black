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

public class PBRegionData
{
    public static string PBPath => Plugin.MOD_PATH;
    public static string FilePath => Path.Combine(PBPath, "world", "pbworld.json");

    public List<Region> Regions { get; set; }

    /// <summary>
    /// JSON object
    /// </summary>
    public class Region
    {
        public string ID { get; set; }
        public List<Creature> Creatures { get; set; }
        public List<Dreamer> DreamerEncounters { get; set; }

        public class Creature
        {
            public EntityID ID { get; set; }
            public CreatureTemplate.Type Name { get; set; }
            public Cycle.State State { get; set; }
        }

        public class Dreamer
        {
            public int ID { get; set; }
            public string Room { get; set; }
            public bool Encountered { get; set; }
        }
    }

    public PBRegionData()
    {
    }

    public static PBRegionData TemplateData()
    {
        PBRegionData data = new();

        Region region = new()
        {
            ID = "TestRegion",
        };
        region.Creatures = [];
        region.DreamerEncounters = [];
        data.Regions = [region];

        return data;
    }

    public static PBRegionData LoadFromFile()
    {
        PBRegionData data = new();

        if (!File.Exists(FilePath))
        {
            data = TemplateData();
        }
        else
        {
            data = JsonConvert.DeserializeObject<PBRegionData>(File.ReadAllText(FilePath));
        }
        
        return data;
    }

    public void SaveToFile()
    {
        string obj = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(FilePath, obj);
    }

    public void OnLoadWorld(World world, List<AbstractRoom> roomsForDreamer)
    {
        int regionCount = Regions.Count;
        int failureToFindDataCount = 0;

        for (int i = 0; i < regionCount; i++)
        {
            var r = Regions[i];
            if (r.ID != world.name)
            {
                failureToFindDataCount++;
            }

            // We need to add a new region to the list because it doesn't exist
            if (failureToFindDataCount == regionCount)
            {
                Region region = new();
                region.ID = world.name;
                region.Creatures = [];
                region.DreamerEncounters = [];
                for (int j = 0; j < roomsForDreamer.Count; j++)
                {
                    region.DreamerEncounters.Add(new()
                    {
                        ID = j,
                        Room = roomsForDreamer[j].name,
                        Encountered = false,
                    });
                }
                Regions.Add(region);
                break;
            }
        }
    }
}
        