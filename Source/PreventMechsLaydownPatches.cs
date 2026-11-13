using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Tolgrim_MechanitorExpanded
{
    /// <summary>
    /// Mantiene a los mecanoides de pie durante el autoapagado (SelfShutdown) y permite que
    /// los colonos/lifters transporten al cargador a los que se quedaron sin energía.
    /// </summary>
    [HarmonyPatch(typeof(Toils_LayDown), nameof(Toils_LayDown.SelfShutdown))]
    public static class Patch_Toils_LayDown_SelfShutdown_PreventLay
    {
        public static void Postfix(Toil __result)
        {
            if (__result == null)
            {
                return;
            }

            Action? existing = __result.initAction;
            __result.initAction = delegate
            {
                existing?.Invoke();
                if (MechanitorExpandedMod.Settings == null || !MechanitorExpandedMod.Settings.enablePreventMechsLaydown)
                {
                    return;
                }

                Pawn? actor = __result.actor;
                if (actor != null && actor.RaceProps.IsMechanoid)
                {
                    actor.jobs.posture = PawnPosture.Standing;
                }
            };
        }
    }

    [HarmonyPatch(typeof(Need_MechEnergy), "get_IsSelfShutdown")]
    public static class Patch_Need_MechEnergy_IsSelfShutdown_PreventLay
    {
        private static readonly AccessTools.FieldRef<Need_MechEnergy, Pawn> PawnRef =
            AccessTools.FieldRefAccess<Need_MechEnergy, Pawn>("pawn");

        public static bool Prefix(Need_MechEnergy __instance, ref bool __result)
        {
            if (MechanitorExpandedMod.Settings == null || !MechanitorExpandedMod.Settings.enablePreventMechsLaydown)
            {
                return true;
            }

            Pawn? pawn = PawnRef(__instance);
            if (pawn != null && pawn.CurJobDef == JobDefOf.SelfShutdown)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(WorkGiver_HaulMechToCharger), nameof(WorkGiver_HaulMechToCharger.HasJobOnThing))]
    public static class Patch_WorkGiver_HaulMechToCharger_HasJobOnThing_Depleted
    {
        public static void Postfix(Pawn pawn, Thing t, bool forced, ref bool __result)
        {
            if (__result)
            {
                return;
            }

            if (MechanitorExpandedMod.Settings == null || !MechanitorExpandedMod.Settings.enablePreventMechsLaydown)
            {
                return;
            }

            if (!(t is Pawn mech) || !mech.RaceProps.IsMechanoid || !mech.IsColonyMech || mech.needs?.energy == null)
            {
                return;
            }

            if (mech.needs.energy.CurLevelPercentage > 0f)
            {
                return;
            }

            MechanitorControlGroup? group = MechanitorUtility.GetMechControlGroup(mech);
            if (group != null && group.WorkMode == MechWorkModeDefOf.SelfShutdown)
            {
                return;
            }

            if (pawn.CanReserve(mech, 1, -1, null, forced) && JobGiver_GetEnergy_Charger.GetClosestCharger(mech, pawn, forced) != null)
            {
                __result = true;
            }
        }
    }
}
