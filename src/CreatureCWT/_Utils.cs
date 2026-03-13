using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack.CreatureCWT
{
    public static class _Utils
    {
        public static readonly ConditionalWeakTable<AbstractCreature, CreatureCWT> creatureCWT = new();

        public static void SetCreatureCWT(this AbstractCreature absCrit)
        {
            creatureCWT.Add(absCrit, new(absCrit));
        }

        public static bool TryGetCreatureCWT(this AbstractCreature absCrit, out CreatureCWT c)
        {
            c = null;
            return (creatureCWT.TryGetValue(absCrit, out c));
        }

        public static bool TryGetRealized(this AbstractCreature absCrit, out Creature crit)
        {
            crit = absCrit.realizedCreature;
            return crit is not null;
        }
    }
}
