using System;
using System.Collections.Generic;
using DevInterface;
using UnityEngine;

namespace PitchBlack;

public static class DevToolsHooks
{
    /// <summary>
    /// Effects and such need to be added to the 3 hooks
    /// - Room.Loaded to add the object
    /// - Room.NowViewed for backgrounds to apply a fix
    /// - RoomSettingsPage.DevEffectGetCategoryFromEffectType to add to correct catagory
    /// </summary> -Lur
    
    public static List<GhostWorldPresence> dreamerPresences = new List<GhostWorldPresence>();

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
        for (int num = 0; num < self.roomSettings.effects.Count; num++)
        {
            if (self.roomSettings.effects[num].type == Enums.RoomEffectType.ElsehowView)
            {
                self.AddObject(new ElsehowView(self, self.roomSettings.effects[num]));
            }
        }

        // iM LOSING MY FUCKING MIND omg - Based on L889 for SpinningTopSpot, I just want to spawn it
#if false
        for (int num2 = 0; num2 < self.roomSettings.placedObjects.Count; num2++)
        {
            if ((num2 != 1 || !self.roomSettings.placedObjects[num2].deactivatedByWarpFilter)
                && (num2 != 2 || self.roomSettings.placedObjects[num2].deactivatedByWarpFilter)
                && self.roomSettings.placedObjects[num2].active)
            {
                if (self.roomSettings.placedObjects[num2].type == Enums.PlacedObjectType.DreamerSpot
                    && self.game.IsStorySession)
                {
                    DreamerPresence dreamerWorldPresence = null;
                    for (int num3 = 0; num3 < dreamerPresences.Count; num3++)
                    {
                        if (dreamerPresences[num3].ghostRoom == self.abstractRoom)
                        {
                            dreamerWorldPresence = (DreamerPresence)dreamerPresences[num3];
                            break;
                        }
                    }
                    if (dreamerWorldPresence == null)
                    {
                        // Todo: Requires SpinningTopData equivalent to assign presence
                        //dreamerWorldPresence = new DreamerPresence(self.world, Enums.GhostID.Dreamer, self.roomSettings.placedObjects[num2].data as DreamerData).spawnIdentifier);
                        dreamerWorldPresence.ghostRoom = self.abstractRoom;
                        if (!BeaconSaveData.GetDreamerEncounters(self.game.GetStorySession.saveState).Contains(dreamerWorldPresence.dreamerSpawnId))
                        {
                            dreamerPresences.Add(dreamerWorldPresence);
                        }
                    }
                    if (!BeaconSaveData.GetDreamerEncounters(self.game.GetStorySession.saveState).Contains(dreamerWorldPresence.dreamerSpawnId))
                    {
                        //this.spawnedSpinningTop = true;
                        //this.AddObject(new SpinningTop(this, this.roomSettings.placedObjects[num11], ghostWorldPresence));
                    }
                }
            }
        }
#endif
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