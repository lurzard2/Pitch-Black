using IL.Menu;
using IL.Stove.Sample.Ownership;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static PitchBlack.MiscUtils;
using Random = UnityEngine.Random;

namespace PitchBlack;

public class DreamSpawnGraphics : VoidSpawnGraphics
{
    public DreamSpawnGraphics(PhysicalObject owner) : base(owner)
    {
        float angleDeterminer;
        float thickness;
        float conRadMinFac;
        float headConRad;

        float sizeFac = Mathf.Lerp(spawn.sizeFac, 0.5f + 0.5f * Random.value, Random.value);
        int antennaeCount;
        VoidSpawnGraphics.TailAntenna tailAntenna = null;
        VoidSpawnGraphics.FrontAntenna frontAntenna = null;
        int segments;
        int rigidSegs;
        float rigid;
        float conRad;
        float angle;
        float forceDirection;

        int encounterNumber = BeaconSaveData.GetDreamerEncountersNumber(owner.room.world.game.GetStorySession.saveState);

        if (IsVariant(spawn, Enums.DreamSpawnType.DreamAmoeba))
        {
            segments = Random.Range(4, 8);
            conRad = 12f * sizeFac;
            thickness = spawn.mainBody[spawn.mainBody.Length - 1].rad;
            angle = 0f;
            rigid = 0.1f * sizeFac;
            rigidSegs = 2;
            forceDirection = 2.2f;

            tailAntenna = new VoidSpawnGraphics.TailAntenna(this, totalSprites, segments, conRad, thickness, angle, rigid, rigidSegs, forceDirection);
            antennae.Add(tailAntenna);
            AddSubModule(antennae[antennae.Count - 1]);
        }
        else if (IsVariant(spawn, Enums.DreamSpawnType.DreamJelly))
        {
            antennaeCount = Random.Range(6, 8);
            angleDeterminer = Mathf.Lerp(Mathf.Lerp(8f, 24f, Random.value) * antennaeCount, Mathf.Lerp(16f, 45f, Random.value), Random.value);
            segments = Random.Range(4, 14);
            conRadMinFac = Mathf.Lerp(0.2f, 1f, Random.value);
            thickness = spawn.mainBody[spawn.mainBody.Length - 1].rad;
            rigid = 0.1f * sizeFac;
            rigidSegs = 2;
            forceDirection = 2.2f;

            for (int i = 0; i < antennaeCount; i++)
            {
                float targetIndex = i / (antennaeCount - 1);
                conRad = 12f * sizeFac * Mathf.Lerp(conRadMinFac, 1f, Mathf.Sin(targetIndex * Mathf.PI));
                angle = Mathf.Lerp(-angleDeterminer, angleDeterminer, targetIndex);

                tailAntenna = new VoidSpawnGraphics.TailAntenna(this, totalSprites, segments, conRad, thickness, angle, rigid, rigidSegs, forceDirection);
                antennae.Add(tailAntenna);
                AddSubModule(antennae[antennae.Count - 1]);
            }
        }
        else if (IsVariant(spawn, Enums.DreamSpawnType.DreamNoodle)
            || IsVariant(spawn, Enums.DreamSpawnType.DreamEater))
        {
            segments = Random.Range(1, 5);
            conRad = 12f * sizeFac;
            thickness = spawn.mainBody[spawn.mainBody.Length - 1].rad;
            rigid = 0.1f * sizeFac;
            rigidSegs = 2;
            forceDirection = 2.2f;

            tailAntenna = new VoidSpawnGraphics.TailAntenna(this, totalSprites, segments, conRad, thickness, 0f, rigid, rigidSegs, forceDirection);
            antennae.Add(tailAntenna);
            AddSubModule(antennae[antennae.Count - 1]);
        }
        else if (IsVariant(spawn, Enums.DreamSpawnType.DreamKin))
        {
            antennaeCount = Random.Range(0, Random.Range(0, encounterNumber));
            segments = Random.Range(0, Random.Range(0, encounterNumber));
            conRad = 12f * sizeFac;
            conRadMinFac = Mathf.Lerp(0.2f, 1f, Random.value);
            thickness = spawn.mainBody[spawn.mainBody.Length - 1].rad;
            rigid = 0.1f * sizeFac;
            rigidSegs = 2;
            forceDirection = 2.2f;

            for (int m = 0; m < antennaeCount; m++)
            {
                float targetIndex = m / (antennaeCount - 1);
                angleDeterminer = Mathf.Lerp(Mathf.Lerp(2f, 15f, Random.value) * antennaeCount, Mathf.Lerp(8f, 70f, Random.value), Random.value);
                conRad = 12f * sizeFac * Mathf.Lerp(conRadMinFac, 1f, Mathf.Sin(targetIndex * 3.1415927f));
                angle = Mathf.Lerp(-angleDeterminer, angleDeterminer, targetIndex);

                tailAntenna = new VoidSpawnGraphics.TailAntenna(this, totalSprites, segments, conRad, thickness, angle, rigid, rigidSegs, forceDirection);
                antennae.Add(tailAntenna);
                AddSubModule(antennae[antennae.Count - 1]);
            }
        }
        else
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    segments = Random.Range(3, 18);
                    conRad = 12f * sizeFac;
                    thickness = spawn.mainBody[spawn.mainBody.Length - 1].rad;
                    rigid = 0.1f * sizeFac;
                    rigidSegs = 2;
                    forceDirection = 2.2f;

                    tailAntenna = new VoidSpawnGraphics.TailAntenna(this, totalSprites, segments, conRad, thickness, 0f, rigid, rigidSegs, forceDirection);
                    antennae.Add(tailAntenna);
                    AddSubModule(antennae[antennae.Count - 1]);

                    break;
                case 1:

                    antennaeCount = Random.Range(2, Random.Range(2, 5));
                    angleDeterminer = Mathf.Lerp(Mathf.Lerp(2f, 15f, Random.value) * antennaeCount, Mathf.Lerp(8f, 70f, Random.value), Random.value);
                    segments = Random.Range(3, 18);
                    conRadMinFac = Mathf.Lerp(0.2f, 1f, Random.value);
                    thickness = spawn.mainBody[spawn.mainBody.Length - 1].rad;
                    rigid = 0.1f * sizeFac;
                    rigidSegs = 2;
                    forceDirection = 2.2f;

                    for (int j = 0; j < antennaeCount; j++)
                    {
                        float targetIndex = j / (float)(antennaeCount - 1);
                        conRad = 12f * sizeFac * Mathf.Lerp(conRadMinFac, 1f, Mathf.Sin(targetIndex * 3.1415927f));
                        angle = Mathf.Lerp(-angleDeterminer, angleDeterminer, targetIndex);

                        tailAntenna = new VoidSpawnGraphics.TailAntenna(this, totalSprites, segments, conRad, thickness, angle, rigid, rigidSegs, forceDirection);
                        antennae.Add(tailAntenna);
                        AddSubModule(antennae[antennae.Count - 1]);
                    }
                    break;

                case 2:

                    antennaeCount = Random.Range(2, 6);
                    rigid = Mathf.Lerp(0.1f, 1.8f, Random.value);
                    angleDeterminer = Mathf.Lerp(Mathf.Lerp(2f, 15f, Random.value) * antennaeCount, Mathf.Lerp(8f, 70f, Random.value), Random.value);
                    segments = Random.Range(3, Random.Range(5, 8));
                    rigidSegs = Random.Range(1, segments + 1);
                    forceDirection = Mathf.Lerp(1.5f, 7f, Random.value) * rigid / Mathf.Lerp(1f, rigidSegs, 0.5f);
                    float conRadMult = Mathf.Lerp(4f, 12f, Random.value) * sizeFac;
                    conRadMinFac = Mathf.Lerp(0.2f, 1f, Random.value);
                    thickness = 2f;

                    for (int k = 0; k < antennaeCount; k++)
                    {
                        float targetIndex = k / (antennaeCount - 1);
                        conRad = conRadMult * Mathf.Lerp(conRadMinFac, 1f, Mathf.Sin(targetIndex * 3.1415927f));
                        angle = Mathf.Lerp(-angleDeterminer, angleDeterminer, targetIndex);

                        tailAntenna = new VoidSpawnGraphics.TailAntenna(this, totalSprites, segments, conRad, thickness, angle, rigid, rigidSegs, forceDirection);
                        antennae.Add(tailAntenna);
                        AddSubModule(antennae[antennae.Count - 1]);
                    }
                    break;
                default:
                    break;
            }
        }

        antennaeCount = Random.Range(2, Random.Range(2, 7));
        segments = Random.Range(2, Random.Range(4, (int)Custom.LerpMap(spawn.mainBody.Length, 3f, 16f, 6f, 12f, 0.5f)));
        int frontRigidSegs = segments;
        if (Random.value < 0.5f)
        {
            frontRigidSegs = Random.Range(1, segments + 1);
        }
        headConRad = Mathf.Lerp(3f, 8f, Random.value);
        angleDeterminer = Mathf.Lerp(12f, 50f, Mathf.Pow(Random.value, 1.5f));
        forceDirection = Mathf.Lerp(2f, 7f, Random.value) / Mathf.Lerp(1f, frontRigidSegs, 0.5f);
        rigid = Mathf.Lerp(0.4f, 2.2f, Random.value);

        if (!IsVariant(spawn, Enums.DreamSpawnType.DreamJelly)
            && !IsVariant(spawn, Enums.DreamSpawnType.DreamKin))
        {
            thickness = 2f * sizeFac;

            for (int l = 0; l < antennaeCount; l++)
            {
                float targetIndex = l / (antennaeCount - 1);
                angle = Mathf.Lerp(-angleDeterminer, angleDeterminer, targetIndex);

                frontAntenna = new VoidSpawnGraphics.FrontAntenna(this, totalSprites, segments, headConRad, thickness, angle, rigid, frontRigidSegs, forceDirection);
                antennae.Add(frontAntenna);
                AddSubModule(antennae[antennae.Count - 1]);
            }
        }
        else if (IsVariant(spawn, Enums.DreamSpawnType.DreamKin))
        {
            antennaeCount = 4;
            segments = 5;
            thickness = 2f * sizeFac;

            for (int l = 0; l < antennaeCount; l++)
            {
                float targetIndex = l / (antennaeCount - 1);
                angle = Mathf.Lerp(-angleDeterminer, angleDeterminer, targetIndex);

                frontAntenna = new VoidSpawnGraphics.FrontAntenna(this, totalSprites, segments, headConRad, thickness, angle, rigid, frontRigidSegs, forceDirection);
                antennae.Add(frontAntenna);
                AddSubModule(antennae[antennae.Count - 1]);
            }
        }

        Reset();
    }

    public override void Update()
    {
        base.Update();
    }

    new private FShader BodyShader => Custom.rainWorld.Shaders["DreamSpawnBody"];
    new private FShader EffectShader => Custom.rainWorld.Shaders["RoseGlow"];
    new private FShader GlowShader => Custom.rainWorld.Shaders["FlatWaterLightBothSides"];

    new private void UpdateGlowSpriteColor(RoomCamera.SpriteLeaser sLeaser)
    {
        if (dayLightMode)
        {
            sLeaser.sprites[GlowSprite].color = Colors.SaturatedRose;
            return;
        }
        sLeaser.sprites[GlowSprite].color = Color.Lerp(Colors.SaturatedRose, Colors.Rose, Mathf.InverseLerp(0.3f, 0.9f, owner.room.Darkness(glowPos)));
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);

        sLeaser.sprites[BodyMeshSprite].shader = BodyShader;
        sLeaser.sprites[GlowSprite].shader = GlowShader;
        if (hasOwnGoldEffect)
        {
            sLeaser.sprites[EffectSprite].shader = EffectShader;
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        sLeaser.sprites[BodyMeshSprite].shader = BodyShader;
        sLeaser.sprites[GlowSprite].shader = GlowShader;
        if (hasOwnGoldEffect)
        {
            sLeaser.sprites[EffectSprite].shader = EffectShader;
        }
        UpdateGlowSpriteColor(sLeaser);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);

        UpdateGlowSpriteColor(sLeaser);
    }

}