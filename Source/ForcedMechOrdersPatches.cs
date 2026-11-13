using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    [HarmonyPatch]
    public static class Patch_FloatMenuOptionProvider_ForcedMechOrders
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            var baseMethod = typeof(FloatMenuOptionProvider).GetMethod("get_MechanoidCanDo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (baseMethod != null)
            {
                yield return baseMethod;
            }

            foreach (Type subclass in GenTypes.AllSubclassesNonAbstract(typeof(FloatMenuOptionProvider)))
            {
                MethodInfo method = subclass.GetMethod("get_MechanoidCanDo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null && method.DeclaringType == subclass)
                {
                    yield return method;
                }
            }
        }

        private static void Postfix(ref bool __result)
        {
            if (MechanitorExpandedMod.Settings != null && MechanitorExpandedMod.Settings.enableForcedMechOrders)
            {
                __result = true;
            }
        }
    }

    /// <summary>
    /// Preserva el arma nativa del mecanoide (destroyOnDrop) en su inventario al equipar otra arma,
    /// para que pueda recuperarla si suelta el arma externa.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.AddEquipment))]
    public static class Patch_Pawn_EquipmentTracker_AddEquipment_PreserveNativeMechWeapon
    {
        public static void Prefix(Pawn_EquipmentTracker __instance, ThingWithComps newEq)
        {
            if (MechanitorExpandedMod.Settings == null || !MechanitorExpandedMod.Settings.enableForcedMechOrders)
            {
                return;
            }

            Pawn? pawn = __instance?.pawn;
            if (pawn == null || !pawn.RaceProps.IsMechanoid || !pawn.IsColonyMech || newEq == null || __instance == null)
            {
                return;
            }

            if (__instance.Primary != null && __instance.Primary.def.destroyOnDrop && __instance.Primary != newEq)
            {
                ThingWithComps nativeWeapon = __instance.Primary;
                __instance.Remove(nativeWeapon);
                if (pawn.inventory?.innerContainer != null)
                {
                    pawn.inventory.innerContainer.TryAdd(nativeWeapon, canMergeWithExistingStacks: false);
                }
            }
        }
    }

    /// <summary>
    /// Si un mecanoide suelta o pierde su arma externa y tiene su arma nativa guardada en inventario, la reequipa automáticamente.
    /// </summary>
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.Remove))]
    public static class Patch_Pawn_EquipmentTracker_Remove_RestoreNativeMechWeapon
    {
        public static void Postfix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {
            if (MechanitorExpandedMod.Settings == null || !MechanitorExpandedMod.Settings.enableForcedMechOrders)
            {
                return;
            }

            Pawn? pawn = __instance?.pawn;
            if (pawn == null || !pawn.RaceProps.IsMechanoid || !pawn.IsColonyMech || pawn.Dead || __instance == null)
            {
                return;
            }

            if (__instance.Primary == null && eq != null && !eq.def.destroyOnDrop && pawn.inventory?.innerContainer != null)
            {
                for (int i = 0; i < pawn.inventory.innerContainer.Count; i++)
                {
                    if (pawn.inventory.innerContainer[i] is ThingWithComps nativeWeapon && nativeWeapon.def.destroyOnDrop && nativeWeapon.def.IsWeapon)
                    {
                        pawn.inventory.innerContainer.Remove(nativeWeapon);
                        __instance.AddEquipment(nativeWeapon);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Al morir un mecanoide de la colonia, suelta al suelo cualquier arma externa equipada (no nativa)
    /// para evitar que se pierda al resucitarlo en un gestador.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_Pawn_Kill_DropExternalMechWeapon
    {
        public static void Prefix(Pawn __instance)
        {
            if (MechanitorExpandedMod.Settings == null || !MechanitorExpandedMod.Settings.enableForcedMechOrders)
            {
                return;
            }

            if (__instance == null || !__instance.Spawned || __instance.Map == null || !__instance.RaceProps.IsMechanoid || !__instance.IsColonyMech)
            {
                return;
            }

            if (__instance.equipment?.Primary != null && !__instance.equipment.Primary.def.destroyOnDrop)
            {
                __instance.equipment.TryDropEquipment(__instance.equipment.Primary, out _, __instance.Position, forbid: false);
            }
        }
    }
}
