using DevInterface;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Watcher;

namespace PitchBlack;
public class DreamerSpotRepresentation : PlacedObjectRepresentation
{
    public DreamerSpotRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, pObj.type.ToString())
    {
        controlPanel = new DreamerPanel(owner, this, new Vector2(0f, 100f));
        subNodes.Add(controlPanel);
        controlPanel.pos = (pObj.data as DreamerData).panelPos;
    }

    private DreamerPanel controlPanel;
}

public class DreamerPanel : Panel, IDevUISignals
{
    // Token: 0x170006D3 RID: 1747
    // (get) Token: 0x0600272A RID: 10026 RVA: 0x002F577A File Offset: 0x002F397A
    public DreamerData Data
    {
        get
        {
            return (parentNode as DreamerSpotRepresentation).pObj.data as DreamerData;
        }
    }

    // Token: 0x0600272B RID: 10027 RVA: 0x002F5798 File Offset: 0x002F3998
    public DreamerPanel(DevUI owner, DevUINode parentNode, Vector2 pos) : base(owner, "Dreamer_Panel", parentNode, pos, new Vector2(250f, 125f), "Dreamer Spot")
    {
        this.subNodes.Add(new Button(owner, "Select_Timeline_Panel_Button", this, new Vector2(5f, 105f), 240f, "Timeline : "));
        this.subNodes.Add(new Button(owner, "Select_Region_Panel_Button", this, new Vector2(5f, 85f), 240f, "Region : "));
        this.subNodes.Add(new Button(owner, "Select_Room_Panel_Button", this, new Vector2(5f, 65f), 240f, "Room : "));
        this.subNodes.Add(new Button(owner, "Select_Position_Panel_Button", this, new Vector2(5f, 45f), 240f, "Position : "));
        this.subNodes.Add(new SpawnIdentifierController(owner, "SpawnIdentifier", this, new Vector2(5f, 25f), "Spawn ID : "));
        this.rippleWarpButton = new Button(owner, "ripple_warp_button", this, new Vector2(5f, 5f), 240f, "Ripple Warp: " + this.Data.rippleWarp.ToString());
        this.subNodes.Add(this.rippleWarpButton);
        this.RefreshLabels();
    }

    // Token: 0x0600272C RID: 10028 RVA: 0x002F590C File Offset: 0x002F3B0C
    public override void Update()
    {
        base.Update();
        if (this.roomPositionPanel != null && this.roomPositionPanel.tileClicked)
        {
            this.Data.destPos = new Vector2?(new Vector2((float)this.roomPositionPanel.tileClickedX * 20f + 10f, (float)this.roomPositionPanel.tileClickedY * 20f + 10f));
            this.HideRoomPositionPanel();
            this.RefreshLabels();
        }
    }

    // Token: 0x0600272D RID: 10029 RVA: 0x002F5985 File Offset: 0x002F3B85
    public override void Refresh()
    {
        base.Refresh();
        this.rippleWarpButton.Text = string.Format("Ripple Warp: {0}", this.Data.rippleWarp);
    }

