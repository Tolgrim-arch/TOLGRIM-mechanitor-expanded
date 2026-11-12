using UnityEngine;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    public class MechanitorExpandedMod : Mod
    {
        public static MechanitorExpandedSettings Settings { get; private set; } = null!;
        private Vector2 scrollPosition = Vector2.zero;

        public MechanitorExpandedMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<MechanitorExpandedSettings>();
        }

        public override string SettingsCategory()
        {
            return "Tolgrim_SettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect viewRect = new Rect(0f, 0f, inRect.width - 20f, 780f);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            // Encabezado de organización y fabricación
            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsCraftingSection".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            bool previousMoveSetting = Settings.moveRecipesToSubcoreEncoder;
            listing.CheckboxLabeled("Tolgrim_SettingsMoveRecipes".Translate(), ref Settings.moveRecipesToSubcoreEncoder, "Tolgrim_SettingsMoveRecipesTip".Translate());
            if (previousMoveSetting != Settings.moveRecipesToSubcoreEncoder)
            {
                SubcoreEncoderRecipeManager.ApplyRecipeRouting(Settings.moveRecipesToSubcoreEncoder);
            }
            listing.Gap(10f);

            // Fase 2: Neurofibra y Sueros Bandnode
            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsPhase2Section".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            listing.CheckboxLabeled("Tolgrim_SettingsEnableNeurofibre".Translate(), ref Settings.enableNeurofibre, "Tolgrim_SettingsEnableNeurofibreTip".Translate());
            listing.CheckboxLabeled("Tolgrim_SettingsEnableBandnodeSera".Translate(), ref Settings.enableBandnodeSera, "Tolgrim_SettingsEnableBandnodeSeraTip".Translate());

            listing.Label($"{"Tolgrim_SettingsMaxBandnodeDoses".Translate()}: {Settings.maxBandnodeSeraDoses}");
            Settings.maxBandnodeSeraDoses = Mathf.RoundToInt(listing.Slider(Settings.maxBandnodeSeraDoses, 1f, 100f));

            listing.Label($"{"Tolgrim_SettingsNeurofibreMult".Translate()}: {Settings.neurofibreMultiplier:0.0}x");
            Settings.neurofibreMultiplier = Mathf.Round(listing.Slider(Settings.neurofibreMultiplier, 0.5f, 3.0f) * 2f) / 2f;

            listing.Label($"{"Tolgrim_SettingsSerumMult".Translate()}: {Settings.serumMultiplier:0.0}x");
            Settings.serumMultiplier = Mathf.Round(listing.Slider(Settings.serumMultiplier, 0.5f, 3.0f) * 2f) / 2f;
            listing.Gap(10f);

            // Fase 3: Resurrección de mecanoides hostiles
            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsEnemyMechsSection".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            listing.CheckboxLabeled("Tolgrim_SettingsEnableEnemyMechs".Translate(), ref Settings.enableResurrectEnemyMechs, "Tolgrim_SettingsEnableEnemyMechsTip".Translate());
            listing.Gap(10f);

            // Encabezado de visualización y tamaño
            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsLayoutSection".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            // Ancho del panel
            listing.Label($"{"Tolgrim_SettingsTabWidth".Translate()}: {Settings.tabWidth:F0} px (430 - 650)");
            Settings.tabWidth = Mathf.Round(listing.Slider(Settings.tabWidth, 430f, 650f));

            // Alto del panel
            listing.Label($"{"Tolgrim_SettingsTabHeight".Translate()}: {Settings.tabHeight:F0} px (400 - 700)");
            Settings.tabHeight = Mathf.Round(listing.Slider(Settings.tabHeight, 400f, 700f));
            listing.Gap(10f);

            // Encabezado de visibilidad de filas y secciones
            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsVisibilitySection".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            listing.CheckboxLabeled("Tolgrim_SettingsShowCommandRange".Translate(), ref Settings.showCommandRange, "Tolgrim_SettingsShowCommandRangeTip".Translate());
            listing.CheckboxLabeled("Tolgrim_SettingsShowControlGroups".Translate(), ref Settings.showControlGroups, "Tolgrim_SettingsShowControlGroupsTip".Translate());
            listing.CheckboxLabeled("Tolgrim_SettingsShowImplants".Translate(), ref Settings.showImplantsSection, "Tolgrim_SettingsShowImplantsTip".Translate());
            listing.CheckboxLabeled("Tolgrim_SettingsShowEquipment".Translate(), ref Settings.showEquipmentSection, "Tolgrim_SettingsShowEquipmentTip".Translate());
            listing.Gap(10f);

            // Encabezado de estado de acordeones por defecto
            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsAccordionsSection".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            listing.CheckboxLabeled("Tolgrim_SettingsExpandGeneral".Translate(), ref Settings.defaultExpandGeneral);
            listing.CheckboxLabeled("Tolgrim_SettingsExpandStats".Translate(), ref Settings.defaultExpandStats);
            listing.CheckboxLabeled("Tolgrim_SettingsExpandImplants".Translate(), ref Settings.defaultExpandImplants);
            listing.CheckboxLabeled("Tolgrim_SettingsExpandEquipment".Translate(), ref Settings.defaultExpandEquipment);
            listing.Gap(14f);

            // Botón de restablecer por defecto
            Rect resetRect = listing.GetRect(30f);
            resetRect.width = 220f;
            if (Widgets.ButtonText(resetRect, "Tolgrim_SettingsResetDefaults".Translate()))
            {
                Settings.ResetToDefaults();
                SubcoreEncoderRecipeManager.ApplyRecipeRouting(Settings.moveRecipesToSubcoreEncoder);
            }

            listing.End();
            Widgets.EndScrollView();
            base.DoSettingsWindowContents(inRect);
        }
    }
}
