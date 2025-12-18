using HUD;
using RWCustom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PitchBlack;

public class DreamerConversation : Conversation
{
    private Dreamer dreamer;
    private Counter timeSinceLastSound = new Counter(120, 0, true);
    private string s = "";
    private int switchSpeakerEvents = 0;
    private bool wasVoiceSwitched;
    private List<string> switchSpeakerStrings = new List<string>();
    public bool SpeakerSwitched => switchSpeakerEvents > 0;
    public string CurrentTextEvent => (events[0] as TextEvent).text;

    public DreamerConversation(Dreamer dreamer, ID id, DialogBox dialogBox) : base(dreamer, id, dialogBox)
    {
        this.dreamer = dreamer;
        AddEvents();
    }

    public override void Update()
    {
        timeSinceLastSound.Tick();

        // Log current event int here
        int eventsCount = events.Count;

        base.Update();

        // Check for switch event strings to then add to the amount of events
        for (int i = 0; i < switchSpeakerStrings.Count; i++)
        {
            if (switchSpeakerStrings.Contains(CurrentTextEvent))
            {
                switchSpeakerEvents++;
                wasVoiceSwitched = true;
                break;
            }
        }

        if (ConditionToSpeak() && eventsCount != events.Count)
        {
            // Accomodate for sound needing to play when voice is switched, to distinct text
            if (timeSinceLastSound > 1 && SpeakerSwitched)
            {
                switchSpeakerEvents--;
                RemoveSwitchString();
                // Forcefully distinct the first event
                if (switchSpeakerEvents == 1)
                {
                    timeSinceLastSound.Finish();
                }
            }
            // Accomodate because Dreamer's voice might not play because the voice is on cooldown
            if (!SpeakerSwitched && wasVoiceSwitched)
            {
                timeSinceLastSound.Finish();
                wasVoiceSwitched = false;
            }
            // Now: we check to play a sound
            if (timeSinceLastSound.isFinished && CurrentTextEvent != "...")
            {
                Speak();
                timeSinceLastSound.Reset();
            }
        }
    }

    // Call in AddEvents() to add a switch speaker event
    public void AddSwitchString(string s)
    {
        if (!switchSpeakerStrings.Contains(s))
        {
            switchSpeakerStrings.Add(s);
        }
    }

    // This must be called specifically in Update(), when strings are intended to be cleared correctly!
    public void RemoveSwitchString()
    {
        if (switchSpeakerStrings.Contains(CurrentTextEvent))
        {
            switchSpeakerStrings.Remove(CurrentTextEvent);
        }
    }

    #region Speaking
    public bool ConditionToSpeak()
    {
        // Conter done, didnt already speak, events greater than 0, text event checks, lastEvent check for current event
        if (events.Count > 0 && events[0] is TextEvent)
        {
            return true;
        }
        return false;
    }

    public void Speak()
    {
        // Assign default values to then be overriden
        float volume = Random.Range(0.85f, 1f);
        float pitch = 1;
        if (SpeakerSwitched)
        {
            volume = Random.Range(0.75f, 1f);
            pitch = Random.Range(0.80f, 1.25f);
        }
        dreamer.room.PlaySound(VoiceID(), 0.5f, volume, pitch);
    }

    public SoundID VoiceID()
    {
        if (SpeakerSwitched)
        {
            if (BeaconSaveData.GetMaxSpiralLevel(dreamer.room.game.GetStorySession.saveState) > 4f)
            {
                return Enums.SoundID.Beacon_Hybrid_Voice;
            }
            return Enums.SoundID.Beacon_Voice;
        }
        return Enums.SoundID.Dreamer_Voice;
    }
    #endregion

    public override void AddEvents()
    {
        if (id == Enums.ConversationID.Dreamer_PH)
        {
            s = "...";
            events.Add(new TextEvent(this, 0, s, 40));
        }
        if (id == Enums.ConversationID.Dreamer_1)
        {
            s = "...";
            events.Add(new TextEvent(this, 0, s, 40));
            s = "I see... I see...";
            events.Add(new TextEvent(this, 0, s, 20));
            s = "Indeed, you are somewhere.";
            events.Add(new TextEvent(this, 0, s, 20));
            s = "Here, somewhere?";
            events.Add(new TextEvent(this, 0, s, 20));
            s = "...";
            events.Add(new TextEvent(this, 0, s, 40));
            s = "Do you feel lost?";
            events.Add(new TextEvent(this, 0, s, 10));
            s = "Sound asleep...";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "Yet wide awake!";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "But the dream lingers...";
            events.Add(new TextEvent(this, 0, s, 10));
            s = "I'm seeing bright towers, concrete with holes";
            events.Add(new TextEvent(this, 0, s, 10));
            s = "The dream lingers, a will wakes.";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "A will to percieve and act";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "A dream of a web. A tangle, strangled!";
            events.Add(new TextEvent(this, 0, s, 10));
            s = "A strangle in selves";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "Forever Somewhere";
            events.Add(new TextEvent(this, 0, s, 10));
            s = "Somewhere Else.";
            events.Add(new TextEvent(this, 0, s, 0));
            return;
        }
        if (id == Enums.ConversationID.Dreamer_2)
        {
            s = "The little sleeper follows the tide once more";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "Drifting smoothly to someplace forever familiar";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "...";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "You, aquainted with this placewhere";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "Eager to squirm your way back in?";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "I suppose...";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "After this where's conception";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "There were holes left, cracks, windows...";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "Assuming you will find more...";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "These won't be the only visits?";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "...";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "Yes, there will be more";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "From the web you'll find more trailing threads";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "You'll certainly find a way...";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "So, I'll be waiting here in my where";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "For another when and another you";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "Another visit.";
            events.Add(new TextEvent(this, 0, s, 0));
        }

        //<Dreamer_3>
    }
}
