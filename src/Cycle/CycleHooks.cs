using static PitchBlack.Plugin;

namespace PitchBlack;
public static class CycleHooks
{
    public static void Apply()
    {
        On.AbstractCreature.ctor += AbstractCreature_Cycle_ctor;
        On.AbstractCreature.Update += AbstractCreature_Cycle_Update;

        // Control flow of calls
        On.Creature.Die += RealizedCreature_Cycle_Die;
        On.Player.Die += Player_Cycle_Die;
    }

    private static void Player_Cycle_Die(On.Player.orig_Die orig, Player self)
    {
        if (scugCWT.TryGetValue(self, out var s) && s is Beacon beacon)
        {
            if (beacon.cycle.SpiralDie())
            {
                orig(self);
            }
        }
        else
        {
            orig(self);
        }
    }

    private static void RealizedCreature_Cycle_Die(On.Creature.orig_Die orig, Creature self)
    {
        if (creatureCycle.TryGetValue(self.abstractCreature, out var cycle) && cycle is not BeaconCycle)
        {
            if (cycle.Die())
            {
                orig(self);
            }
        }
        else
        {
            orig(self);
        }
    }

    private static void AbstractCreature_Cycle_Update(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
    {
        orig(self, time);

        if (creatureCycle.TryGetValue(self, out var cycle))
        {
            cycle.AbstractUpdate();
        }
    }

    private static void AbstractCreature_Cycle_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
    {
        orig(self, world, creatureTemplate, realizedCreature, pos, ID);

        if (!creatureCycle.TryGetValue(self, out var _))
        {
            creatureCycle.Add(self, new CreatureCycle(self));
        }
    }
}
