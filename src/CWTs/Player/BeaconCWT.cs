using RWCustom;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable IDE0090

namespace PitchBlack;

public class BeaconCWT : ScugCWT
{
    public Color currentSkinColor;
    public Color currentEyeColor;

    // Blinds player
    public int brightSquint = 0;

    // Stops crafting
    public bool heldCraft = false;

    public FlareStore storage;
    public int dontThrowTimer = 0;
    //flashbangs to recover after respawning in jollycoop
    public int coopRefund = 0;

    public Cycle cycle;
    public bool deathToggle; //toggle tracking
    public bool isDead; //state tracking
    public bool isDeadButDeniedDeath; //for later implementing coming back from GameOver
    public bool diedInThanatosis = false; //used to call GameOver
    public bool thanatosisDeathBumpNeedsToPlay = false; //stops recursive true death sound
    public int thanatosisCounter; //tracking current time spent in Thanatosis
    public float thanatosisLerp; //for lerping player color based on time spent in Thanatosis
    public int inputForThanatosisCounter = 0; //spec input doesn't recursively flip isDead
    public bool graspsNeedToBeReleased = false; //stops grasp-losing recursion
    public bool spawnLeftBody = false;

    public float thanatosisLimit = 480f;

    public BeaconCWT(Player player) : base()
    {
        storage = new FlareStore(player);
        //cycle = new Cycle(player);
    }

    public class Cycle
    {
        public Creature owner;
        public State state;
        public bool realizedPlayer;

        public Cycle(Creature owner)
        {
            this.owner = owner;
            state = State.Init;
            realizedPlayer = false;
        }

        public void Update(bool eu)
        {
            realizedPlayer = owner.abstractCreature.realizedCreature != null;

            if (state == State.Init)
            {
                InitState();
                return;
            }
        }

        public void InitState()
        {
            if (owner.dead)
            {
                ChangeState(State.Dead);
                return;
            }
            if (owner.Stunned)
            {
                ChangeState(State.Stunned);
                return;
            }
            ChangeState(State.Alive);
            return;
        }

        public void ChangeState(State state)
        {
            this.state = state;
        }

        public class State : ExtEnum<State>
        {
            public State(string value, bool register) : base(value, register) { }

            public static readonly State Init = new(nameof(Init), true);

            public static readonly State Alive = new(nameof(Alive), true);
            public static readonly State Stunned = new(nameof(Stunned), true);
            public static readonly State Thanatosis = new(nameof(Thanatosis), true);
            public static readonly State Dead = new(nameof(Dead), true);
            public static readonly State Cycled = new(nameof(Cycled), true);
        }
    }

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
    public class FlareStore
    {
        public Player owner;
        public Stack<FlareBomb> storedFlares;
        public bool increment;
        public int counter;

        // Change this to increase the number of flares stored
        public int capacity = 4; //PBOptions.maxFlashStore.Value;
        public bool interactionLocked;
        public Stack<AbstractStoredFlare> abstractFlare;

        public FlareStore(Player owner)
        {
            if (storedFlares == null)
            {
                storedFlares = new Stack<FlareBomb>(capacity);
                abstractFlare = new Stack<AbstractStoredFlare>(capacity);
            }
            this.owner = owner;
        }

        public void Update(bool eu)
        {
            if (increment)
            {
                counter++;
                if (counter > 20 && storedFlares.Count < capacity)
                {
                    // Move flare from any hand to store if store is empty
                    //WW- WHY ONLY MAIN HAND IF STORAGE IS NOT FULL? SEEMS LIKE THIS SHOULD WORK FROM ANY HAND
                    for (int i = 0; i < 2; i++)
                    {
                        if (owner.grasps[i]?.grabbed is FlareBomb f)
                        {
                            FlarebombtoStorage(f);
                            counter = 0;
                            break;
                        }
                    }
                }
                if (counter > 20 && storedFlares.Count > 0)
                {
                    // Move flare from store to paw
                    FlarebombFromStorageToPaw(eu);
                    counter = 0;
                }
            }
            else
            {
                counter = 0;
            }
            if (!owner.input[0].pckp)
            {
                interactionLocked = false;
            }
            increment = false;
        }

        public void GraphicsModuleUpdated(bool eu)
        {
            // Skip drawing if storage is empty
            if (storedFlares.Count <= 0)
                return;

            PlayerGraphics pG = owner.graphicsModule as PlayerGraphics;

            if (pG == null) return;


            for (int i = 0; i < storedFlares.Count; i++)
            {
                float necklaceLength = 2; //capacity / 2; //WW- Didn't work well for numbers past 4, changing it.
                // These may be able to be replaced with math involving bodyChunks of the player, which while may be more intuitive to understand, could come with positioning issues.
                Vector2 drawPointLeft = pG.drawPositions[0, 0];
                Vector2 drawPointRight = pG.drawPositions[1, 0];
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
                Vector2 vector = drawPointLeft + Custom.RotateAroundOrigo(flarePositionStart, n);
                Vector2 vector1 = drawPointLeft + Custom.RotateAroundOrigo(flarePositionEnd, n);

                // num is a fraction, that essentially determines at what point the flare is in between the flare position caps.

                float fractionOfDistance = (i + 1f) / (Mathf.Min(storedFlares.Count, necklaceLength) + 1f);
                if (i >= necklaceLength)
                {
                    fractionOfDistance = (i - necklaceLength + 1f) / (storedFlares.Count - necklaceLength + 1f);
                }
                Vector2 calculatedDestination = vector + (vector1 - vector) * fractionOfDistance;

                storedFlares.ToArray()[i].firstChunk.MoveFromOutsideMyUpdate(eu, calculatedDestination);
                storedFlares.ToArray()[i].firstChunk.vel = Vector2.zero;
                storedFlares.ToArray()[i].rotationSpeed = 0f;
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

            int toPaw = owner.FreeHand();
            // If empty hand has been detected
            if (toPaw != -1)
            {
                FlareBomb fb = storedFlares.Pop();
                AbstractStoredFlare af = abstractFlare.Pop();
                if (owner.graphicsModule != null)
                {
                    fb.firstChunk.MoveFromOutsideMyUpdate(eu, (owner.graphicsModule as PlayerGraphics).hands[toPaw].pos);
                }

                af?.Deactivate();

                fb.CollideWithObjects = true;
                fb.CollideWithTerrain = true;
                fb.collisionRange = 50f;
                fb.ChangeMode(Weapon.Mode.Free);
                owner.SlugcatGrab(fb, toPaw);
                interactionLocked = true;
                owner.noPickUpOnRelease = 20;
                owner.room.PlaySound(SoundID.Slugcat_Pick_Up_Flare_Bomb, owner.mainBodyChunk);
                // Debug.log("Successfully applied flare to paw! Storage index is now: " + storedFlares.Count);

                return toPaw;
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
            abstractFlare.Push(new AbstractStoredFlare(owner.abstractPhysicalObject, f.abstractPhysicalObject));
            // Debug.log("Applied flare into storage! Storage index is now: " + storedFlares.Count);
        }
    }
}