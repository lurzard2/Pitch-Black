using System.Linq;
using UnityEngine;

namespace PitchBlack;

public class  DreamSpawnBehavior
{
    public class Caught : VoidSpawn.Behavior
    {
        public Caught(VoidSpawn owner, Room room) : base(owner) { }

        public override Vector2 SwimTowards
        {
            get
            {
                // Defaults
                Dreamer target = null;
                float distance = 0f;

                // Assigning
                if (owner.room is not null && owner.room.updateList.FirstOrDefault(x => x is Dreamer) is Dreamer dummyTarget)
                {
                    for (int i = 0; i < owner.room.updateList.Count; i++)
                    {
                        if (owner.room.updateList[i] is Dreamer)
                        {
                            dummyTarget = owner.room.updateList[i] as Dreamer;
                        }
                        if (dummyTarget != null
                            && dummyTarget.room != null
                            && dummyTarget.room.abstractRoom.index == owner.room.abstractRoom.index)
                        {
                            float distanceFromTarget = Vector2.Distance(owner.firstChunk.pos, dummyTarget.Pos);
                            if (target == null || distanceFromTarget < distance)
                            {
                                target = dummyTarget;
                                distance = distanceFromTarget;
                            }
                        }
                    }
                }
                else if (target == null)
                {
                    return new Vector2(owner.mainBody[0].pos.x, owner.mainBody[1].pos.y);
                }
                return target.Pos;

            }
        }
    }

}