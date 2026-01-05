using RWCustom;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable IDE0090

namespace PitchBlack;

public class BeaconCWT : ScugCWT
{
    // We need not access player
    private readonly Player player;
    public SaveState StorySaveState => player.room.world.game.GetStorySession.saveState;
    public Color currentSkinColor;
    public Color currentEyeColor;

    public Squinter squinter;

    // Stops crafting
    public bool heldCraft = false;

    public FlareStorage storage;
    public int dontThrowTimer = 0;
    //flashbangs to recover after respawning in jollycoop
    public int coopRefundFlares = 0;

    // Cycle module
    public BeaconCycle beaconCycle;

    public BeaconCWT(Player player) : base()
    {
        this.player = player;

        squinter = new(player);
        if (BeaconSaveData.GetOrSetBool(StorySaveState, BeaconSaveData.canStoreFlares))
        {
            storage = new(player);
        }
        var localCycle = new Cycle(player.abstractCreature);
        beaconCycle = new(localCycle, player);
    }
}