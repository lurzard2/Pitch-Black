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
    public EtherealGraphics GhostGraphics => Stillborn.visibleEntity as EtherealGraphics;
    public EncounterType encounterType;
    public Counter visibleCounter = new(100, 0, true);
    private float deathFac = 0.01f;
    private Counter deathCount = new(50, 0, true);
    private CreatureSpasmer spasmer = null;
    private RiftManager nightmareRiftManager = null;
    private bool readyToKill = false;
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
            if (Plugin.scugCWT.TryGetValue(target, out var c) && c is BeaconCWT beacon)
            {
                if (beacon.playerCycle != null)
                {
                    // Remove controls
                    var distance = UnityEngine.Vector2.Distance(Stillborn.Pos, beacon.playerCycle.owner.mainBodyChunk.pos);
                    Plugin.logger.LogDebug($"{distance}");
                    if (!readyToKill)
                    {
                        // Right below them
                        if (distance < 322f)
                        {
                            beacon.playerCycle.owner.controller ??= new InputController(this);
                            readyToKill = true;
                        }
                        return;
                    }
                    // Dying process
                    if (UnityEngine.Random.value < deathFac)
                    {
                        if (!deathCount.isFinished)
                        {
                            deathFac += 0.001f;
                            deathCount.Tick();
                            beacon.playerCycle.ToggleThanatosis(false);
                            beacon.playerCycle.owner.Stun(deathCount > 15 ? 80 : 40);
                        }
                    }
                    if (deathCount > 25)
                    {
                        if (Stillborn.RoomCamera.ghostMode <= 0.5f)
                        {
                            Stillborn.RoomCamera.ghostMode += 0.002f;
                        }
                    }
                    // Post-process death stuff
                    if (deathCount.isFinished)
                    {
                        // "Kill"
                        if (spasmer == null)
                        {
                            spasmer = new CreatureSpasmer(beacon.playerCycle.owner, true, 666);
                            if (!beacon.playerCycle.isDead)
                            {
                                beacon.playerCycle.ToggleThanatosis(false);
                            }
                        }
                        if (Stillborn.RoomCamera.ghostMode < 1)
                        {
                            Stillborn.RoomCamera.ghostMode += 0.004f;
                        }
                        else
                        {
                            if (nightmareRiftManager == null)
                            {
                                beacon.playerCycle.owner.room.AddObject(spasmer);
                                nightmareRiftManager = new RiftManager(Stillborn.room, Stillborn.placedObject, false);
                                if (encounterType == EncounterType.Ghost)
                                {
                                    nightmareRiftManager.placedRift = nightmareRiftManager.ScriptedRift(Enums.Timeline.Beacon, "ud", "ud_test");
                                }
                                var nmRift = nightmareRiftManager.placedRift;
                                nmRift.Data.effectSettings.badWarpCosmetic = true;
                                nmRift.Data.effectSettings.spawnBigRift = true;
                                //nmRift.triggerTime = (float)((int)(nmRift.triggerActivationTime - 1f));
                                nmRift.strongPull = true;
                                nmRift.guaranteeTrigger = true;
                                // Make completely one way
                                nmRift.Data.oneWay = true;
                                nmRift.Data.oneWayEntrance = true;
                                nmRift.Data.oneWayEntranceIdentified = true;
                                // Pass it off
                                MiscUtils.PlaceRift(nightmareRiftManager, nmRift, true);
                            }
                            if (nightmareRiftManager?.placedRift.currentState == WarpPoint.State.EnterWarp)
                            {
                                beacon.playerCycle.owner.controller = null;
                            }
                        }
                    }
                }
            }
        }
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
