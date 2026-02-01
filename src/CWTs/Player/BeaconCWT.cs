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

    public Squinter Squinter {  get; set; }

    // Stops crafting
    public bool heldCraft = false;

    public FlareStorage Storage { get; set; }
    public int dontThrowTimer = 0;
    //flashbangs to recover after respawning in jollycoop
    public int coopRefundFlares = 0;

    public BeaconCycle2 BeaconCycle {  get; private set; }

    public BeaconCWT(Player player) : base()
    {
        Squinter = new(player);
        BeaconCycle = new(player);
        // Assign cycle, so absCrit doesn't override it
        creatureCycle.Add(player.abstractCreature, BeaconCycle);
        // storage is added in BeaconUpdate.
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {

    }
}