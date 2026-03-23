using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack.Dimensions
{
    public static class _Utils
    {
        public static readonly ConditionalWeakTable<AbstractPhysicalObject, AbstractDimensionData> objDimensionData = new();
        public static readonly ConditionalWeakTable<AbstractRoom, RoomRippleExposure> roomRippleExposure = new();
        public static RoomRippleExposure GetRippleExposure(this AbstractRoom room)
        {
            if (!roomRippleExposure.TryGetValue(room, out var re))
            {
                roomRippleExposure.Add(room, new(room));
            }
            return re;
        }

        public static AbstractDimensionData GetDimensionData(this AbstractPhysicalObject absCrit)
        {
            objDimensionData.TryGetValue(absCrit, out var c);
            return c;
        }
    }

    //[ImplicitModHook]
    public static class _Implement
    {
        public static void Apply()
        {
            On.AbstractCreature.ctor += AbstractCreature_ctor;
            On.AbstractCreature.Update += AbstractCreature_Update;
            On.Room.Update += Room_Update;
        }

        private static void Room_Update(On.Room.orig_Update orig, Room self)
        {
            orig(self);
            self.abstractRoom.GetRippleExposure().Update();
        }

        private static void AbstractCreature_Update(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
        {
            orig(self, time);
            self.GetDimensionData().Update();
        }

        private static void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            orig(self, world, creatureTemplate, realizedCreature, pos, ID);
            if (!_Utils.objDimensionData.TryGetValue(self, out _))
            {
                _Utils.objDimensionData.Add(self, new(self));
            }
        }
    }
}