    // Token: 0x0600272E RID: 10030 RVA: 0x002F59B4 File Offset: 0x002F3BB4
    public void Signal(DevUISignalType type, DevUINode sender, string message)
    {
        if (sender.IDstring == "ripple_warp_button")
        {
            this.Data.rippleWarp = !this.Data.rippleWarp;
        }
        if (sender.IDstring == "Select_Timeline_Panel_Button")
        {
            if (this.timelineSelectPanel != null)
            {
                this.HideTimelineSelectPanel();
                return;
            }
            this.HideRegionSelectPanel();
            this.HideRoomSelectPanel();
            this.HideRoomPositionPanel();
            string[] decalNames = ExtEnum<SlugcatStats.Timeline>.values.entries.ToArray();
            this.timelineSelectPanel = new CustomDecalRepresentation.SelectDecalPanel(this.owner, this, new Vector2(200f, 15f) - this.absPos, decalNames);
            this.subNodes.Add(this.timelineSelectPanel);
            return;
        }
        else
        {
            if (!(sender.IDstring == "Select_Region_Panel_Button"))
            {
                if (sender.IDstring == "Select_Room_Panel_Button")
                {
                    if (this.roomSelectPanel != null || this.Data.RegionString == null)
                    {
                        this.HideRoomSelectPanel();
                        return;
                    }
                    this.HideRegionSelectPanel();
                    this.HideTimelineSelectPanel();
                    this.HideRoomPositionPanel();
                    string regionString = this.Data.RegionString;
                    if (regionString != null)
                    {
                        List<string> list = new List<string>();
                        string[] array = AssetManager.ListDirectory("world/" + regionString + "-rooms", false, false, false);
                        for (int i = 0; i < array.Length; i++)
                        {
                            string text = Path.GetFileName(array[i]).ToLowerInvariant();
                            if (text.EndsWith(".txt") && !text.Contains("_settings"))
                            {
                                list.Add(Path.GetFileNameWithoutExtension(array[i]));
                            }
                        }
                        this.roomSelectPanel = new CustomDecalRepresentation.SelectDecalPanel(this.owner, this, new Vector2(200f, 15f) - this.absPos, list.ToArray());
                        this.subNodes.Add(this.roomSelectPanel);
                        return;
                    }
                }
                else if (sender.IDstring == "Select_Position_Panel_Button")
                {
                    if (this.roomPositionPanel != null || this.Data.destTimeline == null || this.Data.destRoom == null)
                    {
                        this.HideRoomPositionPanel();
                        return;
                    }
                    this.HideTimelineSelectPanel();
                    this.HideRoomSelectPanel();
                    this.HideRegionSelectPanel();
                    this.roomPositionPanel = new RoomPositionPanel(this.owner, this, new Vector2(200f, 15f) - this.absPos, this.Data.destTimeline, this.Data.destRoom);
                    this.subNodes.Add(this.roomPositionPanel);
                    return;
                }
                else if (sender.IDstring == "BackPage99289..?/~")
                {
                    if (sender.parentNode == this.regionSelectPanel)
                    {
                        this.regionSelectPanel.PrevPage();
                        return;
                    }
                    if (sender.parentNode == this.roomSelectPanel)
                    {
                        this.roomSelectPanel.PrevPage();
                        return;
                    }
                    if (sender.parentNode == this.timelineSelectPanel)
                    {
                        this.timelineSelectPanel.PrevPage();
                        return;
                    }
                }
                else if (sender.IDstring == "NextPage99289..?/~")
                {
                    if (sender.parentNode == this.regionSelectPanel)
                    {
                        this.regionSelectPanel.NextPage();
                        return;
                    }
                    if (sender.parentNode == this.roomSelectPanel)
                    {
                        this.roomSelectPanel.NextPage();
                        return;
                    }
                    if (sender.parentNode == this.timelineSelectPanel)
                    {
                        this.timelineSelectPanel.NextPage();
                        return;
                    }
                }
                else
                {
                    if (sender.parentNode == this.regionSelectPanel)
                    {
                        this.Data.RegionString = sender.IDstring;
                        this.Data.destRoom = null;
                        this.Data.destPos = null;
                        this.HideRegionSelectPanel();
                    }
                    else if (sender.parentNode == this.roomSelectPanel)
                    {
                        this.Data.destRoom = sender.IDstring;
                        this.Data.destPos = null;
                        this.HideRoomSelectPanel();
                    }
                    else if (sender.parentNode == this.timelineSelectPanel)
                    {
                        this.Data.destTimeline = new SlugcatStats.Timeline(sender.IDstring, false);
                        this.Data.RegionString = null;
                        this.Data.destRoom = null;
                        this.Data.destPos = null;
                        this.HideTimelineSelectPanel();
                    }
                    this.RefreshLabels();
                }
                return;
            }
            if (this.regionSelectPanel != null || this.Data.destTimeline == null)
            {
                this.HideRegionSelectPanel();
                return;
            }
            this.HideTimelineSelectPanel();
            this.HideRoomSelectPanel();
            this.HideRoomPositionPanel();
            string[] decalNames2 = Region.GetFullRegionOrder(this.Data.destTimeline).ToArray();
            this.regionSelectPanel = new CustomDecalRepresentation.SelectDecalPanel(this.owner, this, new Vector2(200f, 15f) - this.absPos, decalNames2);
            this.subNodes.Add(this.regionSelectPanel);
            return;
        }
    }

