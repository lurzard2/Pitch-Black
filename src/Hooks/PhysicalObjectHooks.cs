using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PitchBlack.MiscUtils;
using RWCustom;

namespace PitchBlack;

public static class VoidSpawnHooks
{
    private static FShader BodyShader => Custom.rainWorld.Shaders["DreamSpawnBody"];
    private static FShader EffectShader => Custom.rainWorld.Shaders["RoseGlow"];
    private static FShader GlowShader => Custom.rainWorld.Shaders["FlatWaterLightBothSides"];

    public static bool IsDreamSpawn(VoidSpawn self)
    {
        if (self.variant == Enums.DreamSpawnType.DreamSpawn
            || self.variant == Enums.DreamSpawnType.DreamAmoeba
            || self.variant == Enums.DreamSpawnType.DreamJelly
            || self.variant == Enums.DreamSpawnType.DreamNoodle
            || self.variant == Enums.DreamSpawnType.DreamEater
            || self.variant == Enums.DreamSpawnType.DreamKin)
        {
            return true;    
        }
        return false;
    }

    public static void Inject()
    {
        On.VoidSpawn.GenerateBody += VoidSpawn_GenerateBody;
        On.VoidSpawnGraphics.Antenna.DrawSprites += Antenna_DrawSprites;
        On.Watcher.WarpPoint.ctor += WarpPoint_ctor;

    }

    private static void WarpPoint_ctor(On.Watcher.WarpPoint.orig_ctor orig, Watcher.WarpPoint self, Room room, PlacedObject placedObject)
    {
        orig(self, room, placedObject);
        if (!self.blackListedObjectTypes.Contains(Enums.AbstractObjectType.DreamSpawn))
        {
            self.blackListedObjectTypes.Add(Enums.AbstractObjectType.DreamSpawn);
        }
    }

    private static void Antenna_DrawSprites(On.VoidSpawnGraphics.Antenna.orig_DrawSprites orig, VoidSpawnGraphics.Antenna self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (IsDreamSpawn(self.vsGraphics.spawn))
        {
            sLeaser.sprites[self.firstSprite].shader = BodyShader;
        }
    }

    // We're gonna have to make our own implementation seperate from , cause it isn't virtual
    private static void VoidSpawn_GenerateBody(On.VoidSpawn.orig_GenerateBody orig, VoidSpawn self)
    {
        if (IsDreamSpawn(self))
        {
            DreamSpawn_GenerateBody(self);
        }
        else
        {
            orig(self);
        }
    }

    private static void DreamSpawn_GenerateBody(VoidSpawn self)
    {
        // Defaults
        int segments = Random.Range(3, Random.Range(3, 16));
        int index = 0;
        List<BodyChunk> chunks = new List<BodyChunk>();
        List<PhysicalObject.BodyChunkConnection> connections = new List<PhysicalObject.BodyChunkConnection>();
        float sizeMult = 1f;
        float length = Mathf.Lerp(3f, 8f, Random.value);

        if (IsVariant(self, Enums.DreamSpawnType.DreamAmoeba))
        {
            sizeMult = 2f;
            segments = Random.Range(5, 8);
            self.mainChunkFactor = 0.2f;
        }
        else if (IsVariant(self, Enums.DreamSpawnType.DreamJelly))
        {
            segments = Random.Range(3, 4);
        }
        else if (IsVariant(self, Enums.DreamSpawnType.DreamNoodle))
        {
            sizeMult = Random.Range(0.75f, 1.25f);
            segments = Random.Range(6, 10);
        }
        else if (IsVariant(self, Enums.DreamSpawnType.DreamEater))
        {
            sizeMult = Random.Range(0.25f, 0.75f);
            length = Mathf.Lerp(1f, 6f, Random.value);
            segments = Random.Range(5, 8);
        }
        else if (IsVariant(self, Enums.DreamSpawnType.DreamKin))
        {
            sizeMult = 1.25f;
            length = 7;
            segments = 7;
        }

        // Idk what these do, someone edit the names so they make sense
        float num = Mathf.Lerp(Mathf.Lerp(0.5f, 4f, Random.value), length / 2f, Random.value);
        float num2 = Mathf.Lerp(0.1f, 0.7f, Random.value);

        self.sizeFac = Mathf.Lerp(0.5f, 1.2f, Random.value) * sizeMult;
        self.swimSpeed = Mathf.Lerp(0.5f, 1f, Random.value);
        self.dominance = Mathf.InverseLerp(0f, 2.4f, self.sizeFac);
        self.dominance *= Mathf.InverseLerp(3f, 8f, (float)segments);
        for (int i = 0; i < segments; i++)
        {
            float head = i / (segments - 1);
            float rad = Mathf.Lerp(Mathf.Lerp(length, num, head), Mathf.Lerp(num, length, Mathf.Sin(Mathf.Pow(head, num2) * 3.1415927f)), 0.5f) * self.sizeFac;
            chunks.Add(new BodyChunk(self, index, default, rad, rad * 0.1f));
            if (i > 0)
            {
                connections.Add(new PhysicalObject.BodyChunkConnection(chunks[i - 1], chunks[i], Mathf.Lerp((chunks[i - 1].rad + chunks[i].rad) * 1.25f, Mathf.Max(chunks[i - 1].rad, chunks[i].rad), 0.5f), PhysicalObject.BodyChunkConnection.Type.Normal, 1f, -1f));
            }
            index++;
        }
        self.mainBody = chunks.ToArray();
        self.bodyChunks = chunks.ToArray();
        self.bodyChunkConnections = connections.ToArray();
    }
}

