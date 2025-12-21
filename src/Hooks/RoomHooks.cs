using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public static class RoomSpecificScriptHooks
{
    private static void NewUAD(Room room, UpdatableAndDeletable scriptObj)
    {
        room.AddObject(scriptObj);
    }

    public static void Inject()
    {
        On.RoomSpecificScript.AddRoomSpecificScript += AddScriptToRoom;
    }

    private static void AddScriptToRoom(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
    {
        orig(room);
        if (room.game.session is StoryGameSession storyCheck && !MiscUtils.IsBeacon(storyCheck.saveStateNumber))
        {
            return;
        }

        var saveState = room.world.game.GetStorySession.saveState;

        if (room.abstractRoom.name == "VV_E01" && room.world.game.GetStorySession.saveState.cycleNumber == 0)
        {
            for (int i = 0; i < room.game.Players.Count; i++)
            {
                NewUAD(room, new VV_E01(room, i));
            }
        }
    }
}

public static class RoomHooks
{
    public static void Apply()
    {
        RoomSpecificScriptHooks.Inject();
    }
}
