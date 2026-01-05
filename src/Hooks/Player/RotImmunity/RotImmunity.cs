using IL.Menu;
using System.Runtime.CompilerServices;
using static PitchBlack.Plugin;
using RWCustom;

namespace PitchBlack;

public class RotImmunity
{
    private static bool DisableRotCollisions(Player player)
    {
        if (scugCWT.TryGetValue(player, out var c)
            && c is BeaconCWT cwt
            && cwt.beaconCycle != null
            && cwt.beaconCycle.thanatosisLerp > 0.05f)
        {
            return true;
        }
        return false;
    }

    public static void Apply()
    {
        On.DaddyCorruption.BulbNibbleAtChunk += DaddyCorruption_BulbNibbleAtChunk_IMMUNITY;
        On.DaddyTentacle.Touch += DaddyTentacle_Touch_IMMUNITY;
    }

    private static void DaddyTentacle_Touch_IMMUNITY(On.DaddyTentacle.orig_Touch orig, DaddyTentacle self)
    {
        // We gotta access the player in a convoluted way :heart:
        for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++)
        {
            if (self.room.abstractRoom.creatures[i].realizedCreature != null
                && !self.room.abstractRoom.creatures[i].realizedCreature.inShortcut
                && self.room.abstractRoom.creatures[i].realizedCreature != self.daddy)
            {
                Creature realizedCreature = self.room.abstractRoom.creatures[i].realizedCreature;
                for (int j = 0; j < self.tChunks.Length; j++)
                {
                    for (int k = 0; k < realizedCreature.bodyChunks.Length; k++)
                    {
                        if (Custom.DistLess(self.tChunks[j].pos, realizedCreature.bodyChunks[k].pos, self.tChunks[j].rad + realizedCreature.bodyChunks[k].rad))
                        {
                            if (realizedCreature is Player player && DisableRotCollisions(player))
                            {
                                // DONT LET IT EAT ME NOO!!!!
                                self.SwitchTask(DaddyTentacle.Task.Locomotion);
                                return;
                            }
                        }
                    }
                }
            }
        }

        orig(self);
    }

    private static void DaddyCorruption_BulbNibbleAtChunk_IMMUNITY(On.DaddyCorruption.orig_BulbNibbleAtChunk orig, DaddyCorruption self, DaddyCorruption.Bulb bulb, BodyChunk chunk)
    {
        if (chunk.owner != null && chunk.owner is Player player && DisableRotCollisions(player))
        {
            bool iWantToEat = false;
            int i = 0;
            // Find creatures to nibble
            while (i < self.eatCreatures.Count && !iWantToEat)
            {
                if (self.eatCreatures[i].creature == chunk.owner)
                {
                    self.eatCreatures[i].BulbInteraction(bulb.eyeStalkPos, bulb.rad);
                    iWantToEat = true;
                }
                i++;
            }
            // Add creatures to nibble
            if (!iWantToEat && chunk.owner is Creature crit
                && !chunk.owner.slatedForDeletetion
                && chunk.owner.room == self.room)
            {
                // Do not NIBBLE. DO NOT. I COMMAND YOU.
                if (chunk.owner != null && chunk.owner is Player beacon)
                {
                    return;
                }
                self.eatCreatures.Add(new DaddyCorruption.EatenCreature(chunk.owner as Creature, bulb.eyeStalkPos, bulb.rad));
            }
        }
        else
        {
            orig(self, bulb, chunk);
        }
    }
}
