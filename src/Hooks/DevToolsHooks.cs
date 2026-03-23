using System;
using System.Collections.Generic;
using DevInterface;
using Watcher;
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

        if (tp == Enums.PlacedObjectType.DreamerSpot || tp == Enums.PlacedObjectType.StillbornSpot)
        {
            rep = new EntityWarpRepresentation(self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString());
        }
        if (tp == Enums.PlacedObjectType.RiftSpot)
        {
            rep = new WarpPointToRoomRepresentation(self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString());
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
        if (self.type == Enums.PlacedObjectType.DreamerSpot || self.type == Enums.PlacedObjectType.StillbornSpot)
        {
            self.data = new EntityWarpData(self);
        }
        if (self.type == Enums.PlacedObjectType.RiftSpot)
        {
            self.data = new WarpPoint.WarpPointData(self);
        }
    }

    private static ObjectsPage.DevObjectCategories AddToDevObjectCatagory(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, ObjectsPage self, PlacedObject.Type type)
    {
        ObjectsPage.DevObjectCategories res = orig(self, type);
        if (type == Enums.PlacedObjectType.DreamerSpot
            || type == Enums.PlacedObjectType.RiftSpot
            || type == Enums.PlacedObjectType.RiftExitTarget
            || type == Enums.PlacedObjectType.StillbornSpot)
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
        if (DreamerPresence_Functions.timesToAssignDreamerPresence > 0)
        {
            DreamerPresence_Functions.InitDreamerRoomsToPresences(self);
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
            var obj = self.roomSettings.placedObjects[objects];

            if (self.game.IsStorySession)
            {
                if (obj.type == Enums.PlacedObjectType.DreamerSpot
                && dreamerPresence.TryGetValue(self.world, out var dreamerPresences))
                {
                    if (PBSaveData.GetDreamerEncounteredRooms(self.world.game.GetStorySession.saveState).Contains(self.abstractRoom.name))
                    {
                        PlaceWarp(self, objects);
                    }

                    for (int j = 0; j < dreamerPresences.Count; j++)
                    {
                        // Presence exists, which means it needs a dreamer
                        if (dreamerPresences[j].presenceSpawned && dreamerPresences[j].dreamerRoom == self.abstractRoom)
                        {
                            PlaceDreamer(self, objects, dreamerPresences[j]);

                            /* Prevent duplicates
                            * We can do this because its per room, and there is meant to be 1 instantiated Dreamer per encounte
                            */
                            if (dreamerPresences[j].myDreamer.obj != null)
                            {
                                break;
                            }
                        }
                    }
                }

                if (obj.type == Enums.PlacedObjectType.RiftSpot)
                {
                    RiftManager riftManager = new(self, obj, true);
                    MiscUtils.PlaceRift(riftManager);
                }

                if (obj.type == Enums.PlacedObjectType.StillbornSpot)
                {
                    Stillborn stillBorn = new(self, obj);
                    self.AddObject(stillBorn);
                }
            }
        }
    }

    #region Dreamer
    private static void PlaceDreamer(Room self, int objects, DreamerPresence presence)
    {
        logger.LogDebug($"DreamerSpot: Adding Dreamer to room since presence exists and presence room is loaded");

        Dreamer dreamer = new(self, self.roomSettings.placedObjects[objects]);
        self.AddObject(dreamer);
        presence.dreamerSpawned = true;
        presence.myDreamer = new(presence.dreamerRoom, presence.dreamerSpawned, dreamer);

        logger.LogDebug($"DreamerPresence: Dreamer active - {presence.dreamerSpawned}");
        logger.LogDebug($"DreamerSpawner: ROOM:{presence.myDreamer.abstractRoom.name} - {presence.myDreamer.hasSpawned} - {presence.myDreamer.obj}");
    }

    private static void PlaceWarp(Room self, int objects)
    {
        logger.LogDebug($"DreamerSpot: Dreamer already encountered and can spawn warp - Placing warp");

        DreamerBehavior.SpawnBackupWarpPoint(self, self.roomSettings.placedObjects[objects]);
    }
    #endregion
}