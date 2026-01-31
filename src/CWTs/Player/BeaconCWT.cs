using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class BeaconCWT : ScugCWT
{
    public Color currentSkinColor;
    public Color currentEyeColor;

    public Squinter squinter;

    // Stops crafting
    public bool heldCraft = false;

    public FlareStorage storage;
    public int dontThrowTimer = 0;
    //flashbangs to recover after respawning in jollycoop
    public int coopRefundFlares = 0;

    public BeaconCycle2 beaconCycle;
    public bool CycleExists => beaconCycle is not null;

    public BeaconCWT(Player player) : base()
    {
        squinter = new(player);
        beaconCycle = new(player);
        creatureCycle.Add(player.abstractCreature, beaconCycle);
        // storage is added in BeaconUpdate.
    }
}