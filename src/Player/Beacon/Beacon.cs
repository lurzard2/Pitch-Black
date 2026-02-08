using UnityEngine;

namespace PitchBlack;

public class Beacon : ScugCWT
{
    public readonly Player player;
    public SaveState SaveState => player.abstractCreature.world.game.GetSaveState();

    public Squinter squinter { get; private set; }

    // Values with arena fallbacks
    public float SpiralLevel { get; set; } = 0;

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

    public Color currentSkinColor;
    public Color currentEyeColor;

    public Beacon(Player player) : base()
    {
        this.player = player;
        squinter = new(player);

        // Set current level to max once, effectively refreshing the value each cycle. Check savestate properly!!
        SpiralLevel = SaveState.GetMaxSpiralLevel_CurrentOrArenaDefault();

        // absCrit already has its key in creatureCycle before this is created, so we have to re-add it.
        Plugin.creatureCycle.Remove(player.abstractCreature);
        cycle = new(this, player);
        Plugin.creatureCycle.Add(player.abstractCreature, cycle);

        // for Playtest, for now
        if (SaveState is not null && SaveState.GetCompletedBeacon())
        {
            string ptText = $"[THIS MARKS THE END OF THE PLAYTEST CURRENTLY] ~ {Plugin.MOD_VERSION}";
            MiscUtils.AddHUDMessage(player.room.game.cameras[0].hud, true, ptText, 40 * 30, 120, true, true);
        }
    }

    public void Update()
    {
        squinter.Update();
        cycle.Update();

        if (dontThrowTimer > 0)
        {
            dontThrowTimer--;
        }
    }
}