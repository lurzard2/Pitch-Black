using PitchBlack.Dimensions;
using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public static partial class PBSaveData
{
    /// <summary>
    /// Custom region savedata
    /// </summary>
    public class PBRegionState
    {
        // Dictionary : Key=AbstractRoom.name string, Value=RipplePointData struct
        #region RipplePoints
        public static Dictionary<string, List<RipplePointData>> RoomRipplePoints = [];
        
        public List<RipplePointData> GetRipplePointsInRoom(AbstractRoom room)
        {
            string s = room.name;

            if (!RoomRipplePoints.ContainsKey(s))
            {
                RoomRipplePoints.Add(s, []);
            }
            return RoomRipplePoints[s];
        }

        public void SetRipplePointsInRoom(AbstractRoom room, RipplePointData data)
        {
            var points = GetRipplePointsInRoom(room);
            if (!points.Contains(data))
            {
                points.Add(data);
            }
        }
        #endregion
    }

    public static Dictionary<string, PBRegionState> GetPBRegionStates(this SaveState save)
    {
        return save.deathPersistentSaveData.GetSlugBaseData().TryGet(nameof(PBRegionState), out Dictionary<string, PBRegionState> state) ? state : [];
    }

    public static PBRegionState GetCurrentPBRegionState(this SaveState save, World world)
    {
        string s = world.name;

        if (!save.GetPBRegionStates().ContainsKey(s))
        {
            save.GetPBRegionStates().Add(s, new());
        }
        return save.GetPBRegionStates()[s];
    }
}
