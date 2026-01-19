using RWCustom;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PitchBlack.Creatures.Citizen;

public class CitizenMask : ScavengerCosmetic.Template
{
	public CitizenMask(ScavengerGraphics owner, int firstSprite, Color? color = null) : base(owner, firstSprite)
	{
		totalSprites = 3;
		//mainColor = color ?? Random.Range(0, 2) switch
		//{
		//	0 => Color.red,
		//	1 => Color.blue,
		//	_ => Color.green,
		//};
	}

	#region Config variables
		//normally vulture mask has like KrakenMask0 or KrakenMask8
		//because the whole semicircle of directions head can look at is divided into 9 parts
		//we can increase that number to make more smoother sprite swapping, which is important for large sprites 
		const int maximumSpriteNameNumber = 8;
		const string spriteName = "KrakenMask";
	#endregion
	#region runtime variables
		//rotationA is responsible for direct rotation of sprites
		Vector2 rotationA;
		Vector2 lastRotationA;
		//would be set by apply palette. needed for shadows
		Color blackColor = new(0, 0, 0);
		//main color of mask
		readonly Color mainColor;
	#endregion

	
	
	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		blackColor = palette.blackColor;
	}

	public override void Update()
	{
		base.Update();
		//base game calculations
		float2 headDirection = scavGrphs.HeadDir(0f);
		float bodyAxisDegrees = scavGrphs.BodyAxis(0f);
		//something about head direction and body axis
		float lastLookUp = scavGrphs.lastLookUp;
		//something about emotion ig
		float lastNeutralFace = scavGrphs.lastNeutralFace; 
		float2 bodyMaskDirection = math.lerp(
			headDirection, 
			-Custom.DegToFloat2(-bodyAxisDegrees),
			Mathf.Lerp(0.5f, 
				1f, 
				Mathf.Max(Mathf.Pow(lastLookUp, 1.1f), lastNeutralFace))).normalized();
		lastRotationA = rotationA;
		rotationA = Vector3Ext.Slerp2(rotationA, -bodyMaskDirection, 0.5f);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		//sprite elements are assigned at DrawSprites, so we can have a placeholder here
		sLeaser.sprites[firstSprite] = sLeaser.sprites[firstSprite + 1] = sLeaser.sprites[firstSprite + 2] = new FSprite("pixel");
		//container will be reassigned later when scav calls it, mask would share container with scav
		AddToContainer(sLeaser, rCam, null);
		//since scaleY is more or less constant we can assign it once
		sLeaser.sprites[firstSprite + 1].scaleY = 0.9f;
		sLeaser.sprites[firstSprite + 2].scaleY = 1.1f;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		//add sprites to container
		newContatiner ??= rCam.ReturnFContainer("Items");
		newContatiner.AddChild(sLeaser.sprites[firstSprite + 2]);
		newContatiner.AddChild(sLeaser.sprites[firstSprite + 1]);
		newContatiner.AddChild(sLeaser.sprites[firstSprite]);
	}
	
	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		//direct rotation and positioning
		Vector2 rotation = Vector2.Lerp(lastRotationA, rotationA, timeStacker);
		Vector2 pos = math.lerp(scavGrphs.drawPositions[scavGrphs.headDrawPos, 1],
			scavGrphs.drawPositions[scavGrphs.headDrawPos, 0], timeStacker);
		pos.y += 4f;
		
		//sideways rotation. map direction -> degrees -> [0;~~8~~ however many sprites is made for drawing] int
		float headDirectionDegrees = Custom.VecToDeg(-scavGrphs.HeadDir(timeStacker));
		//the number of sprite used. vulture mask has [0;8] but config above allows to increase this amount
		byte spriteUsed = (byte)Custom.IntClamp(
			Mathf.RoundToInt(
				Mathf.Abs(headDirectionDegrees / 180f)*maximumSpriteNameNumber),
			0,
			maximumSpriteNameNumber);
		//how dark mask shadow should be
		float darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos)) * 0.8f;

		
		for (byte spriteIndex = (byte)firstSprite; spriteIndex < firstSprite + totalSprites; spriteIndex++)
		{
			FSprite sprite = sLeaser.sprites[spriteIndex];
			sprite.element = Futile.atlasManager.GetElementWithName(spriteName + spriteUsed);
			sprite.anchorY = Custom.LerpMap(90f, 0f, 100f, 0.5f, 0.675f, 2.1f);
			sprite.rotation = Custom.VecToDeg(rotation);
			sprite.scaleX = Mathf.Sign(headDirectionDegrees);
			sprite.x = pos.x-camPos.x;
			sprite.y = pos.y-camPos.y;
			// Same color as head
			sprite.color = sLeaser.sprites[scavGrphs.HeadSprite].color;
		}

		sLeaser.sprites[firstSprite + 1].scaleX *= 0.85f;
		sLeaser.sprites[firstSprite + 2].anchorY += 0.015f;
	}
}