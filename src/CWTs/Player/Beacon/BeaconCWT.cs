using UnityEngine;

namespace PitchBlack;

public class BeaconCWT : ScugCWT
{
    public readonly Player player;
    public SaveState SaveState => player.abstractCreature.world.game.GetSaveState();

    // Values with arena fallbacks
    public float SpiralLevel()
    {
        if (SaveState is not null)
        {
            return SaveState.GetSpiralLevel();
        }
        return 1;
    }
    public float MinSpiralLevel()
    {
        if (SaveState is not null)
        {
            return SaveState.GetMinSpiralLevel();
        }
        return 0;
    }
    public float MaxSpiralLevel()
    {
        if (SaveState is not null)
        {
            return SaveState.GetMaxSpiralLevel();
        }
        return 1;
    }
    public bool CanUseThanatosis()
    {
        if (SaveState is not null)
        {
            return SaveState.GetCanUseThanatosis();
        }
        return true;
    }

    public Squinter squinter {  get; private set; }

    // Stops crafting
    public bool heldCraft = false;

    public FlareStorage storage { get; private set; }
    public int dontThrowTimer = 0;
    //flashbangs to recover after respawning in jollycoop
    public int coopRefundFlares = 0;

    // Cycle module
    public BeaconCycle beaconCycle { get; private set; }

    public Color currentSkinColor;
    public Color currentEyeColor;

    public BeaconCWT(Player player) : base()
    {
        this.player = player;

        // Set current level to max once, effectively refreshing the value each cycle. Check savestate properly!!
        if (SpiralLevel() < MaxSpiralLevel())
        {
            SaveState?.SetSpiralLevel(MaxSpiralLevel());
        }

        // absCrit already has its key in creatureCycle before this is created, so we have to re-add it.
        Plugin.creatureCycle.Remove(player.abstractCreature);
        beaconCycle = new(player, this);
        Plugin.creatureCycle.Add(player.abstractCreature, beaconCycle);

        squinter = new(player);

        // for Playtest, for now
        if (SaveState is not null)
        {
            if (SaveState.GetCompletedBeacon())
            {
                string ptText = $"[THIS MARKS THE END OF THE PLAYTEST CURRENTLY] ~ {Plugin.MOD_VERSION}";
                MiscUtils.AddHUDMessage(player.room.game.cameras[0].hud, true, ptText, 40 * 30, 120, true, true);
            }
        }
    }

    public void Update()
    {
        squinter.Update();
        beaconCycle.Update();

        // We want to add flare mechanics retroactively based on savedata updating
        if (SaveState is not null)
        {
            if (BeaconSaveData.GetOrSetBool(SaveState, BeaconSaveData.canStoreFlares))
            {
                storage ??= new(player);
            }

            if (dontThrowTimer > 0)
            {
                dontThrowTimer--;
            }
        }
    }
}