public class FlareBombHooks
{
    public static void Inject()
    {

        On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
        On.FlareBomb.DrawSprites += FlareBomb_DrawSprites;
        On.FlareBomb.HitByExplosion += FlareBomb_HitByExplosion;
        On.FlareBomb.Update += FlareBomb_Update;
    }

    private static void FlareBomb_Update(On.FlareBomb.orig_Update orig, FlareBomb self, bool eu)
    {
        orig(self, eu);

        //<Flarebomb stunning and KILLING creatures>
    }

    /// <summary>
    /// Prevent stored flares from detonating, which otherwise would break the storage slot.
    /// </summary>
    private static void FlareBomb_HitByExplosion(On.FlareBomb.orig_HitByExplosion orig, FlareBomb self, float hitFac, Explosion explosion, int hitChunk)
    {
        if (self.mode != Weapon.Mode.OnBack)
        {
            orig(self, hitFac, explosion, hitChunk);
        }
    }

    /// <summary>
    /// Colors FlareBomb glow sprite appropriately when thrown by Beacon.
    /// </summary>
    private static void FlareBomb_DrawSprites(On.FlareBomb.orig_DrawSprites orig, FlareBomb self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (MiscUtils.IsBeacon(self.thrownBy))
        {
            sLeaser.sprites[2].color = new Color(0.4f, 0f, 1f);
        }
    }

    /// <summary>
    /// Makes Scavengers not interested in stealing flares from beacon's storage if option is disabled.
    /// Implements scavStealing configurable from ModOptions.cs
    /// [spinch]
    /// </summary>
    private static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
    {
        var val = orig(self, obj, weaponFiltered);

        if (!ModOptions.scavStealing.Value)
        {
            if (obj is FlareBomb flarebomb && self.scavenger.room != null)
            {
                foreach (var abstrCrit in self.scavenger.room.game.Players)
                {
                    if (abstrCrit.realizedCreature == null)
                    {
                        continue;
                    }
                    if (Plugin.scugCWT.TryGetValue(abstrCrit.realizedCreature as Player, out ScugCWT c) && c is BeaconCWT beaconCWT && beaconCWT.storage.storedFlares.Contains(flarebomb))
                    {
                        return 0;
                    }
                }
            }
        }

        return val;
    }
}

public static class PhysicalObjectHooks
{
    public static void Apply()
    {
        FlareBombHooks.Inject();
        VoidSpawnHooks.Inject();

        On.AbstractPhysicalObject.Realize += AbstractPhysicalObject_Realize;
    }

    private static void AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        orig(self);

        if (self.type == Enums.AbstractObjectType.DreamSpawn)
        {
            // DreamSpawn as VoidSpawn object
            bool realizedRoom = self.Room.realizedRoom != null;
            float voidMeltInRoom = realizedRoom ? self.Room.realizedRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt) : 0f;
            bool daylightMode = realizedRoom && VoidSpawnKeeper.DayLightMode(self.Room.realizedRoom);
            self.realizedObject = new VoidSpawn(self, voidMeltInRoom, daylightMode, Enums.DreamSpawnType.DreamSpawn);
            return;
        }
    }
}