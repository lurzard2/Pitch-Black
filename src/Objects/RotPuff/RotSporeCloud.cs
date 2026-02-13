using RWCustom;
using UnityEngine;

namespace PitchBlack.Objects.RotPuff;

public class RotSporeCloud : SporeCloud
{
	public RotSporeCloud(Vector2 pos, Vector2 vel, Color color, float size, AbstractCreature killTag, int checkInsectsDelay, InsectCoordinator smallInsects) : base(pos, vel, color, size, killTag, checkInsectsDelay, smallInsects)
	{
		
	}
	public RotSporeCloud(Vector2 pos, Vector2 vel, Color color, float size, AbstractCreature killTag, int checkInsectsDelay, InsectCoordinator smallInsects, int rippleLayer) : base(pos, vel, color, size, killTag, checkInsectsDelay, smallInsects, rippleLayer)
	{
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		//this thing already has a timer, so we can reuse it
		//matching against 1 because at 1 in orig it becomes 20 immediately
		if (checkInsectsDelay == 1)
		{
			room.abstractRoom.creatures.ForEach(absCreature =>
			{
				if(absCreature is null || absCreature.rippleLayer != rippleLayer && !absCreature.rippleBothSides || !absCreature.IsRotCreature()) return;
				
				if (Custom.DistLess(pos, absCreature.realizedCreature.mainBodyChunk.pos, rad + absCreature.realizedCreature.mainBodyChunk.rad + 20f))
				{
					absCreature.realizedCreature.Stun(Random.Range(10, 120));
				}
				
			});
		}
	}
}