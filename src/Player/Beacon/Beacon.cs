  using UnityEngine;

namespace PitchBlack;

public class Beacon
{
    public readonly Player player;
    public SaveState SaveState => player.abstractCreature.world.game.TryGetSaveState(out var data) ? data : null;

    // Revive count that counts down on usage
    public float SpiralLevel { get; set; }
    // Max amount of revives to spawn with
    public float MaxSpiralLevel => SaveState.GetMaxSpiralLevel();

    public BeaconGraphics graphics;
    public BeaconInputs inputs;
    public BeaconAbilityHandler abilityHandler;
    public BeaconCycle cycle;

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

    public Color currentSkinColor;
    public Color currentEyeColor;

    public Beacon(Player player)
    {
        this.player = player;
        abilityHandler = new(this);
        graphics = new(this);
        inputs = new(this);

        // Set current level to max once, effectively refreshing the value each cycle.
        SpiralLevel = MaxSpiralLevel;

        // for Playtest, for now
        if (SaveState is not null && SaveState.GetCompletedBeacon())
        {
            string ptText = $"[THIS MARKS THE END OF THE PLAYTEST CURRENTLY] ~ {Plugin.MOD_VERSION}";
            MiscUtils.AddHUDMessage(player.room.game.cameras[0].hud, true, ptText, 40 * 30, 120, true, true);
        }
    }

    public void Update()
    {
        abilityHandler.Update();
        // graphics + inputs are ran by hooks

        if (dontThrowTimer > 0)
        {
            dontThrowTimer--;
        }
    }
}