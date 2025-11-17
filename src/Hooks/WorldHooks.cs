using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PitchBlack;

public static class WorldHooks
{
    public static void UpdateDreamerMode(Room room, int newCamPos, RoomCamera rCam)
    {
        if (room == null)
        {
            return;
        }
        if (!DevToolsHooks.spawnedDreamer)
        {
            dreamerInRoomMode = false;
            return;
        }
        dreamerInRoomMode = true;
    }

    public static void RemoveDreamerRoomEffects(RoomCamera self)
    {
        bool activeDreamer = targetDreamIntensity > 0f && self.ghostMode > 0f;
        if (activeDreamer)
        {
            targetDreamIntensity -= 0.005f;
            self.ghostMode -= 0.005f;
            self.ghostMode = Mathf.Lerp(self.ghostMode, targetDreamIntensity, 0.06f);
        }
    }

    public static bool dreamerInRoomMode;
    public static float targetDreamIntensity;

    public static void Apply()
    {
        On.Region.ctor_string_int_int_RainWorldGame_Timeline += Region_ctor_string_int_int_RainWorldGame_Timeline;
        //On.RoomCamera.Update += RoomCamera_Update;
        On.RoomCamera.ApplyPositionChange += RoomCamera_ApplyPositionChange;
    }

    // Update dreamer's room effects
    private static void RoomCamera_ApplyPositionChange(On.RoomCamera.orig_ApplyPositionChange orig, RoomCamera self)
    {
        orig(self);
        UpdateDreamerMode(self.room, self.currentCameraPosition, self);
    }

    // Adding effects to rooms, using SpinningTop's radial effect
    private static void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera self)
    {
        orig(self);

        Creature creature = (self.followAbstractCreature != null) ? self.followAbstractCreature.realizedCreature : null;
        if (self.room != null && creature != null && creature is Player)
        {
            if (dreamerInRoomMode)
            {
                // SpinningTop's radial room effect is executed like this for Watcher
                int i = 0;
                while (i < self.room.updateList.Count)
                {
                    if (self.room.updateList[i] is Dreamer)
                    {
                        float distanceFromDreamer = Vector2.Distance((self.room.updateList[i] as Dreamer).placedObject.pos, creature.mainBodyChunk.pos);
                        targetDreamIntensity = Mathf.Lerp(0.11f, 1f, Mathf.InverseLerp(1500f, 0f, distanceFromDreamer));
                        if ((self.room.updateList[i] as Dreamer).conversation != null)
                        {
                            targetDreamIntensity = 1f;
                            break;
                        }
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
                self.ghostMode = Mathf.Lerp(self.ghostMode, targetDreamIntensity, 0.06f);
            }
            else
            {
                RemoveDreamerRoomEffects(self);
            }
        }
    }

    // Replace rot eye+effect color for Beacon
    private static void Region_ctor_string_int_int_RainWorldGame_Timeline(On.Region.orig_ctor_string_int_int_RainWorldGame_Timeline orig, Region self, string name, int firstRoomIndex, int regionNumber, RainWorldGame game, SlugcatStats.Timeline timelineIndex)
    {
        orig(self, name, firstRoomIndex, regionNumber, game, timelineIndex);

        if (timelineIndex != null && timelineIndex == Enums.Timeline.Beacon)
        {
            self.regionParams.corruptionEffectColor = RainWorld.RippleColor;
            self.regionParams.corruptionEyeColor = RainWorld.RippleColor;

            //Todo: Add conditional for Nightmare Rot when that's being developed.
        }
    }
}