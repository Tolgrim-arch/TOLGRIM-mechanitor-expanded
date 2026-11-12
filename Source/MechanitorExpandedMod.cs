using UnityEngine;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    public class MechanitorExpandedMod : Mod
    {
        public static MechanitorExpandedSettings Settings { get; private set; } = null!;

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
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

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
            base.DoSettingsWindowContents(inRect);
        }
    }
}
