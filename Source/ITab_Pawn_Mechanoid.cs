using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Tolgrim_MechanitorExpanded
{
    public class ITab_Pawn_Mechanoid : ITab
    {
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollViewHeight = 420f;

        private const float TopPadding = 24f;
        private const float SectionHeaderHeight = 26f;
        private const float RowHeight = 24f;

        private static readonly Color HeaderBgColor = new Color(0.18f, 0.20f, 0.23f, 0.85f);
        private static readonly Color AlternateRowColor = new Color(1f, 1f, 1f, 0.04f);
        private static readonly Color KeyLabelColor = new Color(0.80f, 0.80f, 0.80f);

        private Pawn? PawnToShowInfoAbout
        {
            get
            {
                if (SelPawn != null) return SelPawn;
                if (base.SelThing is Corpse corpse) return corpse.InnerPawn;
                return null;
            }
        }

        public override bool IsVisible
        {
            get
            {
                if (!ModsConfig.BiotechActive) return false;
                Pawn? p = PawnToShowInfoAbout;
                if (p == null || !p.RaceProps.IsMechanoid) return false;
                return p.Faction == Faction.OfPlayer;
            }
        }

        public ITab_Pawn_Mechanoid()
        {
            size = new Vector2(480f, 460f);
            labelKey = "Tolgrim_TabMechanoid";
        }

        protected override void FillTab()
        {
            Pawn? pawn = PawnToShowInfoAbout;
            if (pawn == null) return;

            Rect rect = new Rect(0f, TopPadding, size.x, size.y - TopPadding).ContractedBy(10f);
            Widgets.BeginGroup(rect);

            Rect outRect = new Rect(0f, 0f, rect.width, rect.height);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float curY = 0f;

            DrawGeneralSection(pawn, ref curY, viewRect.width);
            DrawColorSection(pawn, ref curY, viewRect.width);
            DrawCombatCapabilitiesSection(pawn, ref curY, viewRect.width);

            if (Event.current.type == EventType.Layout)
            {
                scrollViewHeight = curY + 20f;
            }

            Widgets.EndScrollView();
            Widgets.EndGroup();
        }

        private void DrawGeneralSection(Pawn pawn, ref float curY, float width)
        {
            DrawSectionHeader("Tolgrim_MechGeneralHeader".Translate(), ref curY, width);
            int rowIdx = 0;

            // Supervisor asignado
            Pawn overseer = pawn.GetOverseer();
            string overseerLabel = overseer != null ? overseer.LabelShortCap : "None".Translate();
            DrawKeyValueRow("Tolgrim_MechOverseerLabel".Translate(), overseerLabel, ref curY, width, null, rowIdx++);

            // Grupo de control
            MechanitorControlGroup? group = MechanitorUtility.GetMechControlGroup(pawn);
            string groupName;
            if (group != null)
            {
                string? custom = WorldComponent_ControlGroupNames.GetCustomName(group);
                groupName = !string.IsNullOrWhiteSpace(custom) ? custom! : ($"Group".Translate() + " " + group.Index);
                groupName += $" ({group.WorkMode.LabelCap})";
            }
            else
            {
                groupName = "None".Translate();
            }
            DrawKeyValueRow("Tolgrim_ControlGroups".Translate(), groupName, ref curY, width, null, rowIdx++);

            // Costo de ancho de banda
            float bwCost = pawn.GetStatValue(StatDefOf.BandwidthCost);
            string bwLabel = "Tolgrim_BandwidthCost".Translate();
            DrawKeyValueRow(bwLabel, bwCost.ToString("0"), ref curY, width, "Bandwidth".Translate(), rowIdx++);

            // Nivel de energía actual
            if (pawn.needs?.energy != null)
            {
                float pct = pawn.needs.energy.CurLevelPercentage;
                string energyStr = $"{pct.ToStringPercent()} ({pawn.needs.energy.CurLevel:F0} / {pawn.needs.energy.MaxLevel:F0})";
                Color energyCol = pct < 0.2f ? ColorLibrary.Red : (pct < 0.5f ? ColorLibrary.Yellow : Color.white);
                DrawKeyValueRow("MechEnergy".Translate(), energyStr, ref curY, width, null, rowIdx++, energyCol);
            }

            // Desechos tóxicos por recarga
            StatDef wasteStat = StatDefOf.WastepacksPerRecharge;
            if (wasteStat != null)
            {
                float waste = pawn.GetStatValue(wasteStat);
                DrawKeyValueRow(wasteStat.LabelCap, waste.ToString("0"), ref curY, width, wasteStat.description, rowIdx++);
            }

            curY += 6f;
        }

        private void DrawColorSection(Pawn pawn, ref float curY, float width)
        {
            DrawSectionHeader("Tolgrim_MechColorCustomizationHeader".Translate(), ref curY, width);

            MechColorState? state = MechColorState.For(pawn);
            if (state == null) return;

            Rect boxRect = new Rect(0f, curY, width, 40f);
            Widgets.DrawBoxSolid(boxRect, AlternateRowColor);

            // Muestra del color activo
            Rect colorSwatch = new Rect(boxRect.x + 6f, boxRect.y + 6f, 28f, 28f);
            Widgets.DrawBoxSolid(colorSwatch, new Color(0.1f, 0.1f, 0.1f, 1f));
            Widgets.DrawBoxSolid(colorSwatch.ContractedBy(2f), state.ActiveColor);
            Widgets.DrawBox(colorSwatch);

            // Etiqueta del estado de color
            // Boton de personalizacion
            float btnW = 160f;
            float btnH = 28f;
            Rect btnRect = new Rect(boxRect.x + boxRect.width - btnW - 6f, boxRect.y + 6f, btnW, btnH);

            // Etiqueta del estado de color calculada dinamicamente
            float labelStartX = boxRect.x + 42f;
            float labelWidth = Mathf.Max(60f, btnRect.x - labelStartX - 8f);
            Rect labelRect = new Rect(labelStartX, boxRect.y, labelWidth, boxRect.height);
            string status = state.color.HasValue ? "Tolgrim_CustomColorActive".Translate() : "Tolgrim_InheritedColorActive".Translate();
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, status);
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonText(btnRect, "Tolgrim_SetMechColorLabel".Translate()))
            {
                state.DoDialog();
            }

            curY += 46f;
        }

        private void DrawCombatCapabilitiesSection(Pawn pawn, ref float curY, float width)
        {
            DrawSectionHeader("Tolgrim_MechStatsHeader".Translate(), ref curY, width);
            int rowIdx = 0;

            DrawPawnStatRow(pawn, StatDefOf.MoveSpeed, ref curY, width, rowIdx++);
            DrawPawnStatRow(pawn, StatDefOf.ArmorRating_Sharp, ref curY, width, rowIdx++);
            DrawPawnStatRow(pawn, StatDefOf.ArmorRating_Blunt, ref curY, width, rowIdx++);
            DrawPawnStatRow(pawn, StatDefOf.ArmorRating_Heat, ref curY, width, rowIdx++);

            if (pawn.equipment?.Primary != null)
            {
                DrawKeyValueRow("Equipment".Translate(), pawn.equipment.Primary.LabelCap, ref curY, width, pawn.equipment.Primary.def.description, rowIdx++);
            }

            curY += 6f;
        }

        private void DrawPawnStatRow(Pawn pawn, StatDef stat, ref float curY, float width, int rowIndex)
        {
            if (stat == null) return;
            float val = pawn.GetStatValue(stat);
            string valStr = stat.ValueToString(val, stat.toStringNumberSense);
            string tip = stat.description;
            DrawKeyValueRow(stat.LabelCap, valStr, ref curY, width, tip, rowIndex);
        }

        private void DrawSectionHeader(string label, ref float curY, float width)
        {
            Rect rect = new Rect(0f, curY, width, SectionHeaderHeight);
            Widgets.DrawBoxSolid(rect, HeaderBgColor);

            Rect labelRect = new Rect(rect.x + 8f, rect.y, rect.width - 16f, rect.height);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Widgets.Label(labelRect, label);
            Text.Anchor = TextAnchor.UpperLeft;

            curY += SectionHeaderHeight + 2f;
        }

        private void DrawKeyValueRow(string key, string value, ref float curY, float width, string? tooltip, int rowIndex, Color? valColor = null)
        {
            Rect rowRect = new Rect(0f, curY, width, RowHeight);

            if (rowIndex % 2 == 1)
            {
                Widgets.DrawBoxSolid(rowRect, AlternateRowColor);
            }

            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
            }

            if (!string.IsNullOrEmpty(tooltip))
            {
                TooltipHandler.TipRegion(rowRect, tooltip);
            }

            float labelWidth = width * 0.55f;
            Rect keyRect = new Rect(rowRect.x + 8f, rowRect.y, labelWidth - 8f, rowRect.height);
            Rect valRect = new Rect(rowRect.x + labelWidth, rowRect.y, width - labelWidth - 8f, rowRect.height);

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;

            GUI.color = KeyLabelColor;
            Widgets.Label(keyRect, key);

            GUI.color = valColor ?? Color.white;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(valRect, value);

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;

            curY += RowHeight;
        }
    }
}
