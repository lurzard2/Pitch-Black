using HUD;
using IL.ScavengerCosmetic;
using Newtonsoft.Json.Linq;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Watcher;

namespace PitchBlack;
/// <summary>
/// Entire class referencing VoidWeaver + Ghost implementations of CosmeticSprite
/// 
/// - Todo -
/// * Remove from room code logic and methods
/// * VoidWeaver.Whisker implementations
/// * Deactivation logic
/// * Conversationstuff
/// 
/// - Notes -
/// "flip" is for x rotation of graphics, the visible turning effect
/// 
/// </summary>
public class Dreamer : CosmeticSprite, Conversation.IOwnAConversation
{
    public string ReplaceParts(string s)
    {
        return s;
    }

    public void SpecialEvent(string eventName)
    {
    }

    #region Sprite Gets
    public int LightSprite
    {
        get
        {
            return 0;
        }
    }

    public int BodyMeshSprite
    {
        get
        {
            return behindBodySprites;
        }
    }

    public int ButtockSprite(int side)
    {
        return behindBodySprites + 1 + side;
    }
    public int ThightSprite(int side)
    {
        return behindBodySprites + 3 + side;
    }

    public int LowerLegSprite(int side)
    {
        return behindBodySprites + 5 + side;
    }

    public int NeckConnectorSprite
    {
        get
        {
            return behindBodySprites + 7;
        }
    }

    public int HeadMeshSprite
    {
        get
        {
            return behindBodySprites + 8;
        }
    }

    public int DistortionSprite
    {
        get
        {
            return behindBodySprites + 9;
        }
    }
    #endregion

    public DreamerData SpecialData
    {
        get
        {
            return placedObject.data as DreamerData;
        }
    }

    public Dreamer(Room room, PlacedObject placedObject)
    {
        this.placedObject = placedObject;
        pos = placedObject.pos;
        dreamSpawnCaught = BeaconSaveData.GetDreamerEncountersNumber(room.world.game.GetStorySession.saveState);
        for (int i = 0; i < dreamSpawnCaught; i++)
        {
            MiscUtils.MaterializeDreamSpawn(room, headPos, Enums.DreamSpawnSource.Dreamcatcher, default, true);
        }
        headPos = pos;

        scale = 0.5f;
        UnityEngine.Random.State state = UnityEngine.Random.state;
        UnityEngine.Random.InitState(0);
        LoadElement("ghostScales");
        LoadElement("ghostPlates");
        LoadElement("ghostBand");
        UnityEngine.Random.state = state;

        spine = new Part[spineSegments];
        for (int i = 0; i < spine.Length; i++)
        {
            spine[i] = new Part(scale);
        }
        legs = new Part[2, 3];
        for (int j = 0; j < legs.GetLength(0); j++)
        {
            for (int k = 0; k < legs.GetLength(1); k++)
            {
                legs[j, k] = new Part(scale);
            }
        }

        // Sprite array delegating and adding sprites, very convoluted.
        this.totalSprites = 1;
        this.rags = new Rags(this, this.totalSprites);
        this.behindBodySprites = 1 + this.rags.totalSprites;
        this.totalSprites = this.behindBodySprites + this.totalStaticSprites;
        this.chains = new Chains(this, this.totalSprites);
        this.totalSprites += this.chains.totalSprites;

        // Spawns with varied position each time
        sinBob = UnityEngine.Random.value;
        Reset();
    }

    public void Reset()
    {
        for (int i = 0; i < spine.Length; i++)
        {
            spine[i].pos = pos + Custom.RNV();
            spine[i].lastPos = spine[i].pos;
            spine[i].vel *= 0f;
        }
        for (int j = 0; j < legs.GetLength(0); j++)
        {
            for (int k = 0; k < legs.GetLength(1); k++)
            {
                legs[j, k].pos = pos + Custom.RNV();
                legs[j, k].lastPos = legs[j, k].pos;
                legs[j, k].vel *= 0f;
            }
        }
        chains.Reset(pos);
        rags.Reset(pos);
        flip = defaultFlip;
        flipFrom = defaultFlip;
        flipTo = defaultFlip;
        flipProg = 1f;
        flipSpeed = 1f;
    }

    #region Parts of Graphics
    /// <summary>
    /// Base class for adding parts (spine + legs)
    /// </summary>
    public class Part
    {
        public Part(float scale)
        {
            this.scale = scale;
        }

        public void Update()
        {
            lastPos = pos;
            pos += vel;
            vel += randomMovement * 1.4f * scale;
            randomMovement = Vector2.ClampMagnitude(randomMovement + Custom.RNV() * UnityEngine.Random.value * 0.1f, 1f);
        }

        public Vector2 pos;
        public Vector2 lastPos;
        public Vector2 vel;
        private Vector2 randomMovement;
        public float scale;
    }

