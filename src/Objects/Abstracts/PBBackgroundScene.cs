using System.Collections.Generic;
using UnityEngine;

namespace PitchBlack;

public abstract class PBBackgroundScene : BackgroundScene
{
    public Simple2DBackgroundIllustration fullScreenSky;
    public float startAltitude = 1f;
    public float endAltitude = 31400f;
    public Vector2 sceneOrigin;

    public Color atmosphereColor;
    public Color multiplyColor;

    public float cloudsStartDepth = 5f;
    public float cloudsEndDepth = 40f;
    public float distantCloudsEndDepth = 200f;

    public List<string> loadedGraphics = [];

    public PBBackgroundScene(Room room) : base(room)
    {
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
    }

    public void LoadGraphics()
    {
        for (int i = 0; i < loadedGraphics.Count; i++)
        {
            LoadGraphic(loadedGraphics[i], false, false);
        }
    }
}
