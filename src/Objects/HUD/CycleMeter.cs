using HUD;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class CycleMeter : HudPart
{
    public Vector2 CornerPos => hud.karmaMeter != null ? hud.karmaMeter.pos : Vector2.zero;
    // instead of value and lastValue we're just gonna do this!
    private (Vector2 a, Vector2 b) pos;
    public (float a, float b) fade;
    private float fadeLerp = 0f;

    public bool Unlocked => SaveState.GetMaxSpiralLevel() >= 1f && SaveState.GetHasUsedThanatosis();

    public Player HUDOwner
    {
        get
        {
            if (multiHud != null)
            {
                return multiHud.RealizedPlayer;
            }
            return hud.owner as Player;
        } 
    }
    public SaveState SaveState => HUDOwner.abstractCreature.world.game.TryGetSaveState(out var data) ? data : null;
    public PlayerSpecificMultiplayerHud multiHud;

    public List<HUDCycle> cycles = [];
    public int selectedCycleIndex = 0;
    public HUDCycle currentCycle = null;

    public CycleCursor cursor;

    public bool IsOutsideCycle => MiscUtils.IsRegionOutSideCycle(HUDOwner.abstractCreature.world);

    public List<bool> BeaconTrackedInThanatosis()
    {
        List<bool> flags = [];
        if (multiHud != null)
        {
            // placeholder
        }
        if (HUDOwner.TryGetBeacon(out var beacon))
        {
            bool flag = beacon.cycle.isDead || beacon.cycle.thanatosisLerp > 0.1f;
            flags.Add(flag);
        }
        return flags;
    }

    public List<bool> BeaconOutOfTimeInThanatosis()
    {
        List<bool> flags = [];
        if (multiHud != null)
        {
            // placeholder
        }
        if (HUDOwner.TryGetBeacon(out var beacon))
        {
            bool flag = beacon.cycle.ReachedThanatosisLimit && beacon.cycle.thanatosisDeathCounter.isFinished;
            flags.Add(flag);
        }
        return flags;
    }

    public CycleMeter(HUD.HUD hud, PlayerSpecificMultiplayerHud multiHud, FContainer fContainer) : base(hud)
    {
        this.multiHud = multiHud;
        fade.a = 0f;

        // Adding cycles, with an extra one to serve as the fixed 0 index cycle
        for (int i = 0; i < SaveState.GetMaxSpiralLevel() + 1; i++)
        {
            cycles.Add(new HUDCycle(this, i));
        }

        for (int j = 0; j < cycles.Count; j++)
        {
            if (cycles[j] == cycles.First())
            {
                cycles[j].baseCycle = true;
            }
            if (cycles[j] == cycles.Last())
            {
                cycles[j].selected = true;
                selectedCycleIndex = j;
                currentCycle = cycles[j];
            }
            // Need this for adding sprites
            fContainer.AddChild(cycles[j].sprite);
        }

        cursor = new(this);
        fContainer.AddChild(cursor.cursorSprite);
        fContainer.AddChild(cursor.cursorGlowSprite);
    }

    public override void Update()
    {
        if (currentCycle.state == HUDCycle.State.Sacrificed)
        {
            // we are not reaching an invalid index nuhuh
            if (currentCycle.index > 0)
            {
                selectedCycleIndex--;
                currentCycle = cycles[selectedCycleIndex];
            } 
            if (HUDOwner.TryGetBeacon(out var beacon))
            {
                beacon.cycle.killMe = true;
            }
        }

        pos.b = pos.a;
        fade.b = fade.a;

        cycles.ForEach(cycle =>
        {
            cycle.Update();
            cycle.selected = cycle.index == currentCycle.index;
        });
        
        cursor?.Update();
    }

    public override void Draw(float timeStacker)
    {
        cycles.ForEach((cycle) =>
        {
            cycle.Draw(timeStacker);
        });

        cursor.Draw(timeStacker);

        if (HUDOwner.input[1].mp || HUDOwner.input[1].spec || BeaconTrackedInThanatosis()[0] || BeaconOutOfTimeInThanatosis()[0])
        {
            // hide for a little bit so karma meter and this don't overlap weird
            if (HUDOwner.input[1].mp && hud.karmaMeter.fade < 0.4f && !HUDOwner.input[1].spec && fade.a < 0.3f)
            {
                fadeLerp = 0f;
                fade.a = 0f;
                return;
            }
            fadeLerp = Mathf.Lerp(fadeLerp, 1, 0.06f);
        }
        else if (fadeLerp > 0)
        {
            fadeLerp = Mathf.Lerp(fadeLerp, 0, 0.02f);
        }

        fade.a = Mathf.Lerp(0, 1, fadeLerp);
    }
}