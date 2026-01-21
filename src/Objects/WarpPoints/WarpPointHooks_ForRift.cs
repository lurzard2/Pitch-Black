using EffExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher;
using RWCustom;

namespace PitchBlack;

public static class WarpPointHooks_ForRift
{
    public static void Apply()
    {
        On.Player.ApplyWarpFatigue += Player_ApplyWarpFatigue_MODIFY;
        // Temp removing warp fatigue completely from Beacon
        On.Region.HasWarpFatigueResistance += HasWarpFatigueResistence_MODIFY;
        On.Watcher.WarpTear.DrawSprites += WarpTear_DrawSprites_RIFT;
        On.Region.IsSentientRotRegion += Region_IsSentientRotRegion;
    }

    private static bool Region_IsSentientRotRegion(On.Region.orig_IsSentientRotRegion orig, string name)
    {
        return orig(name)
            || MiscUtils.IsDissolvedFieldsRegion(name);
    }

    private static void WarpTear_DrawSprites_RIFT(On.Watcher.WarpTear.orig_DrawSprites orig, WarpTear self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        Rift rift = null;

        // Find rift
        for (int i = 0; i < self.room.warpPoints.Count; i++)
        {
            var warpPoint = self.room.warpPoints[i];
            if (warpPoint is Rift)
            {
                rift = warpPoint as Rift;
            }
        }

        if (rift != null)
        {
            int dreamAssociation = MiscUtils.RiftAssociatedWithDreamscape(self.room, rift);
            FShader newShader = null;
            switch (dreamAssociation)
            {
                case 1:
                    // Override cosmetics and current shader
                    if (MiscUtils.IsDissolvedFieldsRegion(rift.Data.destRegion))
                    {
                        rift.Data.effectSettings.badWarpCosmetic = true;
                        newShader = Custom.rainWorld.Shaders["WarpTearBad"];
                        break;
                    }
                    newShader = Custom.rainWorld.Shaders["DreamWarpTear"];
                    break;
                case 2:
                    newShader = Custom.rainWorld.Shaders["IntoDreamWarpTear"];
                    break;
                default:
                    break;
            }
            sLeaser.sprites[0].shader = newShader != null ? newShader : sLeaser.sprites[0].shader;
        }
    }

    private static bool HasWarpFatigueResistence_MODIFY(On.Region.orig_HasWarpFatigueResistance orig, string name)
    {
        return orig(name)
            || MiscUtils.IsVhosRegion(name);
    }

    private static void Player_ApplyWarpFatigue_MODIFY(On.Player.orig_ApplyWarpFatigue orig, Player self, RainWorldGame game)
    {
        if (MiscUtils.IsBeacon(self))
        {
            self.warpExhausionTime = 0;
        }
        else
        {
            orig(self, game);
        }
    }
}
