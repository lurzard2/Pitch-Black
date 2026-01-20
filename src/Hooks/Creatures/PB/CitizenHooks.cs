using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PitchBlack.Creatures.Citizen;
using ScavengerCosmetic;
using UnityEngine;

namespace PitchBlack;

public static class CitizenHooks
{
    private static readonly CreatureTemplate.Type citizen = Enums.CreatureTemplateType.Citizen;

    public static void Apply()
    {
        On.Scavenger.ctor += Scavenger_ctor;
        On.Scavenger.Update += Scavenger_Update;
        On.Scavenger.Grab += Scavenger_Grab;
        On.Scavenger.Collide += Scavenger_Collide;
        On.ScavengerAbstractAI.InitGearUp += ScavengerAbstractAI_InitGearUp;
        
        On.ScavengerGraphics.DrawSprites += ScavengerGraphics_DrawSprites;
        On.ScavengerGraphics.GenerateColors += ScavengerGraphics_GenerateColors;
        On.ScavengerGraphics.AddToContainer += ScavengerGraphics_AddToContainer;

        On.Watcher.WarpPoint.ctor += WarpPoint_ctor;
        //removing spine
        IL.ScavengerGraphics.ctor += ScavengerGraphicsOnctor;
        //add looking module
        On.ScavengerAI.ctor += ScavengerAIOnctor;
        //force scav looking without agitation
        On.Scavenger.ForcedLookCreature += ScavengerOnForcedLookCreature;

    }

    static Tracker.CreatureRepresentation ScavengerOnForcedLookCreature(On.Scavenger.orig_ForcedLookCreature orig, Scavenger self)
    {
        Tracker.CreatureRepresentation result = orig(self);
        if (self.Template.type == Enums.CreatureTemplateType.Citizen && result is null)
        {
            return self.AI.focusCreature;
        }
        return result;
    }

    static void ScavengerAIOnctor(On.ScavengerAI.orig_ctor orig, ScavengerAI self, AbstractCreature creature, World world)
    {
        orig(self, creature, world);
        if (creature.creatureTemplate.type == Enums.CreatureTemplateType.Citizen)
        {
            self.AddModule(new CitizenLooking(self));
        }
    }

    delegate bool ContinousScavGraphsInline(ScavengerGraphics scavengerGraphics, ref int num);
    static void ScavengerGraphicsOnctor(ILContext il)
    {
        //removing spine is pretty much impossible without IL. spines add amount of sprites, every scav has spine, spine affects readonly property of specific indexes
        //like chest sprite index. You can't fix this after ctor or before ctor, it needs to be fixed inside ctor 
        
        //<if citizen add graphics module update sprite counter and jump to B>if (UnityEngine.Random.value < 0.1f || scavenger.Elite) (if not satisfied jumps to A)
        // {
        // 	AddSubModule(new HardBackSpikes(this, num));
        // 	totBckCosSprs += subModules[subModules.Count - 1].totalSprites;
        // 	num += subModules[subModules.Count - 1].totalSprites;
        // JUMP B
        // }
        // else
        // {
        // 	A:  AddSubModule(new WobblyBackTufts(this, num));
        // 	totBckCosSprs += subModules[subModules.Count - 1].totalSprites;
        // 	num += subModules[subModules.Count - 1].totalSprites;
        // }
        // B: ChestSprite = num++;
        
        ILCursor cursor = new(il);
        #nullable enable
        ILLabel? skipWobblyBackTuftLabel = null;
        if (cursor.TryGotoNext(MoveType.Before, x => x.MatchNewobj(typeof(WobblyBackTufts)))
            && cursor.TryGotoPrev(MoveType.After, x => x.MatchBr(out skipWobblyBackTuftLabel))
            && cursor.TryGotoPrev(MoveType.AfterLabel, x => x.MatchCall(typeof(UnityEngine.Random).GetProperty(nameof(UnityEngine.Random.value))!.GetGetMethod()))
            )
        {
                
            cursor
                .Emit(OpCodes.Ldarg, 0)
                .Emit(OpCodes.Ldloca, 1)
                .EmitDelegate<ContinousScavGraphsInline>((ScavengerGraphics graphics, ref int ongoingSpriteNumber) =>
                {
                    if (graphics.scavenger.Template.type == Enums.CreatureTemplateType.Citizen)
                    {
                        CitizenMask mask = new(graphics, ongoingSpriteNumber, graphics.BlendedHeadColor);
                        graphics.AddSubModule(mask);
                        ongoingSpriteNumber += mask.totalSprites;
                        return true; 
                    }
                    return false;
                });
            cursor.Emit(OpCodes.Brtrue, skipWobblyBackTuftLabel);
        }
        else MiscUtils.LogExErr("IL match for citizen features failed. Citizens would have spines and yield no mask");
        #nullable disable
    }

