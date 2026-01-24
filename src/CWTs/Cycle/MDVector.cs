using Unity.Mathematics;
using UnityEngine;

namespace PitchBlack;

public class MDVector
{
    public float x {  get; set; }
    public float y { get; set; }
    public float z { get; set; }
    // Ripple: value from 0f-3f, see specifics in SpacialTracker
    public float w {  get; set; }
    public float h { get; set; }

    public float gravity;

    public MDVector(float x = 0, float y = 0, float z = 0, float w = 0)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
}
