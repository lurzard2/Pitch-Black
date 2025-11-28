using System;
using System.Collections.Generic;
using DevInterface;
using RWCustom;
using UnityEngine;
using static PitchBlack.Plugin;

namespace PitchBlack;

/// <summary>
/// Effects and such need to be added to the 3 hooks
/// - Room.Loaded to add the object
/// - Room.NowViewed for backgrounds to apply a fix
/// - RoomSettingsPage.DevEffectGetCategoryFromEffectType to add to correct catagory
/// </summary> -Lur

public static class DevEffectHooks
{
    public static void Inject()
    {
        On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += AddToDevEffectCatagory;
        On.Room.NowViewed += Room_NowViewed;
    }

    // Background shader fix, seems mandatory for some things.
    private static void Room_NowViewed(On.Room.orig_NowViewed orig, Room self)
    {
        orig(self);
        for (int i = 0; i < self.roomSettings.effects.Count; i++)
        {
            if (self.roomSettings.effects[i].type == Enums.RoomEffectType.ElsehowView)
            {
                Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 1f);
            }
        }
    }

    private static RoomSettingsPage.DevEffectsCategories AddToDevEffectCatagory(On.DevInterface.RoomSettingsPage.orig_DevEffectGetCategoryFromEffectType orig, RoomSettingsPage self, RoomSettings.RoomEffect.Type type)
    {
        RoomSettingsPage.DevEffectsCategories res = orig(self, type);
        if (type == Enums.RoomEffectType.ElsehowView)
        {
            res = Enums.RoomEffectType.PitchBlackCatagory;
        }
        return res;
    }
}

public static class DevObjectHooks
{
    public static void Inject()
    {
        On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += AddToDevObjectCatagory;

        On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;
        On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;
    }

    private static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
    {
        if (pObj == null)
        {
            pObj = new PlacedObject(tp, null);
            pObj.pos = self.owner.room.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(UnityEngine.Random.value * 360f) * 0.2f;
            self.RoomSettings.placedObjects.Add(pObj);
        }

        PlacedObjectRepresentation rep = null;

        if (tp == Enums.PlacedObjectType.DreamerSpot)
        {
            rep = new DreamerSpotRepresentation(self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString());
        }

        if (rep != null)
        {
            self.tempNodes.Add(rep);
            self.subNodes.Add(rep);
        }

        // Call orig HERE
        orig(self, tp, pObj);
    }

    private static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
    {
        orig(self);
        if (self.type == Enums.PlacedObjectType.DreamerSpot)
        {
            self.data = new DreamerData(self);
        }
    }

    private static ObjectsPage.DevObjectCategories AddToDevObjectCatagory(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, ObjectsPage self, PlacedObject.Type type)
    {
        ObjectsPage.DevObjectCategories res = orig(self, type);
        if (type == Enums.PlacedObjectType.DreamerSpot)
        {
            res = Enums.PlacedObjectType.PitchBlackCatagory;
        }
        return res;
    }
}

public class DevToolsHooks

{

    public static void Apply()
    {
        DevEffectHooks.Inject(); 
        DevObjectHooks.Inject();

        On.Room.Loaded += Room_Loaded;
    }

    // Actually adds our effects and objects -Lur
    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig(self);
        if (DreamersHooks.timesToAssignDreamerPresence > 0)
        {
            DreamersHooks.InitDreamerRoomsToPresences(self);
        }
        LoadEffects(self);
        LoadObjects(self);
    }

    private static void LoadEffects(Room self)
    {
        for (int effects = 0; effects < self.roomSettings.effects.Count; effects++)
        {
            var type = self.roomSettings.effects[effects].type;
            if (type == Enums.RoomEffectType.ElsehowView)
            {
                self.AddObject(new ElsehowView(self, self.roomSettings.effects[effects]));
            }
        }
    }

    private static void LoadObjects(Room self)
    {
        for (int objects = 0; objects < self.roomSettings.placedObjects.Count; objects++)
        {
            if (self.roomSettings.placedObjects[objects].type == Enums.PlacedObjectType.DreamerSpot && self.game.IsStorySession)
            {
                // I didn't know bool methods could do that??? peak???
                bool flowControl = InitDreamerSpot(self, objects);
                if (!flowControl)
                {
                    // Go to the next object
                    continue;
                }
            }
        }
    }

    private static bool InitDreamerSpot(Room self, int objects)
    {
        if (!dreamerPresence.TryGetValue(self.world, out var dreamerPresences))
        {
            // Go back to LoadObjects() when we return false
            return false;
        }

        logger.LogDebug($"DreamerSpot: Found PlacedObject in room");
        var dreamerData = self.roomSettings.placedObjects[objects].data as DreamerData;

        foreach (var presence in dreamerPresences)
        {
            if (presence.presenceSpawned && presence.dreamerRoom == self.abstractRoom)
            {
                logger.LogDebug($"DreamerSpot: Adding Dreamer to room since presence exists and presence room is loaded");
                self.AddObject(new Dreamer(self, self.roomSettings.placedObjects[objects]));
                presence.dreamerSpawned = true;
                logger.LogDebug($"DreamerPresence: Dreamer active - {presence.dreamerSpawned}");
                return false;
            }
            else if (dreamerData.destRoom != null)
            {
                logger.LogDebug($"DreamerSpot: Dreamer already encountered and can spawn warp - Placing warp");
                Dreamer.SpawnBackupWarpPoint(self, self.roomSettings.placedObjects[objects]);
                return false;
            }
        }

        // Bool has to return a value, why not be true
        return true;
    }
}