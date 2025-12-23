using IL.Rewired.Data;
using Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

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

    public override void Update()
    {
        base.Update();
    }
}
