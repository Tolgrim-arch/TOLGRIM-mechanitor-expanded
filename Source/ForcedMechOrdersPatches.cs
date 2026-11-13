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
}
