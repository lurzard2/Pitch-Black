using Unity.Mathematics;
using UnityEngine;

namespace PitchBlack;

public class MDVector
{
    public float x {  get; set; }
    public float y { get; set; }
    // Creatures don't use 3d vectors unfortunately but it's here
    public float z { get; set; }

    // See specifics for these values in SpacialTracker

    // Ripple: value from 0f-5f
    public float v {  get; set; }

    // Dream: value from 0f-1f
    public float w { get; set; }

    public float gravity { get; set; }

    public MDVector(float x = 0, float y = 0, float z = 0, float v = 0)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.v = v;
    }
}
