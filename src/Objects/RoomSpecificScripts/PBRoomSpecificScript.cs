using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PitchBlack.Plugin;

namespace PitchBlack;

public abstract class PBRoomSpecificScript : UpdatableAndDeletable
{
    public bool AllPlayersRealized
    {
        get
        {
            return room.game.AllPlayersRealized;
        }
    }

    public bool RoomIsBeingLoaded
    {
        get
        {
            return room.game.manager.FadeDelayInProgress || !room.fullyLoaded || !room.BeingViewed;
        }
    }

    public PBRoomSpecificScript(Room room)
    {
        this.room = room;
        time.Reset();
        logger.LogDebug($"PBRSS: Added script to {room.abstractRoom.name}");
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        time.Tick();
    }

    public void ChangePhase(Phase phase)
    {
        this.phase = phase;
        timeInPhase.Reset();
    }

    public class Phase : ExtEnum<Phase>
    {
        public Phase(string value, bool register) : base(value, register) { }

        public static readonly Phase Init = new(nameof(Init), true);
        public static readonly Phase End = new(nameof(End), true);
    }

    public Phase phase;

    public Counter time = new(Int32.MaxValue, 0, true);
    public Counter timeInPhase = new(Int32.MaxValue, 0, true);

    //public bool LoadingRoom => room.game.manager.FadeDelayInProgress || !room.fullyLoaded || !room.BeingViewed;
    //public Player RealizedPlayer => room.game.Players.Count > 0 ? room.game.Players[0].realizedCreature as Player : null;
}
