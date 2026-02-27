using System;
using System.Collections.Generic;

namespace PitchBlack;

public partial class BeaconAbilityHandler
{
    public class CycleController
    {
        public class CycleData
        {
            public CycleData(int index)
            {
                this.index = index;
                status = Status.Nominal;
                Sacrificed = false;
                NotAccountedFor = true;
                baseCycle = false;
            }

            public readonly int index;
            public bool NotAccountedFor { get; set; }
            public enum Status
            {
                Nominal,
                Spiraling,
                Cached,
            }
            public Status status;
            public bool Sacrificed { get; set; }
            public bool baseCycle;
        }
        public List<CycleData> cycles = [];
        public static int maxPossibleCycles = 5;
        public void UpdateCycleUsability(int index)
        {
            cycles[index].NotAccountedFor = false;
        }

        public int CurrentCycleIndex { get; set; }

        public CycleController(int maxLvl)
        {
            for (int i = 0; i < maxPossibleCycles; i++)
            {
                cycles.Add(new(i));
            }
            for (int i = 0; i < maxLvl; i++)
            {
                UpdateCycleUsability(i);
            }

            cycles[0].baseCycle = true;
        }
    }
}
