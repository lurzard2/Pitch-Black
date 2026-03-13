using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack.CreatureCWT
{
    public static class _Hooks
    {
        public static void Apply()
        {
            On.AbstractCreature.ctor += AbstractCreature_ctor_CreatureCWT;
            On.AbstractCreature.Update += AbstractCreature_Update_CreatureCWT;
        }

        private static void AbstractCreature_Update_CreatureCWT(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
        {
            orig(self, time);
            if (self.TryGetCreatureCWT(out var c))
            {
                c.Update();
            }
        }

        private static void AbstractCreature_ctor_CreatureCWT(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            orig(self, world, creatureTemplate, realizedCreature, pos, ID);
            self.SetCreatureCWT();
        }
    }
}