    // Token: 0x0600272F RID: 10031 RVA: 0x002F5E60 File Offset: 0x002F4060
    public void RefreshLabels()
    {
        string text = this.Data.RegionString;
        if (text == null)
        {
            text = "UNDEFINED";
        }
        string text2 = this.Data.destRoom;
        if (text2 == null)
        {
            text2 = "UNDEFINED";
        }
        string str = (this.Data.destTimeline == null) ? "UNDEFINED" : this.Data.destTimeline.ToString();
        string text3;
        if (this.Data.destPos == null)
        {
            text3 = "UNDEFINED";
        }
        else
        {
            string[] array = new string[5];
            array[0] = "(";
            int num = 1;
            Vector2 value = this.Data.destPos.Value;
            array[num] = value.x.ToString();
            array[2] = ", ";
            int num2 = 3;
            value = this.Data.destPos.Value;
            array[num2] = value.y.ToString();
            array[4] = ")";
            text3 = string.Concat(array);
        }
        string str2 = text3;
        for (int i = 0; i < this.subNodes.Count; i++)
        {
            if (this.subNodes[i].IDstring == "Select_Region_Panel_Button")
            {
                (this.subNodes[i] as Button).Text = "Region : " + text;
            }
            else if (this.subNodes[i].IDstring == "Select_Room_Panel_Button")
            {
                (this.subNodes[i] as Button).Text = "Room : " + text2;
            }
            else if (this.subNodes[i].IDstring == "Select_Timeline_Panel_Button")
            {
                (this.subNodes[i] as Button).Text = "Timeline : " + str;
            }
            else if (this.subNodes[i].IDstring == "Select_Position_Panel_Button")
            {
                (this.subNodes[i] as Button).Text = "Position : " + str2;
            }
        }
    }

    // Token: 0x06002730 RID: 10032 RVA: 0x002F606D File Offset: 0x002F426D
    public void HideRegionSelectPanel()
    {
        if (this.regionSelectPanel != null)
        {
            this.subNodes.Remove(this.regionSelectPanel);
            this.regionSelectPanel.ClearSprites();
            this.regionSelectPanel = null;
        }
    }

    // Token: 0x06002731 RID: 10033 RVA: 0x002F609B File Offset: 0x002F429B
    public void HideRoomSelectPanel()
    {
        if (this.roomSelectPanel != null)
        {
            this.subNodes.Remove(this.roomSelectPanel);
            this.roomSelectPanel.ClearSprites();
            this.roomSelectPanel = null;
        }
    }

    // Token: 0x06002732 RID: 10034 RVA: 0x002F60C9 File Offset: 0x002F42C9
    public void HideTimelineSelectPanel()
    {
        if (this.timelineSelectPanel != null)
        {
            this.subNodes.Remove(this.timelineSelectPanel);
            this.timelineSelectPanel.ClearSprites();
            this.timelineSelectPanel = null;
        }
    }

    // Token: 0x06002733 RID: 10035 RVA: 0x002F60F7 File Offset: 0x002F42F7
    public void HideRoomPositionPanel()
    {
        if (this.roomPositionPanel != null)
        {
            this.subNodes.Remove(this.roomPositionPanel);
            this.roomPositionPanel.ClearSprites();
            this.roomPositionPanel = null;
        }
    }

    // Token: 0x04002444 RID: 9284
    public CustomDecalRepresentation.SelectDecalPanel regionSelectPanel;

    // Token: 0x04002445 RID: 9285
    public CustomDecalRepresentation.SelectDecalPanel roomSelectPanel;

    // Token: 0x04002446 RID: 9286
    public CustomDecalRepresentation.SelectDecalPanel timelineSelectPanel;

    // Token: 0x04002447 RID: 9287
    public RoomPositionPanel roomPositionPanel;

    // Token: 0x04002448 RID: 9288
    public Button rippleWarpButton;

    // Token: 0x020009F0 RID: 2544
    public class SpawnIdentifierController : IntegerControl
    {
        // Token: 0x0600535D RID: 21341 RVA: 0x00593A68 File Offset: 0x00591C68
        public SpawnIdentifierController(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title)
        {
        }

        // Token: 0x0600535E RID: 21342 RVA: 0x00593A77 File Offset: 0x00591C77
        public override void Refresh()
        {
            base.NumberLabelText = (this.parentNode as DreamerPanel).Data.spawnIdentifier.ToString();
            base.Refresh();
        }

        // Token: 0x0600535F RID: 21343 RVA: 0x00593A9F File Offset: 0x00591C9F
        public override void Increment(int change)
        {
            (this.parentNode as DreamerPanel).Data.spawnIdentifier = Mathf.Max(0, (this.parentNode as DreamerPanel).Data.spawnIdentifier + change);
            this.Refresh();
        }
    }
}