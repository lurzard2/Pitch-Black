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
            if (timeSinceLastSound > 1 && switchVoice)
            {
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
        float volRange = Random.Range(0.65f, 1f);
        float pitchRange = Random.Range(0.90f, 1.20f);
        if (switchVoice)
        {
            volRange = Random.Range(0.25f, 0.45f);
            pitchRange = Random.Range(0.75f, 1.35f);
        }
        dreamer.room.PlaySound(VoiceID(), Random.Range(0f, 1f), volRange, pitchRange);
    }

    public SoundID VoiceID()
    {
        if (switchVoice)
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
            switchVoice = true;
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
            switchVoice = false;
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
    private bool switchVoice;
}
