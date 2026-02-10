using Fisobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PitchBlack.Plugin;
using Music;
using Watcher;

namespace PitchBlack;

public class BeaconThreatTracker
{
    // Replacing threat tracker logic
    public static void UpdateThreatProcess(PlayerThreatTracker self, RainWorldGame game, Player player)
    {
        Room room = player.room;
        float conditionalGhostMode = 0f;
        string songName = null;
        float associatedGhostMode;

        self.recommendedDroneVolume = room.roomSettings.BkgDroneVolume;
        if (!room.world.rainCycle.MusicAllowed && room.roomSettings.DangerType != RoomRain.DangerType.None)
        {
            self.recommendedDroneVolume = 0f;
        }

        if (game.cameras[0].ghostMode > conditionalGhostMode)
        {
            //  Dreamer presences song
            if (DreamerMode_Hooks.currentTarget != null && DreamerMode_Hooks.currentTarget.presenceSpawned)
            {
                songName = DreamerMode_Hooks.currentTarget.SongName;
                associatedGhostMode = game.cameras[0].ghostMode;
                self.ghostMode = associatedGhostMode;
            }
            else
            {
                self.ghostMode = 0f;
            }
        }
        else
        {
            self.ghostMode = 0f;
        }

        // If threat tracker's ghostMode (disambiguated from rCam.ghostMode) is set
        if (self.ghostMode > 0f)
        {
            self.recommendedDroneVolume = 0f;
            self.musicPlayer.FadeOutAllNonGhostSongs(120f);
            if (songName != null
                && (self.musicPlayer.song == null
                    || self.musicPlayer.song is not GhostSong)
                && (DreamerMode_Hooks.currentTarget != null
                    && !DreamerMode_Hooks.currentTarget.myDreamer.songPlaying))
            {
                self.musicPlayer.RequestGhostSong(songName);
                DreamerMode_Hooks.currentTarget.myDreamer.songPlaying = true;
            }
        }
        // If there is no presence, and there is a song, remove it (fixes music triggers after encountering Dreamer)
        if (game.cameras[0].ghostMode <= 0f
            && self.musicPlayer.song != null
            && self.musicPlayer.song is GhostSong
            && (DreamerMode_Hooks.currentTarget != null
                && DreamerMode_Hooks.currentTarget.myDreamer.songPlaying))
        {
            self.musicPlayer.song = null;
            DreamerMode_Hooks.currentTarget.myDreamer.songPlaying = false;
        }

        #region Other Code
        if (!player.room.world.singleRoomWorld)
        {
            if (player.room.abstractRoom.index != self.room)
            {
                self.lastLastRoom = self.lastRoom;
                self.lastRoom = self.room;
                self.room = player.room.abstractRoom.index;
                if (self.room != self.lastLastRoom)
                {
                    self.roomSwitches++;
                    string a = (player.room.world.region.regionParams.proceduralMusicBank == "") ? player.room.world.region.name : player.room.world.region.regionParams.proceduralMusicBank;
                    if (a != self.region)
                    {
                        self.region = a;
                        self.musicPlayer.NewRegion(self.region);
                    }
                }
            }
            if (self.roomSwitches > 0 && self.roomSwitchDelay > 0)
            {
                self.roomSwitchDelay--;
                if (self.roomSwitchDelay < 1)
                {
                    if (self.musicPlayer.song != null)
                    {
                        self.musicPlayer.song.PlayerToNewRoom();
                    }
                    if (self.musicPlayer.nextSong != null)
                    {
                        self.musicPlayer.nextSong.PlayerToNewRoom();
                    }
                    self.roomSwitchDelay = UnityEngine.Random.Range(80, 400);
                    self.roomSwitches--;
                }
            }
        }
        else if ((self.musicPlayer.manager.currentMainLoop as RainWorldGame).IsArenaSession && (self.musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge && (self.musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.chMeta != null && !string.IsNullOrEmpty((self.musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.chMeta.threatMusic))
        {
            string threatMusic = (self.musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.chMeta.threatMusic;
            if (self.region != threatMusic)
            {
                self.region = threatMusic;
                self.musicPlayer.NewRegion(self.region);
            }
        }
        self.threatDetermine.Update(self.musicPlayer.manager.currentMainLoop as RainWorldGame);
        if (self.musicPlayer.song != null)
        {
            self.threatDetermine.currentThreat = 0f;
        }
        self.currentThreat = self.threatDetermine.currentThreat;
        self.currentMusicAgnosticThreat = self.threatDetermine.currentMusicAgnosticThreat;
        #endregion
    }

    // Checks for a bunch of things the regular Update function does, but isolated
    public static bool CanContinueToProcess(PlayerThreatTracker self)
    {
        if (self.musicPlayer.manager.currentMainLoop == null || self.musicPlayer.manager.currentMainLoop.ID != ProcessManager.ProcessID.Game)
        {
            self.recommendedDroneVolume = 0f;
            self.currentThreat = 0f;
            self.currentMusicAgnosticThreat = 0f;
            self.region = null;
            return false;
        }
        if (self.playerNumber >= (self.musicPlayer.manager.currentMainLoop as RainWorldGame).Players.Count)
        {
            return false;
        }
        Player player = (self.musicPlayer.manager.currentMainLoop as RainWorldGame).Players[self.playerNumber].realizedCreature as Player;
        if (player == null || player.room == null)
        {
            return false;
        }
        if (player.room.game.GameOverModeActive || player.redsIllness != null)
        {
            self.recommendedDroneVolume = 0f;
            self.currentThreat = 0f;
            self.currentMusicAgnosticThreat = 0f;
            return false;
        }
        return true;
    }
}

public static class MusicHooks
{
    public static void Apply()
    {
        // Adding GhostSong for Dreamer
        On.Music.PlayerThreatTracker.Update += ThreatTracker_Update;
    }

    private static void ThreatTracker_Update(On.Music.PlayerThreatTracker.orig_Update orig, PlayerThreatTracker self)
    {
        // Checks for proper conditions: process is RWG, Player isn't null
        if (BeaconThreatTracker.CanContinueToProcess(self))
        {
            RainWorldGame game = self.musicPlayer.manager.currentMainLoop as RainWorldGame;
            Player player = game.Players[self.playerNumber].realizedCreature as Player;
            if (BeaconUtils.IsBeacon(player))
            {
                BeaconThreatTracker.UpdateThreatProcess(self, game, player);
            }
            else
            {
                orig(self);
            }
        }
        else
        {
            orig(self);
        }
    }
}
