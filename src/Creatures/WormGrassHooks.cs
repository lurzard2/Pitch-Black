using UnityEngine;

namespace PitchBlack;

public class WormGrassHooks
{
    public static void Inject()
    {
        On.WormGrass.Worm.ApplyPalette += Worm_ApplyPalette;
    }

    private static void Worm_ApplyPalette(On.WormGrass.Worm.orig_ApplyPalette orig, WormGrass.Worm self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        bool isDissolvedFieldsRegion = self.room != null
            && self.room.world.region != null
            && MiscUtils.IsDissolvedFieldsRegion(self.room.world.name);

        // Remake orig, but include conditional coloring.

        Color color = rCam.PixelColorAtCoordinate(self.belowGroundPos);
        Color highlightColor = isDissolvedFieldsRegion ? RainWorld.RippleColor : new(1f, 0f, 0f);
        Color color2 = Color.Lerp(palette.texture.GetPixel(self.color, 3), highlightColor, self.iFac * 0.5f);
        if (ModManager.MSC)
        {
            Room room = self.room;
            if (((room != null) ? room.world.region : null) != null)
            {
                Room room2 = self.room;
                if (((room2 != null) ? room2.world.region.name : null) == "OE")
                {
                    float num = 1000f;
                    float num2 = (float)self.room.world.rainCycle.dayNightCounter / num;
                    color = Color.Lerp(color, Color.Lerp(new Color(0.17f, 0.38f, 0.17f), color2, 0.5f), num2 * 0.04f);
                    color2 = Color.Lerp(color2, new Color(0.17f, 0.38f, 0.17f), num2 * 0.4f);
                }
            }
        }
        sLeaser.sprites[1].color = isDissolvedFieldsRegion ? RainWorld.RippleColor : new(0.2f, 0f, 1f);
        for (int i = 0; i < self.segments.Length; i++)
        {
            (sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4] = Color.Lerp(color2, color, (float)i / (float)(self.segments.Length - 1));
            (sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4 + 1] = Color.Lerp(color2, color, (float)i / (float)(self.segments.Length - 1));
            (sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4 + 2] = Color.Lerp(color2, color, ((float)i + 0.5f) / (float)(self.segments.Length - 1));
            if (i < self.segments.Length - 1)
            {
                (sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4 + 3] = Color.Lerp(color2, color, ((float)i + 0.5f) / (float)(self.segments.Length - 1));
            }
        }
    }
}
