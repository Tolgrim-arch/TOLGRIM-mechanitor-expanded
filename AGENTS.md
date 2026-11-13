# AGENTS.md — TOLGRIM: Mechanitor Expanded

Permanent guidance for AI development agents working in this repository.

## 1. Project context

- **Mod:** TOLGRIM: Mechanitor Expanded
- **PackageId:** `Tolgrim.MechanitorExpanded`
- **Target game:** RimWorld `1.6.4871+`
- **Required DLC:** Biotech (`Ludeon.RimWorld.Biotech`)
- **Technology:** C#, XML, .NET Framework 4.8 (`net48`), C# `latest`, nullable enabled
- **Repository:** `C:\Users\jelys\3D Objects\antigravity-projects\rimworld-modding\TOLGRIM-mechanitor-expanded`
- **Local installation:** `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\TOLGRIM - Mechanitor Expanded`
- **Changelog:** `C:\Users\jelys\3D Objects\antigravity-projects\rimworld-modding\TOLGRIM-mechanitor-expanded\CHANGELOG.md`

The mod follows a **Vanilla Expanded** philosophy, preserving RimWorld's aesthetics, mechanics, and conventions. Feature scope is defined by each task, not this file.

## 2. Permanent engineering rules

1. **Vanilla first.** Reuse RimWorld/Biotech APIs, `Defs`, stats, components, UI, and assets. Prefer native extension points such as `ITab`, `StatPart`, `ThingComp`, `CompUseEffect`, and recipes. Use Harmony only when necessary.
2. **Simplicity and modularity.** Apply KISS and DRY; use SOLID where it improves clarity. Avoid duplication, one-off exceptions, and speculative abstractions. Inspect and reuse existing architecture before making changes.
3. **Non-destructive changes.** Avoid replacing whole vanilla `Defs` or methods. Use targeted, conditional XML patches where appropriate; do not add redundant `PatchOperationFindMod` checks for guaranteed dependencies.
4. **Native UI.** Prefer integrating mechanitor features into the existing `ITab_Pawn_Mechanitor` alongside pawn inspection tabs. Preserve vanilla styling, controls, tooltips, and existing collapsible sections. Do not add windows or nested tabs unless requested.
5. **Synchronized visual assets.** Reference Core/Biotech assets instead of copying PNGs. Verify actual definitions and paths for the Mechlink and healer mech serum (possible paths: `Things/Pawn/Humanlike/Apparel/Mechlink`, `Things/Item/Special/MechSerumHealer`). Support third-party texture replacements where technically feasible; never assume universal compatibility.
6. **Reliable stats.** Integrate bonuses with native calculations, track their sources, prevent double application, and keep displayed explanations consistent with actual gameplay values.
7. **Persistence and compatibility.** Use native serialization and entity-bound components when appropriate. Protect existing saves; disabling a feature in Mod Settings must not destroy saved data or leave invalid references.
8. **Localization.** All player-facing text must be localized in `1.6/Languages/English/Keyed/`, `1.6/Languages/Spanish/Keyed/`, and `1.6/Languages/SpanishLatin/Keyed/`.
9. **Performance.** Avoid costly per-tick work, repeated global scans, and invasive patches. Cache only when necessary, with correct invalidation.
10. **Verification and scope.** Implement only what was requested. Verify real APIs before using them, build and run available tests, and never equate successful compilation with in-game validation.

## 3. Implemented features (context, not pending tasks)

### Phase 1 — Mechanitor interface

- Native `ITab_Pawn_Mechanitor`, visible only for pawns with a Mechlink.
- Collapsible sections: General Information, Capabilities, Implants, and Equipment.
- Mod Settings for tab dimensions and visibility.
- Optional runtime relocation of mechanitor implant recipes to the **Subcore Encoder**.

### Phase 2 — Bandwidth reinforcement

- Modular permanent bonus registry: `BandwidthBonusRegistry`.
- Integration with `StatDefOf.MechBandwidth` through `StatPart_MechanitorExpandedBandwidth`.
- **Mech Neurofibre I–IV** implants: +3, +6, +9, and +12; mutually exclusive and research-gated.
- **Bandnode Sera**: +1 bandwidth per dose; configurable cap, default 36.
- Per-mechanitor persistence through `CompMechanitorBandwidth`.
- Dynamic bonus breakdown tooltip in the Mechanitor tab.

### Phase 3 — Hostile Mechanoid Resurrection & Reprogramming

- Reprogramming recipes for gestators (`MechGestator`, `LargeMechGestator`) to resurrect enemy mechanoid corpses using subcores and steel.
- Ability and job driver `Tolgrim_JobResurrectMech` for instant battlefield resurrection and faction alignment.
- Friendly Apocriton behavior patches: suppresses Hate Aura on colonists and enables mech recharger gizmos.
- Gestation of Apocriton from scratch (`Tolgrim_GestateApocriton`) in `LargeMechGestator` with balanced colony stats (Bandwidth: 6, UltraMechtech).
- Friendly Apocriton resurrection patch in vanilla `ResurrectUltraheavyMech`.
- Runtime toggle in Mod Settings (`enableResurrectEnemyMechs`) with zero overhead when disabled.
- Harmony integration (`0Harmony.dll`) referenced as compile dependency.

These descriptions reflect the reported project state; inspect the code before assuming implementation details.

## 4. Build and deployment

Run commands from the repository root.

### Build C#

```powershell
& "C:\Users\jelys\scoop\apps\dotnet-sdk\current\dotnet.exe" build "Source\Tolgrim_MechanitorExpanded.csproj" -c Release
```

### Sync to local game installation

```powershell
Copy-Item -Path "Source\1.6\Assemblies\*" -Destination "1.6\Assemblies" -Force
$target = "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\TOLGRIM - Mechanitor Expanded"
Copy-Item -Path "About" -Destination "$target\About" -Recurse -Force
Copy-Item -Path "1.6" -Destination "$target\1.6" -Recurse -Force
```

Before deploying, confirm that paths exist and the build succeeded. Do not delete unrelated files at the destination.

## 5. Workflow

**Inspect → Implement → Verify → Report.**

After each task, briefly summarize changes, affected files, tests performed, and remaining limitations. Always prioritize **native integration, stability, compatibility, and simplicity**.

### Git & GitHub Workflow

The user (TOLGRIM) is actively working toward the **Pull Shark** and **Pair Extraordinaire** GitHub achievements. While developing the mod:

- **Use branches and PRs:** Create feature/fix branches, push meaningful changes, open Pull Requests, and merge them rather than committing directly to the default branch.
- **Include co-authors:** Add the following trailers to every commit message:

```text
Co-Authored-By: Claude <noreply@anthropic.com>
Co-authored-by: Codex <noreply@openai.com>
```