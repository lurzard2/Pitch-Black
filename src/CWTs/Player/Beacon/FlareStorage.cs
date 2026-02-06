using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static PitchBlack.Plugin;

namespace PitchBlack;

public class AbstractStoredFlare : AbstractPhysicalObject.AbstractObjectStick
{
    public AbstractPhysicalObject Player
    {
        get
        {
            return A;
        }
        set
        {
            A = value;
        }
    }

    public AbstractPhysicalObject FlareBomb
    {
        get
        {
            return B;
        }
        set
        {
            B = value;
        }
    }

    public AbstractStoredFlare(AbstractPhysicalObject player, AbstractPhysicalObject bomb) : base(player, bomb) { }
}

public class FlareStorage
{
    public Player owner;
    public PlayerGraphics Graphics => owner.graphicsModule as PlayerGraphics;
    public Stack<FlareBomb> storedFlares;
    public bool increment = false;
    public Counter storageCounter = new(20, 0, true);

    public int capacity = 4; //PBOptions.maxFlashStore.Value;
    public bool interactionLocked;
    public Stack<AbstractStoredFlare> abstractStoredFlares;

    public FlareStorage(Player owner)
    {
        this.owner = owner;
        storedFlares = new Stack<FlareBomb>(capacity);
        abstractStoredFlares = new Stack<AbstractStoredFlare>(capacity);
    }

    public void Update(bool eu)
    {
        if (increment)
        {
            storageCounter.Tick();
            if (storageCounter.isFinished && storedFlares.Count < capacity)
            {
                for (int i = 0; i < 2; i++)
                {
                    // Move flare from any hand to store if store is empty
                    if (owner.grasps[i]?.grabbed is FlareBomb f)
                    {
                        FlarebombtoStorage(f);
                        storageCounter.Reset();
                        break;
                    }
                }
            }
            if (storageCounter.isFinished && storedFlares.Count > 0)
            {
                FlarebombFromStorageToPaw(eu);
                storageCounter.Reset();
            }

        }
        else
        {
            storageCounter.Reset();
        }
        if (!owner.input[0].pckp)
        {
            interactionLocked = false;
        }
    }

    public void GraphicsModuleUpdated(bool eu)
    {
        // Skip drawing if storage is empty
        if (storedFlares.Count <= 0)
            return;

        PlayerGraphics pGraphics = owner.graphicsModule as PlayerGraphics;

        if (pGraphics == null) return;


        for (int i = 0; i < storedFlares.Count; i++)
        {
            float necklaceLength = 2; //capacity / 2; //WW- Didn't work well for numbers past 4, changing it.
            // These may be able to be replaced with math involving bodyChunks of the player, which while may be more intuitive to understand, could come with positioning issues.
            Vector2 drawPointLeft = pGraphics.drawPositions[0, 0];
            Vector2 drawPointRight = pGraphics.drawPositions[1, 0];
            // n is the angle created by going from the left draw point to the right draw point, based on a horizontal line as 0 degrees
            float n = Custom.VecToDeg((drawPointLeft - drawPointRight).normalized);
            // These vectors are the limits on the linear position displacement of flarebombs in between them
            Vector2 flarePositionStart = new(-30f, -8f);
            Vector2 flarePositionEnd = new(30f, -8f);
            if (i >= necklaceLength)
            {
                flarePositionStart = new Vector2(-8f, -8f);
                flarePositionEnd = new Vector2(8f, -8f);
            }
            if (owner.bodyMode == Player.BodyModeIndex.Crawl)
            {
                flarePositionStart.y += 3.25f;
                flarePositionEnd.y += 3.25f;
            }

            // The same as the vectors previously defined, but rotated around with the player's rotation.
            Vector2 startPos = drawPointLeft + Custom.RotateAroundOrigo(flarePositionStart, n);
            Vector2 endPos = drawPointLeft + Custom.RotateAroundOrigo(flarePositionEnd, n);

            // num is a fraction, that essentially determines at what point the flare is in between the flare position caps.

            float distanceFrac = (i + 1f) / (Mathf.Min(storedFlares.Count, necklaceLength) + 1f);
            if (i >= necklaceLength)
            {
                distanceFrac = (i - necklaceLength + 1f) / (storedFlares.Count - necklaceLength + 1f);
            }
            Vector2 calculatedDestination = startPos + (endPos - startPos) * distanceFrac;

            // Accessing indexed flare
            var aStoredFlares = storedFlares.ToArray()[i];
            aStoredFlares.firstChunk.MoveFromOutsideMyUpdate(eu, calculatedDestination);
            aStoredFlares.firstChunk.vel = Vector2.zero;
            aStoredFlares.rotationSpeed = 0f;
        }
    }

