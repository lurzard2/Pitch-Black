using System.Linq;
using MoreSlugcats;
using RWCustom;
using Smoke;
using UnityEngine;

namespace PitchBlack.Objects.RotPuff;

//this is pretty much a copypaste from PuffBall code
//sadly inheriting from PuffBall would inherit its insect killing functionality
//added custom place in room: since POM doesn't call 
public class RotPuff : Weapon
{
	static readonly Color mainColor = new(0.4f, 0f, 0.6f);
	static readonly Color sporeDefiningColor = new(0.02f, 0.1f, 0.08f);
	static readonly Color dotsDefiningColor = new(0.7f, 0f, 0.5f);
	
	
	public RotPuff(AbstractConsumable abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
	{
		bodyChunks = [new BodyChunk(this, 0, new Vector2(0f, 0f), 7f, 0.11f)];
		bodyChunkConnections = [];
		airFriction = 0.98f;
		gravity = 0.86f;
		bounce = 0.2f;
		surfaceFriction = 0.3f;
		collisionLayer = 2;
		waterFriction = 0.98f;
		buoyancy = 1.8f;
		tailPos = firstChunk.pos;
		Random.State state = Random.state;
		Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		//tail length, from 3 to 15
		segments = new Vector2[(int)Mathf.Lerp(3f, 15f, Random.value), 3];
		exitThrownModeSpeed = 15f;
		//visual dots on the body
		dots = new Vector2[Random.Range(6, 11)];
		for (int dotNumber = 0; dotNumber < dots.Length; dotNumber++)
		{
			dots[dotNumber] = Custom.DegToVec((float)dotNumber / (float)dots.Length * 360f) * Random.value + Custom.RNV() * 0.2f;
		}
		Random.state = state;
		for (int j = 0; j < 3; j++)
		{
			for (int firstDotIndex = 0; firstDotIndex < dots.Length; firstDotIndex++)
			{
				for (int secondDotIndex = 0; secondDotIndex < dots.Length; secondDotIndex++)
				{
					if (Custom.DistLess(dots[firstDotIndex], dots[secondDotIndex], 1.4f))
					{
						Vector2 vector = Custom.DirVec(dots[firstDotIndex], dots[secondDotIndex]) * (Vector2.Distance(dots[firstDotIndex], dots[secondDotIndex]) - 1.4f);
						//always < 1
						float pseudorandomnumber = (float)firstDotIndex / ((float)firstDotIndex + (float)secondDotIndex);
						dots[firstDotIndex] += vector * pseudorandomnumber;
						dots[secondDotIndex] -= vector * (1f - pseudorandomnumber);
					}
				}
			}
		}
		float minimalDotXPos = 1f;
		float maximalDotXPos = -1f;
		float minimalDotYPos = 1f;
		float maximalDotYPos = -1f;
		foreach (Vector2 dot in dots)
		{
			minimalDotXPos = Mathf.Min(minimalDotXPos, dot.x);
			maximalDotXPos = Mathf.Max(maximalDotXPos, dot.x);
			minimalDotYPos = Mathf.Min(minimalDotYPos, dot.y);
			maximalDotYPos = Mathf.Max(maximalDotYPos, dot.y);
		}
		for (int dotNumber = 0; dotNumber < dots.Length; dotNumber++)
		{
			dots[dotNumber].x = -1f + 2f * Mathf.InverseLerp(minimalDotXPos, maximalDotXPos, dots[dotNumber].x);
			dots[dotNumber].y = -1f + 2f * Mathf.InverseLerp(minimalDotYPos, maximalDotYPos, dots[dotNumber].y);
		}
		float maximumDotDistance = dots.Aggregate(0f, (current, dot) => Mathf.Max(current, dot.magnitude));
		for (int dotNumber = 0; dotNumber < dots.Length; dotNumber++)
		{
			dots[dotNumber] /= maximumDotDistance;
		}
		
	}

	public Color sporeColor;
	
	//first coord - segment number
	//second coord:
	//0 - pos
	//1 - lastpos
	//2 - velocity
	Vector2[,] segments;

	SporesSmoke smoke;

	public float beingEaten;

	bool lastModeThrown;

	public float swallowed;

	public readonly Vector2[] dots;
	
	AbstractConsumable AbstractConsumable => (abstractPhysicalObject as AbstractConsumable);
	
	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		for (int i = 0; i < segments.GetLength(0); i++)
		{
			segments[i, 0] = firstChunk.pos + new Vector2(0f, 5f * (float)i);
			segments[i, 1] = segments[i, 0];
			segments[i, 2] *= 0f;
		}
	}

