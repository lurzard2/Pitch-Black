using static PitchBlack.Plugin;

namespace PitchBlack;
public static class CycleHooks
{
    public static void Apply()
    {
        On.AbstractCreature.ctor += Add_Cycle_CWT;
        On.AbstractCreature.Update += AbstractCreature_Update;

        // Replace functionality conditionally for the same call
        On.Creature.Die += Creature_Die;
        On.Player.Die += Player_Die;
    }

    private static void Player_Die(On.Player.orig_Die orig, Player self)
    {
        if (scugCWT.TryGetValue(self, out var c) && c is Beacon beacon)
        {
            if (beacon.cycle.KillMe())
            {
                orig(self);
            }
        }
        else
        {
            orig(self);
        }
    }

    private static void Creature_Die(On.Creature.orig_Die orig, Creature self)
    {
        if (creatureCycle.TryGetValue(self.abstractCreature, out var cycle))
        {
            if (cycle.KillMe())
            {
                orig(self);
            }
        }
        else
        {
            orig(self);
        }
    }

    private static void AbstractCreature_Update(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
    {
        orig(self, time);

        if (creatureCycle.TryGetValue(self, out var cycle))
        {
            cycle.AbstractUpdate();
        }
    }

    private static void Add_Cycle_CWT(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
    {
        orig(self, world, creatureTemplate, realizedCreature, pos, ID);

        if (!creatureCycle.TryGetValue(self, out var _))
        {
            creatureCycle.Add(self, new CreatureCycle(self));
        }
    }
}
