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
            MechanitorControlGroup controlGroup = ControlGroupField(__instance);
            if (controlGroup == null) return;

            List<MechanitorControlGroup> merged = MergedGroupsField(__instance);

            Rect rect = new Rect(topLeft.x, topLeft.y, __instance.GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(6f);

            // Re-render group label if custom name exists (or if merged)
            string? customName = WorldComponent_ControlGroupNames.GetCustomName(controlGroup);
            if (customName != null && (merged == null || merged.Count == 0))
            {
                // Draw background box over vanilla label to cleanly replace text
                Rect labelArea = new Rect(rect2.x, rect2.y, rect2.width - 56f, 24f);
                
                // Redraw standard background texture portion or simple clear/label
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
                string displayLabel = customName.Truncate(labelArea.width);
                Vector2 textSize = Text.CalcSize(displayLabel);
                Rect textRect = new Rect(labelArea.x, labelArea.y, textSize.x, textSize.y);

                // Draw overlay backing and custom text
                Widgets.DrawRectFast(new Rect(labelArea.x, labelArea.y, labelArea.width, textSize.y), new Color(0.12f, 0.14f, 0.16f, 0.95f));
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

            // Draw Rename Button in top right between icon buttons if single group
            if (merged == null || merged.Count == 0)
            {
                float btnSize = 18f;
                // Position rename icon right next to work mode icon
                Rect renameRect = new Rect(rect.x + rect.width - 52f - 6f - btnSize - 2f, rect.y + 6f + 4f, btnSize, btnSize);

                GUI.color = Color.white;
                if (Widgets.ButtonImage(renameRect, TexButton.Rename, Color.white, GenUI.MouseoverColor))
                {
                    Event.current.Use();
                    Find.WindowStack.Add(new Dialog_RenameControlGroup(controlGroup));
                }
                TooltipHandler.TipRegion(renameRect, "Tolgrim_RenameControlGroupTip".Translate());
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
            foreach (FloatMenuOption opt in __result)
            {
                yield return opt;
            }

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
}
