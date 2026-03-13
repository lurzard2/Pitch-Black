using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PitchBlack;

public class Stillborn : PBEntity
{
    public EntityWarpData SpecialData => placedObject.data as EntityWarpData;
    public PlacedObject placedObject;
    public Vector2 Pos => placedObject.pos;
    public RoomCamera RoomCamera => room.game.cameras[0];
    public bool spawned = false;
    public virtual bool OnScreen()
    {
        return room.VisibleInAnyCameraScreenBounds(Pos);
    }
    private bool readyToSpawnAtAll = false;
    public Stillborn(Room room, PlacedObject placedObject) : base(room, placedObject)
    {
        this.placedObject = placedObject;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (room == null || !readyToSpawnAtAll)
        {
            if (OnScreen())
            {
                // Gate rooms don't like loading F03 with it spawning during the process, so we wait    
                readyToSpawnAtAll = true;
            }
            return;
        }

        StillbornBehavior.EncounterType type = null;
        if (room.world != null && room.world.name.ToLowerInvariant() == "pbsb")
        {
            type = StillbornBehavior.EncounterType.Ghost;
            visibleEntity = new PlaceholderEchoGraphics(this);
        }
        if (!spawned && visibleEntity != null && type != null)
        {
            room.AddObject(visibleEntity);
            behaviorModule = new StillbornBehavior(this, type);
            spawned = true;
        }
    }
}
