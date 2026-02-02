using RWCustom;
using System.Collections.Generic;
using UnityEngine;

namespace PitchBlack;

public class BeaconCWT : ScugCWT
{
    public readonly Player Beacon;
    public SaveState SaveState => Beacon.abstractCreature.world.game.GetSaveState();
     
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
        Beacon = player;
        squinter = new(player);
        // storage is added in BeaconUpdate
        beaconCycle = new(player);
        Plugin.creatureCycle.Remove(player.abstractCreature);
        Plugin.creatureCycle.Add(player.abstractCreature, beaconCycle);
    }

    public void Update()
    {
        squinter?.Update();

        if (SaveState is not null)
        {
            beaconCycle?.Update();

            if (BeaconSaveData.GetOrSetBool(SaveState, BeaconSaveData.canStoreFlares))
            {
                storage ??= new(Beacon);
            }

            if (dontThrowTimer > 0)
            {
                dontThrowTimer--;
            }
        }
    }
}