using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Tolgrim_MechanitorExpanded
{
    /// <summary>
    /// Gestión e integración nativa de transporte de mecanoides al cargador.
    /// Permite que mecanoides con capacidad de transporte (Lifters, etc.) transporten mecanoides,
    /// y que mecanoides heridos/dañados sean llevados a un cargador para repararse pasivamente.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class MechHaulToChargerManager
    {
        private static bool originalCanBeDoneByMechsSet = false;
        private static bool originalCanBeDoneByMechs = false;

        static MechHaulToChargerManager()
        {
            ApplySettings();
        }

        public static void ApplySettings()
        {
            WorkGiverDef? def = DefDatabase<WorkGiverDef>.GetNamedSilentFail("HaulMechsToCharger");
            if (def == null)
            {
                return;
            }

            if (!originalCanBeDoneByMechsSet)
            {
                originalCanBeDoneByMechs = def.canBeDoneByMechs;
                originalCanBeDoneByMechsSet = true;
            }

            bool enabled = MechanitorExpandedMod.Settings?.enableMechsHaulToCharger ?? true;
            def.canBeDoneByMechs = enabled;
        }

        public static bool MechNeedsRepair(Pawn mech)
        {
            if (mech?.health?.hediffSet == null)
            {
                return false;
            }

            List<Hediff> hediffs = mech.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i] is Hediff_Injury inj && inj.Severity > 0.01f)
                {
                    return true;
                }
                if (MechanitorExpandedMod.Settings.restoreMissingPartsInCharger && hediffs[i] is Hediff_MissingPart mp && mp.Part != null)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsMechCurrentlyChargingAtPoweredCharger(Pawn mech)
        {
            if (mech?.CurJobDef != JobDefOf.MechCharge)
            {
                return false;
            }

            Building_MechCharger? charger = mech.CurJob?.targetA.Thing as Building_MechCharger;
            if (charger == null || !charger.Spawned)
            {
                return false;
            }

            CompPowerTrader? power = charger.TryGetComp<CompPowerTrader>();
            if (power != null && !power.PowerOn)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Extiende WorkGiver_HaulMechToCharger para permitir transportar mecanoides heridos hacia un cargador
    /// cuando el sistema de autorreparación en cargadores está desbloqueado y activo.
    /// </summary>
    [HarmonyPatch(typeof(WorkGiver_HaulMechToCharger), nameof(WorkGiver_HaulMechToCharger.HasJobOnThing))]
    public static class Patch_WorkGiver_HaulMechToCharger_HasJobOnThing
    {
        public static void Postfix(WorkGiver_HaulMechToCharger __instance, Pawn pawn, Thing t, bool forced, ref bool __result)
        {
            // Si ya tiene trabajo asignado por vanilla, no interferir
            if (__result)
            {
                return;
            }

            MechanitorExpandedSettings? s = MechanitorExpandedMod.Settings;
            if (s == null || !s.haulInjuredMechsToCharger || !s.enableChargerRepair)
            {
                return;
            }

            // Solo aplica si la colonia ha desbloqueado al menos el nivel 1 de reparación (BasicMechtech)
            if (MechChargerRepairSystem.CurrentTier == 0)
            {
                return;
            }

            if (!ModLister.CheckBiotech("Haul mech to recharger"))
            {
                return;
            }

            if (!(t is Pawn { Spawned: true } mech) || !mech.RaceProps.IsMechanoid || !mech.IsColonyMech)
            {
                return;
            }

            // Un mecanoide no se transporta a sí mismo
            if (mech == pawn)
            {
                return;
            }

            // Si ya se encuentra cargándose activamente
            if (mech.CurJobDef == JobDefOf.MechCharge)
            {
                return;
            }

            // Respetar modo de apagado voluntario
            MechanitorControlGroup? group = mech.GetMechControlGroup();
            if (group != null && group.WorkMode == MechWorkModeDefOf.SelfShutdown)
            {
                return;
            }

            // Verificar si el mecanoide tiene heridas o partes faltantes a reparar
            if (!MechHaulToChargerManager.MechNeedsRepair(mech))
            {
                return;
            }

            // Comprobar reserva
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return;
            }

            // Buscar cargador compatible y disponible
            Building_MechCharger? closestCharger = JobGiver_GetEnergy_Charger.GetClosestCharger(mech, pawn, forced);
            if (closestCharger != null)
            {
                __result = true;
            }
        }
    }

    /// <summary>
    /// Retiene al mecanoide en el cargador si su energía ya alcanzó el máximo pero aún tiene heridas
    /// o miembros faltantes por regenerar en el cargador activo.
    /// </summary>
    [HarmonyPatch(typeof(JobGiver_GetEnergy), nameof(JobGiver_GetEnergy.GetMaxRechargeLimit))]
    public static class Patch_JobGiver_GetEnergy_GetMaxRechargeLimit
    {
        public static void Postfix(Pawn pawn, ref float __result)
        {
            MechanitorExpandedSettings? s = MechanitorExpandedMod.Settings;
            if (s == null || !s.enableChargerRepair)
            {
                return;
            }

            if (MechChargerRepairSystem.CurrentTier == 0)
            {
                return;
            }

            // Solo retener si el mecanoide está actualmente cargándose en un cargador con energía y necesita reparación
            if (pawn != null && MechHaulToChargerManager.IsMechCurrentlyChargingAtPoweredCharger(pawn))
            {
                if (MechHaulToChargerManager.MechNeedsRepair(pawn))
                {
                    // Al devolver un valor superior al nivel máximo de energía, JobDriver_MechCharge no finalizará prematuramente
                    __result = float.MaxValue;
                }
            }
        }
    }
}
