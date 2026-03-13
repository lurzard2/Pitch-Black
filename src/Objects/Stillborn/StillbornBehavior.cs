using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher;
using RWCustom;
using System.Numerics;
using BepInEx.Logging;

namespace PitchBlack;

public class StillbornBehavior : PBEntity.BehaviorModule
{
    public Stillborn Stillborn => owner as Stillborn;
    public PlaceholderEchoGraphics GhostGraphics => Stillborn.visibleEntity as PlaceholderEchoGraphics;
    public EncounterType encounterType;
    public Counter visibleCounter = new(100, 0, true);
    private float deathFac = 0.01f;
    private Counter deathCount = new(30, 0, true);
    private CreatureSpasmer spasmer = null;
    private bool readyToKill = false;
    private bool riftSpawned = false;
    public class EncounterType : ExtEnum<EncounterType>
    {
        public EncounterType(string value, bool register) : base(value, register) { }
        public static readonly EncounterType Ghost = new(nameof(Ghost), true);
        //public static readonly EncounterType LTTM = new(nameof(LTTM), true);
        //public static readonly EncounterType Beacon = new(nameof(Beacon), true);
        //public static readonly EncounterType Dreamer = new(nameof(Dreamer), true);
    }

    public StillbornBehavior(Stillborn owner, EncounterType encounterType) : base(owner)
    {
        this.encounterType = encounterType;
    }

    public override void Update()
    {
        base.Update();

        Player target = null;

        var player = Stillborn.room.PlayersInRoom[0];
        if (player != null)
        {
            target = player;
        }

        if (Stillborn.OnScreen())
        {
            visibleCounter.Tick();
        }
        else if (deathCount < 1)
        {
            visibleCounter.Reset();
        }

        if (visibleCounter.isFinished && target != null)
        {
            if (target.TryGetBeacon(out var beacon))
            {
                //if (beacon.cycle != null)
                //{
                //    // Remove controls
                //    var distance = UnityEngine.Vector2.Distance(Stillborn.Pos, beacon.cycle.playerObj.mainBodyChunk.pos);
                //    Plugin.logger.LogDebug($"{distance}");
                //    if (!readyToKill)
                //    {
                //        // Right below them
                //        if (distance < 321f)
                //        {
                //            beacon.cycle.playerObj.controller ??= new InputController(this);
                //            readyToKill = true;
                //        }
                //        return;
                //    }
                //    // Dying process
                //    if (UnityEngine.Random.value < deathFac)
                //    {
                //        if (!deathCount.isFinished)
                //        {
                //            deathFac += 0.001f;
                //            deathCount.Tick();
                //            beacon.cycle.ToggleThanatosis(false);
                //            beacon.cycle.playerObj.Stun(deathCount > 15 ? 80 : 40);
                //        }
                //    }
                //    if (deathCount > 17)
                //    {
                //        if (Stillborn.RoomCamera.ghostMode <= 0.5f)
                //        {
                //            Stillborn.RoomCamera.ghostMode += 0.002f;
                //        }
                //    }
                //    // Post-process death stuff
                //    if (deathCount.isFinished)
                //    {
                //        // "Kill"
                //        if (spasmer == null)
                //        {
                //            spasmer = new CreatureSpasmer(beacon.cycle.playerObj, true, 666);
                //            if (!beacon.cycle.isDead)
                //            {
                //                beacon.cycle.ToggleThanatosis(false);
                //                beacon.cycle.playerObj.abstractCreature.rippleLayer = 0;
                //            }
                //        }
                //        if (Stillborn.RoomCamera.ghostMode < 1)
                //        {
                //            Stillborn.RoomCamera.ghostMode += 0.004f;
                //        }
                //        else
                //        {
                //            if (!riftSpawned)
                //            {
                //                SpawnRift();
                //                beacon.cycle.playerObj.room.AddObject(spasmer);
                //                beacon.cycle.playerObj.controller = null;
                //                riftSpawned = true;
                //            }
                //        }
                //    }
                //}
            }
        }
    }

    private void SpawnRift()
    {
        EntityWarpData data = Stillborn.SpecialData;
        if (data == null)
        {
            return;
        }
        if (data.destRoom == null)
        {
            return;
        }

        PlacedObject placedObj = new PlacedObject(PlacedObject.Type.WarpPoint, data.CreateWarpPointDataForRift(Stillborn.room));
        placedObj.pos = Stillborn.Pos;

        var riftManager = new RiftManager(Stillborn.room, placedObj, false);
        var rift = riftManager.placedRift;
        rift = riftManager.ScriptedRift(data.destTimeline, data.destRegion, data.destRoom);

        rift.guaranteeTrigger = true;
        rift.strongPull = true;
        rift.Data.effectSettings.badWarpCosmetic = true;
        rift.Data.effectSettings.spawnBigRift = true;
        rift.Data.oneWay = true;
        rift.Data.oneWayEntrance = true;
        rift.Data.oneWayEntranceIdentified = true;

        MiscUtils.PlaceRift(riftManager, rift);
    }

    private Player.InputPackage GetInput()
    {
        return new Player.InputPackage(false, Options.ControlSetup.Preset.None, 0, 0, false, false, false, false, false, false);
    }

    public class InputController : Player.PlayerController
    {
        public InputController(StillbornBehavior owner)
        {
            this.owner = owner;
        }

        public override Player.InputPackage GetInput()
        {
            return owner.GetInput();
        }

        private StillbornBehavior owner;
    }
}
