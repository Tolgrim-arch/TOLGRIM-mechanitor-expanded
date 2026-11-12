using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Tolgrim_MechanitorExpanded
{
    [StaticConstructorOnStartup]
    public static class EnemyMechResurrectionPatches
    {
        static EnemyMechResurrectionPatches()
        {
            Harmony harmony = new Harmony("Tolgrim.MechanitorExpanded");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    /// <summary>
    /// Permite que los cadáveres de mecanoides hostiles sean aceptados en las tareas de resurrección de mecanoides.
    /// </summary>
    [HarmonyPatch(typeof(Bill_ResurrectMech), nameof(Bill_ResurrectMech.IsFixedOrAllowedIngredient))]
    public static class Patch_Bill_ResurrectMech_IsFixedOrAllowedIngredient
    {
        public static void Postfix(Bill_ResurrectMech __instance, Thing thing, ref bool __result)
        {
            if (!MechanitorExpandedMod.Settings.enableResurrectEnemyMechs)
            {
                return;
            }

            if (!__result && thing is Corpse corpse && corpse.InnerPawn != null && corpse.InnerPawn.RaceProps.IsMechanoid)
            {
                if (__instance.recipe.fixedIngredientFilter.Allows(thing))
                {
                    __result = true;
                }
            }
        }
    }

    /// <summary>
    /// Al completar la resurrección en el gestador, convierte la facción del mecanoide a la facción del jugador.
    /// </summary>
    [HarmonyPatch(typeof(Bill_ResurrectMech), nameof(Bill_ResurrectMech.CreateProducts))]
    public static class Patch_Bill_ResurrectMech_CreateProducts
    {
        public static bool Prefix(Bill_ResurrectMech __instance)
        {
            if (!MechanitorExpandedMod.Settings.enableResurrectEnemyMechs)
            {
                return true;
            }

            Corpse? corpse = __instance.Gestator?.ResurrectingMechCorpse;
            if (corpse?.InnerPawn != null)
            {
                Pawn innerPawn = corpse.InnerPawn;
                if (innerPawn.Faction != Faction.OfPlayer)
                {
                    innerPawn.SetFaction(Faction.OfPlayer);
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Vincula automáticamente al mecanoide resucitado con el supervisor si se usa la habilidad de resurrección.
    /// </summary>
    [HarmonyPatch(typeof(CompAbilityEffect_ResurrectMech), nameof(CompAbilityEffect_ResurrectMech.Apply))]
    public static class Patch_CompAbilityEffect_ResurrectMech_Apply
    {
        public static void Prefix(LocalTargetInfo target, out Pawn? __state)
        {
            if (!MechanitorExpandedMod.Settings.enableResurrectEnemyMechs)
            {
                __state = null;
                return;
            }

            if (target.Thing is Corpse corpse && corpse.InnerPawn != null && corpse.InnerPawn.IsColonyMech)
            {
                __state = corpse.InnerPawn;
            }
            else
            {
                __state = null;
            }
        }

        public static void Postfix(CompAbilityEffect_ResurrectMech __instance, Pawn? __state)
        {
            if (!MechanitorExpandedMod.Settings.enableResurrectEnemyMechs || __state == null)
            {
                return;
            }

            Pawn caster = __instance.parent.pawn;
            Pawn overseer = caster.GetOverseer();

            if (overseer != null && MechanitorUtility.CanControlMech(overseer, __state))
            {
                overseer.relations.AddDirectRelation(PawnRelationDefOf.Overseer, __state);
            }

            FieldInfo? chargesField = __instance.parent.GetType().GetField("charges", BindingFlags.NonPublic | BindingFlags.Instance);
            if (chargesField != null)
            {
                chargesField.SetValue(__instance.parent, __instance.ChargesRemaining);
            }
        }
    }

    /// <summary>
    /// Recarga gradual de cargas de resurrección para un Apocriton domesticado o propio.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.TickRare))]
    public static class Patch_Pawn_TickRare
    {
        private static readonly Dictionary<string, int> ApocritonTicks = new Dictionary<string, int>();
        private const int MaxCharges = 30;
        private const int RechargeCooldownTicks = GenDate.TicksPerDay / 5;

        public static void Postfix(Pawn __instance)
        {
            if (!MechanitorExpandedMod.Settings.enableApocritonRecharge)
            {
                return;
            }

            if (__instance.def != ThingDefOf.Mech_Apocriton || __instance.Faction != Faction.OfPlayer || __instance.Dead)
            {
                return;
            }

            if (!ApocritonTicks.TryGetValue(__instance.ThingID, out int lastTick))
            {
                lastTick = Find.TickManager.TicksGame;
                ApocritonTicks[__instance.ThingID] = lastTick;
            }

            int elapsed = Find.TickManager.TicksGame - lastTick;
            if (elapsed < 0)
            {
                ApocritonTicks.Clear();
                return;
            }

            if (elapsed < RechargeCooldownTicks)
            {
                return;
            }

            Ability? resurrection = __instance.abilities?.GetAbility(AbilityDefOf.ResurrectionMech);
            if (resurrection?.EffectComps == null)
            {
                return;
            }

            for (int i = 0; i < resurrection.EffectComps.Count; i++)
            {
                if (resurrection.EffectComps[i] is CompAbilityEffect_ResurrectMech comp)
                {
                    if (comp.ChargesRemaining < MaxCharges)
                    {
                        int newCharges = comp.ChargesRemaining + 1;
                        FieldInfo? resChargesField = comp.GetType().GetField("resurrectCharges", BindingFlags.NonPublic | BindingFlags.Instance);
                        resChargesField?.SetValue(comp, newCharges);

                        FieldInfo? chargesField = resurrection.GetType().GetField("charges", BindingFlags.NonPublic | BindingFlags.Instance);
                        chargesField?.SetValue(resurrection, newCharges);

                        ApocritonTicks[__instance.ThingID] = Find.TickManager.TicksGame;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Evita que un Apocriton propio o de facción aliada genere Hate Aura (aura de odio) a los colonos.
    /// </summary>
    [HarmonyPatch(typeof(ThoughtWorker_HateAura), "CurrentStateInternal")]
    public static class Patch_ThoughtWorker_HateAura_CurrentStateInternal
    {
        public static bool Prefix(Pawn p, ref ThoughtState __result)
        {
            if (!MechanitorExpandedMod.Settings.suppressFriendlyApocritonHateAura)
            {
                return true;
            }

            if (p.Map == null)
            {
                __result = ThoughtState.Inactive;
                return false;
            }

            List<Thing> apocritons = p.Map.listerThings.ThingsOfDef(ThingDefOf.Mech_Apocriton);
            if (apocritons.NullOrEmpty())
            {
                __result = ThoughtState.Inactive;
                return false;
            }

            // Filtrar solo Apocritons hostiles al colono
            float closestDistance = float.MaxValue;
            bool foundHostile = false;

            for (int i = 0; i < apocritons.Count; i++)
            {
                Thing apocriton = apocritons[i];
                if (apocriton.HostileTo(p.Faction))
                {
                    foundHostile = true;
                    float dist = apocriton.Position.DistanceTo(p.Position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                    }
                }
            }

            if (!foundHostile)
            {
                __result = ThoughtState.Inactive;
                return false;
            }

            if (closestDistance <= 6.9f)
            {
                __result = ThoughtState.ActiveAtStage(0);
            }
            else if (closestDistance <= 15.9f)
            {
                __result = ThoughtState.ActiveAtStage(1);
            }
            else
            {
                __result = ThoughtState.ActiveAtStage(2);
            }

            return false;
        }
    }

    /// <summary>
    /// Corrige el targeting de la habilidad ResurrectionMech cuando la usa un mecanoide aliado.
    /// </summary>
    [HarmonyPatch(typeof(Verb_CastAbility), nameof(Verb_CastAbility.OrderForceTarget))]
    public static class Patch_Verb_CastAbility_OrderForceTarget
    {
        public static bool Prefix(Verb_CastAbility __instance, LocalTargetInfo target)
        {
            if (!MechanitorExpandedMod.Settings.enableResurrectEnemyMechs || __instance.ability.def != AbilityDefOf.ResurrectionMech)
            {
                return true;
            }

            if (CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = __instance.CasterPawn,
                target = target.Thing,
                verb = __instance.ability.verb,
                maxRangeFromTarget = __instance.ability.verb.verbProps.range,
                wantCoverFromTarget = false,
                maxRegions = 50
            }, out IntVec3 dest))
            {
                __instance.ability.QueueCastingJob(target, dest);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Verb_CastAbility), nameof(Verb_CastAbility.IsApplicableTo))]
    public static class Patch_Verb_CastAbility_IsApplicableTo
    {
        public static bool Prefix(Verb_CastAbility __instance, ref bool __result, LocalTargetInfo target)
        {
            if (!MechanitorExpandedMod.Settings.enableResurrectEnemyMechs || __instance.ability.def != AbilityDefOf.ResurrectionMech)
            {
                return true;
            }

            __result = __instance.ability.CanApplyOn(target);
            return false;
        }
    }

    /// <summary>
    /// Controla dinámicamente la disponibilidad de la receta Tolgrim_GestateApocriton según ModSettings.
    /// </summary>
    [HarmonyPatch(typeof(RecipeDef), nameof(RecipeDef.AvailableNow), MethodType.Getter)]
    public static class Patch_RecipeDef_AvailableNow_Apocriton
    {
        public static void Postfix(RecipeDef __instance, ref bool __result)
        {
            if (__result && __instance.defName == "Tolgrim_GestateApocriton" && !MechanitorExpandedMod.Settings.enableApocritonGestation)
            {
                __result = false;
            }
        }
    }
}
