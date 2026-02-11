using UnityEngine;

namespace PitchBlack;

public class Beacon
{
    public readonly Player player;
    public SaveState SaveState => player.abstractCreature.world.game.GetSaveState();

    public BeaconGraphics graphics;

    // Values with arena fallbacks
    public float SpiralLevel { get; set; } = 0;
    public float AvailableCycles => SaveState.GetMaxSpiralLevel_CurrentOrArenaDefault();
    public float SubtractSpiralLevel()
    {
        if (SaveState is not null)
        {
            SaveState.SetSpiralLevel(SpiralLevel - 1);
            SpiralLevel = SaveState.GetSpiralLevel();
        }
        else
        {
            SpiralLevel--;
        }

        return SpiralLevel;
    }

    // Stops crafting
    public bool heldCraft = false;

    public FlareStorage storage { get; private set; } 
    public FlareStorage GetFlareStorage()
    {
        if (SaveState is not null && SaveState.GetCanStoreFlares())
        {
            // Assign in case it's not created, since it's assigned by progression during a cycle.
            storage ??= new(player);
            return storage;
        }
        return null;
    }
    public int dontThrowTimer = 0;
    //flashbangs to recover after respawning in jollycoop
    public int coopRefundFlares = 0;

    // Cycle module
    public BeaconCycle cycle { get; private set; }

    public BeaconInputs inputs { get; private set; }

    public Color currentSkinColor;
    public Color currentEyeColor;

    public Beacon(Player player)
    {
        this.player = player;
        graphics = new(this);
        inputs = new(this);

        // Set current level to max once, effectively refreshing the value each cycle. Check savestate properly!!
        SpiralLevel = SaveState.GetMaxSpiralLevel_CurrentOrArenaDefault();

        #region Replace Cycle with BeaconCycle
        // absCrit already has its key in creatureCycle before this is created, so we have to re-add it.
        Plugin.creatureCycle.Remove(player.abstractCreature);
        cycle = new(this, player);
        Plugin.creatureCycle.Add(player.abstractCreature, cycle);
        #endregion

        // for Playtest, for now
        if (SaveState is not null && SaveState.GetCompletedBeacon())
        {
            string ptText = $"[THIS MARKS THE END OF THE PLAYTEST CURRENTLY] ~ {Plugin.MOD_VERSION}";
            MiscUtils.AddHUDMessage(player.room.game.cameras[0].hud, true, ptText, 40 * 30, 120, true, true);
        }
    }

    public void Update()
    {
        if (dontThrowTimer > 0)
        {
            dontThrowTimer--;
        }
    }
}