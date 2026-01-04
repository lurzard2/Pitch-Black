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
    bool triggersInstantly;

    public Rift(Room room, PlacedObject placedObject, bool triggersInstantly = false) : base(room, placedObject)
    {
        this.triggersInstantly = triggersInstantly;

        if (triggersInstantly && BeaconSaveData.GetDreamerEncountersNumber(room.game.GetStorySession.saveState) > 1)
        {
            triggerTime = (float)((int)(triggerActivationTime - 1f));
            strongPull = true;
            guaranteeTrigger = true;
        }
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