	public override void Update(bool eu)
	{
		if (beingEaten > 0f)
		{
			beingEaten += 0.1f;
			for (int segmentNum = 0; segmentNum < segments.GetLength(0); segmentNum++)
			{
				segments[segmentNum, 0] = Vector2.Lerp(segments[segmentNum, 0], base.firstChunk.pos, beingEaten);
			}
			if (beingEaten > 1f)
			{
				Destroy();
			}
		}
		if (lastModeThrown && (firstChunk.ContactPoint.x != 0 || firstChunk.ContactPoint.y != 0))
		{
			Explode();
		}
		lastModeThrown = mode == Mode.Thrown;
		if (firstChunk.ContactPoint.y != 0)
		{
			rotationSpeed = (rotationSpeed * 2f + base.firstChunk.vel.x * 5f) / 3f;
		}
		for (int segmentNum = 0; segmentNum < segments.GetLength(0); segmentNum++)
		{
			float segmentPositionProgress = (float)segmentNum / (float)(segments.GetLength(0) - 1);
			segments[segmentNum, 1] = segments[segmentNum, 0];
			segments[segmentNum, 0] += segments[segmentNum, 2];
			segments[segmentNum, 2] *= Mathf.Lerp(1f, 0.85f, segmentPositionProgress);
			segments[segmentNum, 2] += Vector2.Lerp(
				rotation * 5f, 
				new Vector2(
					Mathf.Clamp(
						base.firstChunk.pos.x - segments[segmentNum, 0].x,
						-2f, 
						2f)
					* 0.0025f,
					0.25f) 
				* (1f - segmentPositionProgress),
				Mathf.Pow(segmentPositionProgress, 0.01f));
			//vertical float
			segments[segmentNum, 2].y += 0.01f;
			ConnectSegment(segmentNum);
		}
		// for (int num2 = segments.GetLength(0) - 1; num2 >= 0; num2--)
		// {
		// 	ConnectSegment(num2);
		// }
		
		if (smoke != null)
		{
			if (room.ViewedByAnyCamera(base.firstChunk.pos, 300f))
			{
				smoke.EmitSmoke(
					segments[segments.GetLength(0) - 1, 0],
					Custom.DirVec(segments[segments.GetLength(0) - 2, 0], 
							segments[segments.GetLength(0) - 1, 0]) + Custom.RNV() + segments[segments.GetLength(0) - 1, 2], 
						sporeColor);
			}
			if (smoke.slatedForDeletetion || smoke.room != room)
			{
				smoke = null;
			}
		}
		else
		{
			smoke = new SporesSmoke(room);
			room.AddObject(smoke);
		}
		if (!AbstractConsumable.isConsumed && grabbedBy.Count > 0)
		{
			AbstractConsumable.Consume();
		}
		bool isBeingSwallowed = false;
		if (mode == Mode.Carried 
		    && grabbedBy.Count > 0 
		    && grabbedBy[0].grabber is Player grabbingPlayer 
		    && grabbingPlayer.swallowAndRegurgitateCounter > 50 
		    && grabbingPlayer.objectInStomach == null 
		    && grabbingPlayer.input[0].pckp)
		{
			int whichGrabToSwallow = -1;
			for (int graspNum = 0; graspNum < 2; graspNum++)
			{
				if (grabbingPlayer.grasps[graspNum] != null && grabbingPlayer.CanBeSwallowed(grabbingPlayer.grasps[graspNum].grabbed))
				{
					whichGrabToSwallow = graspNum;
					break;
				}
			}
			if (whichGrabToSwallow > -1 && grabbingPlayer.grasps[whichGrabToSwallow] != null && grabbingPlayer.grasps[whichGrabToSwallow].grabbed == this)
			{
				isBeingSwallowed = true;
			}
		}
		swallowed = Custom.LerpAndTick(swallowed, isBeingSwallowed ? 1f : 0f, 0.05f, 0.05f);
		base.Update(eu);
	}
	
