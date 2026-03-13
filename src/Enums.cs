    using System.Diagnostics.CodeAnalysis;
using DevInterface;
using Menu;

namespace PitchBlack;

public static class Enums
{
    public static class SlugcatStatsName
    {
        public static readonly SlugcatStats.Name Beacon = new("Beacon", false);
        // Most code for Photo has been gutted (for now.. idk) -Lur
        public static readonly SlugcatStats.Name Photomaniac = new(nameof(Photomaniac), false);
    }

    public static class Timeline
    {
        public static readonly SlugcatStats.Timeline Beacon = new("Beacon", true);
        public static readonly SlugcatStats.Timeline Dreamer = new(nameof(Dreamer), true);
    }

    public static class DreamSpawnType
    {
        public static readonly VoidSpawn.SpawnType DreamSpawn = new(nameof(DreamSpawn), true);
        public static readonly VoidSpawn.SpawnType DreamJelly = new(nameof(DreamJelly), true);
        public static readonly VoidSpawn.SpawnType DreamAmoeba = new(nameof(DreamAmoeba), true);
        public static readonly VoidSpawn.SpawnType DreamNoodle = new(nameof(DreamNoodle), true);
        public static readonly VoidSpawn.SpawnType DreamEater = new(nameof(DreamEater), true);
        public static readonly VoidSpawn.SpawnType DreamKin = new(nameof(DreamKin), true);
    }
    public static class DreamSpawnSource
    {
        public static readonly Room.RippleSpawnSource Dreamcatcher = new(nameof(Dreamcatcher), true);
        public static readonly Room.RippleSpawnSource Flotsam = new(nameof(Flotsam), true);
        public static readonly Room.RippleSpawnSource Jetsam = new(nameof(Jetsam), true);
    }

    public static class AbstractObjectType
    {
        public static AbstractPhysicalObject.AbstractObjectType DreamSpawn = new(nameof(DreamSpawn), true);
        public static AbstractPhysicalObject.AbstractObjectType RotPuff = new("RotPuff", true);
    }
    public static class CreatureTemplateType
    {
        [AllowNull] public static CreatureTemplate.Type LMiniLongLegs = new(nameof(LMiniLongLegs), true);
        [AllowNull] public static CreatureTemplate.Type NightTerror = new(nameof(NightTerror), true);
        [AllowNull] public static CreatureTemplate.Type Rotrat = new(nameof(Rotrat), true);
        [AllowNull] public static CreatureTemplate.Type Citizen = new(nameof(Citizen), true);
        [AllowNull] public static CreatureTemplate.Type RotDeer = new(nameof(RotDeer), true);

        public static void UnregisterValues()
        {
            if (LMiniLongLegs != null)
            {
                LMiniLongLegs.Unregister();
                LMiniLongLegs = null;
            }
            if (NightTerror != null)
            {
                NightTerror.Unregister();
                NightTerror = null;
            }
            if (Rotrat != null)
            {
                Rotrat.Unregister();
                Rotrat = null;
            }
            if (Citizen != null)
            {
                Citizen.Unregister();
                Citizen = null;
            }
        }
    }
    public static class SandboxUnlockID
    {
        public static MultiplayerUnlocks.SandboxUnlockID LMiniLongLegs = new(nameof(LMiniLongLegs), true);
        public static MultiplayerUnlocks.SandboxUnlockID NightTerror = new(nameof(NightTerror), true);
        public static MultiplayerUnlocks.SandboxUnlockID RotRat = new(nameof(RotRat), true);
        public static MultiplayerUnlocks.SandboxUnlockID RotPuffUnlockID = new("RotPuff", true);
    }
    
    public static class RoomEffectType
    {
        // I just threw this in here, it's used with the others.
        public static RoomSettingsPage.DevEffectsCategories PitchBlackCatagory = new("Pitch-Black", true);
        // Actual effects
        public static RoomSettings.RoomEffect.Type ElsehowView = new("ElsehowView", true);
        public static RoomSettings.RoomEffect.Type RippleSpawn = new("RippleSpawn", true);
        public static RoomSettings.RoomEffect.Type RippleMelt = new("RippleMelt", true);
        public static RoomSettings.RoomEffect.Type RoseSky = new("RoseSky", true);
        public static void UnregisterValues()
        {
            if (PitchBlackCatagory != null)
            {
                PitchBlackCatagory.Unregister();
                PitchBlackCatagory = null;
            }
            if (ElsehowView != null)
            {
                ElsehowView.Unregister();
                ElsehowView = null;
            } 
        }
    }

    public static class PlacedObjectType
    {
        public static ObjectsPage.DevObjectCategories PitchBlackCatagory = new("Pitch-Black", true);
        public static PlacedObject.Type DreamerSpot = new("DreamerSpot", true);
        public static PlacedObject.Type RiftSpot = new("RiftSpot", true);
        public static PlacedObject.Type RiftExitTarget = new(nameof(RiftExitTarget), true);
        public static PlacedObject.Type StillbornSpot = new(nameof(StillbornSpot), true);

        public static void UnregisterValues()
        {
            if (PitchBlackCatagory != null)
            {
                PitchBlackCatagory.Unregister();
                PitchBlackCatagory = null;
            }
            if (DreamerSpot != null)
            {
                DreamerSpot.Unregister();
                DreamerSpot = null;
            }
            if (RiftSpot  != null)
            {
                RiftSpot.Unregister();
                RiftSpot = null;
            }
            if (RiftExitTarget  != null)
            {
                RiftExitTarget.Unregister();
                RiftExitTarget = null;
            }
            if (StillbornSpot != null)
            {
                StillbornSpot.Unregister();
                StillbornSpot = null;
            }
        }
    }

