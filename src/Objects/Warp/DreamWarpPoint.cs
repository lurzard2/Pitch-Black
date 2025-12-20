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
            if (blackListedCreatureTypes.Contains(creatureBlacklist[i]))
            {
                blackListedCreatureTypes.Add(creatureBlacklist[i]);
            }
        }
    }

    public override void Update(bool eu)
    {
        if (MiscUtils.IsVhosRegion(room.world.region.name))
        {
            Plugin.logger.LogDebug($"DreamWarpPoint Contains:");
            Plugin.logger.LogDebug($"> LOCKED?:{warpLocked}");
            Plugin.logger.LogDebug($"> STATE:{currentState}");
            Plugin.logger.LogDebug($"> ACTIVATED?:{activated} - ACTIVATION TIME:{activationTime}");
            Plugin.logger.LogDebug($"> TRIGGER TIME:{triggerTime} - TRIGGER ACTIVATION TIME:{triggerActivationTime}");
            Plugin.logger.LogDebug($"> GUARANTEE TRIGGER:{guaranteeTrigger}");
            Plugin.logger.LogDebug($"< CLOSES EARLY: {closesEarly}");
        }
        base.Update(eu);
    }
}
