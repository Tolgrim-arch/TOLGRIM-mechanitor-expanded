# Changelog — TOLGRIM: Mechanitor Expanded

All notable changes to this project are documented in this file.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and [SemVer](https://semver.org/).

---

## [0.6.0] — 2026-11-12

### Feature: Native Mechanoid Accent Color Customization (Mech Colors Integration)

#### Added
- **Native Mech Accent Color Architecture (`MechColorSystem.cs` & `MechColorPatches.cs`):**
  - Integrated individual accent coloring for colony mechanoids, mechanitor control groups, and overseer mechanitors.
  - Hierarchical inheritance: Mechanoid Color -> Control Group Color -> Mechanitor Default Color -> Colony Allegiance Color.
  - Real-time shader rendering hook via `PawnRenderUtility.SetMatPropBlockOverlay`, `PawnRenderer.DynamicDrawPhaseAt`, and `PawnRenderNodeWorker.GetMaterialPropertyBlock`.
  - Automatic dirtying of UI portraits cache (`PortraitsCache.SetDirty`) upon color or opacity change.
- **Interactive UI & Gizmos:**
  - `Command_MechColor`: Native gizmo with `UI/Commands/ChangeColor` and color dot indicator on selected mechs and mechanitors.
  - Multi-selection support (`MergedMechColorState`) allowing simultaneous recoloring of groups of mechanoids.
  - `Dialog_ChooseMechColor`: Color picker modal with ColorDef palette, default-color reset toggle, and opacity percentage slider.
  - Control group gizmo integration: Right-click float menu options ("Select color", "Copy color", "Reset to default") and color indicator dot.
- **Safe Persistence & Compatibility:**
  - Serialized via `ExposeData` under `tolgrim_mechColor` with backward-compatibility fallback reading `kathanon.MechColors` nodes from existing saves.
- **Localization:**
  - Full translations for commands, dialogs, and tooltips in English, Spanish, and SpanishLatin.

---

## [0.5.0] — 2026-11-12

### Feature: Subcore Encoder Custom Crafting & Resource Cost Customization

#### Added
- **Native Subcore Encoder Crafting Recipes (`1.6/Defs/RecipeDefs/Recipes_SubcoreEncoderCrafting.xml`):**
  - Added crafting recipes to `SubcoreEncoder` for mechanoids and mechanitor technology without requiring analog pattern scanning:
    - **Standard Subcore** (`Tolgrim_CraftSubcoreRegular`): Basic subcore, steel, industrial components (`StandardMechtech`).
    - **High Subcore** (`Tolgrim_CraftSubcoreHigh`): Standard subcore, steel, industrial components (`HighMechtech`).
    - **Signal Chip** (`Tolgrim_CraftSignalChip`): Basic subcore, plasteel, uranium, industrial components (`StandardMechtech`).
    - **Powerfocus Chip** (`Tolgrim_CraftPowerfocusChip`): Signal chip, plasteel, uranium, industrial components (`HighMechtech`).
    - **Nano Structuring Chip** (`Tolgrim_CraftNanostructuringChip`): Powerfocus chip, plasteel, uranium, spacer components (`UltraMechtech`).
    - **Mechlink** (`Tolgrim_CraftMechlink`): Nano structuring chips, spacer components (`UltraMechtech`).
  - Configured with `mechanitorOnlyRecipe` and `SubcoreEncodingSpeed` work speed stat.
- **Dynamic Cost Adjustment & Availability (`SubcoreEncoderCraftingManager`):**
  - Real-time recipe ingredient count adjustment based on user settings via `IngredientCount.SetBaseCount`.
  - Harmony postfix on `RecipeDef.AvailableNow` to toggle recipe visibility and availability at runtime without restarts.
- **New ModSettings Tab: Subcore Crafting (`SettingsTab.Crafting`):**
  - Dedicated tab organizing workstation options.
  - Per-item enable/disable checkboxes.
  - Interactive numeric stepper entries (`Widgets.IntEntry`) with +/- increments to configure costs of each resource.
- **Localization:**
  - Added keyed labels, descriptions, and tab strings across English, Spanish, and SpanishLatin.

---

## [0.4.0] — 2026-11-12

### Feature: Control Group Renaming QoL, Balance Sliders & Apocriton Visual Fixes

#### Added
- **Mechanoid Control Group Renaming (`WorldComponent_ControlGroupNames` & `Dialog_RenameControlGroup`):**
  - Ability to rename mechanoid control groups using RimWorld's native rename window.
  - Rename button icon (`TexButton.Rename`) added directly to `MechanitorControlGroupGizmo`.
  - Right-click float menu context option to trigger rename dialog.
  - Real-time label rendering in the gizmo and in `MechanitorControlGroup.LabelIndexWithWorkMode`.
  - Custom group names displayed in the control group tooltip in `ITab_Pawn_Mechanitor`.
  - World-level persistence preserving names across save/load cycles.
- **Comprehensive Mechanitor & Mech Balance ModSettings (`SettingsTab.Balance`):**
  - Base command radius slider: 24.9 to 999 tiles.
  - General mechanoid work speed slider: 100% to 3000%.
  - Gestation speed slider: 100% to 3000%.
  - Instant gestation toggle (`instantMechGestation`).
  - Infinite energy toggle (`infiniteMechEnergy`).
  - Wastepack suppression toggle (`disableMechWastepacks`).
- **Visual Polish & Bug Fixes:**
  - Patched `CompEffecter.CompTick` to remove hostile pulsing/electrical effect (`ApocritonAttached`) from colony-controlled Apocritons.
  - Harmonized Neurofibre terminology and eliminated Spanglish in UI and DevMode inspector.
  - Replaced multiple split Neurofibre implants with a unified tiered implant progression.

---

## [0.3.0] — 2026-11-12

### Feature: Enemy Mechanoid Resurrection, Reprogramming & Apocriton Gestation

#### Added
- **Apocriton Gestation from Scratch:**
  - Added `Tolgrim_GestateApocriton` recipe to `LargeMechGestator` requiring `UltraMechtech` research, high subcore, boss chips (Signal, Powerfocus, Nanostructuring), plasteel, and steel.
  - Adjusted Apocriton colony balance: `BandwidthCost` (6), `ControlTakingTime` (30s), and `MechEnergyLossPerHP` (0.15).
  - Added `Corpse_Mech_Apocriton` to vanilla `ResurrectUltraheavyMech` recipe for standard friendly resurrection in gestators.
- **Hostile Mech Resurrection & Gestation:**
  - Integrated native gestation recipes (`Tolgrim_ResurrectHostileLightMech`, etc.) to gestate and reprogram downed/dead hostile mechanoids into colony mechanoids using subcores and steel.
  - Native support for `MechGestator` and `LargeMechGestator`.
  - Rebound instant resurrection ability via `Tolgrim_JobResurrectMech` job driver.
- **Friendly Apocriton Corrections:**
  - Harmony patch on `ThoughtWorker_HateAura` ensuring colony-aligned Apocritons do not trigger Hate Aura mental breaks on colonists.
  - Harmony patch on `Pawn.GetGizmos` allowing friendly Apocritons to recharge at colony mech chargers.
- **Mod Settings Integration:**
  - Toggle `enableResurrectEnemyMechs` in `MechanitorExpandedSettings` to dynamically enable or disable hostile mech resurrection at runtime.
- **Localization:** Keyed strings in English, Spanish, and SpanishLatin.

---

## [0.2.0] — 2026-11-12

### Phase 2: Neurofibre, Bandnode Sera & Bonus Architecture

#### Added
- **Bandwidth Bonus System (`BandwidthBonusRegistry`):**
  - Modular bonus provider framework supporting cumulative and non-cumulative modifiers.
  - Native `StatPart_MechanitorExpandedBandwidth` injected into `StatDefOf.MechBandwidth` for unified game-wide calculations without modifier duplication.
- **Per-Mechanitor Persistence (`CompMechanitorBandwidth`):**
  - Custom comp injected into pawns to safely store administered serum doses (`Scribe_Values`).
- **Mech Neurofibre Implants (Tiers I–IV):**
  - 4 progressive bandwidth implants (+3, +6, +9, +12).
  - Mutually exclusive surgical installation (`Recipe_InstallNeurofibre`) cleanly replacing lower tiers.
  - Tiered recipes at `StandardMechtech` and `HighMechtech` using spare Mechlinks, subcores, and components.
  - Native reuse of the vanilla Mechlink sprite (`Things/Pawn/Humanlike/Apparel/Mechlink`).
- **Bandnode Sera (`Tolgrim_BandnodeSerum`):**
  - Exotic ultratech usable item granting permanent +1 Bandwidth per dose (default cap: 36 doses).
  - Validation preventing administration to non-mechanitors or capped pawns.
  - Native reuse of the vanilla Healer Mech Serum sprite (`Things/Item/Special/MechSerumHealer`).
- **Mechanitor Tab & Tooltip Integration:**
  - Real-time bonus breakdown rows (Neurofibre, Bandnode Sera, other sources).
  - Native `StatWorker.GetExplanationFull` bandwidth tooltips showing exact contributor breakdown.
- **Expanded Mod Settings:**
  - Safe toggles for Neurofibre and Bandnode Sera without corrupting existing saves.
  - Configurable serum dose cap (1–100) and bonus multipliers (0.5x–3.0x).
- **Localization:** Full English, Spanish, and Latin American Spanish keys.

---

## [0.1.0] — 2026-11-12

### Phase 1: Native «Mechanitor» Inspector Tab

#### Added
- **Native Inspector Tab (`ITab_Pawn_Mechanitor`):**
  - Integrated into the pawn inspect pane alongside vanilla tabs.
  - Strict visibility: only displays on active mechanitors with an installed Mechlink.
  - Collapsible native accordions with smooth vertical scrolling.
- **Inspector Sections:**
  - **General:** Mechanitor status, total/used/free bandwidth, combat command radius (24.9 cells), controlled mechs, and control groups.
  - **Capabilities:** Real-time Biotech stats (`MechBandwidth`, `MechControlGroups`, repair/shield range, forming/repair speed).
  - **Implants & Equipment:** Dynamic detection of mechanitor-relevant implants and worn gear with compact bonus summaries.
- **Mod Settings System:**
  - Custom tab dimensions (width: 430–650px, height: 400–700px).
  - Row and section visibility toggles and default accordion states.
  - **Hot-swappable Recipe Routing:** Option to move mechanitor implant recipes from the Machining Table to the Subcore Encoder.
- **Patches & Compatibility:**
  - Non-destructive XML patches (`PatchOperationFindMod`) scoped to Biotech.
  - Full localization in English, Spanish, and Latin American Spanish.
