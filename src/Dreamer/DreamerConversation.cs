using HUD;
using RWCustom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace PitchBlack;

public class DreamerConversation : Conversation
{
    public bool VoiceSwitched
    {
        get
        {
            return switchVoiceEvents > 0;
        }
    }

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

        if (ConditionToSpeak() && eventsCount != events.Count)
        {
            if (timeSinceLastSound > 1 && VoiceSwitched)
            {
                switchVoiceEvents--;
                timeSinceLastSound.Finish();
            }
            if (timeSinceLastSound.isFinished && (events[0] as TextEvent).text != "...")
            {
                Speak();
                timeSinceLastSound.Reset();
            }
        }
    }

    public bool ConditionToSpeak()
    {
        // Conter done, didnt already speak, events greater than 0, text event checks, lastEvent check for current event
        if (events.Count > 0 && events[0] is TextEvent textEvent)
        {
            return true;
        }
        return false;
    }

    public void Speak()
    {
        // Assign default values to then be overriden
        float volRange = Random.Range(0.85f, 1f);
        float pitchRange = Random.Range(0.90f, 1.20f);
        if (VoiceSwitched)
        {
            volRange = Random.Range(0.75f, 1f);
            pitchRange = Random.Range(0.80f, 1.25f);
        }
        dreamer.room.PlaySound(VoiceID(), Random.Range(0f, 1f), volRange, pitchRange);
    }

    public SoundID VoiceID()
    {
        if (VoiceSwitched)
        {
            if (BeaconSaveData.GetMaxSpiralLevel(dreamer.room.game.GetStorySession.saveState) > 4f)
            {
                return Enums.SoundID.Beacon_Hybrid_Voice;
            }
            return Enums.SoundID.Beacon_Voice;
        }
        return Enums.SoundID.Dreamer_Voice;
    }

    public override void AddEvents()
    {
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

        //<Dreamer_2>

        //<Dreamer_3>
    }

    private Dreamer dreamer;
    private Counter timeSinceLastSound = new Counter(120, 0, true);
    private string s = "";
    private int switchVoiceEvents = 0;
}
