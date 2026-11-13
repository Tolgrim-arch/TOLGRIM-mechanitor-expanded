using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    public class MechanitorExpandedMod : Mod
    {
        private enum SettingsTab
        {
            Interface,
            Crafting,
            Bandwidth,
            Mechanoids,
            Balance
        }

        public static MechanitorExpandedSettings Settings { get; private set; } = null!;
        private SettingsTab currentTab = SettingsTab.Interface;
        private Vector2 scrollPosition = Vector2.zero;

        // Buffers de edición para campos numéricos
        private static string bufSubcoreRegSteel = "";
        private static string bufSubcoreRegComp = "";
        private static string bufSubcoreHighSteel = "";
        private static string bufSubcoreHighComp = "";
        private static string bufSigChipPlasteel = "";
        private static string bufSigChipUranium = "";
        private static string bufSigChipComp = "";
        private static string bufPowChipPlasteel = "";
        private static string bufPowChipUranium = "";
        private static string bufPowChipComp = "";
        private static string bufNanoChipPlasteel = "";
        private static string bufNanoChipUranium = "";
        private static string bufNanoChipComp = "";
        private static string bufMechlinkNano = "";
        private static string bufMechlinkComp = "";

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
            // Barra de pestañas nativa de RimWorld
            const float tabHeight = 32f;
            Rect tabRect = new Rect(inRect.x, inRect.y + tabHeight, inRect.width, inRect.height - tabHeight);

            List<TabRecord> tabs = new List<TabRecord>
            {
                new TabRecord("Tolgrim_SettingsTabInterface".Translate(), () => { currentTab = SettingsTab.Interface; scrollPosition = Vector2.zero; }, currentTab == SettingsTab.Interface),
                new TabRecord("Tolgrim_SettingsTabCrafting".Translate(), () => { currentTab = SettingsTab.Crafting; scrollPosition = Vector2.zero; SyncCraftingBuffers(); }, currentTab == SettingsTab.Crafting),
                new TabRecord("Tolgrim_SettingsTabBandwidth".Translate(), () => { currentTab = SettingsTab.Bandwidth; scrollPosition = Vector2.zero; }, currentTab == SettingsTab.Bandwidth),
                new TabRecord("Tolgrim_SettingsTabMechanoids".Translate(), () => { currentTab = SettingsTab.Mechanoids; scrollPosition = Vector2.zero; }, currentTab == SettingsTab.Mechanoids),
                new TabRecord("Tolgrim_SettingsTabBalance".Translate(), () => { currentTab = SettingsTab.Balance; scrollPosition = Vector2.zero; }, currentTab == SettingsTab.Balance)
            };

            Widgets.DrawMenuSection(tabRect);
            TabDrawer.DrawTabs(tabRect, tabs);

            Rect contentRect = tabRect.ContractedBy(14f);
            float viewHeight = currentTab == SettingsTab.Crafting ? 820f : (currentTab == SettingsTab.Interface ? 560f : (currentTab == SettingsTab.Balance ? 420f : 460f));
            Rect viewRect = new Rect(0f, 0f, contentRect.width - 20f, viewHeight);

            Widgets.BeginScrollView(contentRect, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            switch (currentTab)
            {
                case SettingsTab.Interface:
                    DrawInterfaceTab(listing);
                    break;
                case SettingsTab.Crafting:
                    DrawCraftingTab(listing);
                    break;
                case SettingsTab.Bandwidth:
                    DrawBandwidthTab(listing);
                    break;
                case SettingsTab.Mechanoids:
                    DrawMechanoidsTab(listing);
                    break;
                case SettingsTab.Balance:
                    DrawBalanceTab(listing);
                    break;
            }

            listing.Gap(14f);
            Rect resetRect = listing.GetRect(30f);
            resetRect.width = 240f;
            if (Widgets.ButtonText(resetRect, "Tolgrim_SettingsResetDefaults".Translate()))
            {
                Settings.ResetToDefaults();
                SubcoreEncoderRecipeManager.ApplyRecipeRouting(Settings.moveRecipesToSubcoreEncoder);
                SubcoreEncoderCraftingManager.ApplyAllIngredientCosts();
                UnbreakableChipsManager.ApplySetting(Settings.unbreakableMechChips);
                MechHaulToChargerManager.ApplySettings();
                SyncCraftingBuffers();
            }

            listing.End();
            Widgets.EndScrollView();
            base.DoSettingsWindowContents(inRect);
        }

        private static void SyncCraftingBuffers()
        {
            bufSubcoreRegSteel = Settings.costSubcoreRegularSteel.ToString();
            bufSubcoreRegComp = Settings.costSubcoreRegularComponents.ToString();
            bufSubcoreHighSteel = Settings.costSubcoreHighSteel.ToString();
            bufSubcoreHighComp = Settings.costSubcoreHighComponents.ToString();
            bufSigChipPlasteel = Settings.costSignalChipPlasteel.ToString();
            bufSigChipUranium = Settings.costSignalChipUranium.ToString();
            bufSigChipComp = Settings.costSignalChipComponents.ToString();
            bufPowChipPlasteel = Settings.costPowerfocusChipPlasteel.ToString();
            bufPowChipUranium = Settings.costPowerfocusChipUranium.ToString();
            bufPowChipComp = Settings.costPowerfocusChipComponents.ToString();
            bufNanoChipPlasteel = Settings.costNanostructuringChipPlasteel.ToString();
            bufNanoChipUranium = Settings.costNanostructuringChipUranium.ToString();
            bufNanoChipComp = Settings.costNanostructuringChipSpacerComponents.ToString();
            bufMechlinkNano = Settings.costMechlinkNanostructuringChips.ToString();
            bufMechlinkComp = Settings.costMechlinkSpacerComponents.ToString();
        }

        private static void DrawCraftingTab(Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsCraftingSectionHeader".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            bool previousMoveSetting = Settings.moveRecipesToSubcoreEncoder;
            listing.CheckboxLabeled("Tolgrim_SettingsMoveRecipes".Translate(), ref Settings.moveRecipesToSubcoreEncoder, "Tolgrim_SettingsMoveRecipesTip".Translate());
            if (previousMoveSetting != Settings.moveRecipesToSubcoreEncoder)
            {
                SubcoreEncoderRecipeManager.ApplyRecipeRouting(Settings.moveRecipesToSubcoreEncoder);
            }
            listing.Gap(10f);

            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsEncoderRecipesTitle".Translate());
            Text.Font = GameFont.Small;
            listing.Label("Tolgrim_SettingsEncoderRecipesDesc".Translate());
            listing.Gap(8f);

            // 1. Standard Subcore
            listing.CheckboxLabeled("Tolgrim_CraftSubcoreRegular".Translate(), ref Settings.craftSubcoreRegular);
            if (Settings.craftSubcoreRegular)
            {
                DrawCostRow(listing, "Steel", ref Settings.costSubcoreRegularSteel, ref bufSubcoreRegSteel);
                DrawCostRow(listing, "ComponentIndustrial", ref Settings.costSubcoreRegularComponents, ref bufSubcoreRegComp);
            }
            listing.Gap(6f);

            // 2. High Subcore
            listing.CheckboxLabeled("Tolgrim_CraftSubcoreHigh".Translate(), ref Settings.craftSubcoreHigh);
            if (Settings.craftSubcoreHigh)
            {
                DrawCostRow(listing, "Steel", ref Settings.costSubcoreHighSteel, ref bufSubcoreHighSteel);
                DrawCostRow(listing, "ComponentIndustrial", ref Settings.costSubcoreHighComponents, ref bufSubcoreHighComp);
            }
            listing.Gap(6f);

            // 3. Signal Chip
            listing.CheckboxLabeled("Tolgrim_CraftSignalChip".Translate(), ref Settings.craftSignalChip);
            if (Settings.craftSignalChip)
            {
                DrawCostRow(listing, "Plasteel", ref Settings.costSignalChipPlasteel, ref bufSigChipPlasteel);
                DrawCostRow(listing, "Uranium", ref Settings.costSignalChipUranium, ref bufSigChipUranium);
                DrawCostRow(listing, "ComponentIndustrial", ref Settings.costSignalChipComponents, ref bufSigChipComp);
            }
            listing.Gap(6f);

            // 4. Powerfocus Chip
            listing.CheckboxLabeled("Tolgrim_CraftPowerfocusChip".Translate(), ref Settings.craftPowerfocusChip);
            if (Settings.craftPowerfocusChip)
            {
                DrawCostRow(listing, "Plasteel", ref Settings.costPowerfocusChipPlasteel, ref bufPowChipPlasteel);
                DrawCostRow(listing, "Uranium", ref Settings.costPowerfocusChipUranium, ref bufPowChipUranium);
                DrawCostRow(listing, "ComponentIndustrial", ref Settings.costPowerfocusChipComponents, ref bufPowChipComp);
            }
            listing.Gap(6f);

            // 5. Nanostructuring Chip
            listing.CheckboxLabeled("Tolgrim_CraftNanostructuringChip".Translate(), ref Settings.craftNanostructuringChip);
            if (Settings.craftNanostructuringChip)
            {
                DrawCostRow(listing, "Plasteel", ref Settings.costNanostructuringChipPlasteel, ref bufNanoChipPlasteel);
                DrawCostRow(listing, "Uranium", ref Settings.costNanostructuringChipUranium, ref bufNanoChipUranium);
                DrawCostRow(listing, "ComponentSpacer", ref Settings.costNanostructuringChipSpacerComponents, ref bufNanoChipComp);
            }
            listing.Gap(6f);

            // 6. Mechlink
            listing.CheckboxLabeled("Tolgrim_CraftMechlink".Translate(), ref Settings.craftMechlink);
            if (Settings.craftMechlink)
            {
                DrawCostRow(listing, "NanostructuringChip", ref Settings.costMechlinkNanostructuringChips, ref bufMechlinkNano);
                DrawCostRow(listing, "ComponentSpacer", ref Settings.costMechlinkSpacerComponents, ref bufMechlinkComp);
            }

            // Aplicar cambios en vivo a las recetas
            SubcoreEncoderCraftingManager.ApplyAllIngredientCosts();
        }

        private static void DrawCostRow(Listing_Standard listing, string thingDefName, ref int costValue, ref string buffer)
        {
            Rect rowRect = listing.GetRect(24f);
            Rect labelRect = new Rect(rowRect.x + 24f, rowRect.y, 220f, rowRect.height);
            Rect entryRect = new Rect(rowRect.x + 250f, rowRect.y, 160f, rowRect.height);

            ThingDef? def = DefDatabase<ThingDef>.GetNamedSilentFail(thingDefName);
            string label = def != null ? def.LabelCap.Resolve() : thingDefName;

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.IntEntry(entryRect, ref costValue, ref buffer, 1);
            if (costValue < 1) costValue = 1;
        }

        private static void DrawInterfaceTab(Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsLayoutSection".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            listing.Label($"{"Tolgrim_SettingsTabWidth".Translate()}: {Settings.tabWidth:F0} px (430 - 650)");
            Settings.tabWidth = Mathf.Round(listing.Slider(Settings.tabWidth, 430f, 650f));

            listing.Label($"{"Tolgrim_SettingsTabHeight".Translate()}: {Settings.tabHeight:F0} px (400 - 700)");
            Settings.tabHeight = Mathf.Round(listing.Slider(Settings.tabHeight, 400f, 700f));
            listing.Gap(10f);

            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsVisibilitySection".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            listing.CheckboxLabeled("Tolgrim_SettingsShowCommandRange".Translate(), ref Settings.showCommandRange, "Tolgrim_SettingsShowCommandRangeTip".Translate());
            listing.CheckboxLabeled("Tolgrim_SettingsShowControlGroups".Translate(), ref Settings.showControlGroups, "Tolgrim_SettingsShowControlGroupsTip".Translate());
            listing.CheckboxLabeled("Tolgrim_SettingsShowImplants".Translate(), ref Settings.showImplantsSection, "Tolgrim_SettingsShowImplantsTip".Translate());
            listing.CheckboxLabeled("Tolgrim_SettingsShowEquipment".Translate(), ref Settings.showEquipmentSection, "Tolgrim_SettingsShowEquipmentTip".Translate());
            listing.Gap(10f);

            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsAccordionsSection".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            listing.CheckboxLabeled("Tolgrim_SettingsExpandGeneral".Translate(), ref Settings.defaultExpandGeneral);
            listing.CheckboxLabeled("Tolgrim_SettingsExpandStats".Translate(), ref Settings.defaultExpandStats);
            listing.CheckboxLabeled("Tolgrim_SettingsExpandImplants".Translate(), ref Settings.defaultExpandImplants);
            listing.CheckboxLabeled("Tolgrim_SettingsExpandEquipment".Translate(), ref Settings.defaultExpandEquipment);
        }

        private static void DrawBandwidthTab(Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsPhase2Section".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            listing.CheckboxLabeled("Tolgrim_SettingsEnableNeurofibre".Translate(), ref Settings.enableNeurofibre, "Tolgrim_SettingsEnableNeurofibreTip".Translate());
            listing.CheckboxLabeled("Tolgrim_SettingsEnableBandnodeSera".Translate(), ref Settings.enableBandnodeSera, "Tolgrim_SettingsEnableBandnodeSeraTip".Translate());
            listing.Gap(8f);

            listing.Label($"{"Tolgrim_SettingsMaxBandnodeDoses".Translate()}: {Settings.maxBandnodeSeraDoses}");
            Settings.maxBandnodeSeraDoses = Mathf.RoundToInt(listing.Slider(Settings.maxBandnodeSeraDoses, 1f, 100f));

            listing.Label($"{"Tolgrim_SettingsNeurofibreMult".Translate()}: {Settings.neurofibreMultiplier:0.0}x");
            Settings.neurofibreMultiplier = Mathf.Round(listing.Slider(Settings.neurofibreMultiplier, 0.5f, 3.0f) * 2f) / 2f;

            listing.Label($"{"Tolgrim_SettingsSerumMult".Translate()}: {Settings.serumMultiplier:0.0}x");
            Settings.serumMultiplier = Mathf.Round(listing.Slider(Settings.serumMultiplier, 0.5f, 3.0f) * 2f) / 2f;
        }

        private static void DrawMechanoidsTab(Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsEnemyMechsSection".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            listing.CheckboxLabeled("Tolgrim_SettingsEnableEnemyMechs".Translate(), ref Settings.enableResurrectEnemyMechs, "Tolgrim_SettingsEnableEnemyMechsTip".Translate());
            listing.CheckboxLabeled("Tolgrim_SettingsEnableForgetfulResurrection".Translate(), ref Settings.enableForgetfulMechResurrection, "Tolgrim_SettingsEnableForgetfulResurrectionTip".Translate());
            listing.Gap(10f);

            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsApocritonSection".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            listing.CheckboxLabeled("Tolgrim_SettingsEnableApocritonGestation".Translate(), ref Settings.enableApocritonGestation, "Tolgrim_SettingsEnableApocritonGestationTip".Translate());
            listing.CheckboxLabeled("Tolgrim_SettingsSuppressHateAura".Translate(), ref Settings.suppressFriendlyApocritonHateAura, "Tolgrim_SettingsSuppressHateAuraTip".Translate());
            listing.CheckboxLabeled("Tolgrim_SettingsEnableApocritonRecharge".Translate(), ref Settings.enableApocritonRecharge, "Tolgrim_SettingsEnableApocritonRechargeTip".Translate());
            listing.Gap(10f);

            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsHaulToChargerSection".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            bool prevMechsHaul = Settings.enableMechsHaulToCharger;
            listing.CheckboxLabeled("Tolgrim_SettingsEnableMechsHaulToCharger".Translate(), ref Settings.enableMechsHaulToCharger, "Tolgrim_SettingsEnableMechsHaulToChargerTip".Translate());
            if (prevMechsHaul != Settings.enableMechsHaulToCharger)
            {
                MechHaulToChargerManager.ApplySettings();
            }
            listing.CheckboxLabeled("Tolgrim_SettingsHaulInjuredMechsToCharger".Translate(), ref Settings.haulInjuredMechsToCharger, "Tolgrim_SettingsHaulInjuredMechsToChargerTip".Translate());
        }

        private static void DrawBalanceTab(Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsBalanceSection".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(6f);

            listing.Label($"{"Tolgrim_SettingsCommandRange".Translate()}: {Settings.baseCommandRange:F1} {"Tolgrim_TilesUnit".Translate()} (24.9 - 999)");
            Settings.baseCommandRange = Mathf.Round(listing.Slider(Settings.baseCommandRange, 24.9f, 999f) * 10f) / 10f;

            listing.Label($"{"Tolgrim_SettingsMechWorkSpeed".Translate()}: {Settings.mechWorkSpeedMultiplier * 100f:F0}% (100% - 3000%)");
            Settings.mechWorkSpeedMultiplier = Mathf.Round(listing.Slider(Settings.mechWorkSpeedMultiplier, 1.0f, 30.0f) * 2f) / 2f;

            listing.Label($"{"Tolgrim_SettingsGestationSpeed".Translate()}: {Settings.mechGestationSpeedMultiplier * 100f:F0}% (100% - 3000%)");
            Settings.mechGestationSpeedMultiplier = Mathf.Round(listing.Slider(Settings.mechGestationSpeedMultiplier, 1.0f, 30.0f) * 2f) / 2f;
            listing.Gap(6f);

            listing.CheckboxLabeled("Tolgrim_SettingsInstantGestation".Translate(), ref Settings.instantMechGestation, "Tolgrim_SettingsInstantGestationTip".Translate());
            listing.CheckboxLabeled("Tolgrim_SettingsInfiniteEnergy".Translate(), ref Settings.infiniteMechEnergy, "Tolgrim_SettingsInfiniteEnergyTip".Translate());
            listing.CheckboxLabeled("Tolgrim_SettingsDisableWastepacks".Translate(), ref Settings.disableMechWastepacks, "Tolgrim_SettingsDisableWastepacksTip".Translate());

            bool prevUnbreakable = Settings.unbreakableMechChips;
            listing.CheckboxLabeled("Tolgrim_SettingsUnbreakableMechChips".Translate(), ref Settings.unbreakableMechChips, "Tolgrim_SettingsUnbreakableMechChipsTip".Translate());
            if (prevUnbreakable != Settings.unbreakableMechChips)
            {
                UnbreakableChipsManager.ApplySetting(Settings.unbreakableMechChips);
            }
            listing.Gap(10f);

            // Fase 6: Reparación en cargadores (Mech Chargers Repair Mechs)
            Text.Font = GameFont.Medium;
            listing.Label("Tolgrim_SettingsChargerRepairHeader".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            listing.CheckboxLabeled("Tolgrim_SettingsEnableChargerRepair".Translate(), ref Settings.enableChargerRepair, "Tolgrim_SettingsEnableChargerRepairTip".Translate());
            if (Settings.enableChargerRepair)
            {
                listing.CheckboxLabeled("Tolgrim_SettingsRestoreMissingPartsInCharger".Translate(), ref Settings.restoreMissingPartsInCharger, "Tolgrim_SettingsRestoreMissingPartsInChargerTip".Translate());
                listing.Gap(4f);
                listing.Label($"{"Tolgrim_SettingsChargerRepairSpeed".Translate()}: {Settings.chargerRepairSpeedMultiplier * 100f:F0}% (50% - 500%)");
                Settings.chargerRepairSpeedMultiplier = Mathf.Round(listing.Slider(Settings.chargerRepairSpeedMultiplier, 0.5f, 5.0f) * 10f) / 10f;
            }
        }
    }
}
