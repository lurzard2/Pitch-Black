using RWCustom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PitchBlack;

public class BeaconAbilityHandler
{
    public Beacon owner;
    public bool ActivationConditionMet => owner.inputs.specialInputCounter == 40;

    public ThanatosisData thanatosisData;
    public class ThanatosisData()
    {
        // Softer check for state, able to group states, and used for mainly player x-eyes
        public bool Dead { get; set; }
        // Set for toggle by proxy
        public bool ToggleConditionMet { get; set; }
        // Prevent player from switching out
        public bool Stuck { get; set; }
        // Prevent player from using it with a popup
        public bool Blacklisted { get; set; }

        public State state;
        public enum State
        {
            None,
            // ON
            Entering,
            Inside,
            // TIMED
            Safe,
            Drowning,
            Persisting,
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
            // Stop if stuck but not if involuntary
            if (Stuck && !involuntary)
            {
                return;
            }
            if (involuntary || ToggleConditionMet)
            {
                if (Blacklisted)
                {
                    PenaltyInteraction(player);
                }
                else
                {
                    ToggleThanatosis(player);
                }
                ToggleConditionMet = false;
            }
        }

        private void ToggleThanatosis(Player player)
        {
            Dead = !Dead;
            state = Dead ? State.Exiting : State.Entering;
            SoundID sound = Dead ? Enums.SoundID.Player_Deactivated_Thanatosis : Enums.SoundID.Player_Activated_Thanatosis;
            if (player.room != null)
            {
                player.room.PlaySound(sound, player.mainBodyChunk);
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
        thanatosisData = new()
        {
            state = ThanatosisData.State.None
        };
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

    public void ChangeState(ThanatosisData.State newState)
    {
        thanatosisData.state = newState;
    }
}
