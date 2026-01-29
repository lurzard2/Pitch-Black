using UnityEngine;
using System.Collections.Generic;
using RWCustom;
using System;

namespace PitchBlack;

public class BeaconManipulator : ManipulationModule, IManipulator
{
    public Player Beacon { get; set; }
    public float SpiralLevel { get; set; }
    public float MinSpiralLevel { get; set; }
    public float MaxSpiralLevel { get; set; }

    private Counter specInputCounter = new(Int32.MaxValue, 0, true);
    private bool isDead;

    public BeaconManipulator(Cycle cycle, Player player) : base(cycle)
    {
        Beacon = player;
        // Assign to have an arena fallback
        if (SaveState != null)
        {
            MinSpiralLevel = SaveState.GetMinSpiralLevel();
            SpiralLevel = SaveState.GetSpiralLevel();
            MaxSpiralLevel = SaveState.GetMaxSpiralLevel();
        }
        else
        {
            MinSpiralLevel = 0;
            SpiralLevel = 1;
            MaxSpiralLevel = 1;
        }
    }

    public override void Realized()
    {
        base.Realized();

        if (Beacon.input[0].spec)
        {
            specInputCounter.Tick();
        }
        else
        {
            specInputCounter.Reset();
        }

        if (cycle.spacialTracker.InDream)
        {
            #region Dream Interaction
            if (!MiscUtils.IsNightmareRegion(cycle.abstractOwner.world.name) && cycle.state == Cycle.State.Thanatosis)
            {
                ToggleThanatosis();
            }

            // Indicator for being unable to use Thanatosis if unlocked
            if (MaxSpiralLevel >= 1 && specInputCounter == UnityEngine.Random.Range(60, 140))
            {
                Beacon.Stun(120);
                specInputCounter.Reset();
                string popupText = "";
                if (MiscUtils.IsNightmareRegion(Beacon.room.world.name))
                    popupText = "These tides are sinister";
                else if (MiscUtils.IsPBSB(Beacon.room.world.name))
                    popupText = "These tides rest still";
                else
                    popupText = "These tides flow without disturbance";
                MiscUtils.AddHUDMessage(Beacon.room.game.cameras[0].hud, true, popupText, 60, 120, false, true);
            }
            #endregion
            return;
        }

        Act();
    }

    public void Act()
    {

    }

    public void ManipulateTarget(Cycle target)
    {

    }

    public void ManipulateGraphics(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
    }

    public void ToggleThanatosis()
    {
        isDead = !isDead;
        var cycleState = isDead
            ? Cycle.State.Thanatosis
            : Cycle.State.ExitThanatosis;
        var soundEffect = isDead
            ? Enums.SoundID.Player_Activated_Thanatosis
            : Enums.SoundID.Player_Deactivated_Thanatosis;

        cycle.ChangeState(cycleState);
        Beacon.room.PlaySound(soundEffect, Beacon.mainBodyChunk);

        // Update collisions/interactions
        cycle.abstractOwner.rippleLayer = isDead ? 1 : 0;
        cycle.abstractOwner.tentacleImmune = isDead;
    }

    // We may not need this until Night Terror is in
    //public override void BeingManipulated(Cycle reference) { }
}