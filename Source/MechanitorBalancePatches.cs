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

            if (___pawn.Position.DistanceTo(target.Cell) <= customRange)
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
            if (___pawn != null && ___pawn.Spawned && __instance.AnySelectedDraftedMechs)
            {
                float customRange = MechanitorExpandedMod.Settings.baseCommandRange;
                if (customRange <= 0f) customRange = 24.9f;

                // Limitar a MaxRadialPatternRadius para evitar desbordar el búfer de círculos radiales de GenDraw
                float maxSafeRadius = GenRadial.MaxRadialPatternRadius - 1f;
                float drawRadius = UnityEngine.Mathf.Min(customRange, maxSafeRadius);

                GenDraw.DrawRadiusRing(___pawn.Position, drawRadius, UnityEngine.Color.white, (IntVec3 c) => __instance.CanCommandTo(c));
            }
            return false;
        }
    }

    /// <summary>
    /// Ajusta WorkSpeedGlobal de mecanoides aliados, MechFormingSpeed y WastepacksPerRecharge según ModSettings.
    /// </summary>
    [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetValueUnfinalized))]
    public static class Patch_StatWorker_GetValueUnfinalized_Balance
    {
        public static void Postfix(StatDef ___stat, StatRequest req, ref float __result)
        {
            if (___stat == null)
            {
                return;
            }

            if (___stat == StatDefOf.WastepacksPerRecharge && MechanitorExpandedMod.Settings.disableMechWastepacks)
            {
                __result = 0f;
                return;
            }

            if (Current.ProgramState != ProgramState.Playing || Find.FactionManager?.OfPlayer == null)
            {
                return;
            }

            if (req.Thing is Pawn pawn && pawn.Faction != null && pawn.Faction.IsPlayer)
            {
                if (___stat == StatDefOf.WorkSpeedGlobal && pawn.RaceProps.IsMechanoid)
                {
                    float mult = MechanitorExpandedMod.Settings.mechWorkSpeedMultiplier;
                    if (mult > 1.001f)
                    {
                        __result *= mult;
                    }
                }
                else if (___stat.defName == "MechFormingSpeed")
                {
                    if (MechanitorExpandedMod.Settings.instantMechGestation)
                    {
                        __result = 99999f;
                    }
                    else
                    {
                        float mult = MechanitorExpandedMod.Settings.mechGestationSpeedMultiplier;
                        if (mult > 1.001f)
                        {
                            __result *= mult;
                        }
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
}
