using static PitchBlack.Plugin;

namespace PitchBlack;
public static class CreatureCycleHooks
{
    public static void Apply()
    {
        On.AbstractCreature.ctor += Add_Cycle_CWT;
        On.AbstractCreature.Update += Cycle_Update;
    }

    private static void Cycle_Update(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
    {
        if (creatureCycle.TryGetValue(self, out var cycle))
        {
            if (cycle != null)
            {
                cycle.AbstractUpdate();
            }
        }

        orig(self, time);
    }

    private static void Add_Cycle_CWT(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
    {
        orig(self, world, creatureTemplate, realizedCreature, pos, ID);

        // Skip adding slug cause we add it to beacon in BeaconCWT
        if (!creatureCycle.TryGetValue(self, out var _))
        {
            creatureCycle.Add(self, new Cycle(self));
        }
    }
}
