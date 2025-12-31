using MoreSlugcats;
using System.Collections.Generic;
using UnityEngine;

namespace PitchBlack.Creatures.RotDeer
{
    internal class RotDeer : Deer
    {
        public RotDeer(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
            GenerateIVars();
            collisionRange = 1000f;
            bodyChunks = new BodyChunk[6];
            bodyChunkConnections = new PhysicalObject.BodyChunkConnection[5];
            bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 22.5f, 3f);
            for (int i = 1; i < 5; i++)
            {
                float num = (float)i / 4f;
                num = (1f - num) * 0.5f + Mathf.Sin(Mathf.Pow(num, 0.5f) * 3.1415927f) * 0.5f;
                num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(num, 1f, 0.2f)), 0.7f);
                bodyChunks[i] = new BodyChunk(this, i, new Vector2(0f, 0f), Mathf.Lerp(10f, 35f, num), Mathf.Lerp(1f, 8f, num));
                bodyChunks[i].restrictInRoomRange = 2000f;
                bodyChunks[i].defaultRestrictInRoomRange = 2000f;
            }
            bodyChunkConnections[0] = new PhysicalObject.BodyChunkConnection(bodyChunks[0], bodyChunks[1], 38f, PhysicalObject.BodyChunkConnection.Type.Normal, 1f, -1f);
            for (int j = 1; j < 4; j++)
            {
                bodyChunkConnections[j] = new PhysicalObject.BodyChunkConnection(bodyChunks[j], bodyChunks[j + 1], Mathf.Max(bodyChunks[j].rad, bodyChunks[j + 1].rad) * 0.8f, PhysicalObject.BodyChunkConnection.Type.Normal, 1f, -1f);
            }
            if (ModManager.MMF && MMF.cfgDeerBehavior.Value)
            {
                bodyChunks[5] = new BodyChunk(this, 0, new Vector2(0f, 0f), Mathf.Lerp(30f, 60f + 20f * Mathf.InverseLerp(0.8f, 1f, abstractCreature.personality.dominance), abstractCreature.personality.dominance), 0.5f);
            }
            else
            {
                bodyChunks[5] = new BodyChunk(this, 0, new Vector2(0f, 0f), Mathf.Lerp(60f, 90f, abstractCreature.personality.dominance), 0.5f);
            }
            bodyChunkConnections[4] = new PhysicalObject.BodyChunkConnection(bodyChunks[0], antlers, bodyChunks[0].rad + antlers.rad - 10f, PhysicalObject.BodyChunkConnection.Type.Normal, 1f, 0f);
            antlers.collideWithObjects = false;
            bodyChunks[0].rotationChunk = bodyChunks[5];
            bodyChunks[5].rotationChunk = bodyChunks[0];
            legs = new DeerTentacle[4];
            for (int k = 0; k < 4; k++)
            {
                legs[k] = new DeerTentacle(this, bodyChunks[(k < 2) ? 1 : 2], 600f, k);
            }
            flipDir = ((UnityEngine.Random.value < 0.5f) ? -1f : 1f);
            lastControlX = flipDir;
            airFriction = 0.999f;
            gravity = 0.9f;
            bounce = 0.1f;
            surfaceFriction = 0.4f;
            collisionLayer = 1;
            waterFriction = 0.95f;
            waterRetardationImmunity = 0f;
            buoyancy = 0.93f;
            windAffectiveness = 0.5f;
            GoThroughFloors = true;
            playersInAntlers = new List<Deer.PlayerInAntlers>();
        }
    }
}
