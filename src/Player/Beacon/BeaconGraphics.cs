using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

namespace PitchBlack;

public  class BeaconGraphics
{
    // addtocontainer safety
    public bool init;
    public Beacon beacon;
    public Player playerRef;
    public PlayerGraphics playerGraphics;
    public Whiskers whiskers;
    public Squinter squinter;

    private bool usesHat => ModOptions.UsesHatSprite;
    public int HatSprite {  get; set; }

    public BeaconGraphics(Beacon beacon, Player playerRef, PlayerGraphics playerGraphics)
    {
        this.beacon = beacon;
        this.playerRef = playerRef;
        this.playerGraphics = playerGraphics;
    }

    public void Update()
    {
        whiskers.Update();
        squinter.Update();
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (usesHat)
        { 
            HatSprite = sLeaser.sprites.Length;
            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);
            sLeaser.sprites[HatSprite] = new FSprite("PBHat");
        }

        whiskers.initialWhiskerIndex = sLeaser.sprites.Length;
        whiskers.endWhiskerIndex = whiskers.initialWhiskerIndex + whiskers.headScales.Length;
        whiskers.initialLowerWhiskerIndex = whiskers.initialWhiskerIndex + whiskers.headScales.Length / 2;
        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + whiskers.headScales.Length);
        whiskers.InitiateSprites(sLeaser);

        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            Plugin.logger.LogDebug($"{nameof(BeaconGraphics)}: [{i},{sLeaser.sprites[i].element.name}]");
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (usesHat)
        {
            rCam.ReturnFContainer("Foreground").RemoveChild(sLeaser.sprites[HatSprite]);
            rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[HatSprite]);
            sLeaser.sprites[HatSprite].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
        }

        whiskers.AddToContainer(sLeaser, rCam);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Color customColor = sLeaser.sprites[0].color;

        if (usesHat)
        {
            Vector2 vector = Vector2.Lerp(playerGraphics.drawPositions[0, 1], playerGraphics.drawPositions[0, 0], timeStacker);
            Vector2 vector2 = Vector2.Lerp(playerGraphics.drawPositions[1, 1], playerGraphics.drawPositions[1, 0], timeStacker);
            Vector2 position = sLeaser.sprites[9].GetPosition() + 9f * Vector2.up - 4f * playerGraphics.lookDirection.x * Vector2.right;
            position += 4f * Mathf.Clamp(Mathf.Abs(playerGraphics.player.mainBodyChunk.vel.x), 0, playerGraphics.player.standing ? 1 : 0) * playerGraphics.player.flipDirection * Custom.PerpendicularVector(Custom.DirVec(vector, vector2)) * Mathf.Lerp(1, 0, Mathf.Abs(playerGraphics.player.mainBodyChunk.lastPos.y - playerGraphics.player.mainBodyChunk.pos.y) * 2f);
            sLeaser.sprites[HatSprite].SetPosition(position);
            sLeaser.sprites[HatSprite].scaleX = 1.1f;
            sLeaser.sprites[HatSprite].scaleY = 0.8f;
            sLeaser.sprites[HatSprite].rotation = sLeaser.sprites[9].rotation + 0.15f * sLeaser.sprites[3].rotation + Mathf.Abs(playerGraphics.player.mainBodyChunk.vel.x);
            sLeaser.sprites[HatSprite].color = new Color(customColor.r * 0.75f, customColor.g * 0.75f, customColor.b * 0.75f, customColor.a);
        }

        whiskers.DrawSprites(sLeaser, timeStacker, camPos);
        squinter.DrawSprites(playerGraphics, sLeaser);
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        whiskers.ApplyPalette(playerGraphics, sLeaser);

        Colors.PlayerPaletteBlack = palette.blackColor;
    }
}