    public static class MenuSceneID
    {
        // Slugbase registers scene jsons but you can also do them in code
        public static MenuScene.SceneID Slugcat_Beacon = new("Slugcat_Beacon", true);
        public static MenuScene.SceneID Slugcat_Beacon_Dreamer = new("Slugcat_Beacon_Dreamer", false);
        public static MenuScene.SceneID Slugcat_Spawn = new("Slugcat_Spawn", false);
        // Dream - Birth
        public static MenuScene.SceneID Dream_Birth_4 = new(nameof(Dream_Birth_4), false);
        public static MenuScene.SceneID Dream_Birth_5 = new(nameof(Dream_Birth_5), false);
        public static MenuScene.SceneID Dream_Birth_6 = new(nameof(Dream_Birth_6), false);
        public static MenuScene.SceneID Dream_Birth_7 = new(nameof(Dream_Birth_7), false);
        public static MenuScene.SceneID Dream_Birth_8 = new(nameof(Dream_Birth_8), false);

        public static void UnregisterValues()
        {
            if (Slugcat_Beacon != null)
            {
                Slugcat_Beacon.Unregister();
                Slugcat_Beacon = null;
            }
        }
    }
    public static class SlideShowID
    {
        public static SlideShow.SlideShowID Dream_Birth = new("Dream_Birth", false);
    }

    // These apparently HAVE to be registered to play ingame. -Lur
    public static class SoundID
    {
        public static global::SoundID Player_Canceled_Thanatosis;
        public static global::SoundID Player_Inducing_Thanatosis;
        public static global::SoundID Player_Deactivated_Thanatosis_From_Stun;
        public static global::SoundID Player_Died_From_Thanatosis;
        public static global::SoundID Player_Revived;
        public static global::SoundID Thanatosis_Drowning_LOOP;
        public static global::SoundID Dreamer_Voice;
        public static global::SoundID Beacon_Voice;
        public static global::SoundID Beacon_Hybrid_Voice;
        public static void RegisterValues()
        {
            Player_Canceled_Thanatosis = new global::SoundID("Player_Activated_Thanatosis", true);
            Player_Inducing_Thanatosis = new global::SoundID("Player_Deactivated_Thanatosis", true);
            Player_Deactivated_Thanatosis_From_Stun = new global::SoundID("Player_Deactivated_Thanatosis_From_Stun", true);
            Player_Died_From_Thanatosis = new global::SoundID("Player_Died_From_Thanatosis", true);
            Player_Revived = new global::SoundID("Player_Revived", true);
            Thanatosis_Drowning_LOOP = new global::SoundID("Drowning_Thanatosis_LOOP", true);
            Dreamer_Voice = new global::SoundID("Dreamer_Voice", true);
            Beacon_Voice = new global::SoundID("Beacon_Voice", true);
            Beacon_Hybrid_Voice = new global::SoundID("Beacon_Hybrid_Voice", true);
        }
        public static void UnregisterValues()
        {
            if (Player_Canceled_Thanatosis != null)
            {
                Player_Canceled_Thanatosis.Unregister();
                Player_Canceled_Thanatosis = null;
            }
            if (Player_Inducing_Thanatosis != null)
            {
                Player_Inducing_Thanatosis.Unregister();
                Player_Inducing_Thanatosis = null;
            }
            if (Player_Deactivated_Thanatosis_From_Stun != null)
            {
                Player_Deactivated_Thanatosis_From_Stun.Unregister();
                Player_Deactivated_Thanatosis_From_Stun = null;
            }
            if (Player_Died_From_Thanatosis != null)
            {
                Player_Died_From_Thanatosis.Unregister();
                Player_Died_From_Thanatosis = null;
            }
            if (Player_Revived != null)
            {
                Player_Revived.Unregister();
                Player_Revived = null;
            }
            if (Thanatosis_Drowning_LOOP != null)
            {
                Thanatosis_Drowning_LOOP.Unregister();
                Thanatosis_Drowning_LOOP = null;
            }
            if (Dreamer_Voice != null)
            {
                Dreamer_Voice.Unregister();
                Dreamer_Voice = null;
            }
            if (Beacon_Voice != null)
            {
                Beacon_Voice.Unregister();
                Beacon_Voice = null;
            }
            if (Beacon_Hybrid_Voice != null)
            {
                Beacon_Hybrid_Voice.Unregister();
                Beacon_Hybrid_Voice = null;
            }
        }
    }

    public class ConversationID
    {
        public static Conversation.ID Dreamer_Placeholder = new(nameof(Dreamer_Placeholder), true);
        public static Conversation.ID Dreamer_Start = new(nameof(Dreamer_Start), true);
        public static Conversation.ID Dreamer_Prologue_1 = new(nameof(Dreamer_Prologue_1), true);
        public static Conversation.ID Dreamer_Prologue_2 = new(nameof(Dreamer_Prologue_2), true);
        public static Conversation.ID Dreamer_Prologue_Intermission = new(nameof(Dreamer_Prologue_Intermission), true);

        public static void UnregisterValues()
        {
            if (Dreamer_Start != null)
            {
                Dreamer_Start.Unregister();
                Dreamer_Start = null;
            }
            if (Dreamer_Prologue_1 != null)
            {
                Dreamer_Prologue_1.Unregister();
                Dreamer_Prologue_1 = null;
            }
            if (Dreamer_Prologue_2 != null)
            {
                Dreamer_Prologue_2.Unregister();
                Dreamer_Prologue_2 = null;
            }
        }
    }
}