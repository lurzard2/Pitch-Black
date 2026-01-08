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
    private (float a, float b) fade;

    public bool Unlocked => BeaconSaveData.GetMaxSpiralLevel(SaveState) >= 1f;

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
    public SaveState SaveState => MiscUtils.StoryState(HUDOwner.abstractCreature.world.game);
    public PlayerSpecificMultiplayerHud multiHud;

    public List<HUDCycle> cycles = [];
    public int selectedCycleIndex = 0;
    public HUDCycle currentCycle = null;

    public bool IsInLimbo => MiscUtils.BeaconThanatosis(HUDOwner);
    public bool IsCached => MiscUtils.BeaconIsCached(HUDOwner);

    public CycleCursor cursor;

    public CycleMeter(HUD.HUD hud, PlayerSpecificMultiplayerHud multiHud, FContainer fContainer) : base(hud)
    {
        this.multiHud = multiHud;
        fade.a = 0f;

        // Adding cycles, with an extra one to serve as the fixed 0 index cycle
        for (int i = 0; i < BeaconSaveData.GetMaxSpiralLevel(SaveState) + 1; i++)
        {
            cycles.Add(new HUDCycle(this, i));
        }

        // Choose cycle at the end of the list to select
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
    }

    public override void Update()
    {
        pos.b = pos.a;
        fade.b = fade.a;

        if (currentCycle.state == HUDCycle.State.Sacrificed)
        {
            selectedCycleIndex--;
            currentCycle = cycles[selectedCycleIndex];
        }

        int first = 0;
        int last = 0;
        for (int j = 0; j < cycles.Count; j++)
        {
            cycles[j]?.Update();
            cycles[j]?.selected = cycles[j] == currentCycle ? true : false;

            if (cycles[j] == cycles.First())
            {
                first = j;
            }
            else if (cycles[j] == cycles.Last())
            {
                last = j;
            }
        }

        cursor?.Update();

        logger.LogDebug($"CycleMeter: CYCLES:{cycles.Count}[{first},{last}] - CURSORON:{selectedCycleIndex}|{currentCycle.state.value} - LIMBO:{IsInLimbo} - CACHED:{IsCached}");
    }

    public override void Draw(float timeStacker)
    {
        for (int i = 0; i < cycles.Count; i++)
        {
            cycles[i].Draw(timeStacker);
        }

        cursor.Draw(timeStacker);
    }
}