    private static void WarpPoint_ctor(On.Watcher.WarpPoint.orig_ctor orig, Watcher.WarpPoint self, Room room, PlacedObject placedObject)
    {
        orig(self, room, placedObject);
        if (!self.blackListedCreatureTypes.Contains(Enums.CreatureTemplateType.Citizen))
            self.blackListedCreatureTypes.Add(Enums.CreatureTemplateType.Citizen);
    }

    #region Collision disabling hooks

    private static void ScavengerAbstractAI_InitGearUp(On.ScavengerAbstractAI.orig_InitGearUp orig, ScavengerAbstractAI self)
    {
        if (self.parent.creatureTemplate.type == citizen)
        {
            return;
        }

        orig(self);
    }

    private static void Scavenger_Collide(On.Scavenger.orig_Collide orig, Scavenger self, PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        if (self.Template.type == citizen)
        {
            return;
        }

        orig(self, otherObject, myChunk, otherChunk);
    }

    private static bool Scavenger_Grab(On.Scavenger.orig_Grab orig, Scavenger self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        if (self.Template.type == citizen)
        {
            return false;
        }

        return orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
    }

    private static void Scavenger_Update(On.Scavenger.orig_Update orig, Scavenger self, bool eu)
    {
        // Doing this twice before and after orig, to guarantee intended collision. An IL hook would probably be about as effective.
        if (self.Template.type == citizen)
        {
            self.CollideWithObjects = false;
            
            
        }

        orig(self, eu);
        if (self.Template.type == citizen)
        {
            self.CollideWithObjects = false;
        }
    }
    
    #endregion
    
    private static void ScavengerGraphics_GenerateColors(On.ScavengerGraphics.orig_GenerateColors orig, ScavengerGraphics self)
    {
        orig(self);
        if (self.scavenger.Template.type == Enums.CreatureTemplateType.Citizen)
        {
            string[] attributes = self.scavenger.abstractCreature.unrecognizedAttributes;
            if (attributes == null)
            {
                self.bodyColor = 
                    self.headColor = 
                        self.decorationColor = 
                            self.eyeColor = 
                                self.bellyColor = new HSLColor(0.33f, 1f, 0.5f);
            }
            else if (attributes.Contains("BornInUD"))
            {
                self.bodyColor = self.headColor = self.bellyColor = new HSLColor(0.08184808f, 0.06207584f, 0.8753151f);
                self.decorationColor = new HSLColor(0.6535784f, 0.1437009f, 0.3652394f);
                self.eyeColor = new HSLColor(0.6535784f, 0.7f, 0.1f);
            }
            else if (attributes.Contains("NotBornInUD"))
            {
                self.bodyColor = 
                    self.headColor = 
                        self.decorationColor = 
                            self.eyeColor = 
                                self.bellyColor = new HSLColor(0.67f, 0.9f, 0.95f);
            }
            
        }
    }
    
