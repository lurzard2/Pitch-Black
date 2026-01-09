using HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HUD.HUD;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace PitchBlack;

public class HUDCycle
{
    public CycleMeter meter;
    public Vector2 aboveMeterPos;
    public Vector2 atMeterPos;
    private bool decidedPos = false;
    private float posLerp;
    public Vector2 realPos;

    public State state;
    public class State : ExtEnum<State>
    {
        public State(string value, bool register) : base(value, register) { }
        public static readonly State Active = new(nameof(Active), true);
        public static readonly State Limbo = new(nameof(Limbo), true);
        public static readonly State Sacrificed = new(nameof(Sacrificed), true);
        public static readonly State Locked = new(nameof(Locked), true);
    }

    public int index;
    public bool selected = false;
    public bool baseCycle = false;
    public bool Usable => state != State.Sacrificed || state != State.Locked;

    public FSprite sprite;

    public HUDCycle(CycleMeter meter, int index)
    {
        this.meter = meter;
        this.index = index;
        state = State.Active;
        sprite = new FSprite("Futile_White", true);
    }

    public void Draw(float t)
    {
        var dead = Futile.atlasManager.GetElementWithName("Multiplayer_Death");
        var alive = Futile.atlasManager.GetElementWithName("Kill_Slugcat");

        bool notUnlocked = !meter.Unlocked && baseCycle;
        if (notUnlocked)
        {
            sprite.element = alive;
            sprite.color = Color.grey;
        }
        else if (meter.IsOutsideCycle)
        {
            sprite.element = baseCycle ? dead : alive;
            sprite.color = baseCycle ? Color.white : Color.grey;
        }
        else
        {
            if (state == State.Active)
            {
                sprite.element = baseCycle ? dead : alive;
                sprite.color = Color.white;
            }
            if (state == State.Limbo)
            {
                // Chosen eye color from thanatosis progression
                sprite.color = ScugGraphics.SpriteColors[1];
            }
            if (state == State.Sacrificed)
            {
                sprite.element = dead;
                sprite.color = Colors.PlayerPaletteBlack;
            }
        }

        if (meter.hud.karmaMeter != null)
        {
            if (!decidedPos)
            {
                // offset position
                float xPosOffsetForIndex = 0;
                for (int i = 0; i < meter.cycles.Count; i++)
                {
                    if (i == index)
                    {
                        aboveMeterPos.x = meter.CornerPos.x + xPosOffsetForIndex;
                        aboveMeterPos.y = (meter.CornerPos.y) + 55f;
                        break;
                    }
                    xPosOffsetForIndex += 35f;
                }
                atMeterPos = aboveMeterPos;
                atMeterPos.y -= 50f;
                decidedPos = true;
            }

            if (meter.hud.karmaMeter.fade > 0.15)
            {
                posLerp = Mathf.Lerp(posLerp, 1f, 0.04f);
            }
            else
            {
                posLerp = Mathf.Lerp(posLerp, 0f, 0.04f);
            }
        }

        sprite.x = Mathf.Lerp(atMeterPos.x, aboveMeterPos.x, posLerp);
        sprite.y = Mathf.Lerp(atMeterPos.y, aboveMeterPos.y, posLerp);
        realPos = new(sprite.x, sprite.y);
        sprite.alpha = meter.fade.a;
    }

    public void Update()
    {
        if (selected)
        {
            SelectedUpdate();
        }
    }

    public void SelectedUpdate()
    {
        Sync();
    }

    public void Sync()
    {
        if (meter.IsCached)
        {
            state = State.Sacrificed;
        }
        // Targets Alive
        else if (Usable)
        {
            state = meter.IsInLimbo ? State.Limbo : State.Active;
        }
    }
}
