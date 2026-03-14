using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack.AbstractDimensionData
{
    public static class _Utils
    {
        public static readonly ConditionalWeakTable<AbstractPhysicalObject, AbstractDimensionData> objDimensionData = new();

        public static void AddDimensionData(this AbstractPhysicalObject absCrit)
        {
            objDimensionData.Add(absCrit, new(absCrit));
        }

        public static bool TryGetDimensionData(this AbstractPhysicalObject absCrit, out AbstractDimensionData c)
        {
            c = null;
            return (objDimensionData.TryGetValue(absCrit, out c));
        }

        public static bool TryGetRealizedObj(this AbstractPhysicalObject absObj, out PhysicalObject realizedObj)
        {
            realizedObj = absObj.realizedObject;
            return realizedObj is not null && realizedObj.room is not null;
        }
    }
}