    private static void ScavengerGraphics_DrawSprites(On.ScavengerGraphics.orig_DrawSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPosV2)
    {
        orig(self, sLeaser, rCam, timeStacker, camPosV2);
        
        // to remove its eyes
        if (self.scavenger.Template.type == Enums.CreatureTemplateType.Citizen)
        {
            for (int j = 0; j < 2; j++)
            {
                sLeaser.sprites[self.EyeSprite(j, 0)].isVisible = false;
                if (self.iVars.pupilSize > 0f)
                {
                    sLeaser.sprites[self.EyeSprite(j, 1)].isVisible = false;
                }
            }
        }
        /*
        // umbra scav stuff
        float2 float2 = math.lerp(self.drawPositions[self.headDrawPos, 1], self.drawPositions[self.headDrawPos, 0], timeStacker);
        float2 floatTheSequel = camPosV2.ToF2(); //@float in ScavengerGraphics.DrawSPrites
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if (self.scavenger.Template.type == Enums.CreatureTemplateType.UmbraScav)
            {
                //Mark
                sLeaser.sprites[self.TotalSprites - 1].x = float2.x - floatTheSequel.x;
                sLeaser.sprites[self.TotalSprites - 1].y = float2.y - floatTheSequel.y + 32f;
                sLeaser.sprites[self.TotalSprites - 1].alpha = Mathf.Lerp(self.lastMarkAlpha, self.markAlpha, timeStacker);
                sLeaser.sprites[self.TotalSprites - 1].scale = 5f;
                sLeaser.sprites[self.TotalSprites - 1].color = Color.white;
                sLeaser.sprites[self.TotalSprites - 1].element = Futile.atlasManager.GetElementWithName("pixel");
                sLeaser.sprites[self.TotalSprites - 1].isVisible = true;
                sLeaser.sprites[self.TotalSprites - 1].rotation = 0f;

                //Mark Glow
                sLeaser.sprites[self.TotalSprites - 2].x = float2.x - floatTheSequel.x;
                sLeaser.sprites[self.TotalSprites - 2].y = float2.y - floatTheSequel.y + 32f;
                sLeaser.sprites[self.TotalSprites - 2].alpha = 0.2f * Mathf.Lerp(self.lastMarkAlpha, self.markAlpha, timeStacker);
                sLeaser.sprites[self.TotalSprites - 2].scale = 2f + Mathf.Lerp(self.lastMarkAlpha, self.markAlpha, timeStacker);
                sLeaser.sprites[self.TotalSprites - 2].element = Futile.atlasManager.GetElementWithName("Futile_White");
                sLeaser.sprites[self.TotalSprites - 2].shader = rCam.game.rainWorld.Shaders["FlatLight"];
                sLeaser.sprites[self.TotalSprites - 2].color = Color.white;
                sLeaser.sprites[self.TotalSprites - 2].isVisible = true;
                sLeaser.sprites[self.totalSprites - 2].rotation = 0f;

                //glow inherits MASK POSITION???
                //sprites work as intended without a mask

                //sLeaser.sprites[self.TotalSprites - 1].SetPosition(200f, 250f); //pixel debugging
                //sLeaser.sprites[self.TotalSprites - 2].SetPosition(250f, 250f); //glow debugging
            }
        }*/
    }

    private static void Scavenger_ctor(On.Scavenger.orig_ctor orig, Scavenger self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        
        if (self.Template.type == Enums.CreatureTemplateType.Citizen)
        {
            self.collisionLayer = 2;
            if (abstractCreature.unrecognizedAttributes == null || !Array.Exists(abstractCreature.unrecognizedAttributes,
                    str => str is "BornInUD" or "NotBornInUD"))
            {
                List<string> resizedAttributes = abstractCreature.unrecognizedAttributes == null ? [] : abstractCreature.unrecognizedAttributes.ToList();
                    resizedAttributes.Add(world.name == "UD" ? "BornInUD" : "NotBornInUD");
                abstractCreature.unrecognizedAttributes = resizedAttributes.ToArray();
            }
        }
        // if (self.Template.type == PBEnums.CreatureTemplateType.UmbraScav) //umbra scav stuff
        // {
        //     self.abstractCreature.personality.aggression = 0.4f;
        //     self.abstractCreature.personality.bravery = 0.6f;
        //     self.abstractCreature.personality.dominance = 0.6f;
        //     self.abstractCreature.personality.energy = 0.8f;
        //     self.abstractCreature.personality.nervous = 0.2f;
        //     self.abstractCreature.personality.sympathy = 0.7f;
        // }
    }
    
private static void ScavengerGraphics_AddToContainer(On.ScavengerGraphics.orig_AddToContainer orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        int randomContainerInt = UnityEngine.Random.Range(0, 2);
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
        string RandomContainerStr = randomContainerInt switch
        {
            0 => "Background",
            1 => "Midground",
            2 => "Foreground",
        };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
        if (self.scavenger.Template.type == Enums.CreatureTemplateType.Citizen)
        {
            sLeaser.RemoveAllSpritesFromContainer();
            
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer(RandomContainerStr);
            }
            newContatiner.AddChild(sLeaser.containers[0]);
            
            for (int i = 0; i < self.FirstInFrontLimbSprite; i++)
            {
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
            
            for (int m = self.FirstInFrontLimbSprite; m < self.FirstInFrontLimbSprite + 2; m++)
            {
                newContatiner.AddChild(sLeaser.sprites[m]);
            }
            newContatiner.AddChild(sLeaser.containers[1]);
        }
        
        orig(self, sLeaser,rCam,newContatiner);
        
        foreach (CitizenMask mask in self.subModules.OfType<CitizenMask>())
        {
            mask.AddToContainer(sLeaser, rCam, newContatiner);
        }
    }
}