using System;
using System.Collections.Generic;
using DevInterface;
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

    public static void Apply()
    {
        On.Room.NowViewed += Room_NowViewed;
        On.Room.Loaded += Room_Loaded;
        On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += RoomSettingsPage_DevEffectGetCategoryFromEffectType;
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

        // iM LOSING MY FUCKING MIND omg
        for (int objects = 0; objects < self.roomSettings.placedObjects.Count; objects++)
        {
            if (self.roomSettings.placedObjects[objects].type == Enums.PlacedObjectType.DreamerSpot
                && self.game.IsStorySession)
            {
                var dreamerRooms = BeaconSaveData.GetDreamerEncountersRoom(self.world.game.GetStorySession.saveState);
                // We have to ASSIGN this to a room so it isn't null: See World.SpawnGhost(), World.InitiateGeneralWeaverHintTrail(), World.InitiateWeaverPresence()
                var dreamerPresence = Dreamer.dreamerPresence;
                if (dreamerPresence != null && !dreamerRooms.Contains(self.abstractRoom.name))
                {
                    spawnedDreamer = true;
                    self.AddObject(new Dreamer(self.roomSettings.placedObjects[objects]));
                }
            }
        }
    }

    // Adding effect to Pitch-Black page in Devtools Effects
    private static RoomSettingsPage.DevEffectsCategories RoomSettingsPage_DevEffectGetCategoryFromEffectType(On.DevInterface.RoomSettingsPage.orig_DevEffectGetCategoryFromEffectType orig, RoomSettingsPage self, RoomSettings.RoomEffect.Type type)
    {
        RoomSettingsPage.DevEffectsCategories res = orig(self, type);
        if (type == Enums.RoomEffectType.ElsehowView)
        {
            res = Enums.RoomEffectType.PitchBlackCatagory;
        }
        return res;
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