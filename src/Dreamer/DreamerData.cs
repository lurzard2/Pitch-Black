using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Watcher;

namespace PitchBlack;
public class DreamerData : PlacedObject.Data
{
    public string RegionString
    {
        get
        {
            if (destRegion == null && destRoom == null)
            {
                return null;
            }
            if (destRegion != null)
            {
                return destRegion;
            }
            return destRoom.Split(new char[]
            {
                    '_'
            })[0];
        }
        set
        {
            destRegion = value;
        }
    }

    public DreamerData(PlacedObject owner) : base(owner)
    {
    }

    public override void FromString(string s)
    {
        string[] array = Regex.Split(s, "~");
        int num = array.Length;
        panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
        panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
        destTimeline = ((array[2] == "NULL") ? null : new SlugcatStats.Timeline(array[2], false));
        destRegion = ((array[3] == "NULL") ? null : array[3]);
        destRoom = ((array[4] == "NULL") ? null : array[4]);
        if (array[5] == "NULL" || array[6] == "NULL")
        {
            destPos = null;
        }
        else
        {
            Vector2 zero = Vector2.zero;
            zero.x = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
            zero.y = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
            destPos = new Vector2?(zero);
        }
        spawnIdentifier = ((array.Length > 7) ? int.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture) : 0);
        rippleWarp = ((array.Length > 8) ? (array[8] == "true") : rippleWarp);
        unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 9);
    }

    public override string ToString()
    {
        IFormatProvider invariantCulture = CultureInfo.InvariantCulture;
        string format = "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}";
        object[] array = new object[9];
        array[0] = panelPos.x;
        array[1] = panelPos.y;
        array[2] = ((destTimeline == null) ? "NULL" : destTimeline.ToString());
        array[3] = ((RegionString == null) ? "NULL" : RegionString);
        array[4] = ((destRoom == null) ? "NULL" : destRoom);
        int num = 5;
        object obj;
        if (destPos != null)
        {
            Vector2 value = destPos.Value;
            obj = value.x.ToString();
        }
        else
        {
            obj = "NULL";
        }
        array[num] = obj;
        int num2 = 6;
        object obj2;
        if (destPos != null)
        {
            Vector2 value = destPos.Value;
            obj2 = value.y.ToString();
        }
        else
        {
            obj2 = "NULL";
        }
        array[num2] = obj2;
        array[7] = spawnIdentifier.ToString();
        array[8] = (rippleWarp ? "true" : "false");
        string text = string.Format(invariantCulture, format, array);
        text = SaveState.SetCustomData(this, text);
        return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
    }

    public WarpPoint.WarpPointData CreateWarpPointData(Room room)
    {
        WarpPoint.WarpPointData warpPointData = new WarpPoint.WarpPointData(null);
        warpPointData.destPos = destPos;
        warpPointData.RegionString = RegionString;
        warpPointData.destRoom = destRoom;
        warpPointData.destTimeline = destTimeline;
        warpPointData.panelPos = panelPos;
        warpPointData.deathPersistentWarpPoint = true;
        warpPointData.rippleWarp = rippleWarp;
        warpPointData.oneWay = (rippleWarp || Region.IsWatcherVanillaRegion(room.world.name) || Region.IsVanillaSentientRotRegion(room.world.name));
        if (warpPointData.oneWay)
        {
            warpPointData.oneWayEntrance = true;
            warpPointData.oneWayEntranceIdentified = true;
        }
        if (room.game.IsStorySession)
        {
            warpPointData.cycleSpawnedOn = room.game.GetStorySession.saveState.cycleNumber;
        }
        warpPointData.destCam = WarpPoint.GetDestCam(warpPointData);
        return warpPointData;
    }

    public Vector2 panelPos;
    public Vector2? destPos;
    public SlugcatStats.Timeline destTimeline;
    private string destRegion;
    public string destRoom;
    public int spawnIdentifier;
    public bool rippleWarp;
}
