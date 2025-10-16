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

    public static class GhostID
    {
        public static GhostWorldPresence.GhostID Dreamer;

        public static void UnregisterVaues()
        {
            if (Dreamer != null)
            {
                Dreamer.Unregister();
                Dreamer = null;
            }
        }
    }

    public static class Timeline
    {
        public static readonly SlugcatStats.Timeline Beacon = new("Beacon", false);
    }

    public static class CreatureTemplateType
    {
        [AllowNull] public static CreatureTemplate.Type LMiniLongLegs = new(nameof(LMiniLongLegs), true);
        [AllowNull] public static CreatureTemplate.Type NightTerror = new(nameof(NightTerror), true);
        [AllowNull] public static CreatureTemplate.Type Rotrat = new(nameof(Rotrat), true);
        [AllowNull] public static CreatureTemplate.Type Citizen = new(nameof(Citizen), true);

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
        [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID LMiniLongLegs = new(nameof(LMiniLongLegs), true);
        [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID NightTerror = new(nameof(NightTerror), true);
        [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID RotRat = new(nameof(RotRat), true);

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
            if (RotRat != null)
            {
                RotRat.Unregister();
                RotRat = null;
            }
        }
    }
    
    public static class RoomEffectType
    {
        // I just threw this in here, it's used with the others.
        public static RoomSettingsPage.DevEffectsCategories PitchBlackCatagory = new ("Pitch-Black", true);
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
        public static PlacedObject.Type DreamerSpot = new("DreamerSpot", true);

        public static void UnregisterValues()
        {
            if (DreamerSpot != null)
            {
                DreamerSpot.Unregister();
                DreamerSpot = null;
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
        public static MenuScene.SceneID Dream_Birth_1 = new(nameof(Dream_Birth_1), false);
        public static MenuScene.SceneID Dream_Birth_2 = new(nameof(Dream_Birth_2),  false);
        public static MenuScene.SceneID Dream_Birth_3 = new(nameof(Dream_Birth_3), false);
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
        public static SlideShow.SlideShowID DreamBirth = new("DreamBirth", true);

        public static void UnregisterValues()
        {
            if (DreamBirth != null)
            {
                DreamBirth.Unregister();
                DreamBirth = null;
            }
        }
    }
}