	void ConnectSegment(int i)
	{
		if (i == 0)
		{
			Vector2 pos = base.firstChunk.pos;
			Vector2 vector = Custom.DirVec(segments[i, 0], pos);
			float num = Vector2.Distance(segments[i, 0], pos);
			segments[i, 0] -= vector * (5f - num);
			segments[i, 2] -= vector * (5f - num);
		}
		else
		{
			Vector2 vector2 = Custom.DirVec(segments[i, 0], segments[i - 1, 0]);
			float num2 = Vector2.Distance(segments[i, 0], segments[i - 1, 0]);
			float num3 = 0.52f;
			Vector2 vector3 = vector2 * ((5f - num2) * num3);
			segments[i, 0] -= vector3;
			segments[i, 2] -= vector3;
			Vector2 vector4 = vector2 * ((5f - num2) * (1f - num3));
			segments[i - 1, 0] += vector4;
			segments[i - 1, 2] += vector4;
		}
	}
	
		public override void PlaceInRoom(Room placeRoom)
	{
		while (abstractPhysicalObject.pos.y >= 0 && !placeRoom.GetTile(abstractPhysicalObject.pos.Tile + new IntVector2(0, -1)).Solid)
		{
			abstractPhysicalObject.pos.y--;
		}
		base.PlaceInRoom(placeRoom);
		rotation = Custom.DegToVec(Mathf.Lerp(-45f, 45f, abstractPhysicalObject.world.game.SeededRandom(abstractPhysicalObject.ID.RandomSeed)));
	}

	public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
	{
		if (result.chunk == null)
		{
			return false;
		}
		if (result.chunk.owner.abstractPhysicalObject.rippleLayer != abstractPhysicalObject.rippleLayer && !result.chunk.owner.abstractPhysicalObject.rippleBothSides && !abstractPhysicalObject.rippleBothSides)
		{
			return false;
		}
		result.chunk.vel += base.firstChunk.vel * 0.1f / result.chunk.mass;
		base.HitSomething(result, eu);
		Explode();
		return true;
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
		room?.AddObject(new SporeCloud(base.firstChunk.pos, Custom.RNV() * Random.value + throwDir.ToVector2() * 10f, sporeColor, 0.5f, null, -1, null, abstractPhysicalObject.rippleLayer));
		room?.PlaySound(SoundID.Slugcat_Throw_Puffball, base.firstChunk);
	}

