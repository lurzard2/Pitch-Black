using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class BeaconCycle2 : Cycle
{
    public Player Beacon => RealizedOwner as Player;

    public SaveState SaveState => abstractOwner.world.game.GetSaveState();
    public float MinSpiralLevel;
    public float SpiralLevel;
    public float MaxSpiralLevel;
    public bool CanUseThanatosis;
    public bool ToggleThanatosis;

    public BeaconInputHandler InputHandler { get; set; }
    public BeaconDeathHandler MyDeathHandler { get; set; }

    public BeaconCycle2(Player player) : base(player.abstractCreature)
    {
        InputHandler = new(this);
        modules.Add(InputHandler);
        // Replace death handler with mine
        if (modules.Contains(deathHandler))
        {
            MyDeathHandler = new(this);
            deathHandler = MyDeathHandler;
            modules.Remove(deathHandler);
            modules.Add(MyDeathHandler);
        }

        // Set values for both arena and storysession compatability with mechanics
        if (SaveState is not null)
        {
            MinSpiralLevel = SaveState.GetMinSpiralLevel();
            SpiralLevel = SaveState.GetSpiralLevel();
            MaxSpiralLevel = SaveState.GetMaxSpiralLevel();
            CanUseThanatosis = SaveState.GetCanUseThanatosis();
        }
        else
        {
            MinSpiralLevel = 0;
            SpiralLevel = 1;
            MaxSpiralLevel = 1;
            CanUseThanatosis = true;
        }

        // for Playtest, for now
        if (SaveState.GetCompletedBeacon())
        {
            string ptText = $"[THIS MARKS THE END OF THE PLAYTEST CURRENTLY] ~ {MOD_VERSION}";
            MiscUtils.AddHUDMessage(player.room.game.cameras[0].hud, true, ptText, 40 * 30, 120, true, true);
        }
    }
}
