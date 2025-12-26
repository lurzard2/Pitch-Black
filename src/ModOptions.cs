using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace PitchBlack;

public class ModOptions : OptionInterface
{
    public static readonly ModOptions Instance = new();

    public static Configurable<bool> pursuer;
    public static Configurable<int> pursuerAgro;
    public static Configurable<bool> universalPursuer;

    public static Configurable<bool> shockStun;
	public static Configurable<bool> elecImmune;
	public static Configurable<bool> chargeSpears;

    // Scavenger taking from Beacon's flare storage
    public static Configurable<bool> scavStealing;

    // Hat graphic in player sprites
    private static Configurable<bool> hazHat;
    public static bool UsesHatSprite => hazHat.Value;

    // Thanatosis dropping flares on activate
    private static Configurable<bool> spoiler_RippleLayerDropsFlares;
    public static bool RippleLayerDropsFlares => spoiler_RippleLayerDropsFlares.Value;

    // Do not require a super long manual input for the sequence
    private static Configurable<bool> spoiler_SpeedUpThanatosisSequence;
    public static bool SpeedUpThanatosisSequence => spoiler_SpeedUpThanatosisSequence.Value;

    public ModOptions()
    {
		pursuer = config.Bind("pursuer", true);
        pursuerAgro = config.Bind("pursuerAgro", 2, new ConfigAcceptableRange<int>(0, 10));
        universalPursuer = config.Bind("universalPursuer", false);
        //shockStun = config.Bind<bool>("shockStun", true);
        elecImmune = config.Bind("elecImmune", false);
        chargeSpears = config.Bind("chargeSpears", false);
        scavStealing = config.Bind("scavStealing", false);
        hazHat = config.Bind("hazHat", false);

        spoiler_RippleLayerDropsFlares = config.Bind(nameof(spoiler_RippleLayerDropsFlares), true);
        spoiler_SpeedUpThanatosisSequence = config.Bind(nameof(spoiler_SpeedUpThanatosisSequence), false);
    }
    public override void Initialize()
    {
        OpTab page1 = new OpTab(this, "Main");
        OpTab page2 = new OpTab(this, "Spoilers");
        Tabs =
        [
	        page1,
	        page2
        ];

		const int sliderBarLength = 135;
        const int rightSidePos = 360;
        const int leftSidePos = 60;

        #nullable enable
        UIelement[]? page1Elements =
        [
	        new OpLabel(200, 575, Translate("Pitch Black Options"), true) {alignment=FLabelAlignment.Center},

            // Make the options on the right side
            //new OpSlider(maxFlashStore, new Vector2(rightSidePos, 520), sliderBarLength) {description=Translate("Beacon's Max Stored Flashbangs")},
            //new OpLabel(rightSidePos, 500, Translate("Flashbang storage amount")),

            new OpSlider(pursuerAgro, new Vector2(rightSidePos, 440), sliderBarLength) {description = Translate("Determines how long it takes for the pursuer to spawn")},
            new OpLabel(rightSidePos, 420, Translate("Pursuer Aggro")),

            new OpCheckBox(hazHat, new Vector2(rightSidePos, 360)) {description=Translate("PB slugcats wear a hat to protect their eyes in other campaigns")},
            new OpLabel(rightSidePos+30, 363, Translate("Wear Hats")),

            // Make the options on the left side
            new OpCheckBox(pursuer, new Vector2(leftSidePos, 520)) {description=Translate("Something is pursuing you...")},
            new OpLabel(leftSidePos+30, 523, Translate("Beacon's Pursuer Spawns")),

            new OpCheckBox(scavStealing, new Vector2(leftSidePos, 440)) {description = Translate("Scavengers can steal Beacon's flares (causes graphical issues with storage)")},
            new OpLabel(leftSidePos+30, 443, Translate("Scavenger Griefing")),

            new OpCheckBox(elecImmune, new Vector2(leftSidePos, 360)) {description = Translate("Photomaniac gains resistance to electricity")},
            new OpLabel(leftSidePos+30, 363, Translate("Photomaniac's Electricity Resistance")),

            // Put the universal pursuer option in the middle
            new OpCheckBox(universalPursuer, new Vector2(230f, 280f)) {description = Translate("The pursuer appears in all campaigns for all slugcats")},
            new OpLabel(260f, 283f, Translate("Universal Pursuer")),

            // Make the text at the bottom
	        // NOTE: Increment YPos by 20
            new OpLabel(25, 225, "The Beacon:"),
            new OpLabel(25, 205, Translate("Flare creation: Costs 1 food pip per rock + SHIFT (Grab).")),
            new OpLabel(25, 185, Translate("Add flare to storage: Have a flashbang in hand + hold SHIFT (Grab).")),
            new OpLabel(25, 165, Translate("Remove flare from storage: Have a stored flashbang + hold SHIFT (Grab).")),
            new OpLabel(25, 145, Translate("Quick-throw flare: Have a stored flashbang + X / Throw on an empty hand.")),
            //new OpLabel(25, 100, "Photomaniac:"),
            //new OpLabel(25, 80, Translate("Electric Spear creation: Costs 1 food pip per spear + SHIFT / Grab.")),
            //new OpLabel(25, 60, Translate("Electric shockwave ability: SHIFT / Grab + Z / Jump."))
        ];
        page1.AddItems(page1Elements);

        //Not exactly sure what to do with this so I will leave it here for now
        /*
		lineCount -= 60;
		dsc = Translate("Photomaniac will charge uncharged electric spears");
		Tabs[0].AddItems(new UIelement[]
		{
			mpBox7 = new OpCheckBox(PBOptions.chargeSpears, new Vector2(margin, lineCount))
			{description = dsc},
			new OpLabel(mpBox7.pos.x + 30, mpBox7.pos.y+3, Translate("Charge Spears"))
			{description = dsc}
		});
		*/
    }
}