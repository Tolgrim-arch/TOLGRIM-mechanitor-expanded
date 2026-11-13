# Changelog — TOLGRIM: Mechanitor Expanded

All notable changes to this project are documented in this file.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and [SemVer](https://semver.org/).

---

## [0.10.0] — 2026-11-13

### Feature: Extended Mechanitor & Mech Balance Controls, Summon Gizmo Toggle & Incompatibilities

#### Added
- **Extended ModSettings Balance Sliders (`MechanitorBalancePatches.cs` & `MechanitorExpandedMod.cs`):**
  - **Bonus Mechanitor Bandwidth (`baseBandwidthBonus`):** Slider (`0` to `+100`) adding flat bonus bandwidth to all player mechanitors.
  - **Gestation Cycles Multiplier (`gestationCyclesMultiplier`):** Slider (`10%` to `100%`) dynamically scaling `RecipeDef.gestationCycles` via `MechGestationCyclesManager` without requiring a restart.
  - **Mechanoid Bandwidth Cost Multiplier (`mechBandwidthCostMultiplier`):** Slider (`25%` to `100%`) reducing bandwidth consumption for colony mechanoids.
  - **Mechanoid Movement Speed Multiplier (`mechMoveSpeedMultiplier`):** Slider (`100%` to `300%`) increasing movement speed for colony mechanoids.
- **No Summon Mech Threat Gizmo Integration:**
  - Added `hideSummonThreatGizmo` toggle in ModSettings (Interface tab) to hide `Command_CallBossgroup` from the mechanitor gizmo bar while keeping boss summoning available at the comms console.
- **Mod Incompatibilities (`About.xml`):**
  - Added `Canon.NoMechSummonerGizmo` and `AzzyH.ForceMechToDoAnything` to `<incompatibleWith>`.

---

## [0.9.3] — 2026-11-13

### Feature: Prevent Mechs Laydown Integration

#### Added
- **Keep Mechanoids Standing During Self-Shutdown (`PreventMechsLaydownPatches.cs`):**
  - Integrated configurable `PreventMechsLaydown` mechanics: keeps mechanoids standing (`PawnPosture.Standing`) during `SelfShutdown`, ensures `Need_MechEnergy.IsSelfShutdown` returns `true` while executing `JobDefOf.SelfShutdown`, and allows hauling depleted (`0%` energy) mechanoids to available chargers when not in `SelfShutdown` work mode.
  - Added `enablePreventMechsLaydown` toggle (default `false`) in ModSettings under *Mecanoides y Apocriton* with save persistence and localization (`English`, `Spanish`, `SpanishLatin`).

---

## [0.9.2] — 2026-11-13

### Feature: Forced Manual Mechanoid Orders & UI/Apocriton Polish

#### Added
- **Forced Right-Click Orders for Mechanoids (`ForcedMechOrdersPatches.cs`):**
  - Integrated configurable override for `FloatMenuOptionProvider.MechanoidCanDo` via Harmony `Postfix`, allowing mechanoids to receive direct right-click priority orders when enabled.
  - Added QoL equipment handling for colony mechanoids (`ForcedMechOrdersPatches.cs`): preserves native `destroyOnDrop` weapons in inventory when equipping external weapons, auto-restores them when unequipped, and drops external equipped weapons on death so they are not lost upon gestator resurrection.
  - Added `enableForcedMechOrders` toggle (default `false`) in ModSettings under *Mecanoides y Apocriton* with save persistence and localization (`English`, `Spanish`, `SpanishLatin`).

#### Changed & Fixed
- **Control Group Gizmo UI (`ControlGroupRenamePatches.cs`):**
  - Removed the redundant inline rename icon from the control group gizmo (still accessible via the right-click protocol menu) and expanded the custom group name area.
  - Replaced the flat-color label backing with a seamless slice of `Command.BGTex` / `Command.BGTexShrunk` to match the gizmo's native gradient.
- **Friendly Apocriton Inspect Card (`EnemyMechResurrectionPatches.cs`):**
  - Added `Patch_CompExplosive_CompInspectStringExtra_Apocriton` to hide the EMP death-pulse warning text on player-owned Apocritons, matching the already suppressed death-pulse behavior.

---

## [0.9.1] — 2026-11-12

### Feature: Native Mechs Hauling to Charger, Injured Mech Transport & Workshop Incompatibility

#### Added
- **Native Haul Mechs to Charger Integration (`MechHaulToChargerManager.cs`):**
  - Dynamically altered vanilla `HaulMechsToCharger` WorkGiverDef (`canBeDoneByMechs`), enabling colony worker mechanoids capable of hauling (such as Lifters) to transport downed, unpowered, or injured mechanoids to compatible rechargers.
  - Runtime toggle in ModSettings under *Mecanoides y Apocriton* (`enableMechsHaulToCharger`), allowing instant enablement or disabling without game restarts.
- **Injured Mechanoid Transport for Charger Repair (`Patch_WorkGiver_HaulMechToCharger_HasJobOnThing`):**
  - Extended `WorkGiver_HaulMechToCharger` to detect damaged or mutilated mechanoids when the Charger Repair system is unlocked (`BasicMechtech+`). Colonists and worker mechanoids automatically transport wounded mechs to available chargers even if battery levels are full.
  - Dedicated ModSettings toggle (`haulInjuredMechsToCharger`) with save persistence.
- **Charger Hold Extension during Repair (`Patch_JobGiver_GetEnergy_GetMaxRechargeLimit`):**
  - Postfixed `JobGiver_GetEnergy.GetMaxRechargeLimit` to prevent mechanoids from prematurely ending `JobDefOf.MechCharge` upon hitting 100% battery if they still have injuries or missing parts awaiting repair at a powered charger.
- **Mod Incompatibility Declaration (`About.xml`):**
  - Added native `<incompatibleWith><li>Witek.MechsCanHaulToCharger</li></incompatibleWith>` tag.
- **Localization:**
  - Added full keyed strings for hauling to chargers across `English`, `Spanish`, and `SpanishLatin`.

---

## [0.9.0] — 2026-11-12

### Feature: Native Charger Maintenance & Repair, Extended Incompatibilities & System Hardening

#### Added
- **Native Mech Charger Maintenance & Repair (`MechChargerRepairSystem.cs`):**
  - Integrated mechanoid maintenance and progressive repair mechanics directly into `Building_MechCharger`.
  - Research-based repair progression tied to Biotech tech tiers:
    - *Basic Mechtech* (Tier 1): 12.5 HP/hour.
    - *Standard Mechtech* (Tier 2): 25.0 HP/hour.
    - *High Mechtech* (Tier 3): 37.5 HP/hour.
    - *Ultra Mechtech* (Tier 4): 50.0 HP/hour.
  - Periodic injury healing hook on charger tick (250 ticks / 0.1 game hours).
  - Configurable missing and destroyed limb/component regeneration (2500 ticks / 1.0 game hours).
  - Real-time inspector component (`CompMechChargerRepairProgress`) dynamically attached to all mech chargers, displaying current tech tier, repair speed, target mechanoid name, and repair progress percentage.
  - ModSettings controls: Master toggle, restore missing parts toggle, and repair speed multiplier slider (50% to 500%).
- **Formal Mod Incompatibility Declarations (`About.xml`):**
  - Declared native `<incompatibleWith>` tags preventing concurrent loading with redundant or conflicting workshop mods:
    - `DanceNinja.UnbreakableMechChips` (Unbreakable Mech Chips)
    - `MrHydralisk.ForgetfulMechResurrection` (Forgetful Mech Resurrection)
    - `kathanon.MechColors` (Mech Colors)
    - `hyaukyuu.mechlinkreinforcement` (Mechlink Reinforcement)
    - `nikidigi.ResurrectEnemyMechanoids` (Resurrect Enemy Mechanoids)
    - `ShepirotGamer.FasterMechGestator` (Faster Mech Gestator)
    - `Slashandz.MechNoWastePack.1.5` (Mechs Don't Create Wastepacks)
    - `GG.craftablemechchips` (Craftable Mechtech Chips and Cores)
    - `swwu.MechanitorCommandRange` (Unlimited Mechanitor Command Range)
    - `forza.fasterWorkMechs` (Faster Workspeed for Mechs)
    - `notherkiller.mechchargersrepairmechs` (Mech Chargers Repair Mechs)

#### Fixed
- **Performance & Harmony Optimization:**
  - Replaced $O(N)$ global scans across `AllPawnsSpawned` with direct $O(1)$ lookups using `charger.CurrentlyChargingMech` and interaction cell inspection.
  - Added immediate tick hash short-circuiting in `Patch_Building_MechCharger_Tick` preventing per-tick evaluation when intervals are not met.
  - Replaced LINQ heap allocations in injury lookups with fast indexed iteration.
  - Fixed static dictionary memory leak in `Patch_Pawn_TickRare` (`ApocritonTicks`) by pruning destroyed, dead, or unspawned pawns.
  - Replaced dynamic reflection invocation in `WorkGiver_DoBill` with compiled `AccessTools.MethodDelegate` instances, removing per-call heap array allocations.
  - Optimized critical path in `StatWorker.GetValueUnfinalized` with early exit and cached `MechFormingSpeed` Def lookup.
  - Eliminated intermediate allocations in `BandwidthBonusRegistry.GetTotalBonus`.
- **Architecture, Surgery & Save Persistence:**
  - Implemented `Recipe_InstallNeurofibre` in `Source/NeurofibreRecipes.cs` with custom `GetPartsToApplyOn` and level-up logic (`ChangeLevel`), resolving surgery blockage where levels 2 through 6 could not be installed over level 1.
  - Fixed save-game color loss in `MechColorSystem` by ensuring `HasState` correctly handles pawns during `LoadingVars` before factions are resolved.
  - Added explicit detection and display for `Tolgrim_MechNeurofibre` in `ITab_Pawn_Mechanitor` so dynamic level bonuses appear accurately in the mechanitor inspect tab.
  - Hardened `ForgetfulMechResurrectionSystem` against world garbage collection, automatically sanitizing discarded or null pawn references during `ExposeData`.
- **Localization:**
  - Achieved strict 1:1 key parity (144 keys) across English, Spanish, and SpanishLatin in `MechanitorExpanded_Keyed.xml`.
  - Added missing `DefInjected` entries for 13 crafting and resurrection recipes and added localized job report strings (`reportString`) for `Tolgrim_JobResurrectMech`.

---

## [0.8.0] — 2026-11-12

### Feature: Native Mechanoid Inspect Tab (`ITab_Pawn_Mechanoid`) & Cleaned Color Dialog

#### Added
- **Native Mechanoid Inspection Tab (`ITab_Pawn_Mechanoid`):**
  - Added dedicated "Mecanoide" / "Mechanoid" tab to all player mechanoids alongside vanilla tabs (Registro, Equipo, Necesidad, Salud).
  - Displays mechanoid general information: Assigned overseer, control group and work mode, bandwidth cost, current energy level (with color-coded alerts), and wastepacks generated per recharge cycle.
  - Displays combat capabilities and stats: Movement speed, armor ratings (Sharp, Blunt, Heat), and primary weapon/equipment.
  - Integrated dedicated accent color section with real-time swatch preview, inheritance status ("Custom color" vs "Default color (inherited)"), and direct button to customize the color.
- **Redesigned Color Customization Modal (`Dialog_ChooseMechColor`):**
  - Replaced the cluttered base modal with a clean, proportioned window without overlapping UI elements.
  - Dedicated preview square, clean checkbox for default inheritance, structured grid palette with highlight indicators, and dedicated opacity slider.
- **Removed Intrusive Bottom Bar Gizmos:**
  - Removed persistent bottom gizmo buttons from mechanoids and mechanitors, decluttering the action command bar. Color customization is now accessed natively through the inspect tab or control group context menus.

---

## [0.7.0] — 2026-11-12

### Feature: Mechanitor Exclusive Resurrection & Memory (Forgetful Mech Resurrection Integration)

#### Added
- **Mechanitor Exclusive Resurrection Architecture (`ForgetfulMechResurrectionSystem.cs` & `ForgetfulMechResurrectionPatches.cs`):**
  - Integrated native memory tracking so mechanitors can only resurrect their own deceased mechanoids unless manually ordered to forget.
  - On mechanoid death (`Pawn_RelationsTracker.RemoveRelationsOnDeath`), the relation with the overseer mechanitor and the exact control group index are preserved in `WorldComponent_ForgetfulMechResurrection`.
  - When resurrected in a gestator (`Bill_ResurrectMech.CreateProducts`), mechs automatically rejoin their former control group instead of going into group 1 or unassigned.
  - Job validation checks (`Bill_ResurrectMech.PawnAllowedToStartAnew` & `WorkGiver_DoBill.TryFindBestBillIngredients`) ensure mechanitors will not pick up or attempt to resurrect mechanoid corpses assigned to another colonist mechanitor.
- **Corpse Interaction & Commands:**
  - Added "Forget overseer" command (`Tolgrim_GizmoForgetOverseerLabel`) to mechanoid corpses with the `UI/Buttons/MakeDefault` icon, allowing players to unbind deceased mechs so any other mechanitor can resurrect them.
  - Added "Select overseer" command to corpses to quickly highlight and inspect the mechanitor who previously owned the mech.
- **Configurability & Localization:**
  - Runtime toggle `enableForgetfulMechResurrection` in ModSettings under the Mechanoids tab.
  - Full translations in English, Spanish, and SpanishLatin.

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
