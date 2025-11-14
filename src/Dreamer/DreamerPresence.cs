using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;
public class DreamerPresence : World.IMigrationInfluence
{
    public DreamerPresence(World world, AbstractRoom dreamerRoom, int id)
    {
        this.world = world;
        this.dreamerRoom = dreamerRoom;
        this.id = id;
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

    int id;
    public World world;
    public AbstractRoom dreamerRoom;
    public List<AbstractRoom> presenceRooms = new List<AbstractRoom>();
}
