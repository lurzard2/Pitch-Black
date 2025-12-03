using RWCustom;
using UnityEngine;

namespace PitchBlack;

public class VV_E01_IntroScript : PBRoomSpecificScript
{
    public VV_E01_IntroScript(Room room) : base(room)
    {
        this.room = room;
        timeCounter = new Counter(0, 1080, true);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (LoadingRoom)
        {
            float xPos = Random.Range(100, 900);
            foreach (var absrCrit in room.game.session.Players)
            {
                if (absrCrit == null || absrCrit.realizedCreature == null)
                {
                    continue;
                }
                var player = absrCrit.realizedCreature as Player;
                player.controller ??= new Player.NullController(); // ??= If null assign new thing

                if (RealizedPlayer != player && !alreadyTeleportedCoopPlayers)
                {
                    player.SuperHardSetPosition(new Vector2(xPos, 294));
                }
            }
            alreadyTeleportedCoopPlayers = true;
            RealizedPlayer?.allowOutOfBounds = true;
            RealizedPlayer?.SuperHardSetPosition(new Vector2(xPos, 5500));
            return;
        }

        destroyScript = true;

        if (RealizedPlayer?.Submersion > 0f || timeCounter.isFinished)
        {
            GiveAllPlayersControllersBack();
            RealizedPlayer?.allowOutOfBounds = false;
            return;
        }

    }
}