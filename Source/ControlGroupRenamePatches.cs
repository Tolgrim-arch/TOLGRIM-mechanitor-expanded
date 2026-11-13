using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    [HarmonyPatch(typeof(MechanitorControlGroupGizmo), nameof(MechanitorControlGroupGizmo.GizmoOnGUI))]
    public static class Patch_MechanitorControlGroupGizmo_GizmoOnGUI
    {
        private static readonly AccessTools.FieldRef<MechanitorControlGroupGizmo, MechanitorControlGroup> ControlGroupField =
            AccessTools.FieldRefAccess<MechanitorControlGroupGizmo, MechanitorControlGroup>("controlGroup");

        private static readonly AccessTools.FieldRef<MechanitorControlGroupGizmo, List<MechanitorControlGroup>> MergedGroupsField =
            AccessTools.FieldRefAccess<MechanitorControlGroupGizmo, List<MechanitorControlGroup>>("mergedControlGroups");

        public static void Postfix(MechanitorControlGroupGizmo __instance, Vector2 topLeft, float maxWidth, GizmoRenderParms parms, GizmoResult __result)
        {
            if (__instance == null) return;
            MechanitorControlGroup controlGroup = ControlGroupField(__instance);
            if (controlGroup == null) return;

            List<MechanitorControlGroup> merged = MergedGroupsField(__instance);

            Rect rect = new Rect(topLeft.x, topLeft.y, __instance.GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(6f);

            // Re-render group label if custom name exists (or if merged)
            string? customName = WorldComponent_ControlGroupNames.GetCustomName(controlGroup);
            if (customName != null && (merged == null || merged.Count == 0))
            {
                // Draw background box over vanilla label to cleanly replace text avoiding buttons (now wider without rename icon)
                Rect labelArea = new Rect(rect2.x, rect2.y, rect2.width - 56f, 22f);
                
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
                string displayLabel = customName.Truncate(labelArea.width);
                Vector2 textSize = Text.CalcSize(displayLabel);
                Rect textRect = new Rect(labelArea.x, labelArea.y, textSize.x, textSize.y);

                // Redraw the gizmo's own background texture slice over the vanilla label so the gradient matches seamlessly
                Texture2D bgTex = parms.shrunk ? Command.BGTexShrunk : Command.BGTex;
                Rect texCoords = new Rect(
                    (labelArea.x - rect.x) / rect.width,
                    1f - ((labelArea.y - rect.y + textSize.y) / rect.height),
                    labelArea.width / rect.width,
                    textSize.y / rect.height);
                GUI.DrawTextureWithTexCoords(new Rect(labelArea.x, labelArea.y, labelArea.width, textSize.y), bgTex, texCoords);
                Widgets.Label(textRect, displayLabel);

                if (Mouse.IsOver(textRect))
                {
                    Widgets.DrawHighlight(textRect);
                    if (Widgets.ButtonInvisible(textRect))
                    {
                        List<Pawn> mechs = controlGroup.MechsForReading;
                        Find.Selector.ClearSelection();
                        for (int i = 0; i < mechs.Count; i++)
                        {
                            Find.Selector.Select(mechs[i]);
                        }
                    }
                }
            }

            // Clic derecho en el gizmo abre menú contextual de Tolgrim (Renombrar grupo, Cambiar color, etc.)
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && Mouse.IsOver(rect))
            {
                Event.current.Use();
                List<FloatMenuOption> options = new List<FloatMenuOption>
                {
                    new FloatMenuOption("Tolgrim_RenameControlGroup".Translate(), () =>
                    {
                        Find.WindowStack.Add(new Dialog_RenameControlGroup(controlGroup));
                    }, TexButton.Rename, Color.white)
                };

                MechColorState? colorState = MechColorState.For(controlGroup);
                if (colorState != null)
                {
                    foreach (FloatMenuOption colorOpt in colorState.Menu)
                    {
                        options.Add(colorOpt);
                    }
                }

                // Opciones de modo de trabajo de vainilla para no perderlas al hacer clic derecho
                foreach (FloatMenuOption wmOpt in MechanitorControlGroupGizmo.GetWorkModeOptions(controlGroup))
                {
                    options.Add(wmOpt);
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }
        }
    }

    [HarmonyPatch(typeof(MechanitorControlGroupGizmo), nameof(MechanitorControlGroupGizmo.RightClickFloatMenuOptions), MethodType.Getter)]
    public static class Patch_MechanitorControlGroupGizmo_RightClickFloatMenuOptions
    {
        private static readonly AccessTools.FieldRef<MechanitorControlGroupGizmo, MechanitorControlGroup> ControlGroupField =
            AccessTools.FieldRefAccess<MechanitorControlGroupGizmo, MechanitorControlGroup>("controlGroup");

        public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, MechanitorControlGroupGizmo __instance)
        {
            if (__result != null)
            {
                foreach (FloatMenuOption opt in __result)
                {
                    yield return opt;
                }
            }

            if (__instance != null)
            {
                MechanitorControlGroup controlGroup = ControlGroupField(__instance);
                if (controlGroup != null)
                {
                    yield return new FloatMenuOption("Tolgrim_RenameControlGroup".Translate(), () =>
                    {
                        Find.WindowStack.Add(new Dialog_RenameControlGroup(controlGroup));
                    }, TexButton.Rename, Color.white);
                }
            }
        }
    }

    [HarmonyPatch(typeof(MechanitorControlGroup), nameof(MechanitorControlGroup.LabelIndexWithWorkMode), MethodType.Getter)]
    public static class Patch_MechanitorControlGroup_LabelIndexWithWorkMode
    {
        public static void Postfix(MechanitorControlGroup __instance, ref string __result)
        {
            string? customName = WorldComponent_ControlGroupNames.GetCustomName(__instance);
            if (!string.IsNullOrWhiteSpace(customName))
            {
                __result = $"{customName} ({__instance.WorkMode.LabelCap})";
            }
        }
    }

    /// <summary>
    /// Amplía el ancho del gizmo de grupo de control para dar espacio suficiente al nombre, iconos y retratos.
    /// </summary>
    [HarmonyPatch(typeof(MechanitorControlGroupGizmo), nameof(MechanitorControlGroupGizmo.GetWidth))]
    public static class Patch_MechanitorControlGroupGizmo_GetWidth
    {
        public static void Postfix(ref float __result)
        {
            __result = 150f;
        }
    }
}
