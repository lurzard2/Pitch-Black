using IL.Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace PitchBlack;

public class Squinter
{
    public Player player;
    public PlayerGraphics playerGraphics;
    public Room Room => player.room;

    public int squintTick = 0;
    public bool Squinting => squintTick > 0;
    public bool BlindBeaconHere()
    {
        bool placeIsBright = Room.Darkness(player.mainBodyChunk.pos) < 0.15f;

        if (Room.world.singleRoomWorld)
        {
            if (placeIsBright)
            {
                return true;
            }
            return false;
        }

        var region = Room.world.region.name.ToLowerInvariant();
        var room = Room.abstractRoom.name.ToLowerInvariant();
        bool vhosOverride = room == "vv_e01"
            || room == "vv_b06"
            || room == "vv_b08"
            || room == "vv_c02"
            || room == "vv_c01";
        bool presentGhostModeOverride = Room.game.cameras[0].ghostMode > 0.40f;

        // Order of priority matters here
        if (vhosOverride || presentGhostModeOverride)
        {
            return false;
        }
        else if (MiscUtils.IsVhosRegion(region) || placeIsBright)
        {
            return true;
        }
        return false;
    }

    public Squinter(Player player, PlayerGraphics playerGraphics)
    {
        this.player = player;
        this.playerGraphics = playerGraphics;
    }

    public void Update()
    {
        if (BlindBeaconHere())
        {
            // Tick down, but not all the way
            if (squintTick > 1)
            {
                squintTick--;
            }
            else if (squintTick == 1)
            {
                player.Blink(5);
            }
            else
            {
                squintTick = 40 * UnityEngine.Random.Range(5, 7);
                player.Blink(8);
            }
        }
        // Otherwise, room is dark enough and should stop squinting
        else if (Squinting)
        {
            squintTick--;
        }
    }

    public void DrawSprites(PlayerGraphics pGraphics, RoomCamera.SpriteLeaser sLeaser)
    {
        // Squinting eyes
        if (squintTick > (40 * 3.5f))
        {
            sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("FaceStunned");
        }
        // Look down
        if (squintTick > 10)
        {
            sLeaser.sprites[9].x -= pGraphics.lookDirection.x * 2;
            sLeaser.sprites[9].y -= pGraphics.lookDirection.y * 2;

            if (playerGraphics.blink <= 0 && Random.value < 0.35f)
            {
                playerGraphics.player.Blink(Mathf.FloorToInt(Mathf.Lerp(3f, 8f, Random.value)));
            }
            playerGraphics.head.vel -= playerGraphics.lookDirection * 3f;
        }
    }
}
