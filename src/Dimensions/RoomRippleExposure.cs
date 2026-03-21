using Watcher;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace PitchBlack.Dimensions
{
    public class RoomRippleExposure
    {
        // Persistent throughout world's abstractRooms, but updated in realized rooms
        public AbstractRoom absRoom;
        public Room RealizedRoom => absRoom.realizedRoom;
        public float globalExposure;
        public RWCustom.Counter exposureCheckCounter = new(20, 0, true);

        public RoomRippleExposure(AbstractRoom a)
        {
            absRoom = a;
            exposureCheckCounter.Set(Random.Range(exposureCheckCounter.min, exposureCheckCounter.max));
        }

        public void Update()
        {
            float exposure = 0.0015f;
            if (RealizedRoom is not null)
            {
                exposure = GetExposure(exposure);
            }
            globalExposure = exposure;
        }

        private float GetExposure(float defaultVal)
        {
            // Checking with a 0.5s delay and refraining from replacing the value during delay
            if (exposureCheckCounter > 0)
            {
                exposureCheckCounter.Tick();
                if (exposureCheckCounter.isFinished)
                {
                    exposureCheckCounter.Reset();
                }
                return globalExposure;
            }
            exposureCheckCounter.Tick();


            float val = defaultVal;
            CosmeticRipple lastRipple = null;

            RealizedRoom.cosmeticRipples.ForEach(ripple =>
            {
                //Value like 0.0015f;
                float tempPassedExposure = ripple.displayIntensity / (ripple.scale * 10);

                // Compare adjacent positions and nerf this one's exposure if close enough
                if (Vector2.Distance(lastRipple.pos, ripple.pos) < (ripple.scale * 1.25f))
                {
                    if (lastRipple.pos == ripple.pos)
                    {
                        val = 0;
                    }
                    else
                    {
                        val /= 2;
                    }
                }
                lastRipple = ripple;

                val += tempPassedExposure;

                // We will also take this opportunity to affect creatures in room
                // Get realized creatures without a null room and flag then store their data class into an ienumerable for iteration
                var filteredData = absRoom.creatures
                    .Select(c => c.GetDimensionData())
                    .Where(data => data.IsRealized
                        && !data.updateDynamicExposureFlag
                        && Vector2.Distance(data.RealizedOwner.firstChunk.pos, ripple.pos) < ripple.scale);
                foreach (var d in filteredData)
                {

                    float sizeOfRipple = ripple.displayIntensity / (ripple.displayScale + 40f);
                    float centerPointProximity = ripple.PointInRipple(d.RealizedOwner.firstChunk.pos);
                    // preventing divide by zero
                    float myLiteralExposure = centerPointProximity > 0 ? centerPointProximity / 10 : 0;
                    
                    // If pos is within a cosmetic ripple, tag allowed for submersion past surface.
                    // Should be true if on ripple side
                    d.rippleData.AllowedInsideRippleTemporarily = myLiteralExposure > 0 || d.rippleData.RippleSideTag;

                    // Set dynamic exposure value
                    d.dynamicRippleExposureFromProximity = sizeOfRipple + myLiteralExposure;
                    d.updateDynamicExposureFlag = true;
                }
            });

            for (int i = 0; i < absRoom.creatures.Count; i++)
            {
                // Unflag to then continue process
                absRoom.creatures[i].GetDimensionData().updateDynamicExposureFlag = false;
            }

            return val;
        }
    }
}
