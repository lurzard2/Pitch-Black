using RWCustom;
using System;
using UnityEngine;

namespace PitchBlack;

public partial class BeaconAbilityHandler
{
    public class Thanatosis()
    {
        // Conditional effects from abusing the mechanic
        public float instability;

        // Softer check for state, able to group states, and used for mainly player x-eyes
        public bool Dead { get; set; }
        // Set for toggle by proxy
        public bool ManualToggleConditionMet { get; set; }
        // Prevent player from switching out
        public bool Stuck { get; set; } = false;
        // Prevent player from using it with a popup
        public bool Blacklisted { get; set; } = false;

        public enum State
        {
            None,
            // ON
            Entering,
            Inside,
            Safe,
            Drowning,
            Drowned,
            // OFF
            Exiting,
            Outside,
        }
        private State state = State.None;
        public State GetState => state;
        public bool IsInbetween => IsState(State.Entering) || IsState(State.Exiting);
        public bool IsState(State stateCheck) => state == stateCheck;
        public void ChangeState(State newState)
        {
            state = newState;
            timeInState.Reset();
        }
        public void ChangeSide()
        {
            ChangeState(Dead ? State.Inside : State.Outside);
        }

        public Counter timeInState = new(Int32.MaxValue, 0, true);
        // Values can be: 120, 240, 360, 480, 600 (ie. 3-15s)
        public int MaxAvailableSafeTime { get; set; } = 40*4;

        public enum Type
        {
            None,
            Involuntary,
            ON,
            OFF,
        }
        public void Toggle(BeaconAbilityHandler a, Type type = Type.None)
        {
            Player player = a.owner.player;
            int spiral = (int)a.owner.SpiralLevel;
            bool involuntary = type == Type.Involuntary;
            bool toggleFlag = involuntary || ManualToggleConditionMet;

            // Conditionally determine whether to toggle or not
            if (Blacklisted)
            {
                PenaltyInteraction(player);
                toggleFlag = false;
            }
            else if (Stuck)
            {
                toggleFlag = involuntary;
            }

            if (toggleFlag)
            {
                ToggleThanatosis(player, type, spiral);
            }

            ManualToggleConditionMet = false;

            Plugin.logger.LogDebug($"{nameof(Thanatosis)}: Thanatosis toggled {Dead}!");
        }

        private void ToggleThanatosis(Player player, Type type, int spiral)
        {
            Dead = !Dead;

            switch (type)
            {
                case Type.Involuntary:
                    ChangeSide(); break;
                case Type.ON: ChangeState(State.Inside); break;
                case Type.OFF: ChangeState(State.Outside); break;
                default:
                    ChangeState(Dead ? State.Entering : State.Exiting); break;
            }

            if (Dead)
            {
                int timeAvailable = 120 * spiral;
                int maxAllowedSafeTime = timeAvailable - (int)instability;
                MaxAvailableSafeTime = maxAllowedSafeTime;
            }

            if (player.room is not null)
            {
                player.room.PlaySound(Enums.SoundID.Player_Inducing_Thanatosis, player.mainBodyChunk);
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
}
