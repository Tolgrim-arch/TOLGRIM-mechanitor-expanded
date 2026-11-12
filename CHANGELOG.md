# Changelog — TOLGRIM: Mechanitor Expanded

All notable changes to this project are documented in this file.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) and [SemVer](https://semver.org/).

---

## [0.3.0] — 2026-11-12

### Feature: Enemy Mechanoid Resurrection & Reprogramming

#### Added
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
