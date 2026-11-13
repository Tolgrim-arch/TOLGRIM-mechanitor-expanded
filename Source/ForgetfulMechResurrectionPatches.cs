using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Tolgrim_MechanitorExpanded
{
    [HarmonyPatch]
    public static class ForgetfulMechResurrectionPatches
    {
        private static readonly CachedTexture SelectOverseerIcon = new CachedTexture("UI/Icons/SelectOverseer");

        private delegate bool TryFindBestIngredientsHelperDelegate(
            Predicate<Thing> validator,
            Predicate<List<Thing>> billGiverIsPawn,
            List<IngredientCount> ingredients,
            Pawn pawn,
            Thing billGiver,
            List<ThingCount> chosen,
            float maxDist);

        private delegate bool TryFindBestBillIngredientsInSetDelegate(
            List<Thing> availableThings,
            Bill bill,
            List<ThingCount> chosen,
            IntVec3 rootCell,
            bool alreadySorted,
            List<IngredientCount> missingIngredients);

        private delegate IntVec3 GetBillGiverRootCellDelegate(Thing billGiver, Pawn pawn);

        private static readonly TryFindBestIngredientsHelperDelegate? TryFindBestIngredientsHelper =
            AccessTools.MethodDelegate<TryFindBestIngredientsHelperDelegate>(AccessTools.Method(
                typeof(WorkGiver_DoBill),
                "TryFindBestIngredientsHelper",
                new[]
                {
                    typeof(Predicate<Thing>),
                    typeof(Predicate<List<Thing>>),
                    typeof(List<IngredientCount>),
                    typeof(Pawn),
                    typeof(Thing),
                    typeof(List<ThingCount>),
                    typeof(float)
                }));

        private static readonly TryFindBestBillIngredientsInSetDelegate? TryFindBestBillIngredientsInSet =
            AccessTools.MethodDelegate<TryFindBestBillIngredientsInSetDelegate>(AccessTools.Method(
                typeof(WorkGiver_DoBill),
                "TryFindBestBillIngredientsInSet",
                new[]
                {
                    typeof(List<Thing>),
                    typeof(Bill),
                    typeof(List<ThingCount>),
                    typeof(IntVec3),
                    typeof(bool),
                    typeof(List<IngredientCount>)
                }));

        private static readonly GetBillGiverRootCellDelegate? GetBillGiverRootCell =
            AccessTools.MethodDelegate<GetBillGiverRootCellDelegate>(AccessTools.Method(
                typeof(WorkGiver_DoBill),
                "GetBillGiverRootCell",
                new[]
                {
                    typeof(Thing),
                    typeof(Pawn)
                }));
        // 1. Al morir un mecanoide, guardar el vínculo con su supervisor y su grupo de control
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn_RelationsTracker), "RemoveRelationsOnDeath")]
        public static bool Pawn_RelationsTracker_RemoveRelationsOnDeath_Prefix(Pawn_RelationsTracker __instance, Pawn ___pawn)
        {
            if (!MechanitorExpandedMod.Settings.enableForgetfulMechResurrection)
            {
                return true;
            }

            if (___pawn?.RaceProps != null && ___pawn.RaceProps.IsMechanoid && ___pawn.Faction == Faction.OfPlayer)
            {
                List<DirectPawnRelation> relations = __instance.DirectRelations;
                for (int i = 0; i < relations.Count; i++)
                {
                    DirectPawnRelation rel = relations[i];
                    if (rel.def == PawnRelationDefOf.Overseer && rel.otherPawn != null)
                    {
                        WorldComponent_ForgetfulMechResurrection.Instance?.AddMech(rel.otherPawn, ___pawn);
                    }
                }
            }

            return true;
        }

        // 2. Validación de inicio de resurrección en el gestador (PawnAllowedToStartAnew)
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Bill_ResurrectMech), nameof(Bill_ResurrectMech.PawnAllowedToStartAnew))]
        public static bool Bill_ResurrectMech_PawnAllowedToStartAnew_Prefix(Bill_ResurrectMech __instance, Pawn p, ref bool __result)
        {
            if (!MechanitorExpandedMod.Settings.enableForgetfulMechResurrection)
            {
                return true;
            }

            if (__instance.State == FormingState.Gathering && __instance.Gestator?.ResurrectingMechCorpse == null && __instance.Map?.listerThings != null)
            {
                List<Thing> corpses = __instance.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
                if (corpses == null || corpses.Count == 0)
                {
                    return true;
                }

                bool foundAnyUsable = false;
                bool foundAnyWithBandwidth = false;
                bool hadAnyCorpsesForRecipe = false;

                WorldComponent_ForgetfulMechResurrection? comp = WorldComponent_ForgetfulMechResurrection.Instance;

                for (int i = 0; i < corpses.Count; i++)
                {
                    if (corpses[i] is Corpse c && c.InnerPawn != null && __instance.recipe?.fixedIngredientFilter != null && __instance.recipe.fixedIngredientFilter.Allows(c) && !c.IsForbidden(p) && p.CanReach(c, PathEndMode.Touch, Danger.Deadly))
                    {
                        hadAnyCorpsesForRecipe = true;
                        if (comp == null || comp.CanMechanitorReviveMech(p, c.InnerPawn))
                        {
                            foundAnyUsable = true;
                            if (p.mechanitor != null && p.mechanitor.HasBandwidthToResurrect(c))
                            {
                                foundAnyWithBandwidth = true;
                                break;
                            }
                        }
                    }
                }

                if (hadAnyCorpsesForRecipe && !foundAnyUsable)
                {
                    JobFailReason.Is("Tolgrim_ResurrectJobFailReasonOtherMechanitor".Translate());
                    __result = false;
                    return false;
                }

                if (foundAnyUsable && !foundAnyWithBandwidth)
                {
                    JobFailReason.Is("NotEnoughBandwidth".Translate());
                    __result = false;
                    return false;
                }
            }

            return true;
        }

        // 3. Filtrado de ingredientes para que solo el mecanizador dueño recoja el cadáver
        [HarmonyPrefix]
        [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
        public static bool WorkGiver_DoBill_TryFindBestBillIngredients_Prefix(Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen, List<IngredientCount> missingIngredients, ref bool __result)
        {
            if (!MechanitorExpandedMod.Settings.enableForgetfulMechResurrection)
            {
                return true;
            }

            if (bill is Bill_ResurrectMech billResurrect && TryFindBestIngredientsHelper != null && TryFindBestBillIngredientsInSet != null && GetBillGiverRootCell != null)
            {
                IntVec3 rootCell = GetBillGiverRootCell(billGiver, pawn);

                Predicate<Thing> validator = t => IsUsableMechCorpseForResurrection(t, pawn, billResurrect);
                Predicate<List<Thing>> foundChooser = foundThings => TryFindBestBillIngredientsInSet(foundThings, bill, chosen, rootCell, billGiver is Pawn, missingIngredients);

                __result = TryFindBestIngredientsHelper(
                    validator,
                    foundChooser,
                    bill.recipe.ingredients,
                    pawn,
                    billGiver,
                    chosen,
                    bill.ingredientSearchRadius
                );
                return false;
            }

            return true;
        }

        private static bool IsUsableMechCorpseForResurrection(Thing t, Pawn mechanitor, Bill_ResurrectMech bill)
        {
            if (!bill.IsFixedOrAllowedIngredient(t)) return false;

            List<IngredientCount> ingredients = bill.recipe.ingredients;
            for (int i = 0; i < ingredients.Count; i++)
            {
                if (ingredients[i].filter.Allows(t))
                {
                    if (t is Corpse corpse && corpse.InnerPawn != null)
                    {
                        WorldComponent_ForgetfulMechResurrection? comp = WorldComponent_ForgetfulMechResurrection.Instance;
                        return comp == null || comp.CanMechanitorReviveMech(mechanitor, corpse.InnerPawn);
                    }
                    return true;
                }
            }
            return false;
        }

        // 4. Restauración de grupo de control al producirse el mecanoide resucitado
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Bill_ResurrectMech), nameof(Bill_ResurrectMech.CreateProducts))]
        public static void Bill_ResurrectMech_CreateProducts_Postfix(Bill_ResurrectMech __instance, ref Thing __result)
        {
            if (!MechanitorExpandedMod.Settings.enableForgetfulMechResurrection)
            {
                return;
            }

            if (__result is Pawn mech)
            {
                Pawn overseer = MechanitorUtility.GetOverseer(mech);
                WorldComponent_ForgetfulMechResurrection? comp = WorldComponent_ForgetfulMechResurrection.Instance;
                if (comp != null && overseer?.mechanitor != null)
                {
                    if (comp.CanMechRememberMechanitor(overseer, mech))
                    {
                        int targetGroup = comp.MechGroup(mech);
                        if (targetGroup >= 0 && targetGroup < overseer.mechanitor.controlGroups.Count)
                        {
                            overseer.mechanitor.controlGroups[targetGroup].Assign(mech);
                        }
                    }
                    comp.RemoveMech(mech);
                }
            }
        }

        // 5. Gizmo "Olvidar supervisor" y "Seleccionar supervisor" en el cadáver del mecanoide
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Corpse), nameof(Corpse.GetGizmos))]
        public static IEnumerable<Gizmo> Corpse_GetGizmos_Postfix(IEnumerable<Gizmo> result, Corpse __instance)
        {
            foreach (Gizmo g in result)
            {
                yield return g;
            }

            if (!MechanitorExpandedMod.Settings.enableForgetfulMechResurrection)
            {
                yield break;
            }

            Pawn mech = __instance.InnerPawn;
            if (mech == null || !mech.IsColonyMech)
            {
                yield break;
            }

            WorldComponent_ForgetfulMechResurrection? comp = WorldComponent_ForgetfulMechResurrection.Instance;
            if (comp == null)
            {
                yield break;
            }

            Pawn? overseer = comp.GetMechOverseer(mech);
            if (overseer == null)
            {
                yield break;
            }

            bool overseerUnavailable = !overseer.Spawned;

            // Gizmo: Olvidar supervisor
            yield return new Command_Action
            {
                defaultLabel = "Tolgrim_GizmoForgetOverseerLabel".Translate(),
                defaultDesc = "Tolgrim_GizmoForgetOverseerDesc".Translate(overseer.LabelShort),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/MakeDefault", true),
                action = () =>
                {
                    comp.RemoveMech(mech);
                    Messages.Message("Tolgrim_MechForgotOverseer".Translate(mech.LabelShortCap, overseer.LabelShort), MessageTypeDefOf.NeutralEvent, false);
                }
            };

            // Gizmo: Seleccionar supervisor previo
            yield return new Command_Action
            {
                defaultLabel = "CommandSelectOverseer".Translate(),
                defaultDesc = overseerUnavailable ? "CommandSelectOverseerDisabledDesc".Translate() : "CommandSelectOverseerDesc".Translate(),
                icon = SelectOverseerIcon.Texture,
                action = () =>
                {
                    Find.Selector.ClearSelection();
                    Find.Selector.Select(overseer);
                },
                onHover = () =>
                {
                    if (overseer.Spawned)
                    {
                        GenDraw.DrawArrowPointingAt(overseer.TrueCenter());
                    }
                },
                Disabled = overseerUnavailable
            };
        }
    }
}
