using Unity.Mathematics;
using UnityEngine;

namespace PitchBlack;

public class MDVector
{
    public Vector2 Main {  get; set; }
    // Creatures don't use 3d vectors unfortunately but it's here
    public float z { get; set; }

    // See specifics for these values in SpacialTracker

    // Ripple: value from 0f-5f
    public float v {  get; set; }

    // Dream: value from 0f-1f
    public float w { get; set; }

    // Spiral: value from 0f-5f
    public float h { get; set; }

    public float gravity { get; set; }

    public MDVector(float x = 0, float y = 0, float z = 0, float v = 0, float w = 0, float h = 0)
    {
        Main = new Vector2(x, y);
        this.z = z;
        this.v = v;
        this.w = w;
        this.h = h;
    }
}
