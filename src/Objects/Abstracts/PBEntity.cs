using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PitchBlack;

// Template class for making "encounterable entities"
// - PBEntity: Class object which hosts every module
// - GraphicsModule: CosmeticSprite
// - BehaviorModule: Update behavior and conversation
// - BehaviorModule.BehaviorConversation: Dialog

public abstract class PBEntity : UpdatableAndDeletable
{
    // visibleEntity must be added to the room within the entity class
    public GraphicsModule visibleEntity;
    public BehaviorModule behaviorModule;

    public bool flaggedAsReadyForDeletion = false;
    public bool deleteMe = false;

    public PBEntity(Room room, PlacedObject placedObject = null)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        if (flaggedAsReadyForDeletion)
        {
            DestroyUpdate();
        }
        base.Update(eu);
        if (behaviorModule != null)
        {
            behaviorModule.Update();
            if (behaviorModule.conversation != null)
            {
                behaviorModule.conversation.Update();
            }
        }
    }

    public void DestroyUpdate()
    {
        if (!slatedForDeletetion)
        {
            OnDestroy();
        }
    }

    public virtual void OnDestroy()
    {
        if (deleteMe)
        {
            slatedForDeletetion = true;
            visibleEntity.slatedForDeletetion = true;
        }
    }

    // Making this a CosmeticSprite means its: a UAD and has IDrawable AND IRunDuringDialog which are all super important!
    public abstract class GraphicsModule : CosmeticSprite
    {
        public PBEntity owner;

        public GraphicsModule(PBEntity owner)
        {
            this.owner = owner;
        }

        public abstract class Part
        {
            public PBEntity owner;
            public Part(PBEntity owner)
            {
                this.owner = owner;
            }
        }
    }

    public abstract class BehaviorModule : Conversation.IOwnAConversation
    {
        public PBEntity owner;
        public BehaviorConversation conversation;

        public string ReplaceParts(string s) => s;

        public void SpecialEvent(string eventName) { }

        public BehaviorModule(PBEntity owner)
        {
            this.owner = owner;
        }

        public virtual void Update()
        {
        }

        public abstract class BehaviorConversation : Conversation
        {
            public BehaviorModule owner;

            public BehaviorConversation(BehaviorModule owner, ID id, HUD.DialogBox dialogBox) : base(owner, id, dialogBox)
            {
                this.owner = owner;
                AddEvents();
            }

            public DialogueEvent Text(string text, int linger = 0, int initial = 0)
            {
                TextEvent t = new(this, initial, text, linger);
                return t;
            }

            public override void AddEvents()
            {
                List<DialogueEvent> l = GetEvents();
                SetEvents(l);
                return;
            }
            public virtual List<DialogueEvent> GetEvents()
            {
                return null;
            }
            public void SetEvents(List<DialogueEvent> events)
            {
                for (int i = 0; i < events.Count; i++)
                {
                    events.Add(events[i]);
                }
            }
        }
    }
}
