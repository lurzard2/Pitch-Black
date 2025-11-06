using HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PitchBlack;

public class DreamerConversation : Conversation
{
    public SoundID Voiceline { get; private set; }

    public DreamerConversation(IOwnAConversation interfaceOwner, ID id, DialogBox dialogBox) : base(interfaceOwner, id, dialogBox)
    {
        Voiceline = GetVoiceLine(id);
        AddEvents();
    }

    private static SoundID GetVoiceLine(ID id)
    {
        return null;
    }

    public override void AddEvents()
    {
        string s;
        if (id == Enums.ConversationID.Dreamer_1)
        {
            s = "I see... I see...";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "Indeed, you are somewhere.";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "Here, somewhere?";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "...";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "Do you feel lost?";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "Sound asleep...";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "Yet wide awake!";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "But the dream lingers...";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "I'm seeing bright towers, concrete with holes";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "The dream lingers, a will wakes.";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "A will to percieve and act";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "A dream of a web. A tangle, strangled!";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "A strangle in selves";
            events.Add(new TextEvent(this, 0, s, 0));
            s = "Forever Somewhere";
            events.Add(new TextEvent(this, 0, s, 10));
            s = "Somewhere Else.";
            events.Add(new TextEvent(this, 0, s, 0));
            return;
        }
    }
}
