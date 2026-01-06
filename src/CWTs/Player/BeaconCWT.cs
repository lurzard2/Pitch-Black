using RWCustom;
using System.Collections.Generic;
using UnityEngine;

namespace PitchBlack;

public class BeaconCWT : ScugCWT
{
    // We need not access player much, but it's here
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
        // storage is added in BeaconUpdate
        var localCycle = new Cycle(player.abstractCreature);
        beaconCycle = new(localCycle, player);
    }
}