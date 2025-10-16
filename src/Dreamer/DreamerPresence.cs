using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;
public class DreamerPresence : GhostWorldPresence
{
    int dreamerSpawnID;

    public DreamerPresence(World world, GhostID ghostID, int dreamerSpawnID) : base(world, ghostID, dreamerSpawnID)
    {
        this.ghostID = ghostID;
        this.dreamerSpawnID = dreamerSpawnID;
        this.world = world;
        string text = "";
        if (ghostID == Enums.GhostID.Dreamer)
        {
            text = "";
            // Placeholder
            songName = "RWTW_ST_ELSE_05";
        }
        if (ghostRoom == null || ghostID != Enums.GhostID.Dreamer)
        {
            Custom.LogWarning(
                [
                    "GHOST ROOM NOT FOUND!",
                    text
                ]);
        }
    }

    public AbstractRoom dreamerRoom;
    public int dreamerSpawnId;
}
