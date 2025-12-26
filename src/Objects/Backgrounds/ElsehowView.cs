using System.Collections.Generic;
using UnityEngine;
using static PitchBlack.Plugin;

namespace PitchBlack;

// Uses borrowed implementation from AboveCloudsView and AnicentUrbanView.
public class ElsehowView : PBBackgroundScene
{

    public Simple2DBackgroundIllustration sky;
    public ElseFog fog;
    public List<ElseCloud> elseClouds;
    public RoomSettings.RoomEffect effect;

    public ElsehowView(Room room, RoomSettings.RoomEffect effect) : base(room)
    {
        this.effect = effect;

        string s = "ElsehowView:";

        Random.State state = Random.state;
        Random.InitState(2);

        startAltitude = -300f;
        endAltitude = 31400f;
        cloudsStartDepth = 5f;
        cloudsEndDepth = 40f;
        distantCloudsEndDepth = 1000f;
        atmosphereColor = new Color(0.274f, 0.6f, 0.819f);

        sceneOrigo = RoomToWorldPos(room.abstractRoom.size.ToVector2() * 5f);
        Shader.SetGlobalVector(RainWorld.ShadPropSceneOrigoPosition, sceneOrigo);
        Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, Color.white);
        Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, atmosphereColor);

        // Sky
        fullScreenSky = new Simple2DBackgroundIllustration(this, "Centens_Sky", new Vector2(683f, 384f));
        AddElement(fullScreenSky);

        // Towers
        float xSprawl = 6500f;
        int towers = 100;
        for (int i = 0; i < towers; i++)
        {
            float depth = Random.Range(70f, 200f);
            float xPlacement = Random.Range(-xSprawl, xSprawl);
            float yPlacement = Random.Range(-550f, -300f);
            int towerVariant = Random.Range(0, 3);
            float scale = Random.Range(0.75f, 1.25f);
            float rotation = Random.Range(-0.1f, 0.1f);
            Vector2 pos = PosFromDrawPosAtNeutralCamPos(new Vector2(xPlacement, yPlacement), depth);
            AddElement(new Tower(this, "Centens_Tower_" + towerVariant.ToString(), pos, depth, scale, rotation, 0.1f));
            //logger.LogDebug($"{s} Tower - pos={pos}(x{xPlacement}, y{yPlacement}) - depth={depth}");
        }
        if (room.world.region != null)
        {
            startAltitude = (room.world.region.regionParams.cloudsStart ?? startAltitude);
            endAltitude = (room.world.region.regionParams.cloudsEnd ?? endAltitude);
            sceneOrigo = new Vector2(2514f, (startAltitude + endAltitude) / 2f);
        }

        elseClouds = new List<ElseCloud>();

        // Adding graphics
        loadedGraphics.Add("elsewhyClouds1");
        loadedGraphics.Add("elsewhyClouds2");
        loadedGraphics.Add("elsewhyClouds3");
        loadedGraphics.Add("elsewhyFlyingClouds1");
        LoadGraphics();

        // Fog
        fog = new ElseFog(this);
        AddElement(fog);

        // Close clouds
        int cloudCount = 9;
        for (int i = 0; i < cloudCount; i++)
        {
            float cloudDepth = (float)i / (float)(cloudCount - 1);
            AddElement(new CloseElseCloud(this, new Vector2(0f, 0f), cloudDepth, i));
            //logger.LogDebug($"{s} CloseElseCloud - depth={cloudDepth} index={i}");
        }

        int distantCloudCount = 140;
        for (int j = 0; j < distantCloudCount; j++)
        {
            float distantCloudDepth = (float)j / (float)(distantCloudCount - 1);
            AddElement(new DistantElseCloud(this, new Vector2(0f, -40f * cloudsEndDepth * (1f - distantCloudDepth)), distantCloudsEndDepth, j));
            //logger.LogDebug($"{s} DistantElseCloud - depth={distantCloudDepth}, index={j}");
        }

        // Flying clouds
        AddElement(new FlyingElseCloud(this, PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 75f), 355f), 355f, 0, 0.35f, 0.5f, 0.9f));
        AddElement(new FlyingElseCloud(this, PosFromDrawPosAtNeutralCamPos(new Vector2(0f, 43f), 920f), 920f, 0, 0.15f, 0.3f, 0.95f));

        Random.state = state;
    }

    // Adding clouds
    public override void AddElement(BackgroundSceneElement element)
    {
        if (element is ElseCloud)
        {
            elseClouds.Add(element as ElseCloud);
        }
        base.AddElement(element);
    }

    private float CloudDepth(float f)
    {
        return Mathf.Lerp(cloudsStartDepth, cloudsEndDepth, f);
    }

    private float DistantCloudDepth(float f)
    {
        return Mathf.Lerp(cloudsEndDepth, distantCloudsEndDepth, Mathf.Pow(f, 1.5f));
    }

    #region ElseCloud
    public abstract class ElseCloud : BackgroundSceneElement
    {
        private ElsehowView vvScene
        {
            get
            {
                return scene as ElsehowView;
            }
        }
        public ElseCloud(ElsehowView vvScene, Vector2 pos, float depth, int index) : base(vvScene, pos, depth)
        {
            this.randomOffset = UnityEngine.Random.value;
            this.index = index;
        }
        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            this.skyColor = palette.skyColor;
        }

        public float randomOffset;
        public Color skyColor;
        public int index;
    }
    #endregion

    #region Towers
    // References AncientUrbanView.Building
    private class Tower : BackgroundSceneElement
    {
        private ElsehowView vvScene
        {
            get
            {
                return scene as ElsehowView;
            }
        }

        public Tower(ElsehowView scene, string assetName, Vector2 pos, float depth, float scale, float rotation, float atmosphericalDepthAdd) : base(scene, pos, depth)
        {
            this.assetName = assetName;
            this.atmosphericalDepthAdd = atmosphericalDepthAdd;
            this.scale = scale;
            this.pos = pos;
            this.depth = depth;
            this.rotation = rotation;
            scene.LoadGraphic(assetName, true, false);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[3];
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i] = new FSprite(assetName, true);
                sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["AncientUrbanBuilding"];
                sLeaser.sprites[i].anchorY = 0f;
                sLeaser.sprites[i].scale = scale;   
                sLeaser.sprites[i].rotation = rotation;
            }
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                Vector2 vector = DrawPos(new Vector2(camPos.x, camPos.y), rCam.hDisplace);
                sLeaser.sprites[i].x = vector.x;
                sLeaser.sprites[i].y = vector.y;
                sLeaser.sprites[i].color = new Color(Mathf.Pow(Mathf.InverseLerp(0f, 600f, depth + atmosphericalDepthAdd), 0.3f) * 0.9f, 1f - (float)i / 3f, 1f);
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public string assetName;
        public float scale;
        public float rotation;
        public float atmosphericalDepthAdd;
    }
    #endregion

    #region CloseElseCloud
    public class CloseElseCloud : ElseCloud
    {
        public ElsehowView vvScene
        {
            get
            {
                return scene as ElsehowView;
            }
        }

        public CloseElseCloud(ElsehowView vvScene, Vector2 pos, float depth, int index) : base(vvScene, pos, vvScene.CloudDepth(depth), index)
        {
            cloudDepth = depth;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("pixel", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Background"];
            sLeaser.sprites[0].anchorY = 0f;
            sLeaser.sprites[0].scaleX = 1400f;
            sLeaser.sprites[0].x = 683f;
            sLeaser.sprites[0].y = 0f;
            sLeaser.sprites[1] = new FSprite("elsewhyClouds" + (index % 3 + 1).ToString(), true);
            sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["Cloud"];
            sLeaser.sprites[1].anchorY = 1f;
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            float y = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y;
            float altitude = Mathf.InverseLerp(vvScene.startAltitude, vvScene.endAltitude, y);
            float depth = cloudDepth;
            if (altitude > 0.5f)
            {
                depth = Mathf.Lerp(depth, 1f, Mathf.InverseLerp(0.5f, 1f, altitude) * 0.5f);
            }
            this.depth = Mathf.Lerp(vvScene.cloudsStartDepth, vvScene.cloudsEndDepth, depth);
            float num3 = Mathf.Lerp(10f, 2f, depth);
            float num4 = DrawPos(new Vector2(camPos.x, camPos.y), rCam.hDisplace).y;
            num4 += Mathf.Lerp(Mathf.Pow(cloudDepth, 0.75f), Mathf.Sin(cloudDepth * 3.1415927f), 0.5f) * Mathf.InverseLerp(0.5f, 0f, altitude) * 600f;
            num4 -= Mathf.InverseLerp(0.18f, 0.1f, altitude) * Mathf.Pow(1f - cloudDepth, 3f) * 100f;
            float num5 = Mathf.Lerp(1f, Mathf.Lerp(0.75f, 0.25f, altitude), depth);
            sLeaser.sprites[0].scaleY = num4 - 150f * num3 * num5;
            sLeaser.sprites[1].scaleY = num5 * num3;
            sLeaser.sprites[1].scaleX = num3;
            sLeaser.sprites[1].color = new Color(depth * 0.75f, randomOffset, Mathf.Lerp(num5, 1f, 0.5f), 1f);
            sLeaser.sprites[1].x = 683f;
            sLeaser.sprites[1].y = num4 - 2f;
            sLeaser.sprites[0].color = Color.Lerp(skyColor, vvScene.atmosphereColor, depth * 0.75f);
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        public float cloudDepth;
    }
    #endregion

    #region DistantElseCloud
    // Don't use, not what we want
    public class DistantElseCloud : ElseCloud
    {
        public ElsehowView vvScene
        {
            get
            {
                return scene as ElsehowView;
            }
        }

        public DistantElseCloud(ElsehowView vvScene, Vector2 pos, float depth, int index) : base(vvScene, pos, vvScene.DistantCloudDepth(depth), index)
        {
            distantCloudDepth = depth;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("pixel", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CloudDistant"];
            sLeaser.sprites[0].anchorY = 0f;
            // this bastard ruined everything v
            sLeaser.sprites[0].scaleX = 0f;
            sLeaser.sprites[0].x = 683f;
            sLeaser.sprites[0].y = 0f;
            sLeaser.sprites[1] = new FSprite("elsewhyClouds" + (index % 3 + 1).ToString(), true);
            sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["CloudDistant"];
            sLeaser.sprites[1].anchorY = 1f;
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            float yPos = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y;
            if (Mathf.InverseLerp(vvScene.startAltitude, vvScene.endAltitude, yPos) < 0.33f)
            {
                sLeaser.sprites[1].isVisible = false;
                sLeaser.sprites[0].isVisible = false;
                return;
            }
            sLeaser.sprites[1].isVisible = true;
            sLeaser.sprites[0].isVisible = true;
            float num = 2f;
            float y = DrawPos(new Vector2(camPos.x, camPos.y), rCam.hDisplace).y;
            float num2 = Mathf.Lerp(0.3f, 0.01f, distantCloudDepth);
            if (index == 8)
            {
                num2 *= 1.5f;
            }
            sLeaser.sprites[0].scaleY = yPos - 150f * num * num2;
            sLeaser.sprites[1].scaleY = num2 * num;
            sLeaser.sprites[1].scaleX = num;
            sLeaser.sprites[1].color = new Color(Mathf.Lerp(0.75f, 0.95f, distantCloudDepth), randomOffset, Mathf.Lerp(num2, 1f, 0.5f), 1f);
            sLeaser.sprites[1].x = 683f;
            sLeaser.sprites[1].y = yPos - 2f;
            sLeaser.sprites[0].color = Color.Lerp(skyColor, vvScene.atmosphereColor, Mathf.Lerp(0.75f, 0.95f, distantCloudDepth));
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        private float distantCloudDepth;
    }
    #endregion

    #region FlyingElseCloud
    public class FlyingElseCloud : ElseCloud
    {
        public ElsehowView vvScene
        {
            get
            {
                return scene as ElsehowView;
            }
        }

        public FlyingElseCloud(ElsehowView vvScene, Vector2 pos, float depth, int index, float flattened, float alpha, float shaderInputColor) : base(vvScene, pos, depth, index)
        {
            this.flattened = flattened;
            this.alpha = alpha;
            this.shaderInputColor = shaderInputColor;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("elsewhyFlyingClouds1", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CloudDistant"];
            sLeaser.sprites[0].anchorY = 1f;
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            float yPos2 = scene.RoomToWorldPos(rCam.room.cameraPositions[rCam.currentCameraPosition]).y;
            if (Mathf.InverseLerp(vvScene.startAltitude, vvScene.endAltitude, yPos2) < 0.33f)
            {
                sLeaser.sprites[0].isVisible = false;
                return;
            }
            sLeaser.sprites[0].isVisible = true;
            float num = 2f;
            float drawPos = DrawPos(camPos, rCam.hDisplace).y;
            sLeaser.sprites[0].scaleY = flattened * num;
            sLeaser.sprites[0].scaleX = num;
            sLeaser.sprites[0].color = new Color(shaderInputColor, randomOffset, Mathf.Lerp(flattened, 1f, 0.5f), alpha);
            sLeaser.sprites[0].x = 683f;
            sLeaser.sprites[0].y = drawPos;
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        private float flattened;
        private float alpha;
        private float shaderInputColor;
    }
    #endregion

    #region ElseFog
    public class ElseFog : FullScreenSingleColor
    {
        public ElsehowView vvScene
        {
            get
            {
                return scene as ElsehowView;
            }
        }

        public ElseFog(ElsehowView vvScene) : base(vvScene, default(Color), 1f, true, float.MaxValue)
        {
            depth = 0f;
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!room.game.IsArenaSession)
            {
                // We really need this to persist past certain altitudes ngl
                //float value = scene.RoomToWorldPos(camPos).y;
                alpha = Mathf.Lerp(1f, 0.5f, vvScene.effect.amount); //0.5f; //Mathf.InverseLerp(22000f, 18000f, value) * 0.6f;
            }
            else
            {
                alpha = 0f;
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            color = palette.skyColor;
            base.ApplyPalette(sLeaser, rCam, palette);
        }
    }
    #endregion
}
