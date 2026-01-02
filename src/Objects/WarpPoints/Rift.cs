using Watcher;
using System;
using System.Collections.Generic;
using static PitchBlack.Plugin;

namespace PitchBlack;

// TODO:
// Replace some graphics
// Unique thingies later

public class Rift : WarpPoint
{
    string s = "Rift:";

    public Rift(Room room, PlacedObject placedObject) : base(room, placedObject)
    {
    }

    public override void Update(bool eu)
    {
        logger.LogDebug($"{s} IM HERE!!! AND IM WORKING!!!");

        base.Update(eu);
    }

    public void AddToBlacklist(List<CreatureTemplate.Type> critBList, List<AbstractPhysicalObject.AbstractObjectType> objBList)
    {
        for (int i = 0; i < critBList.Count; i++)
        {
            if (!blackListedCreatureTypes.Contains(critBList[i]))
            {
                blackListedCreatureTypes.Add(critBList[i]);
            }
        }
        for (int j = 0;j < objBList.Count; j++)
        {
            if (!blackListedObjectTypes.Contains(objBList[j]))
            {
                blackListedObjectTypes.Add(objBList[j]);
            }
        }
    }
}
