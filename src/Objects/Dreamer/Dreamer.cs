using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using Watcher;
using UnityEngine;

namespace PitchBlack;

public class Dreamer : PBEntity
{
    public PlacedObject placedObject;
    public EntityWarpData SpecialData => placedObject.data as EntityWarpData;
    public Vector2 Pos => placedObject.pos;
    public bool convoActive;
    public bool convoFinished;
    public bool encounterFinished;

    public Dreamer(Room room, PlacedObject placedObject) : base(room, placedObject)
    {
        this.placedObject = placedObject;

        visibleEntity = new PlaceholderEchoGraphics(this);
        room.AddObject(visibleEntity);

        DreamerBehavior.EncounterType type = null;
        if (MiscUtils.IsVhosRegion(room.world.name))
        {
            if (SpecialData.destRoom != null)
            {
                type = DreamerBehavior.EncounterType.DreamWarp;
            }
            else
            {
                type = DreamerBehavior.EncounterType.Dream;
            }
        }
        if (MiscUtils.IsNightmareRegion(room.world.name))
        {
            type = DreamerBehavior.EncounterType.Nightmare;
        }

        behaviorModule = new DreamerBehavior(this, type);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (!slatedForDeletetion)
        {
            for (int i = 0; i < room.warpPoints.Count; i++)
            {
                CosmeticRipple ripple = room.warpPoints[i].ripple;
                if (ripple != null)
                {
                    ripple.RemoveFromRoom();
                }
                WarpTear warpTear = room.warpPoints[i].warpTear;
                if (warpTear != null)
                {
                    warpTear.RemoveFromRoom();
                }
                WarpPoint warpPoint = room.warpPoints[i];
                if (warpPoint != null)
                {
                    warpPoint.RemoveFromRoom();
                }
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        deleteMe = true;
    }
}
