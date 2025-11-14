using System;
using System.Collections.Generic;
using DevInterface;
using RWCustom;
using UnityEngine;

namespace PitchBlack;

public class DevToolsHooks
{
    /// <summary>
    /// Effects and such need to be added to the 3 hooks
    /// - Room.Loaded to add the object
    /// - Room.NowViewed for backgrounds to apply a fix
    /// - RoomSettingsPage.DevEffectGetCategoryFromEffectType to add to correct catagory
    /// </summary> -Lur

    public static bool spawnedDreamer;
    public static DreamerPresence dreamerPresence = null;

    public static void Apply()
    {
        On.Room.NowViewed += Room_NowViewed;
        On.Room.Loaded += Room_Loaded;
        On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += RoomSettingsPage_DevEffectGetCategoryFromEffectType;
        On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += ObjectsPage_DevObjectGetCategoryFromPlacedType;
        On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;
        On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;
    }

    // Actually adds our effects' objects -Lur
    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig(self);

        for (int effects = 0; effects < self.roomSettings.effects.Count; effects++)
        {
            if (self.roomSettings.effects[effects].type == Enums.RoomEffectType.ElsehowView)
            {
                self.AddObject(new ElsehowView(self, self.roomSettings.effects[effects]));
            }
        }

        for (int objects = 0; objects < self.roomSettings.placedObjects.Count; objects++)
        {
            if (self.roomSettings.placedObjects[objects].type == Enums.PlacedObjectType.DreamerSpot
                && self.game.IsStorySession)
            {
                if (dreamerPresence == null)
                {
                    dreamerPresence = new DreamerPresence(self.world, self.abstractRoom, (self.roomSettings.placedObjects[objects].data as DreamerData).spawnIdentifier);
                }
                var dreamerRooms = BeaconSaveData.GetDreamerEncountersRoom(self.world.game.GetStorySession.saveState);
                // We have to ASSIGN this to a room so it isn't null: See World.SpawnGhost(), World.InitiateGeneralWeaverHintTrail(), World.InitiateWeaverPresence()
                if (!dreamerRooms.Contains(self.abstractRoom.name) && dreamerPresence.dreamerRoom == self.abstractRoom)
                {
                    spawnedDreamer = true;
                    self.AddObject(new Dreamer(self, self.roomSettings.placedObjects[objects]));
                }
                else
                {
                    Dreamer.SpawnBackupWarpPoint(self, self.roomSettings.placedObjects[objects]);
                }
            }
        }
    }

    #region Catagories
    private static RoomSettingsPage.DevEffectsCategories RoomSettingsPage_DevEffectGetCategoryFromEffectType(On.DevInterface.RoomSettingsPage.orig_DevEffectGetCategoryFromEffectType orig, RoomSettingsPage self, RoomSettings.RoomEffect.Type type)
    {
        RoomSettingsPage.DevEffectsCategories res = orig(self, type);
        if (type == Enums.RoomEffectType.ElsehowView)
        {
            res = Enums.RoomEffectType.PitchBlackCatagory;
        }
        return res;
    }
    private static ObjectsPage.DevObjectCategories ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, ObjectsPage self, PlacedObject.Type type)
    {
        ObjectsPage.DevObjectCategories res = orig(self, type);
        if (type == Enums.PlacedObjectType.DreamerSpot)
        {
            res = Enums.PlacedObjectType.PitchBlackCatagory;
        }
        return res;
    }
    #endregion

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
    
    
}