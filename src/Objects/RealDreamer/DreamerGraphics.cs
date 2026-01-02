using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PitchBlack;

public class DreamerGraphics : PBEntity.GraphicsModule
{
    public DreamerEntity Dreamer => owner as DreamerEntity;

    #region Sprite Gets
    public int LightSprite
    {
        get
        {
            return 0;
        }
    }

    public int BodyMeshSprite
    {
        get
        {
            return behindBodySprites;
        }
    }

    public int ButtockSprite(int side)
    {
        return behindBodySprites + 1 + side;
    }
    public int ThightSprite(int side)
    {
        return behindBodySprites + 3 + side;
    }

    public int LowerLegSprite(int side)
    {
        return behindBodySprites + 5 + side;
    }

    public int NeckConnectorSprite
    {
        get
        {
            return behindBodySprites + 7;
        }
    }

    public int HeadMeshSprite
    {
        get
        {
            return behindBodySprites + 8;
        }
    }

    public int DistortionSprite
    {
        get
        {
            return behindBodySprites + 9;
        }
    }
    #endregion

    private readonly int totalSprites;
    private readonly int behindBodySprites;
    private readonly int totalStaticSprites = 10;
    private float sinBob;

    private float flipProg;
    private float flipSpeed;
    private float flip;
    private float flipFrom;
    private float flipTo;
    private float defaultFlip;

    private float scale;
    private float targetScale = 0.5f;
    private float distortionScaleFac;
    private float lightSpriteScale = 0.3f;
    private int spineSegments = 11;
    private int snoutSegments = 2;
    private int spineBendPoint = 7;
    private int thighSegments = 7;
    private int lowerLegSegments = 17;
    private float airResistance = 0.6f;

    public Color primaryColor = Colors.VisibleWhite;
    public Color accentColor = Colors.Rose;
    public Color glowColor = Colors.ComplementaryRose;

    public DreamerGraphics(DreamerEntity owner) : base(owner)
    {
        this.owner = owner;
    }
}
