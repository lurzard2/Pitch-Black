using static PitchBlack.Plugin;

namespace PitchBlack;
public static class CreatureCycleHooks
{
    public static void Apply()
    {
        On.AbstractCreature.ctor += Add_Cycle_CWT;
        On.AbstractCreature.Update += AbstractCreature_Update;

        // We'll let cwt handle realized update
        //On.Creature.Update += RealizedCreature_Update;
        On.Creature.ctor += RealizedCreature_ctor;


    }

    private static void RealizedCreature_ctor(On.Creature.orig_ctor orig, Creature self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (creatureCycle.TryGetValue(self.abstractCreature, out var cycle))
        {
            cycle.OnRealize();
        }
    }

    private static void AbstractCreature_Update(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
    {
        orig(self, time);

        if (creatureCycle.TryGetValue(self, out var cycle) && cycle != null)
        {
            cycle.AbstractUpdate();
        }
    }

    private static void Add_Cycle_CWT(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
    {
        orig(self, world, creatureTemplate, realizedCreature, pos, ID);

        if (!creatureCycle.TryGetValue(self, out var _))
        {
            creatureCycle.Add(self, new Cycle(self));
        }
    }
}
