using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack
{
    internal class RotDeerHooks
    {
        private static readonly CreatureTemplate.Type deer2electricboogaloo = Enums.CreatureTemplateType.RotDeer;

        public static void Apply()
        {
            On.DeerGraphics.ApplyPalette += _ApplyPalette;
            On.Deer.ctor += Deer_ctor;
        }

        private static void Deer_ctor(On.Deer.orig_ctor orig, Deer self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (abstractCreature.creatureTemplate.type == Enums.CreatureTemplateType.RotDeer)
            {
                for (int i = 0; i < 4; i++)
                {
                    self.legs[i].maxLength = 1000;
                    self.legs[i].idealLength = 900;
                }
            }
        }

        private static void _ApplyPalette(On.DeerGraphics.orig_ApplyPalette orig, DeerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);

            if (self.deer.abstractCreature.creatureTemplate.type ==  deer2electricboogaloo)
            {
                self.bodyColor = palette.blackColor;
                for (int eyeIndex = 0; eyeIndex < 2; eyeIndex++) 
                {
                    sLeaser.sprites[self.EyeSprite(eyeIndex, 0)].color = self.bodyColor;
                    sLeaser.sprites[self.EyeSprite(eyeIndex, 1)].color = RainWorld.RippleColor;
                }
            }
        }
    }
}
