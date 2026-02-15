using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PitchBlack;

public class ModOptions : OptionInterface
{
    public static readonly ModOptions Instance = new();

    public static Configurable<bool> pursuer;
    public static Configurable<int> pursuerAgro;
    public static bool UniversalPursuer => universalPursuer.Value;
    private static Configurable<bool> universalPursuer;

    // Hat graphic in player sprites
    public static bool UsesHatSprite => hazHat.Value;
    private static Configurable<bool> hazHat;

    // Enable beacon's thanatosis mechanic
    public static bool ThanatosisEnabled => thanatosisEnabaled.Value;
    public static Configurable<bool> thanatosisEnabaled;

    // Change beacon's cosmetic effects for thanatosis depending on progression stage   
    public static int ThanatosisVariant => thanatosisVariant.Value;
    public static Configurable<int> thanatosisVariant;

    // Stops the Thanatosis tutorial from spawning
    public static bool SkipThanatosisSequence => skipThanatosisSequence.Value;
    public static Configurable<bool> skipThanatosisSequence;

    // Amount of "times" beacon met Dreamer
    public static int DreamerEncounters => dreamerEncounters.Value;
    public static Configurable<int> dreamerEncounters;

    // Toggle beacon flare crafting and storage
    public static bool UsesFlareMechanics => usesFlareMechanics.Value;
    public static Configurable<bool> usesFlareMechanics;

    // Thanatosis dropping flares
    public static bool RippleLayerDropsFlares => rippleLayerDropsFlares.Value;
    private static Configurable<bool> rippleLayerDropsFlares;

    public ModOptions()
    {
		pursuer = config.Bind("pursuer", true);
        pursuerAgro = config.Bind("pursuerAgro", 2, new ConfigAcceptableRange<int>(0, 10));
        universalPursuer = config.Bind("universalPursuer", false);

        hazHat = config.Bind("hazHat", false);

        rippleLayerDropsFlares = config.Bind("RippleLayerDropsFlares", true);

        thanatosisEnabaled = config.Bind("ThanatosisEnabled", false);
        thanatosisVariant = config.Bind("ThanatosisVariant", 0);
        skipThanatosisSequence = config.Bind("SkipThanatosisSequence", false);

        dreamerEncounters = config.Bind("DreamerEncounters", 0);

        usesFlareMechanics = config.Bind("UsesFlareMechanics", false);
    }
    public override void Initialize()
    {
        OpTab mainPage = new OpTab(this, "Main");
        OpTab devPage = new OpTab(this, "Development");
        Tabs =
        [
	        mainPage,
	        devPage
        ];

		const int sliderBarLength = 135;
        const int rightSidePos = 360;
        const int leftSidePos = 60;

        #nullable enable
        UIelement[]? mainPageElements =
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

            // Put the universal pursuer option in the middle
            new OpCheckBox(universalPursuer, new Vector2(230f, 280f)) {description = Translate("The pursuer appears in all campaigns for all slugcats")},
            new OpLabel(260f, 283f, Translate("Universal Pursuer")),

            // Make the text at the bottom
	        // NOTE: Increment YPos by 20
            //new OpLabel(25, 225, "The Beacon:"),
            //new OpLabel(25, 205, Translate("Flare creation: Costs 1 food pip per rock + SHIFT (Grab).")),
            //new OpLabel(25, 185, Translate("Add flare to storage: Have a flashbang in hand + hold SHIFT (Grab).")),
            //new OpLabel(25, 165, Translate("Remove flare from storage: Have a stored flashbang + hold SHIFT (Grab).")),
            //new OpLabel(25, 145, Translate("Quick-throw flare: Have a stored flashbang + X / Throw on an empty hand.")),
            //new OpLabel(25, 100, "Photomaniac:"),
            //new OpLabel(25, 80, Translate("Electric Spear creation: Costs 1 food pip per spear + SHIFT / Grab.")),
            //new OpLabel(25, 60, Translate("Electric shockwave ability: SHIFT / Grab + Z / Jump."))
        ];
        mainPage.AddItems(mainPageElements);

        var radioButtonGroup = new OpRadioButtonGroup(thanatosisVariant);
        UIelement[]? devPageElements =
        {
            new OpLabel(200f, 570f, Translate("Developer Options"), true) {alignment=FLabelAlignment.Center},

            new OpLabel(leftSidePos + 30, 530, "Enable Thanatosis"),
            new OpCheckBox(thanatosisEnabaled, leftSidePos, 530f),

            new OpLabel(leftSidePos + 30, 500f, "Skip Thanatosis Sequence"),
            new OpCheckBox(skipThanatosisSequence, leftSidePos, 500f),

            new OpLabel(leftSidePos, 475f, "Thanatosis progression"),
            new OpLabel(leftSidePos + 30, 450f, "Don't overwrite"),
            new OpLabel(leftSidePos + 30, 425f, "Starving"),
            new OpLabel(leftSidePos + 30, 400f, "Rot"),
            new OpLabel(leftSidePos + 30, 375f, "Hybrid"),

            new OpLabel(leftSidePos + 30, 325f, "Dreamer Encounters"),
            new OpDragger(dreamerEncounters, leftSidePos, 325f),

            new OpLabel(leftSidePos + 30, 275f, "Enable Flare Storage + Crafting"),
            new OpCheckBox(usesFlareMechanics, leftSidePos, 275f),

            radioButtonGroup,
        };
        UIelement[]? devPageOFFElements =
        {
            new OpLabel(leftSidePos + 30, 500f, "Ah so sorry! This is for developers! We use this to test features."),
            new OpLabel(leftSidePos + 30, 450f, "Please return to the previous page for usable config options!"),
        };

        // Adding the dev page, but also accomodating for when it's off, because dev mod will be off for playtests and release!
        if (Plugin.devMode)
        {
            devPage.AddItems(devPageElements);
            radioButtonGroup.SetButtons
            ([
                new OpRadioButton(new Vector2(leftSidePos, 450f)),
                new OpRadioButton(new Vector2(leftSidePos, 450f - 25)),
                new OpRadioButton(new Vector2(leftSidePos, 450f - 50)),
                new OpRadioButton(new Vector2(leftSidePos, 450f - 75)),
            ]);
        }
        else
        {
            devPage.AddItems(devPageOFFElements);
        }


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