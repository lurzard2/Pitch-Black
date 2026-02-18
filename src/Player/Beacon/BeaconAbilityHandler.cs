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
    public bool ThanatosisToggleConditionMet => owner.inputs.specialInputCounter == 40;
    // This can be static because it will be true for all players
    public static bool CanUseThanatosis { get; set; } = false;

    public ThanatosisData deathData;
    public class ThanatosisData()
    {
        public float instability;

        // Softer check for state, able to group states, and used for mainly player x-eyes
        public bool Dead { get; set; }
        // Set for toggle by proxy
        public bool ToggleConditionMet { get; set; }
        public bool involuntarySwitchFlag;
        // Prevent player from switching out
        public bool Stuck { get; set; }
        // Prevent player from using it with a popup
        public bool Blacklisted { get; set; }

        public State state;
        public enum State
        {
            None,
            Switching,
            // ON
            Entering,
            Inside,
            Safe,
            Drowning,
            Persisting,
            Drowned,
            // OFF
            Exiting,
            Outside,
        }
        public bool IsInside => IsState(State.Inside) || IsState(State.Safe) || IsState(State.Drowning) || IsState(State.Persisting) || IsState(State.Drowned);
        public bool IsInbetween => IsState(State.Entering) || IsState(State.Exiting);
        public bool IsOutside => IsState(State.Outside);
        public bool IsState(State stateCheck) => state == stateCheck;
        public void ChangeState(State newState)
        {
            state = newState;
            timeInState.Reset();
        }

        public Counter timeInState = new(Int32.MaxValue, 0, true);
        public static int insideTimeAvailable = 40 * 4;
        public static int transitionTime = 120;
        // Values can be: 120, 240, 360, 480, 600 (ie. 3-15s)
        public int MaxAvailableSafeTime { get; set; } = 120;

        /// <summary>
        /// Retroactively toggles based on ToggleConditionMet.
        /// </summary>
        /// <param name="involuntary">Set to true to guarantee toggle</param>
        public void Toggle(Player player, bool involuntary = false)
        {
            involuntarySwitchFlag = involuntary;

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
            ChangeState(State.Switching);
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
        deathData = new()
        {
            state = ThanatosisData.State.None
        };
    }

    public void Update()
    {
        // Only have to check savestate once
        if (CanUseThanatosis)
        {
            ThanatosisUpdate();
        }
        else
        {
            CanUseThanatosis = owner.SaveState.GetCanUseThanatosis();
        }
    }

    private void ThanatosisUpdate()
    {
        if (ThanatosisToggleConditionMet)
        {
            deathData.ToggleConditionMet = true;
        }

        // Listens to the toggle method
        deathData.Toggle(owner.player);

        deathData.timeInState.Tick();

        if (deathData.IsState(ThanatosisData.State.Switching))
        {
            Switching();
        }

        if (deathData.Dead)
        {
            Inside();
        }
        else
        {
            Outside();
        }

        Plugin.logger.LogDebug($"{nameof(ThanatosisData)}: State={deathData.state} - Time={deathData.timeInState} - Auto={deathData.involuntarySwitchFlag}");
    }

    private void Switching()
    {
        bool interactionLocked = deathData.timeInState > 80;
        if (interactionLocked)
        {
            if (deathData.timeInState == ThanatosisData.transitionTime)
            {
                deathData.ChangeState(deathData.Dead ? ThanatosisData.State.Exiting : ThanatosisData.State.Entering);
                deathData.involuntarySwitchFlag = false;
                owner.inputs.UseSpecial = false;
            }
        }
        else if (owner.inputs.noSpecInput)
        {
            // Simulate returning to side, without the toggle visual
            deathData.Dead = !deathData.Dead;
            deathData.ChangeState(deathData.Dead ? ThanatosisData.State.Inside : ThanatosisData.State.Outside);
        }
    }

    private void Inside()
    {
        if (deathData.IsState(ThanatosisData.State.Entering))
        {
            deathData.MaxAvailableSafeTime = 120 * (int)owner.SpiralLevel;
            deathData.ChangeState(ThanatosisData.State.Inside);
        }
        else if (deathData.IsState(ThanatosisData.State.Inside))
        {
            deathData.ChangeState(owner.SpiralLevel > 0 ? ThanatosisData.State.Safe : ThanatosisData.State.Drowning);
        }
        else if (deathData.IsState(ThanatosisData.State.Safe))
        {
            if (deathData.timeInState >= deathData.MaxAvailableSafeTime)
            {
                deathData.ChangeState(ThanatosisData.State.Drowning);
            }
        }
        else if (deathData.IsState(ThanatosisData.State.Drowning))
        {
            owner.player.airInLungs = Mathf.Clamp(owner.player.airInLungs, 0.1f, 0.25f);
            if (deathData.timeInState >= ThanatosisData.insideTimeAvailable)
            {
                deathData.ChangeState(owner.SpiralLevel > 0 ? ThanatosisData.State.Persisting : ThanatosisData.State.Drowned);
            }
        }
        else if (deathData.IsState(ThanatosisData.State.Persisting))
        {
            owner.SpiralLevel--;
            deathData.Toggle(owner.player, true);
        }
        else if (deathData.IsState(ThanatosisData.State.Drowned))
        {
            owner.player.Die();
        }
    }
    private void Outside()
    {
        if (deathData.IsState(ThanatosisData.State.Exiting))
        {
            deathData.ChangeState(ThanatosisData.State.Outside);
        }
    }
}
