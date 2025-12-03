using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class PBRoomSpecificScript : UpdatableAndDeletable
{
    public PBRoomSpecificScript(Room room)
    {
        this.room = room;
        timeCounter.Reset();
        alreadyTeleportedCoopPlayers = false;
        logger.LogDebug($"PBRSS: Added RSS to {room.abstractRoom.name}");
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (RealizedPlayer == null)
        {
            return;
        }
        if (destroyScript)
        {
            GiveAllPlayersControllersBack();
            Destroy();
            return;
        }

        timeCounter.Tick();
    }

    public void GiveAllPlayersControllersBack()
    {
        foreach (var abstrCrit in room.game.session.Players)
        {
            if (abstrCrit?.realizedCreature is Player player)
                player.controller = null;
        }
    }

    public Counter timeCounter = new Counter(0, 0, true);
    internal static bool destroyScript; // Assigned in RainWorldGame.ctor in Plugin.cs
    public bool alreadyTeleportedCoopPlayers;
    public bool LoadingRoom => room.game.manager.FadeDelayInProgress || !room.fullyLoaded || !room.BeingViewed;
    public Player RealizedPlayer => room.game.Players.Count > 0 ? room.game.Players[0].realizedCreature as Player : null;
}
