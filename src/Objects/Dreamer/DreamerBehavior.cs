using RWCustom;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Watcher;

namespace PitchBlack;

public class DreamerBehavior : PBEntity.BehaviorModule
{
    public Dreamer Dreamer => owner as Dreamer;
    public Counter onScreenCounter = new Counter(120, 0, true);
    public Counter afterConversationCounter = new Counter(280, 0, true);
    public virtual bool OnScreen()
    {
        return Dreamer.room.VisibleInAnyCameraScreenBounds(Dreamer.Pos);
    }
    public void TickForEncounter()
    {
        if (Dreamer.encounterFinished)
        {
            afterConversationCounter.Tick();
        }
        else
        {
            onScreenCounter.Tick();
        }
    }

    public EncounterType encounterType = null;
    public class EncounterType : ExtEnum<EncounterType>
    {
        public EncounterType(string value, bool register) : base(value, register) { }
        public static readonly EncounterType DreamWarp = new(nameof(DreamWarp), true);
        public static readonly EncounterType Dream = new(nameof(Dream), true);
        public static readonly EncounterType Nightmare = new(nameof(Nightmare), true);
    }

    public DreamerBehavior(Dreamer owner, EncounterType encounterType) : base(owner)
    {
        this.owner = owner;
        this.encounterType = encounterType;
    }

    public override void Update()
    {
        base.Update();
        var room = Dreamer.room;
        var pos = Dreamer.Pos;

        // Todo: Move all this to a Behavior object like VW, then let that handle different encounter types :)
        if (OnScreen())
        {
            TickForEncounter();
        }
        else
        {
            if (conversation != null)
            {
                var conversation2 = conversation;
                if (conversation2 != null)
                {
                    conversation2.Destroy();
                }
                Dreamer.convoActive = false;
                conversation = null;
            }
            onScreenCounter.Reset();
        }
        if (onScreenCounter.isFinished && room.game.cameras[0].hud != null)
        {
            if (conversation == null)
            {
                Dreamer.convoActive = true;
                StartConversation();
            }
            else if (conversation.slatedForDeletion)
            {
                Dreamer.convoFinished = true;
            }
        }
        if (conversation != null && Dreamer.convoActive)
        {
            conversation.Update();
        }
        if (Dreamer.convoFinished)
        {
            Dreamer.convoActive = false;
            MarkEncountered();
        }
        if (afterConversationCounter.isFinished)
        {
            if (encounterType == EncounterType.DreamWarp)
            {
                SpawnWarp();
            }
            Despawn();
        }
        else if (afterConversationCounter > 0)
        {
            for (int i = 0; i < afterConversationCounter; i++)
            {
                (Dreamer.visibleEntity as EtherealGraphics).AfterEncounteredVisual();
            }
        }
    }


    #region Dialogue and Speaking
    private void StartConversation()
    {
        if (Dreamer.room.game.cameras[0].hud.dialogBox == null)
        {
            Dreamer.room.game.cameras[0].hud.InitDialogBox();
        }
        conversation = new DreamerConversation(this, GetConversationID(), Dreamer.room.game.cameras[0].hud.dialogBox);
        Dreamer.convoActive = true;

    }

    private Conversation.ID GetConversationID()
    {
        Conversation.ID result;
        SaveState save = Dreamer.room.game.TryGetSaveState(out var s) ? s : null;
        int encounters = save.GetDreamerEncountersNumber();

        switch (encounters)
        {
            case 0:
                result = Enums.ConversationID.Dreamer_Start;
                break;
            case 1:
                result = Enums.ConversationID.Dreamer_Prologue_1;
                break;
            case 2:
                result = Enums.ConversationID.Dreamer_Prologue_2;
                break;
            case 3:
                // Unfinished
                result = Enums.ConversationID.Dreamer_Prologue_Intermission;
                break;
            default:
                result = Enums.ConversationID.Dreamer_Placeholder;
                break;
        }

        return result;
    }
    #endregion

    public void FinishAfterConversationCounter()
    {
        Plugin.logger.LogDebug($"Dreamer: Counter is - {afterConversationCounter} before Encounter finished");
        afterConversationCounter.Finish();
        return;
    }

