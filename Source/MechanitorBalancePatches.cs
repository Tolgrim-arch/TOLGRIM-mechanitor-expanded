using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    /// <summary>
    /// Ajusta el rango de comando base del mecanizador cuando se modifica en ModSettings.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_MechanitorTracker), nameof(Pawn_MechanitorTracker.CanCommandTo))]
    public static class Patch_Pawn_MechanitorTracker_CanCommandTo
    {
        public static bool Prefix(Pawn ___pawn, LocalTargetInfo target, ref bool __result)
        {
            float customRange = MechanitorExpandedMod.Settings.baseCommandRange;
            if (customRange <= 24.91f)
            {
                return true;
            }

            if (!target.IsValid || ___pawn == null || !___pawn.Spawned)
            {
                __result = false;
                return false;
            }

            if (___pawn.Position.InHorDistOf(target.Cell, customRange))
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Dibuja el círculo visual del radio de comando del mecanizador adaptado a baseCommandRange.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_MechanitorTracker), nameof(Pawn_MechanitorTracker.DrawCommandRadius))]
    public static class Patch_Pawn_MechanitorTracker_DrawCommandRadius
    {
        public static bool Prefix(Pawn_MechanitorTracker __instance, Pawn ___pawn)
        {
            float customRange = MechanitorExpandedMod.Settings.baseCommandRange;
            if (customRange <= 24.91f)
            {
                return true;
            }

            if (___pawn != null && ___pawn.Spawned && __instance.AnySelectedDraftedMechs)
            {
                // Limitar a MaxRadialPatternRadius para evitar desbordar el búfer de círculos radiales de GenDraw
                float maxSafeRadius = GenRadial.MaxRadialPatternRadius - 1f;
                float drawRadius = UnityEngine.Mathf.Min(customRange, maxSafeRadius);

                GenDraw.DrawRadiusRing(___pawn.Position, drawRadius, UnityEngine.Color.white, (IntVec3 c) => __instance.CanCommandTo(c));
            }
            return false;
        }
    }

    /// <summary>
    /// Ajusta WorkSpeedGlobal de mecanoides aliados, MechFormingSpeed y WastepacksPerRecharge según ModSettings con costo O(1).
    /// </summary>
    [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetValueUnfinalized))]
    public static class Patch_StatWorker_GetValueUnfinalized_Balance
    {
        private static StatDef? mechFormingSpeedDef;
        private static bool initializedMechFormingSpeed;

        private static StatDef? MechFormingSpeedDef
        {
            get
            {
                if (!initializedMechFormingSpeed)
                {
                    mechFormingSpeedDef = DefDatabase<StatDef>.GetNamedSilentFail("MechFormingSpeed");
                    initializedMechFormingSpeed = true;
                }
                return mechFormingSpeedDef;
            }
        }

        public static void Postfix(StatDef ___stat, StatRequest req, ref float __result)
        {
            if (___stat == null)
            {
                return;
            }

            MechanitorExpandedSettings settings = MechanitorExpandedMod.Settings;
            if (settings == null)
            {
                return;
            }

            // 1. Desechos tóxicos por ciclo de recarga
            if (settings.disableMechWastepacks && ___stat == StatDefOf.WastepacksPerRecharge)
            {
                __result = 0f;
                return;
            }

            // 2. Filtro ultrarrápido: descartar inmediatamente cualquier otra estadística sin comprobaciones de Pawn
            StatDef? formingSpeed = MechFormingSpeedDef;
            bool isWorkSpeed = ___stat == StatDefOf.WorkSpeedGlobal && settings.mechWorkSpeedMultiplier > 1.001f;
            bool isFormingSpeed = formingSpeed != null && ___stat == formingSpeed && (settings.instantMechGestation || settings.mechGestationSpeedMultiplier > 1.001f);

            if (!isWorkSpeed && !isFormingSpeed)
            {
                return;
            }

            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            // 3. Procesar solo para peones pertenecientes al jugador
            if (req.Thing is Pawn pawn && pawn.Faction != null && pawn.Faction.IsPlayer)
            {
                if (isWorkSpeed && pawn.RaceProps != null && pawn.RaceProps.IsMechanoid)
                {
                    __result *= settings.mechWorkSpeedMultiplier;
                }
                else if (isFormingSpeed)
                {
                    if (settings.instantMechGestation)
                    {
                        __result = 99999f;
                    }
                    else
                    {
                        __result *= settings.mechGestationSpeedMultiplier;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Mantiene la energía de los mecanoides de la colonia al máximo cuando la energía infinita está activada.
    /// </summary>
    [HarmonyPatch(typeof(Need_MechEnergy), nameof(Need_MechEnergy.NeedInterval))]
    public static class Patch_Need_MechEnergy_NeedInterval
    {
        public static bool Prefix(Need_MechEnergy __instance, Pawn ___pawn)
        {
            if (!MechanitorExpandedMod.Settings.infiniteMechEnergy)
            {
                return true;
            }

            if (___pawn != null && ___pawn.Faction != null && ___pawn.Faction.IsPlayer)
            {
                __instance.CurLevel = __instance.MaxLevel;
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Oculta el comando de invocar amenazas de jefes mecanoides (Command_CallBossgroup) en el mecanizador según ModSettings.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_MechanitorTracker), nameof(Pawn_MechanitorTracker.GetGizmos))]
    public static class Patch_Pawn_MechanitorTracker_GetGizmos_HideBossCall
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result)
        {
            if (!MechanitorExpandedMod.Settings.hideSummonThreatGizmo)
            {
                foreach (Gizmo gizmo in __result)
                {
                    yield return gizmo;
                }
                yield break;
            }

            foreach (Gizmo gizmo in __result)
            {
                if (gizmo is not Command_CallBossgroup)
                {
                    yield return gizmo;
                }
            }
        }
    }
}
