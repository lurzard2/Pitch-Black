using Watcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PitchBlack;

// TODO:
// Replace some graphics
// Unique thingies later

public class DreamWarpPoint : WarpPoint
{
    public DreamWarpPoint(Room room, PlacedObject placedObject) : base(room, placedObject)
    {
        this.room = room;
        this.placedObject = placedObject;

        List<CreatureTemplate.Type> creatureBlacklist = new List<CreatureTemplate.Type>
        {
            Enums.CreatureTemplateType.Citizen,
            Enums.CreatureTemplateType.NightTerror
        };
        for (int i = 0; i < creatureBlacklist.Count; i++)
        {
            if (!blackListedCreatureTypes.Contains(creatureBlacklist[i]))
            {
                blackListedCreatureTypes.Add(creatureBlacklist[i]);
            }
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
    }
}
