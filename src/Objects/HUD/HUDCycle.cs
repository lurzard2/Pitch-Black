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
    public Vector2 pos;
    private bool decidedPos = false;

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

    private FAtlasElement element;
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

        if (baseCycle)
        {
            sprite.element = dead;
        }
        else
        {
            if (state == State.Active)
            {
                sprite.element = alive;
                sprite.color = Color.white;
            }
            if (state == State.Limbo)
            {
                sprite.color = ScugGraphics.SpriteColors[1];
            }
            if (state == State.Sacrificed)
            {
                sprite.element = dead;
                sprite.color = Colors.PlayerPaletteBlack;
            }
        }

        if (!decidedPos && meter.hud.karmaMeter != null)
        {
            // offset position
            float xPosOffsetForIndex = 0;
            for (int i = 0; i < meter.cycles.Count; i++)
            {
                if (i == index)
                {
                    pos.x = meter.CornerPos.x + xPosOffsetForIndex;
                    pos.y = (meter.CornerPos.y + 30f) + 200f;
                    break;
                }
                xPosOffsetForIndex += 100f;
            }
            decidedPos = true;
        }
        sprite.x = pos.x;
        sprite.y = pos.y;
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
        // Targets Alive
        bool switchToLimbo = Usable;
        if (switchToLimbo)
        {
            state = meter.IsInLimbo ? State.Limbo : State.Active;
        }
        if (meter.IsCached)
        {
            state = State.Sacrificed;
        }
    }
}
