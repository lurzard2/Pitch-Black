using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack.AbstractDimensionData
{
    public static class _Hooks
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
            if (self.TryGetDimensionData(out var data))
            {
                data.Update();
            }
        }

        private static void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            orig(self, world, creatureTemplate, realizedCreature, pos, ID);
            _Utils.objDimensionData.Add(self, new(self));
        }
    }
}
