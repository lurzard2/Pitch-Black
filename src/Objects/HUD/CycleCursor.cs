using HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HUD.HUD;
using UnityEngine;

namespace PitchBlack;

public class CycleCursor
{
    private CycleMeter meter;
    private HUDCycle CurrentCycle => meter.currentCycle;
    
    private Vector2 TargetPos => CurrentCycle.pos;
    private (Vector2 a, Vector2 b) pos;

    public CycleCursor(CycleMeter meter)
    {
        this.meter = meter;
    }

    public void Update()
    {
        pos.b = pos.a;
    }

    public void Draw(float t)
    {

    }
}
