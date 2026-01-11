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

    public FSprite cursorSprite;
    public FSprite cursorGlowSprite;

    public float colorLerp;

    public CycleCursor(CycleMeter meter)
    {
        this.meter = meter;
        cursorSprite = new FSprite("Futile_White", true);
        cursorSprite.element = Futile.atlasManager.GetElementWithName("EndGameCircle");
        cursorGlowSprite = new FSprite("Futile_White", true);
        cursorGlowSprite.shader = RWCustom.Custom.rainWorld.Shaders["FlatWaterLightBothSides"];
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

        if (meter.HUDOwner.rippleDeathTime > 0 && colorLerp < 1)
        {
            colorLerp += 0.01f;
        }
        else if (meter.HUDOwner.rippleDeathTime == 0 && colorLerp > 0)
        {
            colorLerp -= 0.01f;
        }

        Color baseColor = meter.IsOutsideCycle ? Color.grey : CurrentCycle.sprite.color;
        cursorSprite.color = Color.Lerp(baseColor, CurrentCycle.fullAccentColor, colorLerp);
        cursorSprite.x = Mathf.Lerp(cursorSprite.x, pos.a.x, 0.06f);
        cursorSprite.y = pos.a.y;
        if (!meter.Unlocked)
        {
            cursorSprite.alpha = 0f;
        }
        else if (meter.selectedCycleIndex == 0 && cursorSprite.alpha > 0)
        {
            cursorSprite.alpha -= 0.06f;
        }
        else
        {
            cursorSprite.alpha = meter.fade.a;
        }

        cursorGlowSprite.x = cursorSprite.x;
        cursorGlowSprite.y = cursorSprite.y;
        cursorGlowSprite.color = cursorSprite.color;
        cursorGlowSprite.alpha = colorLerp;
        cursorGlowSprite.scale = colorLerp * 8f;
    }
}
