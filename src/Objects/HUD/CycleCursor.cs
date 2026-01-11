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
    
    private Vector2 TargetPos => CurrentCycle.realPos;
    private (Vector2 a, Vector2 b) pos;

    public FSprite sprite;
    public FAtlasElement element;

    public CycleCursor(CycleMeter meter)
    {
        this.meter = meter;
        sprite = new FSprite("Futile_White", true);
        sprite.element = Futile.atlasManager.GetElementWithName("EndGameCircle");
        pos.a = CurrentCycle.aboveMeterPos;
    }

    public void Update()
    {
        pos.b = pos.a;
    }

    public void Draw(float t)   
    {
        // t becomes 0 if you speed up, so the pos won''t move unintentionally, don't knwo what to do about that
        pos.a = Vector2.Lerp(pos.b, TargetPos, t);

        sprite.color = meter.IsOutsideCycle ? Color.grey : CurrentCycle.sprite.color;
        sprite.x = Mathf.Lerp(sprite.x, pos.a.x, 0.06f);
        sprite.y = pos.a.y;
        if (!meter.Unlocked)
        {
            sprite.alpha = 0f;
        }
        else if (meter.selectedCycleIndex == 0 && sprite.alpha > 0)
        {
            sprite.alpha -= 0.06f;
        }
        else
        {
            sprite.alpha = meter.fade.a;
        }
    }
}
