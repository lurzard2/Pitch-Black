using static PitchBlack.Plugin;

namespace PitchBlack;
public static class CreatureCycleHooks
{
    public static void Apply()
    {
        On.AbstractCreature.ctor += Add_Cycle_CWT;
        On.AbstractCreature.Update += AbstractCreature_Update;
        On.AbstractCreature.Die += AbstractCreature_Die;

        // We'll let cwt handle realized update
        //On.Creature.Update += RealizedCreature_Update;
        On.Creature.ctor += RealizedCreature_ctor;
        On.Creature.Die += RealizedCreature_Die;
        On.Player.Die += Player_Die;
    }

    private static void Player_Die(On.Player.orig_Die orig, Player self)
    {
        if (creatureCycle.TryGetValue(self.abstractCreature, out var cycle))
        {
            cycle.deathHandler.PlayerDie();
        }
        else
        {
            orig(self);
        }
    }

    private static void AbstractCreature_Die(On.AbstractCreature.orig_Die orig, AbstractCreature self)
    {
        if (creatureCycle.TryGetValue(self, out var cycle))
        {
            cycle.deathHandler.AbstractDie();
        }
        else
        {
            orig(self);
        }
    }

    private static void RealizedCreature_Die(On.Creature.orig_Die orig, Creature self)
    {
        if (creatureCycle.TryGetValue(self.abstractCreature, out var cycle))
        {
            cycle.deathHandler.RealizedDie();
        }
        else
        {
            orig(self);
        }
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
            cycle.Update();
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
