using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Tolgrim_MechanitorExpanded
{
    /// <summary>
    /// Gestión de la progresión de investigación y utilidades de reparación en cargadores de mecanoides.
    /// </summary>
    public static class MechChargerRepairSystem
    {
        private static readonly string[] Projects = new string[4]
        {
            "BasicMechtech",
            "StandardMechtech",
            "HighMechtech",
            "UltraMechtech"
        };

        public static int CurrentTier
        {
            get
            {
                if (Find.ResearchManager == null)
                {
                    return 0;
                }
                for (int i = Projects.Length - 1; i >= 0; i--)
                {
                    ResearchProjectDef? def = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(Projects[i]);
                    if (def != null && def.IsFinished)
                    {
                        return i + 1;
                    }
                }
                return 0;
            }
        }

        public static float BaseHpPerHour => (float)CurrentTier * 12.5f;

        public static float EffectiveHpPerHour => BaseHpPerHour * MechanitorExpandedMod.Settings.chargerRepairSpeedMultiplier;

        public static string TierName(int tier)
        {
            if (tier < 1 || tier > Projects.Length)
            {
                return "Tolgrim_MCRM_NotUnlocked".Translate();
            }
            ResearchProjectDef? def = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(Projects[tier - 1]);
            if (def != null)
            {
                return def.LabelCap;
            }
            return Projects[tier - 1];
        }

        public static bool IsValidMech(Pawn? pawn)
        {
            if (pawn == null || pawn.Destroyed || pawn.Dead)
            {
                return false;
            }
            if (pawn.RaceProps == null || !pawn.RaceProps.IsMechanoid)
            {
                return false;
            }
            return true;
        }

        public static bool IsChargingAt(Pawn pawn, Building_MechCharger charger)
        {
            if (charger == null || charger.Map == null || pawn == null || pawn.Map != charger.Map || !IsValidMech(pawn) || pawn.needs == null)
            {
                return false;
            }

            Need_MechEnergy? need = pawn.needs.TryGetNeed<Need_MechEnergy>();
            if (need == null)
            {
                return false;
            }

            if (need.currentCharger == charger)
            {
                return true;
            }
            if (need.currentCharger != null)
            {
                return false;
            }

            Job? curJob = pawn.jobs?.curJob;
            if (curJob != null && pawn.Position.InHorDistOf(charger.Position, 5f))
            {
                if (curJob.targetA.Thing == charger || curJob.targetB.Thing == charger || curJob.targetC.Thing == charger)
                {
                    return true;
                }
            }

            return false;
        }

        public static Pawn? FindChargingMech(Building_MechCharger charger)
        {
            if (charger == null || !charger.Spawned || charger.Map == null)
            {
                return null;
            }

            // 1. Acceso O(1) directo a la propiedad nativa de Biotech
            Pawn currentlyCharging = charger.CurrentlyChargingMech;
            if (currentlyCharging != null && IsValidMech(currentlyCharging))
            {
                return currentlyCharging;
            }

            // 2. Comprobar contenedor interno si aplica
            CompThingContainer? container = charger.Container;
            if (container != null)
            {
                ThingOwner? directlyHeld = (container as IThingHolder)?.GetDirectlyHeldThings();
                if (directlyHeld != null && directlyHeld.Count > 0)
                {
                    for (int i = 0; i < directlyHeld.Count; i++)
                    {
                        if (directlyHeld[i] is Pawn p && IsValidMech(p))
                        {
                            return p;
                        }
                    }
                }
            }

            // 3. Comprobación localizada en la celda de interacción del cargador
            IntVec3 interactionCell = charger.InteractionCell;
            if (interactionCell.InBounds(charger.Map))
            {
                List<Thing> thingList = interactionCell.GetThingList(charger.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i] is Pawn p && IsValidMech(p) && IsChargingAt(p, charger))
                    {
                        return p;
                    }
                }
            }

            return null;
        }

        public static Hediff_Injury? GetWorstRepairableInjury(Pawn mech)
        {
            if (mech?.health?.hediffSet == null)
            {
                return null;
            }
            List<Hediff> hediffs = mech.health.hediffSet.hediffs;
            Hediff_Injury? worst = null;
            float maxSev = 0f;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i] is Hediff_Injury inj && inj.Severity > maxSev)
                {
                    worst = inj;
                    maxSev = inj.Severity;
                }
            }
            return worst;
        }

        public static Hediff_MissingPart? GetMissingPartToRestore(Pawn mech)
        {
            if (mech?.health?.hediffSet == null)
            {
                return null;
            }
            List<Hediff> hediffs = mech.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i] is Hediff_MissingPart mp && mp.Part != null)
                {
                    return mp;
                }
            }
            return null;
        }

        public static bool RestoreMissingPart(Pawn mech, Hediff_MissingPart missingPart)
        {
            if (mech == null || missingPart == null || missingPart.Part == null)
            {
                return false;
            }
            try
            {
                mech.health.RestorePart(missingPart.Part, null, true);
                return true;
            }
            catch (Exception ex)
            {
                Log.ErrorOnce($"[TOLGRIM] Failed to restore missing mech part: {ex}", 37482911);
                return false;
            }
        }
    }

    /// <summary>
    /// Componente que registra el progreso de reparación en el cargador y expone la información al jugador.
    /// </summary>
    public sealed class CompProperties_MechChargerRepairProgress : CompProperties
    {
        public CompProperties_MechChargerRepairProgress()
        {
            compClass = typeof(CompMechChargerRepairProgress);
        }
    }

    public sealed class CompMechChargerRepairProgress : ThingComp
    {
        private string? trackedPawnId;
        private float initialDamage;
        private float latestDamage;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref trackedPawnId, "tolgrim_repairPawn", null);
            Scribe_Values.Look(ref initialDamage, "tolgrim_startDamage", 0f);
            Scribe_Values.Look(ref latestDamage, "tolgrim_currentDamage", 0f);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent != null && parent.IsHashIntervalTick(250))
            {
                RefreshSession();
            }
        }

        public float CalculateRemainingWork(Pawn? pawn)
        {
            if (pawn?.health?.hediffSet == null)
            {
                return 0f;
            }
            float total = 0f;
            bool restoreParts = MechanitorExpandedMod.Settings.restoreMissingPartsInCharger;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff h = hediffs[i];
                if (h is Hediff_Injury inj && inj.Severity > 0f)
                {
                    total += inj.Severity;
                }
                else if (restoreParts && h is Hediff_MissingPart)
                {
                    total += 10f;
                }
            }
            return total;
        }

        public Pawn? RefreshSession()
        {
            if (parent is not Building_MechCharger charger)
            {
                return null;
            }

            Pawn? mech = MechChargerRepairSystem.FindChargingMech(charger);
            if (mech == null)
            {
                return null;
            }

            string uid = mech.GetUniqueLoadID();
            float rem = CalculateRemainingWork(mech);

            if (trackedPawnId != uid)
            {
                trackedPawnId = uid;
                initialDamage = rem;
            }
            else if (rem > initialDamage)
            {
                initialDamage = rem;
            }

            latestDamage = rem;
            return mech;
        }

        public override string CompInspectStringExtra()
        {
            if (!MechanitorExpandedMod.Settings.enableChargerRepair)
            {
                return string.Empty;
            }

            Pawn? mech = RefreshSession();
            int current = MechChargerRepairSystem.CurrentTier;
            string text = "Tolgrim_MCRM_RepairTier".Translate(current, MechChargerRepairSystem.TierName(current)) + "\n" +
                          "Tolgrim_MCRM_RepairSpeed".Translate(MechChargerRepairSystem.EffectiveHpPerHour.ToString("0.#"));

            if (current < 4)
            {
                text += "\n" + "Tolgrim_MCRM_NextTier".Translate(MechChargerRepairSystem.TierName(current + 1));
            }

            if (mech == null)
            {
                return text + "\n" + "Tolgrim_MCRM_NoMech".Translate();
            }

            text += "\n" + "Tolgrim_MCRM_Mech".Translate(mech.LabelShort);

            if (current == 0)
            {
                return text + "\n" + "Tolgrim_MCRM_Locked".Translate();
            }

            CompPowerTrader? power = parent.TryGetComp<CompPowerTrader>();
            if (power != null && !power.PowerOn)
            {
                text += "\n" + "Tolgrim_MCRM_Paused".Translate();
            }

            int percent = (int)Math.Round(((initialDamage <= 0.001f) ? 1f : Math.Max(0f, Math.Min(1f, (initialDamage - latestDamage) / initialDamage))) * 100f);
            text += "\n" + "Tolgrim_MCRM_RepairProgress".Translate(percent);

            if (latestDamage <= 0.001f)
            {
                text += "\n" + "Tolgrim_MCRM_Complete".Translate();
            }

            return text;
        }
    }

    /// <summary>
    /// Inyección de CompProperties_MechChargerRepairProgress en todos los Building_MechCharger al inicio.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class MechChargerRepairBootstrap
    {
        static MechChargerRepairBootstrap()
        {
            try
            {
                int count = 0;
                List<ThingDef> allDefs = DefDatabase<ThingDef>.AllDefsListForReading;
                for (int i = 0; i < allDefs.Count; i++)
                {
                    ThingDef def = allDefs[i];
                    if (def.thingClass == null || !typeof(Building_MechCharger).IsAssignableFrom(def.thingClass))
                    {
                        continue;
                    }

                    if (def.comps == null)
                    {
                        def.comps = new List<CompProperties>();
                    }

                    bool exists = false;
                    for (int j = 0; j < def.comps.Count; j++)
                    {
                        if (def.comps[j] is CompProperties_MechChargerRepairProgress)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        def.comps.Add(new CompProperties_MechChargerRepairProgress());
                        count++;
                    }
                }
                Log.Message($"[TOLGRIM: Mechanitor Expanded] Mech charger repair comp registered on {count} charger definitions.");
            }
            catch (Exception ex)
            {
                Log.Error($"[TOLGRIM: Mechanitor Expanded] Failed to bootstrap charger repair comp: {ex}");
            }
        }
    }

    /// <summary>
    /// Parche en Building_MechCharger.Tick para ejecutar la reparación pasiva cada 250 ticks y restauración de partes cada 2500 ticks.
    /// </summary>
    [HarmonyPatch(typeof(Building_MechCharger), "Tick")]
    public static class Patch_Building_MechCharger_Tick
    {
        public static void Postfix(Building_MechCharger __instance)
        {
            if (!MechanitorExpandedMod.Settings.enableChargerRepair)
            {
                return;
            }

            if (__instance == null || !__instance.Spawned)
            {
                return;
            }

            // Comprobación de intervalos antes de ejecutar cualquier lógica costosa
            bool doHeal = __instance.IsHashIntervalTick(250);
            bool doRestore = MechanitorExpandedMod.Settings.restoreMissingPartsInCharger && __instance.IsHashIntervalTick(2500);

            if (!doHeal && !doRestore)
            {
                return;
            }

            CompPowerTrader? power = __instance.TryGetComp<CompPowerTrader>();
            if (power != null && !power.PowerOn)
            {
                return;
            }

            if (MechChargerRepairSystem.CurrentTier == 0)
            {
                return;
            }

            Pawn? mech = MechChargerRepairSystem.FindChargingMech(__instance);
            if (mech == null)
            {
                return;
            }

            // Reparar heridas (intervalo de 250 ticks = 0.1 horas de juego)
            if (doHeal)
            {
                Hediff_Injury? worst = MechChargerRepairSystem.GetWorstRepairableInjury(mech);
                if (worst != null)
                {
                    float hpPerHour = MechChargerRepairSystem.EffectiveHpPerHour;
                    float healAmount = hpPerHour * 0.1f; // 250 ticks = 0.1 horas
                    worst.Heal(healAmount);
                }
            }

            // Restaurar piezas perdidas si la opción está activada (intervalo de 2500 ticks = 1 hora de juego)
            if (doRestore)
            {
                Hediff_MissingPart? missing = MechChargerRepairSystem.GetMissingPartToRestore(mech);
                if (missing != null)
                {
                    MechChargerRepairSystem.RestoreMissingPart(mech, missing);
                }
            }
        }
    }
}
