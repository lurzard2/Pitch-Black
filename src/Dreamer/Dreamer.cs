using HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PitchBlack;

public class Dreamer : Ghost
{
    //Todo: EVERYTHING

    public Dreamer(Room room, PlacedObject placedObject, GhostWorldPresence worldGhost) : base(room, placedObject, worldGhost)
    {
    }

    public override void InitializeSprites()
    {
        scale = 0.65f;
        snoutSegments = 7;
        base.InitializeSprites();
    }

    private void DespawnDreamer()
    {
        if (!base.slatedForDeletetion)
        {
            base.slatedForDeletetion = true;
            this.room.game.cameras[0].MoveCamera(this.room.game.cameras[0].currentCameraPosition);
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void StartConversation()
    {
        Conversation.ID id = Conversation.ID.None;
        if (room.game.cameras[0].hud.dialogBox == null)
        {
            room.game.cameras[0].hud.InitDialogBox();
        }
        currentConversation = new DreamerConversation(id, this, room.game.cameras[0].hud.dialogBox);
        if (id == Conversation.ID.None)
        {
            currentConversation.events.Add(new Conversation.TextEvent(currentConversation, 0, "HULLOOOOOOOOOOOOOOOOOOOOO :3 I CAN TALK", 300));
        }
    }

    public class DreamerConversation : GhostConversation
    {
        public DreamerConversation(ID id, Ghost ghost, DialogBox dialog) : base(id, ghost, dialog)
        {
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
