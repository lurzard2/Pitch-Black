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
    private Dreamer owner;
    private Counter timeSinceLastSound = new Counter(120, 0, true);
    private string s = "";
    private int switchSpeakerEvents = 0;
    private bool wasVoiceSwitched;
    private List<string> switchSpeakerStrings = new List<string>();
    public bool SpeakerSwitched => switchSpeakerEvents > 0;
    public string CurrentTextEvent => (events[0] as TextEvent).text;

    public DreamerConversation(Dreamer owner, ID id, DialogBox dialogBox) : base(owner, id, dialogBox)
    {
        this.owner = owner;
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
        owner.room.PlaySound(VoiceID(), 0.5f, volume, pitch);
    }

    public SoundID VoiceID()
    {
        if (SpeakerSwitched)
        {
            if (BeaconSaveData.GetMaxSpiralLevel(owner.room.game.GetStorySession.saveState) > 4f)
            {
                return Enums.SoundID.Beacon_Hybrid_Voice;
            }
            return Enums.SoundID.Beacon_Voice;
        }
        return Enums.SoundID.Dreamer_Voice;
    }
    #endregion

    private DialogueEvent Text(string text, int linger = 0, int initial = 0)
    {
        TextEvent t = new(this, initial, text, linger);
        return t;
    }

    private List<DialogueEvent> GetEvents()
    {
        List<DialogueEvent> e = [];

        if (id == Enums.ConversationID.Dreamer_PH)
        {
            e.Add(Text("...", 40));
        }
        else if (id == Enums.ConversationID.Dreamer_1)
        {
            e.Add(Text("...", 40));
            e.Add(Text("I see... I see...", 20));
            e.Add(Text("Indeed, you must be somewhere."));
            e.Add(Text("But is here anywhere at all?"));
            e.Add(Text("...", 35));
            e.Add(Text("Do you feel... lost?", 20));
            e.Add(Text("Stranded here, and helpless against the cold concrete.", 10));
            e.Add(Text("In such a deep sleep...", 20));
            e.Add(Text("Yet wide awake, bursting with life!"));
            e.Add(Text("But the dream still lingers.", 10));
            e.Add(Text("..."));
            e.Add(Text("From torpor, eternal stillness, a will awakens."));
            e.Add(Text("To perceive and act... To change and be changed, clinging to existence.", 20));
            e.Add(Text("A vestige appears to me, to you, of a web. A tangle, strangled by threads abound!"));
            e.Add(Text("A binding of and betwixt many selves.", 20));
            e.Add(Text("Forever somewhere, yet always somewhere else."));
        }
        else if (id == Enums.ConversationID.Dreamer_2)
        {
            e.Add(Text("The little sleeper follows the tide once more."));
            e.Add(Text("Drifting smoothly against tribulation to someplace forever familiar..."));
            e.Add(Text("..."));
            e.Add(Text("You know this place, don't you?"));
            e.Add(Text("So eager to come back again, to know more."));
            e.Add(Text("Last time, on your way out, you left a presence behind..."));
            e.Add(Text("..."));
            e.Add(Text("I suppose..."));
            e.Add(Text("After the conception of this scape..."));
            e.Add(Text("Left in its wake were permanent voids, cracks, and windows."));
            e.Add(Text("Assuming you will find more rifts just big enough to crawl through..."));
            e.Add(Text("These won't be the only visits?"));
            e.Add(Text("..."));
            e.Add(Text("Yes, there will be more."));
            e.Add(Text("From the tangled web, you'll find more threads astray..."));
            e.Add(Text("To chase back up to my little island in a sea of nothing."));
            e.Add(Text("You'll certainly find a way back from there."));
            e.Add(Text("So, I'll be waiting in my where."));
            e.Add(Text("No matter how long it takes, for another when and another you."));
        }
        else if (id == Enums.ConversationID.Dreamer_3)
        {
            // later
        }

        return e;
    }

    private void SetEvents(List<DialogueEvent> events)
    {
        for (int i = 0; i < events.Count; i++)
        {
            this.events.Add(events[i]);
        }
    }

    public override void AddEvents()
    {
        List<DialogueEvent> l = GetEvents();
        SetEvents(l);
        return;

        #region old
        //if (id == Enums.ConversationID.Dreamer_PH)
        //{
        //    s = "...";
        //    l.Add(new TextEvent(this, 0, s, 40));
        //}

        //if (id == Enums.ConversationID.Dreamer_1)
        //{
        //    s = "...";
        //    l.Add(new TextEvent(this, 0, s, 40));
        //    s = "I see... I see...";
        //    l.Add(new TextEvent(this, 0, s, 20));
        //    s = "Indeed, you are somewhere.";
        //    l.Add(new TextEvent(this, 0, s, 20));
        //    s = "Here, somewhere?";
        //    l.Add(new TextEvent(this, 0, s, 20));
        //    s = "...";
        //    l.Add(new TextEvent(this, 0, s, 40));
        //    s = "Do you feel lost?";
        //    l.Add(new TextEvent(this, 0, s, 10));
        //    s = "Sound asleep...";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "Yet wide awake!";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "But the dream lingers...";
        //    l.Add(new TextEvent(this, 0, s, 10));
        //    s = "I'm seeing bright towers, concrete with holes";
        //    l.Add(new TextEvent(this, 0, s, 10));
        //    s = "The dream lingers, a will wakes.";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "A will to percieve and act";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "A dream of a web. A tangle, strangled!";
        //    l.Add(new TextEvent(this, 0, s, 10));
        //    s = "A strangle in selves";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "Forever Somewhere";
        //    l.Add(new TextEvent(this, 0, s, 10));
        //    s = "Somewhere Else.";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    return;
        //}

        //if (id == Enums.ConversationID.Dreamer_2)
        //{
        //    s = "The little sleeper follows the tide once more";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "Drifting smoothly to someplace forever familiar";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "...";
        //    l.Add(new TextEvent(this, 0, s, 40));
        //    s = "You, aquainted with this placewhere";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "Eager to squirm your way back in?";
        //    l.Add(new TextEvent(this, 0, s, 10));
        //    s = "I suppose...";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "After this where's conception";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "There were holes left, cracks, windows...";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "Assuming you will find more...";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "These won't be the only visits?";
        //    l.Add(new TextEvent(this, 0, s, 20));
        //    s = "...";
        //    l.Add(new TextEvent(this, 0, s, 40));
        //    s = "Yes, there will be more";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "From the web you'll find more trailing threads";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "You'll certainly find a way...";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "So, I'll be waiting here in my where";
        //    l.Add(new TextEvent(this, 0, s, 10));
        //    s = "For another when and another you";
        //    l.Add(new TextEvent(this, 0, s, 10));
        //    s = "Another visit.";
        //    l.Add(new TextEvent(this, 0, s, 10));
        //    return;
        //}

        //if (id == Enums.ConversationID.Dreamer_3)
        //{
        //    s = "...";
        //    l.Add(new TextEvent(this, 0, s, 40));
        //    s = "Ah... You see Another?";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "You and these...";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "Now that you're here, is that what its like to be spun like a thread?";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "Spun and caught in twine, in a web?...";
        //    l.Add(new TextEvent(this, 0, s, 10));
        //    s = "...Do you hear me?";
        //    l.Add(new TextEvent(this, 0, s, 10));
        //    s = "Do you understand that?";
        //    l.Add(new TextEvent(this, 0, s, 20));
        //    s = "Can you answer me?";
        //    l.Add(new TextEvent(this, 0, s, 30));
        //    s = "...";
        //    l.Add(new TextEvent(this, 0, s, 50));
        //    s = "Hmm...";
        //    l.Add(new TextEvent(this, 0, s, 20));
        //    s = "Still tied down to the scope of the body, I see...";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "I am too";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "...";
        //    l.Add(new TextEvent(this, 0, s, 10));
        //    s = "But I can see your reflections, writhing";
        //    l.Add(new TextEvent(this, 0, s, 10));
        //    s = "I can see where the window is";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "And I can feel the twine tightening...";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "You're dying";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "And the suffocation is getting worse";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "I'm afraid there's not much time left for you there, friend";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "If you can bear a responsibility for me";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "When I let you back out of this limbo state";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "Find shelter and die peacefully";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //    s = "Then come and find me";
        //    l.Add(new TextEvent(this, 0, s, 0));
        //}
        #endregion
    }
}
