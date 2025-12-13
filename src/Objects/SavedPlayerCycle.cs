using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace PitchBlack;

// WIP and unused for now
public class SavedPlayerCycle
{
    public BeaconCycle savedCycle;
    public int cycleNumber;
    public CycleEndReason cycleEndReason;
    public int sacrifices;
    public List<SavedSacrifice> savedSacrifices = [];

    public SavedPlayerCycle(BeaconCycle playerCycle, int cycleNumber)
    {
        savedCycle = playerCycle;
        this.cycleNumber = cycleNumber;

        if (savedCycle != null)
        {
            if (savedCycle.cycle.state == Cycle.State.Cached)
            {
                cycleEndReason = CycleEndReason.GameOver;
            }
            if (savedCycle.cycle.state == Cycle.State.Cached && savedCycle.cycle.cycleStateTime > savedCycle.ThanatosisLimit)
            {
                cycleEndReason = CycleEndReason.ThanatosisGameOver;
            }
            if (savedCycle.cycle.state == Cycle.State.Alive)
            {
                cycleEndReason = CycleEndReason.Win;
            }
            if (BeaconSaveData.GetCompletedBeacon(savedCycle.saveState))
            {
                cycleEndReason = CycleEndReason.Completion;
            }

            // Player persistent deaths
            sacrifices = (int)savedCycle.MaxSpiralLevel - (int)savedCycle.SpiralLevel;
            if (sacrifices > 0)
            {
                for (int i = 0; i < sacrifices; i++)
                {
                    savedSacrifices.Add(new SavedSacrifice(this));
                }
            }
        }
    }

    public class CycleEndReason : ExtEnum<CycleEndReason>
    {
        public CycleEndReason(string value, bool register) : base(value, register) { }

        public static readonly CycleEndReason GameOver = new(nameof(GameOver), true);
        public static readonly CycleEndReason ThanatosisGameOver = new(nameof(ThanatosisGameOver), true);
        public static readonly CycleEndReason Win = new(nameof(Win), true);
        public static readonly CycleEndReason Completion = new(nameof(Completion), true);
    }

    public class SavedSacrifice
    {
        public SavedPlayerCycle saveCycle;

        public SavedSacrifice(SavedPlayerCycle saveCycle)
        {
            this.saveCycle = saveCycle;
        }
    }
}