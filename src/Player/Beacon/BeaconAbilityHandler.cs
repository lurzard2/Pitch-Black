using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack;

public class BeaconAbilityHandler
{
    public Beacon owner;
    public bool ActivationConditionMet => owner.inputs.specialInputCounter == 80;

    public ThanatosisData thanatosisData;
    public class ThanatosisData()
    {
        public bool Dead { get; set; }
        public bool ToggleConditionMet { get; set; }
        private bool toggleFlag = false;
        public State state;
        public enum State
        {
            // MISC
            None,
            Stuck,
            Blacklisted,
            // ON
            Entering,
            Inside,
            // TIMED
            Safe,
            Drowning,
            Persist,
            Drowned,
            // OFF
            Exiting,
            Outside,
        }

        /// <summary>
        /// Retroactively toggles based on ToggleConditionMet.
        /// </summary>
        /// <param name="involuntary">Set to true to guarantee toggle</param>
        public void Toggle(Player player, bool involuntary = false)
        {
            if (involuntary || ToggleConditionMet)
            {
                if (state == State.Stuck)
                {
                    return;
                }
                else if (state == State.Blacklisted)
                {
                    PenaltyInteraction(player);
                }
                else
                {
                    ThanatosisToggle(player);
                }
                ToggleConditionMet = false;
            }
        }

        private void ThanatosisToggle(Player player)
        {
            Dead = !Dead;
            if (Dead)
            {
                state = State.Exiting;
            }
            else
            {
                state = State.Entering;
            }
        }

        private void PenaltyInteraction(Player player)
        {
            // Indicator for being unable to use Thanatosis if unlocked
            player.Stun(120);
            string popupText = "";
            if (MiscUtils.IsNightmareRegion(player.abstractCreature.world.name))
                popupText = "These tides are sinister";
            else if (MiscUtils.IsPBSB(player.abstractCreature.world.name))
                popupText = "These tides rest still";
            else
                popupText = "These tides flow without disturbance";
            MiscUtils.AddHUDMessage(player.room.game.cameras[0].hud, true, popupText, 60, 120, false, true);
            return;
        }
    }

    public BeaconAbilityHandler(Beacon owner)
    {
        this.owner = owner;
    }

    public void Update()
    {
        if (ActivationConditionMet && owner.SaveState.GetCanUseThanatosis_CurrentOrArenaDefault())
        {
            thanatosisData.ToggleConditionMet = true;
        }

        thanatosisData.Toggle(owner.player);

        if (thanatosisData.Dead)
        {

        }
        else
        {

        }
    }
}