	public override void PickedUp(Creature upPicker)
	{
		room.PlaySound(SoundID.Slugcat_Pick_Up_Puffball, base.firstChunk);
		if (ModManager.MMF && room.game.IsStorySession && room.game.cameras[0].hud != null && !room.game.rainWorld.progression.miscProgressionData.sporePuffTutorialShown && room.world.region != null && room.world.region.name == "LF" && MMF.cfgExtraTutorials.Value)
		{
			room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("The use of some objects may not be obvious at first glance, experiment with everything you find!"), 60, 250, darken: true, hideHud: true);
			room.game.rainWorld.progression.miscProgressionData.sporePuffTutorialShown = true;
		}
	}

	public override void HitWall()
	{
		Explode();
		SetRandomSpin();
		ChangeMode(Mode.Free);
		base.forbiddenToPlayer = 10;
	}

	public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
	{
		base.HitByExplosion(hitFac, explosion, hitChunk);
		Explode();
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		Explode();
	}

	public void Explode()
	{
		if (base.slatedForDeletetion)
		{
			return;
		}
		for (int j = 0; j < 70; j++)
		{
			room.AddObject(new SporeCloud(base.firstChunk.pos, Custom.RNV() * (Random.value * 10f), sporeColor, 1f, (thrownBy != null) ? thrownBy.abstractCreature : null, j % 20, null, abstractPhysicalObject.rippleLayer));
		}
		room.AddObject(new SporePuffVisionObscurer(base.firstChunk.pos, abstractPhysicalObject.rippleLayer));
		for (int k = 0; k < 7; k++)
		{
			room.AddObject(new PuffBallSkin(base.firstChunk.pos, Custom.RNV() * (Random.value * 16f), color, Color.Lerp(color, sporeColor, 0.5f)));
		}
		room.PlaySound(SoundID.Puffball_Eplode, base.firstChunk);
		Destroy();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[3 + dots.Length * 2];
		sLeaser.sprites[0] = new FSprite("BodyA");
		TriangleMesh triangleMesh = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: false);
		sLeaser.sprites[1] = triangleMesh;
		sLeaser.sprites[2] = new FSprite("BodyA");
		sLeaser.sprites[2].alpha = 0.5f;
		for (int i = 0; i < dots.Length; i++)
		{
			sLeaser.sprites[3 + i] = new FSprite("JetFishEyeB");
			sLeaser.sprites[3 + dots.Length + i] = new FSprite("pixel");
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		float degAng = Custom.VecToDeg(Vector3.Slerp(lastRotation, rotation, timeStacker));
		if (vibrate > 0)
		{
			vector += Custom.DegToVec(Random.value * 360f) * (2f * Random.value);
		}
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[2].x = vector.x - camPos.x - 2.5f;
		sLeaser.sprites[2].y = vector.y - camPos.y + 2.5f;
		sLeaser.sprites[0].rotation = degAng;
		sLeaser.sprites[2].rotation = degAng;
		float num = 1f;
		if (beingEaten > 0f || swallowed > 0f)
		{
			num = 1f - Mathf.Max(beingEaten, swallowed * 0.5f);
		}
		sLeaser.sprites[0].scaleY = 0.9f * num;
		sLeaser.sprites[0].scaleX = num;
		sLeaser.sprites[2].scaleY = 0.45f * num;
		sLeaser.sprites[2].scaleX = 0.5f * num;
		if (blink > 0)
		{
			bool flag = blink > 1 && Random.value < 0.5f;
			sLeaser.sprites[0].color = (flag ? base.blinkColor : color);
			sLeaser.sprites[1].color = (flag ? base.blinkColor : color);
		}
		else if (sLeaser.sprites[0].color != color)
		{
			sLeaser.sprites[0].color = color;
			sLeaser.sprites[1].color = color;
		}
		for (int i = 0; i < dots.Length; i++)
		{
			Vector2 vector2 = vector + Custom.RotateAroundOrigo(new Vector2(dots[i].x * 7f, dots[i].y * 8.5f) * num, degAng);
			sLeaser.sprites[3 + i].x = vector2.x - camPos.x;
			sLeaser.sprites[3 + i].y = vector2.y - camPos.y;
			sLeaser.sprites[3 + i].rotation = Custom.VecToDeg(Custom.RotateAroundOrigo(dots[i], degAng).normalized);
			sLeaser.sprites[3 + i].scaleX = num;
			sLeaser.sprites[3 + i].scaleY = Custom.LerpMap(dots[i].magnitude, 0f, 1f, 1f, 0.25f, 4f);
			sLeaser.sprites[3 + dots.Length + i].x = vector2.x - camPos.x;
			sLeaser.sprites[3 + dots.Length + i].y = vector2.y - camPos.y;
		}
		Vector2 vector3 = vector;
		for (int j = 0; j < segments.GetLength(0); j++)
		{
			Vector2 vector4 = Vector2.Lerp(segments[j, 1], segments[j, 0], timeStacker);
			Vector2 normalized = (vector4 - vector3).normalized;
			Vector2 vector5 = Custom.PerpendicularVector(normalized);
			float num2 = Vector2.Distance(vector4, vector3) / 5f;
			if (j == 0)
			{
				(sLeaser.sprites[1] as TriangleMesh).MoveVertice(j * 4, vector3 - vector5 * 0.5f - camPos);
				(sLeaser.sprites[1] as TriangleMesh).MoveVertice(j * 4 + 1, vector3 + vector5 * 0.5f - camPos);
			}
			else
			{
				(sLeaser.sprites[1] as TriangleMesh).MoveVertice(j * 4, vector3 - vector5 * 0.5f + normalized * num2 - camPos);
				(sLeaser.sprites[1] as TriangleMesh).MoveVertice(j * 4 + 1, vector3 + vector5 * 0.5f + normalized * num2 - camPos);
			}
			(sLeaser.sprites[1] as TriangleMesh).MoveVertice(j * 4 + 2, vector4 - vector5 * 0.5f - normalized * num2 - camPos);
			(sLeaser.sprites[1] as TriangleMesh).MoveVertice(j * 4 + 3, vector4 + vector5 * 0.5f - normalized * num2 - camPos);
			vector3 = vector4;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = Color.Lerp(mainColor, palette.texture.GetPixel(11, 4), 0.5f);
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[i].color = color;
		}
		sporeColor = Color.Lerp(color, sporeDefiningColor, 0.85f);
		Color dotsColor = Color.Lerp(Color.Lerp(dotsDefiningColor, palette.texture.GetPixel(11, 4), 0.2f), palette.blackColor, 0.5f);
		for (int j = 0; j < dots.Length; j++)
		{
			sLeaser.sprites[3 + j].color = dotsColor;
			sLeaser.sprites[3 + dots.Length + j].color = sporeColor;
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			if (i == 3 + dots.Length)
			{
				newContatiner.AddChild(sLeaser.sprites[2]);
			}
			else if (i != 2)
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}
	
}