    #region WarpPoints
    // For Dreamer-originating rift
    private void SpawnWarp()
    {
        EntityWarpData data = Dreamer.SpecialData;
        if (data == null)
        {
            return;
        }
        if (data.destRoom == null)
        {
            return;
        }

        PlacedObject placedObj = new PlacedObject(PlacedObject.Type.WarpPoint, data.CreateWarpPointDataForRift(Dreamer.room));
        placedObj.pos = Dreamer.Pos;

        // Reset warp counter so it may open
        Dreamer.room.world.game.GetStorySession.warpsTraversedThisCycle = 0;

        var riftManager = new RiftManager(Dreamer.room, placedObj, false);
        bool makeOneWay = false;
        var rift = riftManager.placedRift;
        if (BeaconSaveData.GetDreamerEncountersNumber(Dreamer.room.world.game.GetStorySession.saveState) == 3)
        {
            rift = riftManager.ScriptedRift(Enums.Timeline.Beacon, "pblf", "pblf_c07");
            rift.Data.effectSettings.badWarpCosmetic = true;
            makeOneWay = true;
        }

        if (makeOneWay)
        {
            rift.Data.oneWay = true;
            rift.Data.oneWayEntrance = true;
            rift.Data.oneWayEntranceIdentified = true;
        }

        MiscUtils.PlaceRift(riftManager, rift, true);
    }

    // For DevTools-originating rift
    public static void SpawnBackupWarpPoint(Room room, PlacedObject oldPlacedObj)
    {
        WarpPoint.WarpPointData warpPointData = (oldPlacedObj.data as EntityWarpData).CreateWarpPointDataForRift(room);
        PlacedObject newPlacedObj = new PlacedObject(Enums.PlacedObjectType.RiftSpot, warpPointData);
        newPlacedObj.pos = oldPlacedObj.pos;
        bool flag = false;
        foreach (Rift rift in room.warpPoints)
        {
            if (rift.Data.destRoom == warpPointData.destRoom && Vector2.Distance(rift.pos, newPlacedObj.pos) < 10f)
            {
                flag = true;
                break;
            }
        }
        if (!flag)
        {
            var riftManager = new RiftManager(room, newPlacedObj, false);
            bool makeOneWay = false;
            if (BeaconSaveData.GetDreamerEncountersNumber(room.world.game.GetStorySession.saveState) == 3)
            {
                riftManager.placedRift = riftManager.ScriptedRift(Enums.Timeline.Beacon, "pblf", "pblf_c07");
                riftManager.placedRift.Data.effectSettings.badWarpCosmetic = true;
                makeOneWay = true;
            }

            if (makeOneWay)
            {
                riftManager.placedRift.Data.oneWay = true;
                riftManager.placedRift.Data.oneWayEntrance = true;
                riftManager.placedRift.Data.oneWayEntranceIdentified = true;
            }

            MiscUtils.PlaceRift(riftManager, riftManager.placedRift);
        }
    }

    #endregion

    #region Encountering and Removing
    private void MarkEncountered()
    {
        if (Dreamer.encounterFinished)
        {
            return;
        }
        EntityWarpData data = Dreamer.SpecialData;
        if (data == null)
        {
            return;
        }

        Plugin.logger.LogDebug($"Dreamer: I have finished my encounter!");
        var game = Dreamer.room.world.game;
        var state = game.GetStorySession.saveState;
        string currentRoomName = Dreamer.room.abstractRoom.name;

        SaveEncounter(state, currentRoomName);
        IncreaseSpiralLevel(state);
        OverwriteSaveDen(game, currentRoomName);
        DreamerPresence_Functions.DeactivateDreamerPresence(Dreamer.room);

        Dreamer.encounterFinished = true;
    }

    private void OverwriteSaveDen(RainWorldGame game, string currentRoomName)
    {
        RainWorldGame.ForceSaveNewDenLocation(game, currentRoomName, false);
        Plugin.logger.LogDebug($"Dreamer: Saved {currentRoomName} as den");
    }

    private void SaveEncounter(SaveState state, string currentRoomName)
    {
        BeaconSaveData.SetDreamerEncounteredRooms(state, currentRoomName);
        var encounterNumber = BeaconSaveData.GetDreamerEncountersNumber(state);
        encounterNumber++;
        BeaconSaveData.SetDreamerEncountersNumber(state, encounterNumber);
        string joinedString = String.Join(",", BeaconSaveData.GetDreamerEncounteredRooms(state));
        Plugin.logger.LogDebug($"Dreamer: Set encountered rooms - {joinedString}");
    }

    private void IncreaseSpiralLevel(SaveState state)
    {
        var maxLevel = BeaconSaveData.GetMaxSpiralLevel(state);
        float increment = 0f;
        if (maxLevel >= 0.5f)
        {
            increment = 0.5f;
        }
        else
        {
            increment = 0.25f;
        }
        BeaconSaveData.SetMaxSpiralLevel(state, maxLevel += increment);
        Plugin.logger.LogDebug($"Dreamer: Increased your level by {increment}, level is {maxLevel}");
    }

    private void Despawn()
    {
        if (!Dreamer.flaggedAsReadyForDeletion)
        {
            DreamerMode_Hooks.targetGhostMode = 0;
            Dreamer.flaggedAsReadyForDeletion = true;
        }
    }
    #endregion

}
