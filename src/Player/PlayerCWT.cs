namespace PitchBlack;

public abstract class PlayerCWT
{
    // bc PlayerGraphics.InitializeSprites calls itself twice in a row gawd dam
    public bool SpritesInitialized;
    // index of the hat sprite
    public int hatIndex;

    public PlayerCWT(Player player)
    {
    }
}