using System.Linq;
using UnityEngine;

namespace PitchBlack.Creatures.Citizen;

public class CitizenLooking(ArtificialIntelligence ai) : AIModule(ai)
{
	ScavengerAI ScavAI => (AI as ScavengerAI)!;
	Vector2 Pos => AI.creature.realizedCreature.firstChunk.pos;
	static Vector2 CreaturePos(AbstractCreature creature) => creature.realizedCreature.firstChunk.pos;

	readonly bool valid = ai is ScavengerAI;

	public override void Update()
	{
		base.Update();
		if (valid)
		{
			//get a list of players in same room
			if (AI.creature.world.game.Players.Where(player => player.Room == AI.creature.Room).ToList() is
			    {
				    Count: > 0
			    } list)
			{
				//find the closest
				AbstractCreature closestPlayer = list.Aggregate<AbstractCreature, AbstractCreature>(null, (closestPlayer, player) =>
				{
					if (closestPlayer == null) return player;
					return (CreaturePos(closestPlayer) - Pos).magnitude > (CreaturePos(player) - Pos).magnitude ? player : closestPlayer;
				});
				//focus it
				ScavAI.focusCreature = AI.tracker.RepresentationForCreature(closestPlayer, true);
				//optional: disable animation
				//ScavAI.scavenger.animation = null;
			}
		}
		
		
	}
}