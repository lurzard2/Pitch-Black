using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Watcher;

namespace PitchBlack
{
    internal class RotDeerCritob : Critob
    {
        // lowkey copying citizens critob, adjusting later idk
        public RotDeerCritob() : base(Enums.CreatureTemplateType.RotDeer)
        {
            Icon = new SimpleIcon("Kill_RainDeer", RainWorld.RippleColor);
            LoadedPerformanceCost = 100f;
            ShelterDanger = 0;
        }
        public override int ExpeditionScore() => 0;
        public override Color DevtoolsMapColor(AbstractCreature acrit) => Colors.VisibleWhite;
        public override string DevtoolsMapName(AbstractCreature acrit) => "rotdeer";
        public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [RoomAttractivenessPanel.Category.LikesOutside];
        public override IEnumerable<string> WorldFileAliases() => ["rotdeer"];

        public override CreatureTemplate CreateTemplate()
        {
            CreatureTemplate t = new CreatureFormula(CreatureTemplate.Type.Deer, Enums.CreatureTemplateType.RotDeer, "RotDeer")
            {
                DefaultRelationship = new(CreatureTemplate.Relationship.Type.DoesntTrack, 1f),
                Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Deer),
            }.IntoTemplate();
            t.dangerousToPlayer = 0;
            t.stowFoodInDen = false;
            t.shortcutColor = Colors.VisibleWhite;

            // Got this from citizen fuuuuucckkkk shoutouts to ovidia
            t.offScreenSpeed = 1f;
            t.grasps = 4;
            t.AI = true;
            t.requireAImap = true;
            t.abstractedLaziness = 50;
            t.bodySize = 1.2f;
            t.doPreBakedPathing = false;
            t.preBakedPathingAncestor = t;
            t.stowFoodInDen = true;
            t.shortcutSegments = 3;
            t.visualRadius = 1200f;
            t.movementBasedVision = 0.3f;
            t.canSwim = true;
            t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
            t.hibernateOffScreen = true;
            t.roamBetweenRoomsChance = 0.5f;
            t.roamInRoomChance = 0.7f;
            t.dangerousToPlayer = 0f;
            t.meatPoints = 0;
            return t;
        }

        public override void EstablishRelationships()
        {
            Relationships citzn = new Relationships(Type);
            // To others:
            // -Friends
            citzn.IsInPack(Enums.CreatureTemplateType.Citizen, 1f);
            // -Basegame
            citzn.Ignores(CreatureTemplate.Type.Slugcat);
            citzn.Ignores(CreatureTemplate.Type.Vulture);
            citzn.Ignores(CreatureTemplate.Type.KingVulture);
            citzn.Ignores(CreatureTemplate.Type.Scavenger);
            citzn.Ignores(CreatureTemplate.Type.LanternMouse);
            citzn.Ignores(CreatureTemplate.Type.LizardTemplate);
            citzn.Ignores(CreatureTemplate.Type.Snail);
            citzn.Ignores(CreatureTemplate.Type.GarbageWorm);
            citzn.Ignores(CreatureTemplate.Type.DaddyLongLegs);
            citzn.Ignores(CreatureTemplate.Type.BrotherLongLegs);
            citzn.Ignores(CreatureTemplate.Type.Centipede);
            citzn.Ignores(CreatureTemplate.Type.RedCentipede);
            citzn.Ignores(CreatureTemplate.Type.SmallCentipede);
            citzn.Ignores(CreatureTemplate.Type.TentaclePlant);
            citzn.Ignores(CreatureTemplate.Type.PoleMimic);
            citzn.Ignores(CreatureTemplate.Type.MirosBird);
            citzn.Ignores(CreatureTemplate.Type.BigSpider);
            citzn.Ignores(CreatureTemplate.Type.BigNeedleWorm);
            citzn.Ignores(CreatureTemplate.Type.SmallNeedleWorm);
            citzn.Ignores(CreatureTemplate.Type.DropBug);
            citzn.Ignores(CreatureTemplate.Type.Overseer);
            // -MSC
            citzn.Ignores(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy);
            citzn.Ignores(MoreSlugcatsEnums.CreatureTemplateType.FireBug);
            citzn.Ignores(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC);
            // -DLCShared
            citzn.Ignores(DLCSharedEnums.CreatureTemplateType.Yeek);
            citzn.Ignores(DLCSharedEnums.CreatureTemplateType.MirosVulture);
            citzn.Ignores(DLCSharedEnums.CreatureTemplateType.Inspector);
            citzn.Ignores(DLCSharedEnums.CreatureTemplateType.TerrorLongLegs);
            // -Watcher
            citzn.Ignores(WatcherEnums.CreatureTemplateType.DrillCrab);
            citzn.Ignores(WatcherEnums.CreatureTemplateType.BigSandGrub);
            citzn.Ignores(WatcherEnums.CreatureTemplateType.FireSprite);

            // From others:
            // -Basegame
            citzn.IgnoredBy(CreatureTemplate.Type.LizardTemplate);
            citzn.IgnoredBy(CreatureTemplate.Type.Vulture);
            citzn.IgnoredBy(CreatureTemplate.Type.Scavenger);
            citzn.IgnoredBy(CreatureTemplate.Type.BigSpider);
            // -DLCShared
            citzn.IgnoredBy(DLCSharedEnums.CreatureTemplateType.MirosVulture);
            // -Watcher
            citzn.IgnoredBy(WatcherEnums.CreatureTemplateType.DrillCrab);
            citzn.IgnoredBy(WatcherEnums.CreatureTemplateType.Rattler);
            citzn.IgnoredBy(WatcherEnums.CreatureTemplateType.FireSprite);
            citzn.IgnoredBy(WatcherEnums.CreatureTemplateType.RotLoach);
            citzn.IgnoredBy(WatcherEnums.CreatureTemplateType.Frog);

            // modded creatures?
        }

        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new DeerAI(acrit, acrit.world);
        public override AbstractCreatureAI CreateAbstractAI(AbstractCreature acrit) => new DeerAbstractAI(acrit.world, acrit);
        public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Deer(acrit, acrit.world);
        public override void LoadResources(RainWorld rainWorld) { }
        public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.Deer;

    }
}
