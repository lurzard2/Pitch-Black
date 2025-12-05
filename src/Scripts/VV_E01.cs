using RWCustom;
using System.Collections.Generic;
using UnityEngine;

namespace PitchBlack;

public class VV_E01 : PBRoomSpecificScript
{
    public static readonly Phase PlayerFalling = new(nameof(PlayerFalling), true);
    public static readonly Phase PlayerSubmerged = new(nameof(PlayerSubmerged), true);
    public static readonly Phase PlayerSurfaced = new(nameof(PlayerSurfaced), true);

    public VV_E01(Room room, int playerIndex) : base(room)
    {
        this.room = room;
        this.playerIndex = playerIndex;
        phase = Phase.Init;
        hasJumped = false;
        isNoLongerSubmerged = false;
        submersionLimit = Random.Range(140, 200);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        // Assign player
        if (player == null && AllPlayersRealized)
        {
            AbstractCreature aCrit = room.game.Players[playerIndex];
            if (aCrit != null && aCrit.realizedCreature != null)
            {
                player = aCrit.realizedCreature as Player;
            }
        }
        if (player != null && player.room == room)
        {
            timeInPhase.Tick();
            if (phase == Phase.Init)
            {
                // We randomize this to be different for everyone every time
                float xPos = Random.Range(100, 900);
                float yPos = Random.Range(6500, 7500);
                player.SuperHardSetPosition(new Vector2(xPos, yPos));

                controller = new InputController(this);
                player.controller = controller;
                player.bodyMode = Player.BodyModeIndex.Dead;
                player.animation = Player.AnimationIndex.Dead;
                
                if (RoomIsBeingLoaded)
                {
                    return;
                }

                ChangePhase(PlayerFalling);
                return;
            }
            if (phase == PlayerFalling)
            {
                if (player.Submersion > 0.5f && player.airInLungs < 0.8f)
                {
                    ChangePhase(PlayerSubmerged);
                    return;
                }
            }
            if (phase == PlayerSubmerged)
            {
                if (player.airInLungs > 0.9f)
                {
                    ChangePhase(PlayerSurfaced);
                    return;
                }
            }
            if (phase == PlayerSurfaced)
            {
                player.airInLungs += 0.02f;
                if (timeInPhase > 30)
                {
                    ChangePhase(Phase.End);
                    return;
                }
            }
            if (phase == Phase.End)
            {
                player.controller = null;
                Destroy();
            }
        }
    }

    public Player.InputPackage GetInput()
    {
        // -1 left, 1 right
        int x = 0;
        // -1 down, 1 up
        int y = 0;
        bool jmp = false;
        if (phase == PlayerFalling)
        {
            y = -1;
        }
        if (phase == PlayerSubmerged)
        {
            int flipInput = Random.Range(-2, 2);
            x = flipInput;

            if (timeInPhase >= submersionLimit)
            {
                y = 1;
            }
            else if (timeInPhase > 40 && timeInPhase < submersionLimit)
            {
                int flipInput2 = Random.Range(-2, 2);
                y = flipInput2;
            }
        }
        if (phase == PlayerSurfaced)
        {
            if (!hasJumped)
            {
                jmp = true;
                hasJumped = true;
            }
        }
        return new Player.InputPackage(false, Options.ControlSetup.Preset.None, x, y, jmp, false, false, false, false, false);
    }

    public class InputController : Player.PlayerController
    {
        public InputController(VV_E01 owner)
        {
            this.owner = owner;
        }

        public override Player.InputPackage GetInput()
        {
            return owner.GetInput();
        }

        private VV_E01 owner;
    }

    private Player player;
    private InputController controller;

    private int playerIndex;
    private bool hasJumped;
    private int submersionLimit;
    private bool isNoLongerSubmerged;
}
