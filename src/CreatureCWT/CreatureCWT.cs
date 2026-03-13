using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RWCustom;

namespace PitchBlack.CreatureCWT
{
    public class CreatureCWT
    {
        public CreatureCWT(AbstractCreature absCrit)
        {
            owner = absCrit;
            rippleAxisPoint = UnityEngine.Random.Range(0, RippleInterfacer.rippleSurface);
        }

        public void Update()
        {
            owner.RippleInteract();
        }

        public AbstractCreature owner;
        public CreatureTemplate.Type CreatureType => owner.creatureTemplate.type;

        public float rippleAxisPoint;
        public bool reboundFromRipple;
        public bool AbleToPassRippleSurface { get; set; }
        public Counter rippleSpawnDelay = new(80, 0, true);
    }
}
