namespace PitchBlack;

public abstract class PlayerCWT
{
    // bc PlayerGraphics.InitializeSprites calls itself twice in a row gawd dam
    public bool SpritesInitialized;
    // initialized in PlayerGraphics.ctor hook
    public Whiskers whiskers;
    // index of the hat sprite
    public int hatIndex;

    public PlayerCWT(Player player)
    {
    }
}