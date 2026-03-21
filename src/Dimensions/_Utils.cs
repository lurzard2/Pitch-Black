using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack.Dimensions
{
    public static partial class _Utils
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
}
