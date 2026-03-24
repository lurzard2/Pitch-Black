using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using Watcher;

namespace PitchBlack.Dimensions
{
    public struct RipplePointData
    {
        public string roomName;
        public Vector2 screenPos;
        public float radius;
        public float intensity;
    }

    public class RipplePoint : UpdatableAndDeletable
    {
        public RipplePointData pointData;

        public RipplePoint(Room room, RipplePointData pointData)
        {
            this.pointData = pointData;
        }

        public override void Update(bool eu)
        {
        }

        public override void Destroy()
        {
        }
    }
}
