using Music;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class SequenceSong : PBSong
{
    public SequenceSong(MusicPlayer musicPlayer, string name) : base(musicPlayer, name)
    {
        logger.LogDebug($"SequenceSong: Now Playing {name}");

        priority = 1.1f;
        stopAtGate = false;
        stopAtDeath = true;
        fadeInTime = 200f;
        Loop = false;
    }
}

public abstract class PBSong : Song
{

    public PBSong(MusicPlayer musicPlayer, string name) : base(musicPlayer, name, MusicPlayer.MusicContext.StoryMode) { }

    public bool ConditionToPlay(string songName)
    {
        if (musicPlayer.song != null && musicPlayer.song.name == songName)
        {
            return false;
        }
        if (musicPlayer.nextSong != null && musicPlayer.nextSong.name == songName)
        {
            return false;
        }
        if (!musicPlayer.manager.rainWorld.setup.playMusic)
        {
            return false;
        }
        return true;
    }

    public void StopCurrentSong()
    {
        if (musicPlayer.song != null)
        {
            musicPlayer.song.StopAndDestroy();
        }
    }

    public override void Update()
    {
        base.Update();
    }
}
