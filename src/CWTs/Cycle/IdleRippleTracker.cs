using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RWCustom;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class IdleRippleTracker : CycleModule
{
    public string s = "IdleRippleTracker:";
    public int ripples;
    public float rippleSpawnChance = 0.0008f;
    public int rippleLimit = 10;
    public bool SpawnIdleRipples;
    public Counter delayCounter = new(20, 0);

    public IdleRippleTracker(Cycle cycle) : base(cycle) { }

    public override void Update()
    {
    }
}