    private void SwapFlares(bool eu)
    {
        if (storedFlares.Count < capacity)
        {
            // Move flare from any hand to store if store is empty
            //WW- why only main hand if storage is not full? seems like this should work with any hand
            for (int i = 0; i < 2; i++)
            {
                if (owner.grasps[i]?.grabbed is FlareBomb f)
                {
                    FlarebombtoStorage(f);
                    storageCounter.Reset();
                    break;
                }
            }
        }
        if (storedFlares.Count > 0)
        {
            // Move flare from store to paw
            FlarebombFromStorageToPaw(eu);
            storageCounter.Reset();
        }
    }

    public int FlarebombFromStorageToPaw(bool eu)
    {
        //spinch: the int return is to find which grasp index the flarebomb is now in

        // See if it's possible to add weapon
        for (int i = 0; i < 2; i++)
        {
            if (owner.grasps[i] != null && owner.Grabability(owner.grasps[i].grabbed) >= Player.ObjectGrabability.TwoHands)
            {
                return -1;
            }
        }

        int pawIndex = owner.FreeHand();
        // If empty hand has been detected
        if (pawIndex != -1)
        {
            FlareBomb realizedFlare = storedFlares.Pop();
            AbstractStoredFlare absractFlare = abstractStoredFlares.Pop();

            if (owner.graphicsModule != null)
            {
                realizedFlare.firstChunk.MoveFromOutsideMyUpdate(eu, (owner.graphicsModule as PlayerGraphics).hands[pawIndex].pos);
            }

            absractFlare?.Deactivate();

            realizedFlare.CollideWithObjects = true;
            realizedFlare.CollideWithTerrain = true;
            realizedFlare.collisionRange = 50f;
            realizedFlare.ChangeMode(Weapon.Mode.Free);
            owner.SlugcatGrab(realizedFlare, pawIndex);
            interactionLocked = true;
            owner.noPickUpOnRelease = 20;
            owner.room.PlaySound(SoundID.Slugcat_Pick_Up_Flare_Bomb, owner.mainBodyChunk);
            logger.LogDebug("Successfully applied flare to paw! Storage index is now: " + storedFlares.Count);

            return pawIndex;
        }
        else
        {
            Debug.Log("Pitch Black: Couldn't add flare to paw! Index is now: " + storedFlares.Count);
            return -1;
        }

    }

    public void FlarebombtoStorage(FlareBomb f)
    {
        // Take off the flare from hand
        for (int i = 0; i < 2; i++)
        {
            if (owner.grasps[i] != null && owner.grasps[i].grabbed == f)
            {
                owner.ReleaseGrasp(i);
                break;
            }
        }
        f.ChangeMode(Weapon.Mode.OnBack);
        f.CollideWithObjects = false;
        f.CollideWithTerrain = false;
        f.collisionRange = 0f;
        storedFlares.Push(f);
        interactionLocked = true;
        owner.noPickUpOnRelease = 20;
        owner.room.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back, owner.mainBodyChunk);
        abstractStoredFlares.Push(new AbstractStoredFlare(owner.abstractPhysicalObject, f.abstractPhysicalObject));
        logger.LogDebug("Applied flare into storage! Storage index is now: " + storedFlares.Count);
    }

    public static void DropAllFlares(Player self)
    {
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT scugCWT)
            && scugCWT is BeaconCWT beaconCWT
            && beaconCWT.GetFlareStorage() is not null)
        {
            while (beaconCWT.storage.storedFlares.Count > 0)
            {
                FlareBomb flare = beaconCWT.storage.storedFlares.Pop();
                AbstractStoredFlare af = beaconCWT.storage.abstractStoredFlares.Pop();
                if (flare != null)
                {
                    flare.firstChunk.vel = self.mainBodyChunk.vel + Custom.RNV() * 3f * Random.value;
                    flare.ChangeMode(Weapon.Mode.Free);
                }
                af?.Deactivate();
            }
        }
    }
}