    /// <summary>
    /// Tentacle looking sprites, but they're actually loose cloth
    /// </summary>
    public class Rags
    {
        public Rags(Dreamer dreamer, int firstSprite)
        {
            this.dreamer = dreamer;
            this.firstSprite = firstSprite;
            conRad = 30f * dreamer.scale;
            int segmentCount = 6;
            segments = new Vector2[segmentCount][,];
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i] = new Vector2[UnityEngine.Random.Range(7, 27), 7];
            }
            totalSprites = segments.Length;
        }

        public void Reset(Vector2 resetPos)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                for (int j = 0; j < segments[i].GetLength(0); j++)
                {
                    segments[i][j, 0] = resetPos + Custom.RNV();
                    segments[i][j, 1] = segments[i][j, 0];
                    segments[i][j, 2] *= 0f;
                }
            }
        }

        public void Update()
        {
            for (int i = 0; i < segments.Length; i++)
            {
                for (int j = 0; j < segments[i].GetLength(0); j++)
                {
                    segments[i][j, 1] = segments[i][j, 0];
                    segments[i][j, 0] += segments[i][j, 2];
                    segments[i][j, 2] *= 0.999f;
                    segments[i][j, 2] += Custom.RNV() * 0.2f * dreamer.scale;
                    segments[i][j, 5] = segments[i][j, 4];
                    segments[i][j, 4] = (segments[i][j, 4] + segments[i][j, 6] * 0.05f).normalized;
                    segments[i][j, 6] = (segments[i][j, 6] + Custom.RNV() * UnityEngine.Random.value * (segments[i][j, 2].magnitude / (dreamer.scale * 3f))).normalized;
                }
                for (int k = 0; k < segments[i].GetLength(0); k++)
                {
                    if (k > 0)
                    {
                        Vector2 normalized = (segments[i][k, 0] - segments[i][k - 1, 0]).normalized;
                        float num = Vector2.Distance(segments[i][k, 0], segments[i][k - 1, 0]);
                        segments[i][k, 0] += normalized * (conRad - num) * 0.5f;
                        segments[i][k, 2] += normalized * (conRad - num) * 0.5f;
                        segments[i][k - 1, 0] -= normalized * (conRad - num) * 0.5f;
                        segments[i][k - 1, 2] -= normalized * (conRad - num) * 0.5f;
                        if (k > 1)
                        {
                            normalized = (segments[i][k, 0] - segments[i][k - 2, 0]).normalized;
                            segments[i][k, 2] += normalized * 0.2f;
                            segments[i][k - 2, 2] -= normalized * 0.2f;
                        }
                        if (k < segments[i].GetLength(0) - 1)
                        {
                            segments[i][k, 4] = Vector3.Slerp(segments[i][k, 4], (segments[i][k - 1, 4] + segments[i][k + 1, 4]) / 2f, 0.05f);
                            segments[i][k, 6] = Vector3.Slerp(segments[i][k, 6], (segments[i][k - 1, 6] + segments[i][k + 1, 6]) / 2f, 0.05f);
                        }
                    }
                    else
                    {
                        segments[i][k, 0] = AttachPos(i, 1f);
                    }
                }
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            bool flag = false;
            for (int i = 0; i < segments.Length; i++)
            {
                sLeaser.sprites[firstSprite + i] = TriangleMesh.MakeLongMesh(segments[i].GetLength(0), false, true);
                sLeaser.sprites[firstSprite + i].shader = rCam.room.game.rainWorld.Shaders[flag ? "DreamerRagRipple" : "DreamerRag"];
                sLeaser.sprites[firstSprite + i].alpha = 0.3f + 0.7f * Mathf.InverseLerp(7f, 27f, (float)segments[i].GetLength(0));
            }
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                float num = 0f;
                Vector2 pos = AttachPos(i, timeStacker);
                float num2 = 0f;
                for (int j = 0; j < segments[i].GetLength(0); j++)
                {
                    Vector2 vector = Vector2.Lerp(segments[i][j, 1], segments[i][j, 0], timeStacker);
                    float num3 = 14f * dreamer.scale * Vector3.Slerp(segments[i][j, 5], segments[i][j, 4], timeStacker).x;
                    Vector2 normalized = (pos - vector).normalized;
                    Vector2 a2 = Custom.PerpendicularVector(normalized);
                    float dist = Vector2.Distance(pos, vector) / 5f;
                    (sLeaser.sprites[firstSprite + i] as TriangleMesh).MoveVertice(j * 4, pos - normalized * dist - a2 * (num3 + num) * 0.5f - camPos);
                    (sLeaser.sprites[firstSprite + i] as TriangleMesh).MoveVertice(j * 4 + 1, pos - normalized * dist + a2 * (num3 + num) * 0.5f - camPos);
                    (sLeaser.sprites[firstSprite + i] as TriangleMesh).MoveVertice(j * 4 + 2, vector + normalized * dist - a2 * num3 - camPos);
                    (sLeaser.sprites[firstSprite + i] as TriangleMesh).MoveVertice(j * 4 + 3, vector + normalized * dist + a2 * num3 - camPos);
                    float lerp = 0.35f + 0.65f * Custom.BackwardsSCurve(Mathf.Pow(Mathf.Abs(Vector2.Dot(Vector3.Slerp(segments[i][j, 5], segments[i][j, 4], timeStacker), Custom.DegToVec(45f + Custom.VecToDeg(normalized)))), 2f), 0.5f);
                    (sLeaser.sprites[firstSprite + i] as TriangleMesh).verticeColors[j * 4] = Color.Lerp(dreamer.primaryColor, dreamer.accentColor, (lerp + num2) / 2f);
                    (sLeaser.sprites[firstSprite + i] as TriangleMesh).verticeColors[j * 4 + 1] = Color.Lerp(dreamer.primaryColor, dreamer.accentColor, (lerp + num2) / 2f);
                    (sLeaser.sprites[firstSprite + i] as TriangleMesh).verticeColors[j * 4 + 2] = Color.Lerp(dreamer.primaryColor, dreamer.accentColor, lerp);
                    (sLeaser.sprites[firstSprite + i] as TriangleMesh).verticeColors[j * 4 + 3] = Color.Lerp(dreamer.primaryColor, dreamer.accentColor, lerp);
                    pos = vector;
                    num = num3;
                    num2 = lerp;
                }
            }
        }

        public Vector2 AttachPos(int rag, float timeStacker)
        {
            return Vector2.Lerp(dreamer.spine[4].lastPos, dreamer.spine[4].pos, timeStacker);
        }

        public Dreamer dreamer;
        public int firstSprite;
        public int totalSprites; //6
        private float conRad;
        public Vector2[][,] segments;
    }

    /// <summary>
    /// The dangly bits attached to it, made of diamonds and dots
    /// </summary>
    public class Chains
    {
        public Chains(Dreamer dreamer, int firstSprite)
        {
            this.dreamer = dreamer;
            this.firstSprite = firstSprite;
            int array = 2;
            segments = new Vector2[array][,];
            firstSpriteOfChains = new int[array];
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i] = new Vector2[27, 7];
                firstSpriteOfChains[i] = totalSprites;
                for (int j = 0; j < segments[i].GetLength(0); j++)
                {
                    if (j % 3 < 2)
                    {
                        segments[i][j, 4] = new Vector2(19f, 0.2f);
                    }
                    else
                    {
                        segments[i][j, 4] = new Vector2(35f, 1f);
                    }
                    totalSprites += 2;
                }
            }
        }

        public void Reset(Vector2 resetPos)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                for (int j = 0; j < segments[i].GetLength(0); j++)
                {
                    segments[i][j, 0] = resetPos + Custom.RNV();
                    segments[i][j, 1] = segments[i][j, 0];
                    segments[i][j, 2] *= 0f;
                }
            }
        }

        // I don't know what ANY of this is.
        public void Update()
        {

            for (int i = 0; i < segments.Length; i++)
            {
                for (int j = 0; j < segments[i].GetLength(0); j++)
                {
                    segments[i][j, 5].y = segments[i][j, 5].x;
                    segments[i][j, 5].x += segments[i][j, 6].x;
                    segments[i][j, 6].x *= 0.99f;
                    if (UnityEngine.Random.value < 0.071428575f)
                    {
                        segments[i][j, 6].x += Mathf.Pow(UnityEngine.Random.value, 5f) * ((UnityEngine.Random.value < 0.5f) ? -1f : 1f) * segments[i][j, 2].magnitude / (15.5f * dreamer.scale);
                    }
                    segments[i][j, 1] = segments[i][j, 0];
                    segments[i][j, 0] += segments[i][j, 2];
                    segments[i][j, 2] *= 0.999f;
                    segments[i][j, 2] += Custom.RNV() * 0.2f * dreamer.scale;
                    segments[i][j, 2] = Vector2.Lerp(segments[i][j, 2], Custom.DirVec(segments[i][j, 0], dreamer.spine[4].pos) * (segments[i][j, 2].magnitude + 3f * dreamer.scale) * 0.5f, Custom.LerpMap(Vector2.Distance(segments[i][j, 0], dreamer.spine[4].pos), 250f * dreamer.scale, 600f * dreamer.scale, 0f, 0.1f, 17f));
                }
                AttachChain(i);
                for (int k = 1; k < segments[i].GetLength(0); k++)
                {
                    Vector2 normalized = (segments[i][k, 0] - segments[i][k - 1, 0]).normalized;
                    float num = Vector2.Distance(segments[i][k, 0], segments[i][k - 1, 0]);
                    float num2 = segments[i][k - 1, 4].y / (segments[i][k, 4].y + segments[i][k - 1, 4].y);
                    segments[i][k, 0] += normalized * (segments[i][k, 4].x - num) * num2;
                    segments[i][k, 2] += normalized * (segments[i][k, 4].x - num) * num2;
                    segments[i][k - 1, 0] -= normalized * (segments[i][k, 4].x - num) * (1f - num2);
                    segments[i][k - 1, 2] -= normalized * (segments[i][k, 4].x - num) * (1f - num2);
                }
                AttachChain(i);
                for (int l = segments[i].GetLength(0) - 2; l >= 0; l--)
                {
                    Vector2 normalized = (segments[i][l, 0] - segments[i][l + 1, 0]).normalized;
                    float num = Vector2.Distance(segments[i][l, 0], segments[i][l + 1, 0]);
                    float num3 = segments[i][l + 1, 4].y / (segments[i][l, 4].y + segments[i][l + 1, 4].y);
                    segments[i][l, 0] += normalized * (segments[i][l + 1, 4].x - num) * num3;
                    segments[i][l, 2] += normalized * (segments[i][l + 1, 4].x - num) * num3;
                    segments[i][l + 1, 0] -= normalized * (segments[i][l + 1, 4].x - num) * (1f - num3);
                    segments[i][l + 1, 2] -= normalized * (segments[i][l + 1, 4].x - num) * (1f - num3);
                }
                AttachChain(i);
            }
        }

        private void AttachChain(int r)
        {
            Vector2 normalized = (segments[r][0, 0] - AttachPos(r, 1f)).normalized;
            float num = Vector2.Distance(segments[r][0, 0], AttachPos(r, 1f));
            segments[r][0, 0] += normalized * (segments[r][0, 4].x - num);
            segments[r][0, 2] += normalized * (segments[r][0, 4].x - num);
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                for (int j = 0; j < segments[i].GetLength(0); j++)
                {
                    if (segments[i][j, 4].y == 0.2f)
                    {
                        sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2] = new FSprite("haloGlyph-1", true);
                        sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1] = new FSprite("pixel", true);
                    }
                    else
                    {
                        sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2] = new FSprite("ghostLink", true);
                        sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2].anchorY = -0.6666667f;
                        sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1] = new FSprite("ghostLink", true);
                        sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1].anchorY = -0.6666667f;
                    }
                    sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2].shader = Custom.rainWorld.Shaders["RippleBasicBothSides"];
                    sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1].shader = Custom.rainWorld.Shaders["RippleBasicBothSides"];
                }
            }
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                Vector2 vector = AttachPos(i, timeStacker);
                for (int j = 0; j < segments[i].GetLength(0); j++)
                {
                    Vector2 vector2 = Vector2.Lerp(segments[i][j, 1], segments[i][j, 0], timeStacker);
                    if (segments[i][j, 4].y == 0.2f)
                    {
                        sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2].x = (vector2.x + vector.x) / 2f - camPos.x;
                        sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2].y = (vector2.y + vector.y) / 2f - camPos.y;
                        sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1].x = (vector2.x + vector.x) / 2f - camPos.x - 1f;
                        sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1].y = (vector2.y + vector.y) / 2f - camPos.y + 1f;
                        sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2].color = Color.Lerp(dreamer.primaryColor, dreamer.accentColor, 0.65f);
                        sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1].color = dreamer.accentColor;
                    }
                    else
                    {
                        Vector2 vector3 = Custom.PerpendicularVector(vector, vector2);
                        float ang = Mathf.Sin(Mathf.Lerp(segments[i][j, 5].y, segments[i][j, 5].x, timeStacker)) * 360f / 3.1415927f;
                        for (int k = 0; k < 2; k++)
                        {
                            sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + k].x = vector2.x + vector3.x * (float)(-1 + k * 2) * dreamer.scale * 0.9f - camPos.x;
                            sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + k].y = vector2.y + vector3.y * (float)(-1 + k * 2) * dreamer.scale * 0.9f - camPos.y;
                            sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + k].rotation = Custom.AimFromOneVectorToAnother(vector2, vector);
                            sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + k].scaleX = Mathf.Max(0.1f, Mathf.Abs(Custom.DegToVec(ang).x));
                        }
                        float curve = Mathf.Abs(Vector2.Dot(Custom.DegToVec(ang), Custom.DirVec(vector, vector2)));
                        curve = Custom.BackwardsSCurve(curve, 0.3f);
                        sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2].color = Color.Lerp(dreamer.primaryColor, dreamer.accentColor, 0.65f + 0.1f * Mathf.Sin(curve * 3.1415927f * 2f));
                        sLeaser.sprites[firstSprite + firstSpriteOfChains[i] + j * 2 + 1].color = Color.Lerp(dreamer.primaryColor, dreamer.accentColor, 0.1f + 0.9f * curve);
                    }
                    vector = vector2;
                }
            }
        }

        public Vector2 AttachPos(int chain, float timeStacker)
        {
            return Vector2.Lerp(dreamer.legs[chain, 2].lastPos, dreamer.legs[chain, 2].pos, timeStacker);
        }

        public Dreamer dreamer;
        public int firstSprite;
        public int totalSprites;
        public Vector2[][,] segments;
        public int[] firstSpriteOfChains;
    }
    #endregion

    private void LoadElement(string elementName)
    {
        if (Futile.atlasManager.GetAtlasWithName(elementName) != null)
        {
            return;
        }
        string str = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar.ToString() + elementName + ".png");
        Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        AssetManager.SafeWWWLoadTexture(ref texture, "file:///" + str, false, true);
        Futile.atlasManager.LoadAtlasFromTexture(elementName, texture, false);
    }

    protected virtual bool OnScreen()
    {
        return room.VisibleInAnyCameraScreenBounds(pos);
    }

    public void TickForEncounter()
    {
        if (encounterFinished)
        {
            afterConversationCounter.Tick();
        }
        else
        {
            onScreenCounter.Tick();
        }
    }

    public override void Update(bool eu)
    {

        base.Update(eu);
        if (slatedForDeletetion)
        {
            Destroy();
            return;
        }

        if (!slatedForDeletetion)
        {
            for (int i = 0; i < room.warpPoints.Count; i++)
            {
                CosmeticRipple ripple = room.warpPoints[i].ripple;
                if (ripple != null)
                {
                    ripple.RemoveFromRoom();
                }
                WarpTear warpTear = room.warpPoints[i].warpTear;
                if (warpTear != null)
                {
                    warpTear.RemoveFromRoom();
                }
                WarpPoint warpPoint = room.warpPoints[i];
                if (warpPoint != null)
                {
                    warpPoint.RemoveFromRoom();
                }
            }
        }

        rags.Update();
        chains.Update();

        // Makes them looked at by player when present
        foreach (AbstractCreature abstractCreature in room.game.Players)
        {
            Player player = abstractCreature.realizedCreature as Player;
            if (player != null && player.room == room)
            {
                PlayerGraphics playerGraphics = player.graphicsModule as PlayerGraphics;
                if (playerGraphics != null)
                {
                    playerGraphics.LookAtPoint(pos, 10000f);
                }
            }
        }

        // Todo: Move all this to a Behavior object like VW, then let that handle different encounter types :)
        if (OnScreen())
        {
            TickForEncounter();
        }
        else
        {
            if (conversation != null)
            {
                DreamerConversation conversation2 = conversation;
                if (conversation2 != null)
                {
                    conversation2.Destroy();
                }
                convoActive = false;
                conversation = null;
            }
            onScreenCounter.Reset();
        }
        if (onScreenCounter.isFinished && room.game.cameras[0].hud != null)
        {
            if (conversation == null)
            {
                convoActive = true;
                StartConversation();
            }
            else if (conversation.slatedForDeletion)
            {
                convoFinished = true;
            }
        }
        if (conversation != null && convoActive)
        {
            conversation.Update();
        }
        if (convoFinished)
        {
            convoActive = false;
            MarkEncountered();
        }
        if (afterConversationCounter.isFinished)
        {
            SpawnWarp();
            Despawn();
        }
        else if (afterConversationCounter > 0)
        {
            for (int i = 0; i < afterConversationCounter; i++)
            {
                AfterEncounteredVisual();
            }
        }

        sinBob += 1f / Mathf.Lerp(140f, 210f, UnityEngine.Random.value);
        pos = placedObject.pos + new Vector2(0f, Mathf.Sin(sinBob * 3.1415927f * 2f) * 18f * scale);
        flipProg = Mathf.Min(1f, flipProg + flipSpeed);
        flip = Mathf.Lerp(flipFrom, flipTo, Custom.SCurve(flipProg, 0.7f));
        if (flipProg >= 1f && UnityEngine.Random.value < 0.1f)
        {
            flipFrom = flip;
            flipTo = Mathf.Clamp((flip + defaultFlip) / 2f + Mathf.Lerp(0.05f, 0.5f, Mathf.Pow(UnityEngine.Random.value, 2.5f)) * ((UnityEngine.Random.value < 0.5f) ? -1f : 1f), -1f, 1f);
            flipProg = 0f;
            flipSpeed = 1f / (Mathf.Lerp(30f, 220f, UnityEngine.Random.value) * Mathf.Abs(flipFrom - flipTo));
        }
        float num = 30f * scale;

        #region Spine
        for (int j = 0; j < spine.Length; j++)
        {
            float t = (float)j / (float)(spine.Length - 1);
            Vector2 a = Custom.FlattenVectorAlongAxis(Custom.DegToVec(Mathf.Lerp(180f, -75f, t)), -15f, 1.3f) * Mathf.Lerp(100f, 40f, t) * scale;
            a.x *= flip;
            a += pos;
            spine[j].vel *= airResistance;
            spine[j].Update();
            spine[j].vel += (a - spine[j].pos) / 10f;
            if (j > 0)
            {
                Vector2 normalized = (spine[j].pos - spine[j - 1].pos).normalized;
                float num2 = Vector2.Distance(spine[j].pos, spine[j - 1].pos);
                float d = (num2 < num && j == spineBendPoint) ? 0f : 0.5f;
                spine[j].pos += normalized * (num - num2) * d;
                spine[j].vel += normalized * (num - num2) * d;
                spine[j - 1].pos -= normalized * (num - num2) * d;
                spine[j - 1].vel -= normalized * (num - num2) * d;
                if (j > 1)
                {
                    normalized = (spine[j].pos - spine[j - 2].pos).normalized;
                    spine[j].vel += normalized * 0.2f;
                    spine[j - 2].vel -= normalized * 0.2f;
                }
            }
        }
        #endregion

        #region Legs
        for (int k = 0; k < this.legs.GetLength(0); k++)
        {
            for (int l = 0; l < this.legs.GetLength(1); l++)
            {
                Vector2 a2;
                float num3 = (k == 0) ? -1f : 1f;
                if (l == 0)
                {
                    a2 = Vector2.Lerp(this.pos, this.spine[this.spineBendPoint - 3].pos, 0.5f) + new Vector2(this.flip * -70f + num3 * Mathf.Lerp(8f, 4f, Mathf.Pow(Mathf.Abs(this.flip), 2f)), -20f) * this.scale;
                }
                else if (l == 1)
                {
                    a2 = Vector2.Lerp(this.pos, this.spine[0].pos, 0.5f) + new Vector2(this.flip * 40f + num3 * Mathf.Lerp(20f, 10f, Mathf.Pow(Mathf.Abs(this.flip), 2f)), -110f) * this.scale;
                }
                else
                {
                    a2 = Vector2.Lerp(this.pos, this.spine[0].pos, 0.5f) + new Vector2(this.flip * 40f + num3 * Mathf.Lerp(20f, 10f, Mathf.Pow(Mathf.Abs(this.flip), 2f)), -130f) * this.scale;
                    this.legs[k, l].vel += Custom.DirVec(this.legs[k, 0].pos, this.legs[k, l].pos) * 2f * this.scale;
                }
                this.legs[k, l].vel *= this.airResistance;
                this.legs[k, l].Update();
                this.legs[k, l].vel += (a2 - this.legs[k, l].pos) / 10f;
            }
            Vector2 normalized2 = (this.legs[k, 0].pos - this.legs[k, 1].pos).normalized;
            float num4 = Vector2.Distance(this.legs[k, 0].pos, this.legs[k, 1].pos);
            float num5 = 210f * this.scale;
            num5 *= 0.6f;
            this.legs[k, 0].pos += normalized2 * (num5 - num4) * 0.5f;
            this.legs[k, 0].vel += normalized2 * (num5 - num4) * 0.5f;
            this.legs[k, 1].pos -= normalized2 * (num5 - num4) * 0.5f;
            this.legs[k, 1].vel -= normalized2 * (num5 - num4) * 0.5f;
            normalized2 = (this.legs[k, 0].pos - this.spine[0].pos).normalized;
            num4 = Vector2.Distance(this.legs[k, 0].pos, this.spine[0].pos);
            num5 = 120f * this.scale;
            num5 *= 0.75f;
            this.legs[k, 0].pos += normalized2 * (num5 - num4) * 0.5f;
            this.legs[k, 0].vel += normalized2 * (num5 - num4) * 0.5f;
            this.spine[0].pos -= normalized2 * (num5 - num4) * 0.5f;
            this.spine[0].vel -= normalized2 * (num5 - num4) * 0.5f;
            normalized2 = (this.legs[k, 1].pos - this.legs[k, 2].pos).normalized;
            num4 = Vector2.Distance(this.legs[k, 1].pos, this.legs[k, 2].pos);
            num5 = 40f * this.scale;
            this.legs[k, 1].pos += normalized2 * (num5 - num4) * 0.15f;
            this.legs[k, 1].vel += normalized2 * (num5 - num4) * 0.15f;
            this.legs[k, 2].pos -= normalized2 * (num5 - num4) * 0.85f;
            this.legs[k, 2].vel -= normalized2 * (num5 - num4) * 0.85f;
        }

        #endregion

    }

    #region Dialogue and Speaking
    private void StartConversation()
    {
        if (room.game.cameras[0].hud.dialogBox == null)
        {
            room.game.cameras[0].hud.InitDialogBox();
        }
        conversation = new DreamerConversation(this, GetConversationID(), room.game.cameras[0].hud.dialogBox);
        convoActive = true;

    }

    private Conversation.ID GetConversationID()
    {
        StoryGameSession session = room.game.GetStorySession;
        Conversation.ID result;
        switch ((session != null) ? BeaconSaveData.GetDreamerEncountersNumber(session.saveState) : 0)
        {
            case 0:
                result = Enums.ConversationID.Dreamer_1;
                break;
            case 1:
                result = Enums.ConversationID.Dreamer_2;
                break;
            default:
                result = Enums.ConversationID.Dreamer_PH;
                break;
        }
        return result;
    }
    #endregion

    #region Graphics

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[totalSprites];
        rags.InitiateSprites(sLeaser, rCam);
        chains.InitiateSprites(sLeaser, rCam);

        sLeaser.sprites[LightSprite] = new FSprite("Futile_White", true)
        {
            shader = rCam.game.rainWorld.Shaders["LightSourceBothSides"],
            color = glowColor,
            isVisible = true
        };

        sLeaser.sprites[DistortionSprite] = new FSprite("Futile_White", true)
        {
            shader = rCam.game.rainWorld.Shaders["DreamerDistortion"]
        };

        sLeaser.sprites[BodyMeshSprite] = TriangleMesh.MakeLongMesh(spineBendPoint, false, true);
        sLeaser.sprites[BodyMeshSprite].shader = rCam.game.rainWorld.Shaders["RippleBasicBothSides"];
        sLeaser.sprites[HeadMeshSprite] = TriangleMesh.MakeLongMesh(spineSegments - spineBendPoint + snoutSegments, false, true, "ghostScales");
        sLeaser.sprites[HeadMeshSprite].shader = rCam.game.rainWorld.Shaders["DreamerSkin"];

        sLeaser.sprites[NeckConnectorSprite] = new FSprite("Circle20", true)
        {
            shader = rCam.game.rainWorld.Shaders["RippleBasicBothSides"]
        };

        for (int i = 0; i < legs.GetLength(0); i++)
        {
            sLeaser.sprites[ThightSprite(i)] = TriangleMesh.MakeLongMesh(thighSegments, false, true, "ghostBand");
            sLeaser.sprites[ThightSprite(i)].shader = rCam.game.rainWorld.Shaders["DreamerSkin"];
            sLeaser.sprites[LowerLegSprite(i)] = TriangleMesh.MakeLongMesh(lowerLegSegments, false, true, "ghostPlates");
            sLeaser.sprites[LowerLegSprite(i)].shader = rCam.game.rainWorld.Shaders["DreamerSkin"];
            sLeaser.sprites[ButtockSprite(i)] = new FSprite("Circle20", true)
            {
                shader = rCam.game.rainWorld.Shaders["RippleBasicBothSides"]
            };
        }

        AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (newContatiner == null)
        {
            newContatiner = rCam.ReturnFContainer("Items");
        }
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].RemoveFromContainer();
            if (i == DistortionSprite)
            {
                rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[i]);
            }
            else if (i == LightSprite)
            {
                rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
            }
            else
            {
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {

        float num = Mathf.Clamp((Mathf.Lerp(spine[spineBendPoint - 2].lastPos.x, spine[spineBendPoint - 2].pos.x, timeStacker) - Mathf.Lerp(spine[spineBendPoint + 2].lastPos.x, spine[spineBendPoint + 2].pos.x, timeStacker)) / (80f * scale), -1f, 1f);
        float num2 = 10f * scale;
        float num3 = 10f * scale;

        rags.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        chains.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        Vector2 vector = Vector2.Lerp(spine[spine.Length - 1].lastPos, spine[spine.Length - 1].pos, timeStacker);
        Vector2 vector2 = Custom.DirVec(Vector2.Lerp(spine[spine.Length - 2].lastPos, spine[spine.Length - 2].pos, timeStacker), vector);
        vector += vector2 * 5f * scale;
        float headLength = 50f;
        Vector2 vector3 = vector + vector2 * headLength * scale + Custom.PerpendicularVector(vector2) * 40f * scale * num;
        Vector2 vector4 = Vector2.Lerp(spine[0].lastPos, spine[0].pos, timeStacker);
        vector4 += Custom.DirVec(Vector2.Lerp(spine[1].lastPos, spine[1].pos, timeStacker), vector4);
        Vector2 vector5 = vector4;

        // Body
        for (int i = 0; i < spineBendPoint; i++)
        {
            float f = (float)i / (float)(this.spineBendPoint - 1);
            Vector2 vector6 = Vector2.Lerp(this.spine[i].lastPos, this.spine[i].pos, timeStacker);
            float num6;
            float num7;
                num6 = Mathf.Lerp(10f, Custom.LerpMap(num, -1f, 1f, 50f, 25f, 2f), Mathf.Sin(3.1415927f * Mathf.Pow(f, 1.3f))) * this.scale;
                num7 = Mathf.Lerp(10f, Custom.LerpMap(num, 1f, -1f, 50f, 25f, 2f), Mathf.Sin(3.1415927f * Mathf.Pow(f, 1.3f))) * this.scale;
            Vector2 normalized = (vector4 - vector6).normalized;
            Vector2 a = Custom.PerpendicularVector(normalized);
            float d = Vector2.Distance(vector4, vector6) / 5f;
            (sLeaser.sprites[this.BodyMeshSprite] as TriangleMesh).MoveVertice(i * 4, vector4 - normalized * d - a * (num2 + num6) * 0.5f - camPos);
            (sLeaser.sprites[this.BodyMeshSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector4 - normalized * d + a * (num3 + num7) * 0.5f - camPos);
            (sLeaser.sprites[this.BodyMeshSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector6 + normalized * d - a * num6 - camPos);
            (sLeaser.sprites[this.BodyMeshSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector6 + normalized * d + a * num7 - camPos);
            if (i == this.spineBendPoint - 2)
            {
                vector5 = vector6;
            }
            vector4 = vector6;
            num2 = num6;
            num3 = num7;
        }

        Vector2 vector7 = Custom.DegToVec(180f - 90f * num);
        vector7.x = Mathf.Pow(Mathf.Abs(vector7.x), 8f) * Mathf.Sign(vector7.x);
        vector7 *= 40f * scale;
        vector7.y -= 7f * scale;
        Vector2 vector8 = (base.pos + new Vector2(0f, -50f) + vector + vector7 + Vector2.Lerp(spine[5].lastPos, spine[5].pos, timeStacker)) / 3f;
        sLeaser.sprites[DistortionSprite].x = vector8.x - camPos.x;
        sLeaser.sprites[DistortionSprite].y = vector8.y - camPos.y;
        sLeaser.sprites[DistortionSprite].scale = (933f * scale / 16f) + distortionScaleFac;
        sLeaser.sprites[LightSprite].x = vector8.x - camPos.x;
        sLeaser.sprites[LightSprite].y = vector8.y - camPos.y;
        sLeaser.sprites[LightSprite].scale = (500f * lightSpriteScale / 16f) + distortionScaleFac;
        vector4 = Vector2.Lerp(spine[spineBendPoint].lastPos, spine[spineBendPoint].pos, timeStacker);
        vector4 += Custom.DirVec(Vector2.Lerp(spine[spineBendPoint + 1].lastPos, spine[spineBendPoint + 1].pos, timeStacker), vector4);
        vector4 += vector7;

        // Neck and head
        for (int j = spineBendPoint; j < spineSegments + snoutSegments; j++)
        {
            float num8 = Mathf.InverseLerp((float)spineBendPoint, (float)(spineSegments + snoutSegments - 1), (float)j);
            Vector2 vector9;
            if (j < spineSegments)
            {
                vector9 = Vector2.Lerp(spine[j].lastPos, spine[j].pos, timeStacker);
            }
            else
            {
                vector9 = Custom.Bezier(vector, vector + vector2 * 50f * scale, vector3, vector + vector2 * 50f * scale, Mathf.InverseLerp((float)spineSegments, (float)(spineSegments + snoutSegments - 1), (float)j));
            }
            vector9 += vector7;
            if (j == spineBendPoint)
            {
                sLeaser.sprites[NeckConnectorSprite].x = (vector9.x + vector5.x) / 2f - camPos.x;
                sLeaser.sprites[NeckConnectorSprite].y = (vector9.y + vector5.y) / 2f - camPos.y;
                sLeaser.sprites[NeckConnectorSprite].rotation = Custom.AimFromOneVectorToAnother(vector5, vector9);
                sLeaser.sprites[NeckConnectorSprite].scaleY = Vector2.Distance(vector5, vector9) * 1.6f / 20f;
                sLeaser.sprites[NeckConnectorSprite].scaleX = scale * 1.6f;
            }
            float num9;
            float num10;
            if (num8 < 0.15f)
            {
                num9 = 10f * scale;
                num10 = 10f * scale;
            }
            else if (num8 < 0.4f)
            {
                num9 = Mathf.Lerp(10f, 20f, Mathf.Sin(Custom.LerpMap(num8, 0.15f, 0.4f, 0f, 0.5f) * 3.1415927f)) * scale;
                num10 = Mathf.Lerp(10f, 20f, Mathf.Sin(Custom.LerpMap(num8, 0.15f, 0.4f, 0f, 0.5f) * 3.1415927f)) * scale;
            }
            else
            {
                num9 = SnoutContour(Mathf.InverseLerp(0.4f, 1f, num8), false, Mathf.Abs(num));
                num10 = SnoutContour(Mathf.InverseLerp(0.4f, 1f, num8), false, Mathf.Abs(num));
            }
            Vector2 normalized2 = (vector4 - vector9).normalized;
            Vector2 a2 = Custom.PerpendicularVector(normalized2);
            float d2 = Vector2.Distance(vector4, vector9) / 5f;
            int num11 = j - spineBendPoint;
            (sLeaser.sprites[HeadMeshSprite] as TriangleMesh).MoveVertice(num11 * 4, vector4 - normalized2 * d2 - a2 * (num2 + num9) * 0.5f - camPos);
            (sLeaser.sprites[HeadMeshSprite] as TriangleMesh).MoveVertice(num11 * 4 + 1, vector4 - normalized2 * d2 + a2 * (num3 + num10) * 0.5f - camPos);
            (sLeaser.sprites[HeadMeshSprite] as TriangleMesh).MoveVertice(num11 * 4 + 2, vector9 + normalized2 * d2 - a2 * num9 - camPos);
            (sLeaser.sprites[HeadMeshSprite] as TriangleMesh).MoveVertice(num11 * 4 + 3, vector9 + normalized2 * d2 + a2 * num10 - camPos);
            vector4 = vector9;
            num2 = num9;
            num3 = num10;
            // Reference head position for usage later
            headPos = vector4 - normalized2 * d2 - a2 * (num2 + num9) * 0.5f - camPos;
        }
        float a3 = Custom.AimFromOneVectorToAnother(vector3, vector) / 360f;
        for (int k = 0; k < (sLeaser.sprites[HeadMeshSprite] as TriangleMesh).verticeColors.Length; k++)
        {
            float num12 = (float)k / (float)((sLeaser.sprites[HeadMeshSprite] as TriangleMesh).verticeColors.Length - 1);
            // space teture takes to fade
            num12 *= 0.5f;
            float num13;
            float num14;
            if (num12 < 0.15f)
            {
                num13 = 10f * scale;
                num14 = 10f * scale;
            }
            else if (num12 < 0.4f)
            {
                num13 = Mathf.Lerp(10f, 20f, Mathf.Sin(Custom.LerpMap(num12, 0.15f, 0.4f, 0f, 0.5f) * 3.1415927f)) * scale;
                num14 = Mathf.Lerp(10f, 20f, Mathf.Sin(Custom.LerpMap(num12, 0.15f, 0.4f, 0f, 0.5f) * 3.1415927f)) * scale;
            }
            else
            {
                num13 = SnoutContour(Mathf.InverseLerp(0.4f, 1f, num12), false, Mathf.Abs(num));
                num14 = SnoutContour(Mathf.InverseLerp(0.4f, 1f, num12), false, Mathf.Abs(num));
            }
            float value = (num13 + num14) / (2f * scale);
            (sLeaser.sprites[HeadMeshSprite] as TriangleMesh).verticeColors[k] = new Color(Mathf.InverseLerp(0.1f, 30f, value), Mathf.InverseLerp(-1f, 1f, num), Mathf.InverseLerp(0.25f, 0.05f, num12), a3);
        }

        // Legs
        Vector2 vector10 = Vector2.Lerp(Vector2.Lerp(spine[0].lastPos, spine[0].pos, timeStacker), Vector2.Lerp(spine[1].lastPos, spine[1].pos, timeStacker), 0.5f);
        vector10 += Custom.DirVec(Vector2.Lerp(spine[2].lastPos, spine[2].pos, timeStacker), vector10) * 20f * scale;
        for (int l = 0; l < legs.GetLength(0); l++)
        {
            Vector2 vector11 = Vector2.Lerp(legs[l, 0].lastPos, legs[l, 0].pos, timeStacker);
            Vector2 vector12 = Vector2.Lerp(legs[l, 1].lastPos, legs[l, 1].pos, timeStacker);
            Vector2 vector13 = Vector2.Lerp(legs[l, 2].lastPos, legs[l, 2].pos, timeStacker);
            Vector2 vector14 = vector10 + Custom.DirVec(vector10, vector) * 5f * scale + Custom.DirVec(vector10, vector11) * 10f * scale;
            vector4 = vector14 + Custom.DirVec(vector11, vector14);
            sLeaser.sprites[ButtockSprite(l)].x = (vector10 + vector14).x / 2f - camPos.x;
            sLeaser.sprites[ButtockSprite(l)].y = (vector10 + vector14).y / 2f - camPos.y;
            sLeaser.sprites[ButtockSprite(l)].scaleX = scale;
            sLeaser.sprites[ButtockSprite(l)].rotation = Custom.AimFromOneVectorToAnother(vector14, Vector2.Lerp(spine[1].lastPos, spine[1].pos, timeStacker));
            sLeaser.sprites[ButtockSprite(l)].scaleY = Mathf.Max(scale / 2f, Vector2.Distance(vector14, Vector2.Lerp(spine[1].lastPos, spine[1].pos, timeStacker)) / 40f);
            for (int m = 0; m < thighSegments; m++)
            {
                float num15 = Mathf.InverseLerp(0f, (float)(thighSegments - 1), (float)m);
                Vector2 vector15 = Vector2.Lerp(vector14, vector11 + Custom.DirVec(vector14, vector11) * 10f * scale, num15);
                float num16 = ThighContour(num15, l == 0);
                float num17 = ThighContour(num15, l == 1);
                Vector2 normalized3 = (vector4 - vector15).normalized;
                Vector2 a4 = Custom.PerpendicularVector(normalized3);
                float d3 = Vector2.Distance(vector4, vector15) / 5f;
                (sLeaser.sprites[ThightSprite(l)] as TriangleMesh).MoveVertice(m * 4, vector4 - normalized3 * d3 - a4 * (num2 + num16) * 0.5f - camPos);
                (sLeaser.sprites[ThightSprite(l)] as TriangleMesh).MoveVertice(m * 4 + 1, vector4 - normalized3 * d3 + a4 * (num3 + num17) * 0.5f - camPos);
                (sLeaser.sprites[ThightSprite(l)] as TriangleMesh).MoveVertice(m * 4 + 2, vector15 + normalized3 * d3 - a4 * num16 - camPos);
                (sLeaser.sprites[ThightSprite(l)] as TriangleMesh).MoveVertice(m * 4 + 3, vector15 + normalized3 * d3 + a4 * num17 - camPos);
                vector4 = vector15;
                num2 = num16;
                num3 = num17;
            }
            float a5 = Custom.AimFromOneVectorToAnother(vector14, vector11) / 360f;
            for (int n = 0; n < (sLeaser.sprites[ThightSprite(l)] as TriangleMesh).verticeColors.Length; n++)
            {
                float num18 = (float)n / (float)((sLeaser.sprites[ThightSprite(l)] as TriangleMesh).verticeColors.Length - 1);
                (sLeaser.sprites[ThightSprite(l)] as TriangleMesh).verticeColors[n] = new Color(1f, Custom.LerpMap(num, -1f, 1f, 0.4f, 0.6f), ((double)num18 < 0.3 || num18 > 0.7f) ? 1f : 0f, a5);
            }
            vector4 = vector11 + Custom.DirVec(vector12, vector11);
            for (int num19 = 0; num19 < lowerLegSegments; num19++)
            {
                float num20 = Mathf.InverseLerp(0f, (float)(lowerLegSegments - 1), (float)num19);
                Vector2 vector16;
                if (num20 < 0.8f)
                {
                    vector16 = Vector2.Lerp(vector11, vector12, Mathf.InverseLerp(0f, 0.8f, num20));
                }
                else
                {
                    vector16 = Vector2.Lerp(vector12, vector13, Mathf.InverseLerp(0.8f, 1f, num20));
                }
                float num21 = LowerLegContour(num20, l == 0, Mathf.Lerp(0.7f, num * ((l == 1) ? -1f : 1f), Mathf.Abs(num)));
                float num22 = LowerLegContour(num20, l == 1, Mathf.Lerp(0.7f, num * ((l == 1) ? -1f : 1f), Mathf.Abs(num)));
                Vector2 normalized4 = (vector4 - vector16).normalized;
                Vector2 a6 = Custom.PerpendicularVector(normalized4);
                float d4 = Vector2.Distance(vector4, vector16) / 5f;
                (sLeaser.sprites[LowerLegSprite(l)] as TriangleMesh).MoveVertice(num19 * 4, vector4 - normalized4 * d4 - a6 * (num2 + num21) * 0.5f - camPos);
                (sLeaser.sprites[LowerLegSprite(l)] as TriangleMesh).MoveVertice(num19 * 4 + 1, vector4 - normalized4 * d4 + a6 * (num3 + num22) * 0.5f - camPos);
                (sLeaser.sprites[LowerLegSprite(l)] as TriangleMesh).MoveVertice(num19 * 4 + 2, vector16 + normalized4 * d4 - a6 * num21 - camPos);
                (sLeaser.sprites[LowerLegSprite(l)] as TriangleMesh).MoveVertice(num19 * 4 + 3, vector16 + normalized4 * d4 + a6 * num22 - camPos);
                vector4 = vector16;
                num2 = num21;
                num3 = num22;
            }
            a5 = Custom.AimFromOneVectorToAnother(vector11, vector13) / 360f;
            for (int num23 = 0; num23 < (sLeaser.sprites[LowerLegSprite(l)] as TriangleMesh).verticeColors.Length; num23++)
            {
                float value2 = (float)num23 / (float)((sLeaser.sprites[LowerLegSprite(l)] as TriangleMesh).verticeColors.Length - 1);
                (sLeaser.sprites[LowerLegSprite(l)] as TriangleMesh).verticeColors[num23] = new Color(1f, Custom.LerpMap(num, -1f, 1f, 0.4f, 0.6f), Mathf.InverseLerp(0.25f, 0.05f, value2), a5);
            }
        }

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public float SnoutContour(float f, bool side, float sideView)
    {
        float num;
        if (f > 0.85f)
        {
            num = 0.2f + 0.8f * Mathf.Sin(Custom.LerpMap(f, 0.85f, 1f, 0.5f, 1f) * 3.1415927f);
        }
        else
        {
            num = Custom.LerpMap(f, 0f, 0.5f, Mathf.Lerp(Custom.LerpMap(f, 0f, 0.5f, 1.5f, 2f), 1f, sideView), 1f);
        }
        num *= Mathf.Lerp(1f, 0.3f, sideView * f);
        return num * 10f * scale;
    }

    public float ThighContour(float f, bool side)
    {
        float num;
        if (f < 0.3f)
        {
            num = 0.2f + 0.6f * Mathf.Sin(Custom.LerpMap(f, 0f, 0.3f, 0f, 0.5f) * 3.1415927f);
        }
        else if (side)
        {
            if (f < 0.85f)
            {
                num = Custom.LerpMap(f, 0.3f, 0.85f, 0.8f, 1f, 0.5f);
            }
            else
            {
                num = 0.2f + 0.8f * Custom.BackwardsSCurve(1f - Mathf.InverseLerp(0.85f, 1f, f), 0.3f);
            }
        }
        else if (f < 0.65f)
        {
            num = Custom.LerpMap(f, 0.3f, 0.65f, 0.8f, 1f, 0.5f);
        }
        else
        {
            num = Custom.LerpMap(f, 0.65f, 1f, 1f, 0.2f);
            num = Mathf.Max(num, 0.1f + 0.6f * Mathf.Sin(Custom.LerpMap(f, 0.85f, 1f, 0.5f, 1f) * 3.1415927f));
        }
        return num * 15f * this.scale;
    }

    public float LowerLegContour(float f, bool side, float flip)
    {
        float num = 0f;
        if (f < 0.1f)
        {
            num = 0.5f + 0.5f * Custom.BackwardsSCurve(Mathf.InverseLerp(0f, 0.1f, f), 0.3f);
        }
        else if (num < 0.8f)
        {
            num = Custom.LerpMap(f, 0.1f, 0.8f, 1f, 0.6f, 0.3f);
        }
        else
        {
            num = 0.6f;
        }
        if (side)
        {
            num = Mathf.Max(num, 0.5f + Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(0f, 0.3f, f), 0.5f) * 3.1415927f));
        }
        else
        {
            num = Mathf.Max(num, Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(0.2f, 0.5f, f), 0.6f) * 3.1415927f));
        }
        num += Mathf.Sin(Mathf.Pow(f, 0.5f) * 3.1415927f) * (side ? -1f : 1f) * flip;
        if (f > 0.85f)
        {
            if (side)
            {
                num += Mathf.Sin(Mathf.InverseLerp(0.85f, 1f, f) * 3.1415927f) * 0.7f;
            }
            num *= 0.3f + 0.7f * Mathf.InverseLerp(1f, 0.94f, f);
        }
        return num * 10f * scale;
    }
    
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        //primaryColor = Color.Lerp(palette.blackColor, Colors.VisibleWhite, .87f);
        sLeaser.sprites[NeckConnectorSprite].color = primaryColor;
        sLeaser.sprites[ButtockSprite(0)].color = primaryColor;
        sLeaser.sprites[ButtockSprite(1)].color = primaryColor;
        for (int i = 0; i < (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).verticeColors.Length; i++)
        {
            (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).verticeColors[i] = primaryColor;
        }
        for (int j = 0; j < legs.GetLength(0); j++)
        {
            for (int k = 0; k < (sLeaser.sprites[ThightSprite(j)] as TriangleMesh).verticeColors.Length; k++)
            {
                (sLeaser.sprites[ThightSprite(j)] as TriangleMesh).verticeColors[k] = primaryColor;
            }
        }
        base.ApplyPalette(sLeaser, rCam, palette);
    }
    #endregion

    #region Post-Encounter Graphics
    public void AfterEncounteredVisual()
    {
        DreamerData data = SpecialData;
        if (data == null)
        {
            return;
        }
        // No warp OR About to completely dissapear
        if (data.destPos == null || scale <= 0.01f)
        {
            AddRippleRing();
            FinishAfterConversationCounter();
            return;
        }

        Shrink();
    }

    private void Shrink()
    {
        targetScale -= 0.0005f;
        distortionScaleFac += 0.002f;
        scale = Mathf.Lerp(scale, targetScale, 0.006f);
        return;
    }

    private void AddRippleRing()
    {
        if (dreamerWarpRing == null)
        {
            dreamerWarpRing = new RippleRing(pos, afterConversationCounter, 1f, 0.5f);
            room.AddObject(dreamerWarpRing);
            if (room.updateList.Contains(dreamerWarpRing))
            {
                Plugin.logger.LogDebug("Dreamer: Added a ripple ring to room");
            }
        }
    }

    private void FinishAfterConversationCounter()
    {
        Plugin.logger.LogDebug($"Dreamer: Counter is - {afterConversationCounter} before Encounter finished");
        afterConversationCounter.Finish();
        return;
    }
    #endregion

    #region WarpPoints
    private void SpawnWarp()
    {
        DreamerData data = SpecialData;
        if (data == null)
        {
            return;
        }
        if (data.destRoom == null)
        {
            return;
        }
        PlacedObject placedObject = new PlacedObject(PlacedObject.Type.WarpPoint, data.CreateWarpPointData(room));
        placedObject.pos = pos;
        WarpPoint warpPoint = room.TrySpawnWarpPoint(placedObject, true);

        if (warpPoint != null)
        {
            warpPoint.triggerTime = (float)((int)(warpPoint.triggerActivationTime - 1f));
            warpPoint.strongPull = true;
            warpPoint.guaranteeTrigger = true;
        }
    }

    public static void SpawnBackupWarpPoint(Room room, PlacedObject o)
    {
        WarpPoint.WarpPointData warpPointData = (o.data as DreamerData).CreateWarpPointData(room);
        PlacedObject placedObject = new PlacedObject(PlacedObject.Type.WarpPoint, warpPointData);
        placedObject.pos = o.pos;
        bool flag = false;
        foreach (WarpPoint warpPoint in room.warpPoints)
        {
            if (warpPoint.Data.destRoom == warpPointData.destRoom && Vector2.Distance(warpPoint.pos, placedObject.pos) < 10f)
            {
                flag = true;
                break;
            }
        }
        if (!flag)
        {
            room.TrySpawnWarpPoint(placedObject, true);
        }
    }
    #endregion

    #region Encountering and Removing
    private void MarkEncountered()
    {
        if (encounterFinished)
        {
            return;
        }
        DreamerData data = SpecialData;
        if (data == null)
        {
            return;
        }

        Plugin.logger.LogDebug($"Dreamer: I have finished my encounter!");
        var game = room.world.game;
        var state = game.GetStorySession.saveState;
        string currentRoomName = room.abstractRoom.name;

        SaveEncounter(state, currentRoomName);
        IncreaseSpiralLevel(state);
        OverwriteSaveDen(game, currentRoomName);
        DreamersHooks.DeactivateDreamerPresence(room);

        encounterFinished = true;
    }

    private void OverwriteSaveDen(RainWorldGame game, string currentRoomName)
    {
        RainWorldGame.ForceSaveNewDenLocation(game, currentRoomName, false);
        Plugin.logger.LogDebug($"Dreamer: Saved {currentRoomName} as den");
    }

    private void SaveEncounter(SaveState state, string currentRoomName)
    {
        BeaconSaveData.SetDreamerEncounteredRooms(state, currentRoomName);
        var encounterNumber = BeaconSaveData.GetDreamerEncountersNumber(state);
        encounterNumber++;
        BeaconSaveData.SetDreamerEncountersNumber(state, encounterNumber);
        string joinedString = String.Join(",", BeaconSaveData.GetDreamerEncounteredRooms(state));
        Plugin.logger.LogDebug($"Dreamer: Set encountered rooms - {joinedString}");
    }

    private void IncreaseSpiralLevel(SaveState state)
    {
        var maxLevel = BeaconSaveData.GetMaxSpiralLevel(state);
        float increment = 0f;
        if (maxLevel >= 0.5f)
        {
            increment = 0.5f;
            if (!BeaconSaveData.GetCanUseThanatosis(state))
            {
                BeaconSaveData.SetCanUseThanatosis(state, true);
            }
        }
        else
        {
            increment = 0.25f;
        }
        BeaconSaveData.SetMaxSpiralLevel(state, maxLevel += increment);
        Plugin.logger.LogDebug($"Dreamer: Increased your level by {increment}, level is {maxLevel}");
    }

    private void Despawn()
    {
        if (!slatedForDeletetion)
        {
            DreamersHooks.targetDreamIntensity = 0f;
            slatedForDeletetion = true;
        }
    }
    #endregion

    private readonly int totalSprites;
    private readonly int behindBodySprites;
    private readonly int totalStaticSprites = 10;
    private float sinBob;

    private float flipProg;
    private float flipSpeed;
    private float flip;
    private float flipFrom;
    private float flipTo;
    private float defaultFlip;

    private Part[] spine;  
    private Part[,] legs;
    private Rags rags;
    private Chains chains;

    public PlacedObject placedObject;
    public DreamerConversation conversation;
    public RippleRing dreamerWarpRing;

    private Counter onScreenCounter = new Counter(120, 0, true);
    private Counter afterConversationCounter = new Counter(280, 0, true);

    private bool convoActive;
    private bool convoFinished;
    private bool encounterFinished;

    private float scale;
    private float targetScale = 0.5f;
    private float distortionScaleFac;
    private float lightSpriteScale = 0.3f;
    private int spineSegments = 11;
    private int snoutSegments = 2;
    private int spineBendPoint = 7;
    private int thighSegments = 7;
    private int lowerLegSegments = 17;
    private float airResistance = 0.6f;

    public Color primaryColor = Colors.VisibleWhite;
    public Color accentColor = Colors.Rose;
    public Color glowColor = Colors.ComplementaryRose;

    public Vector2 headPos;
    public int dreamSpawnCaught;
}
