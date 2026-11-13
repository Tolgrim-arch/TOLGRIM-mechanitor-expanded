# TOLGRIM: Mechanitor Expanded

**RimWorld 1.6+ | Requires Biotech DLC**

A Vanilla-Expanded-style overhaul and quality-of-life suite for Mechanitors and Mechanoids. Designed with native integration, full save compatibility, and granular Mod Settings toggles.

> [Leer en Español](README.es.md)

---

## Features

### 1. Native Mechanitor Inspection Tab
- Adds a dedicated **Mechanitor** tab (`ITab_Pawn_Mechanitor`) to pawns with a Mechlink.
- Collapsible sections: **General Information**, **Capabilities**, **Implants**, and **Equipment**.
- Configurable dimensions, section visibility, and optional relocation of mechanitor implant recipes to the **Subcore Encoder**.

### 2. Bandwidth Reinforcement
- **Mech Neurofibre I–IV:** Mutually exclusive, research-gated implants granting `+3`, `+6`, `+9`, and `+12` bandwidth.
- **Bandnode Sera:** Consumable serum granting `+1` bandwidth per dose (configurable cap, default `36`).
- Full integration with `StatDefOf.MechBandwidth` and dynamic breakdown tooltip in the Mechanitor tab.

### 3. Hostile Mechanoid Resurrection & Apocriton Support
- Reprogramming recipes in Mechanoid Gestators to resurrect enemy mechanoid corpses using subcores and steel.
- **Apocriton Colony Support:**
  - Balanced gestation recipe (`Tolgrim_GestateApocriton`) in `LargeMechGestator` (`UltraMechtech`, Bandwidth: `6`).
  - Suppresses Hate Aura, hostile visual effects, and EMP death-pulse warning/detonation on player-owned Apocritons.
  - Enables Mech Recharger gizmos for friendly Apocritons.

### 4. Charger Maintenance, Repair & Hauling
- **Passive Charger Repair:** Mechanoids actively charging at powered rechargers automatically heal injuries and optionally regenerate missing parts, scaling with Biotech research tiers (`BasicMechtech` → `UltraMechtech`).
- **Haul Mechs to Charger:** Allows worker mechanoids (e.g. Lifters) and colonists to haul downed, depleted, or injured mechanoids to available chargers.
- **Keep Mechs Standing During Self-Shutdown:** Optional toggle to keep mechanoids standing during `SelfShutdown` and allow hauling depleted (`0%` energy) mechanoids to chargers.

### 5. Control Groups, Colors & Manual Orders
- **Control Group Renaming & Colors:** Right-click control group gizmos to rename groups or customize mech color themes.
- **Forced Manual Mechanoid Orders:** Optional toggle enabling direct right-click priority orders for mechanoids, including QoL weapon handling (preserves native `destroyOnDrop` weapons in inventory when equipping external weapons, restores them on unequip, and drops external weapons on death).

---

## Configuration

All major features can be toggled or tuned in **Options → Mod Settings → TOLGRIM: Mechanitor Expanded**.

---

## License & Credits

- **Author:** TOLGRIM
- **PackageId:** `Tolgrim.MechanitorExpanded`
- **Dependencies:** RimWorld Biotech, Harmony (`0Harmony.dll`)
