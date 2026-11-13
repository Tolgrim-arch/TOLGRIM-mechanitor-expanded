using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    [HarmonyPatch]
    public static class MechColorPatches
    {
        private static Pawn? currentRenderingMech;
        private static bool inGetMaterialPropertyBlock;
        private static MechColorState? controlGroupState;
        private static Rect controlGroupRect;

        // 1. Hook de dibujo de mecanoides
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PawnRenderer), "DynamicDrawPhaseAt")]
        public static void DynamicDrawPhaseAt_Prefix(Pawn ___pawn)
        {
            if (___pawn != null && ___pawn.IsColonyMech)
            {
                currentRenderingMech = ___pawn;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PawnRenderer), "DynamicDrawPhaseAt")]
        public static void DynamicDrawPhaseAt_Postfix()
        {
            currentRenderingMech = null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PawnRenderNodeWorker), nameof(PawnRenderNodeWorker.GetMaterialPropertyBlock))]
        public static void GetMaterialPropertyBlock_Prefix()
        {
            inGetMaterialPropertyBlock = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PawnRenderNodeWorker), nameof(PawnRenderNodeWorker.GetMaterialPropertyBlock))]
        public static void GetMaterialPropertyBlock_Postfix()
        {
            inGetMaterialPropertyBlock = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.SetMatPropBlockOverlay))]
        public static void SetMatPropBlockOverlay_Prefix(ref Color color, ref float opacity)
        {
            if (inGetMaterialPropertyBlock && currentRenderingMech != null)
            {
                MechColorState? state = MechColorState.For(currentRenderingMech);
                if (state != null)
                {
                    Color? customCol = state.SetColor;
                    if (customCol.HasValue)
                    {
                        color = customCol.Value;
                    }
                    float? customOpacity = state.SetOpacity;
                    if (customOpacity.HasValue)
                    {
                        opacity = customOpacity.Value;
                    }
                }
            }
        }

        // 2. Gizmos en mecanoides y mecanizadores (desactivados en favor de la pestaña Mecanoide y menús contextuales)
        // Se mantiene vacío para evitar contaminar la barra de comandos con accesos rápidos innecesarios.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
        public static IEnumerable<Gizmo> Pawn_GetGizmos_Postfix(IEnumerable<Gizmo> result, Pawn __instance)
        {
            foreach (Gizmo g in result)
            {
                yield return g;
            }
        }

        // 3. Indicador de color y menú en el gizmo del grupo de control
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MechanitorControlGroupGizmo), nameof(MechanitorControlGroupGizmo.GizmoOnGUI))]
        public static void MechanitorControlGroupGizmo_Prefix(MechanitorControlGroup ___controlGroup)
        {
            controlGroupState = MechColorState.For(___controlGroup);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GenUI), nameof(GenUI.DrawTextureWithMaterial))]
        public static void DrawTextureWithMaterial_Postfix(Rect rect)
        {
            if (controlGroupState != null)
            {
                controlGroupState.DrawColorIndicator(rect, low: true);
                controlGroupState = null;
                controlGroupRect = rect;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MechanitorControlGroupGizmo), nameof(MechanitorControlGroupGizmo.GizmoOnGUI))]
        public static void MechanitorControlGroupGizmo_Postfix(MechanitorControlGroup ___controlGroup)
        {
            // El menú contextual completo (Renombrar, Color, etc.) se gestiona en Patch_MechanitorControlGroupGizmo_GizmoOnGUI
        }

        // 4. Guardado y carga (ExposeData)
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.ExposeData))]
        public static void Pawn_ExposeData_Postfix(Pawn __instance)
        {
            MechColorState.For(__instance)?.ExposeData();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MechanitorControlGroup), nameof(MechanitorControlGroup.ExposeData))]
        public static void MechanitorControlGroup_ExposeData_Postfix(MechanitorControlGroup __instance)
        {
            MechColorState.For(__instance)?.ExposeData();
        }
    }
}
