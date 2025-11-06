using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;
public class DreamerPresence : World.IMigrationInfluence
{
    public DreamerPresence(World world, AbstractRoom dreamerRoom)
    {
        this.world = world;
        this.dreamerRoom = dreamerRoom;
    }

    #region Relations
    public float AttractionValueForCreature(AbstractRoom room, CreatureTemplate.Type tp, float defValue)
    {
        if (room == dreamerRoom)
        {
            return 0f;
        }
        return defValue;
    }

    public float AttractionValueForCreature(AbstractRoom room, string namedAttr, float defValue)
    {
        if (room == dreamerRoom)
        {
            return 0f;
        }
        return defValue;
    }

    public float AttractionValueForCreature(AbstractRoom room, AbstractCreature creature, float defValue)
    {
        if (room == dreamerRoom)
        {
            return 0f;
        }
        return defValue;
    }
    #endregion

    public World world;
    public AbstractRoom dreamerRoom;
    public List<AbstractRoom> presenceRooms = new List<AbstractRoom>();
}
