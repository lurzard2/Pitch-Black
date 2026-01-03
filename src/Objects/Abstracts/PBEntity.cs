using System;
using System.Collections.Generic;
using System.IO;
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
        else
        {
            if (visibleEntity != null)
            {
                visibleEntity.Destroy();
            }
            Destroy();
        }
    }

    public virtual void OnDestroy()
    {
        if (deleteMe)
        {
            visibleEntity.slatedForDeletetion = true;
            slatedForDeletetion = true;
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

        public void LoadElement(string elementName)
        {
            if (Futile.atlasManager.GetAtlasWithName(elementName) != null)
            {
                return;
            }
            string str = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar.ToString() + elementName + ".png");
            Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            AssetManager.SafeWWWLoadTexture(ref texture, "file:///" + str, false, true);
            Futile.atlasManager.LoadAtlasFromTexture(elementName, texture, false);
        }
    }

    public abstract class BehaviorModule : Conversation.IOwnAConversation
    {
        public PBEntity owner;
        public BehaviorConversation conversation;
        public bool addedEvents = false;
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
                // Otherwise duplicates, due to it running once per instance
                if (!owner.addedEvents)
                {
                    List<DialogueEvent> l = [];
                    GetEvents(l);
                    SetEvents(l);
                    owner.addedEvents = true;
                }
            }
            public virtual void GetEvents(List<DialogueEvent> l)
            {
            }
            public void SetEvents(List<DialogueEvent> events)
            {
                if (events != null)
                {
                    for (int i = 0; i < events.Count; i++)
                    {
                        this.events.Add(events[i]);
                    }
                }
            }
        }
    }
}
