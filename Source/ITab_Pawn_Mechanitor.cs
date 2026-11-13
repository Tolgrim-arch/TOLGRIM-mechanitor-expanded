using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Tolgrim_MechanitorExpanded
{
    public class ITab_Pawn_Mechanitor : ITab
    {
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollViewHeight = 460f;

        private static bool? generalInfoExpanded;
        private static bool? statsExpanded;
        private static bool? implantsExpanded;
        private static bool? equipmentExpanded;

        private const float TopPadding = 24f;
        private const float ContentMargin = 12f;
        private const float ScrollBarShare = 16f;
        private const float RowHeight = 24f;
        private const float SectionHeaderHeight = 26f;

        private static readonly Color HeaderBgColor = new Color(0.18f, 0.20f, 0.22f, 0.65f);
        private static readonly Color AlternateRowColor = new Color(1f, 1f, 1f, 0.035f);
        private static readonly Color HighlightRowColor = new Color(1f, 1f, 1f, 0.08f);

        private MechanitorExpandedSettings S => MechanitorExpandedMod.Settings;

        private bool IsGeneralExpanded
        {
            get => generalInfoExpanded ?? S.defaultExpandGeneral;
            set => generalInfoExpanded = value;
        }

        private bool IsStatsExpanded
        {
            get => statsExpanded ?? S.defaultExpandStats;
            set => statsExpanded = value;
        }

        private bool IsImplantsExpanded
        {
            get => implantsExpanded ?? S.defaultExpandImplants;
            set => implantsExpanded = value;
        }

        private bool IsEquipmentExpanded
        {
            get => equipmentExpanded ?? S.defaultExpandEquipment;
            set => equipmentExpanded = value;
        }

        private Pawn? PawnToShowInfoAbout
        {
            get
            {
                if (SelPawn != null)
                {
                    return SelPawn;
                }
                if (base.SelThing is Corpse corpse)
                {
                    return corpse.InnerPawn;
                }
                return null;
            }
        }

        public override bool IsVisible
        {
            get
            {
                if (!ModsConfig.BiotechActive)
                {
                    return false;
                }

                Pawn? p = PawnToShowInfoAbout;
                if (p == null || p.health == null || p.health.hediffSet == null)
                {
                    return false;
                }

                return MechanitorUtility.IsMechanitor(p);
            }
        }

        public ITab_Pawn_Mechanitor()
        {
            size = new Vector2(MechanitorExpandedMod.Settings?.tabWidth ?? 490f, MechanitorExpandedMod.Settings?.tabHeight ?? 480f);
            labelKey = "Tolgrim_TabMechanitor";
            tutorTag = "Mechanitor";
        }

        public override void OnOpen()
        {
            base.OnOpen();
            scrollPosition = Vector2.zero;
            UpdateSize();
        }

        protected override void UpdateSize()
        {
            base.UpdateSize();
            if (S != null)
            {
                size = new Vector2(S.tabWidth, S.tabHeight);
            }
        }

        protected override void FillTab()
        {
            UpdateSize();

            Pawn? pawn = PawnToShowInfoAbout;
            if (pawn == null || pawn.mechanitor == null)
            {
                return;
            }

            Text.Font = GameFont.Small;

            // Recuadro interior respetando el botón X nativo de InspectTabBase
            Rect baseRect = new Rect(0f, TopPadding, size.x, size.y - TopPadding).ContractedBy(ContentMargin);

            // Ancho del view interior considerando el slider de scroll
            float viewWidth = baseRect.width - ScrollBarShare;
            Rect outRect = new Rect(0f, 0f, baseRect.width, baseRect.height);
            Rect viewRect = new Rect(0f, 0f, viewWidth, Mathf.Max(scrollViewHeight, baseRect.height));

            Widgets.BeginGroup(baseRect);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            float curY = 0f;

            // 1. Información General
            DrawGeneralSection(pawn, ref curY, viewWidth);

            // 2. Capacidades y Estadísticas del Mecanizador
            DrawCapabilitiesSection(pawn, ref curY, viewWidth);

            // 3. Implantes del Mecanizador (si está habilitado en ModSettings)
            if (S.showImplantsSection)
            {
                DrawImplantsSection(pawn, ref curY, viewWidth);
            }

            // 4. Equipamiento del Mecanizador (si está habilitado en ModSettings)
            if (S.showEquipmentSection)
            {
                DrawEquipmentSection(pawn, ref curY, viewWidth);
            }

            scrollViewHeight = curY + 12f;

            Widgets.EndScrollView();
            Widgets.EndGroup();
        }

        private void DrawGeneralSection(Pawn pawn, ref float curY, float width)
        {
            Pawn_MechanitorTracker tracker = pawn.mechanitor;
            if (tracker == null) return;

            bool expanded = IsGeneralExpanded;
            DrawSectionHeader("Tolgrim_MechGeneralHeader".Translate(), ref expanded, ref curY, width);
            IsGeneralExpanded = expanded;
            if (!expanded) return;

            int rowIdx = 0;

            // Estado del mecanitor
            AcceptanceReport statusReport = tracker.CanControlMechs;
            string statusStr = statusReport.Accepted ? "Tolgrim_MechStateActive".Translate() : "Tolgrim_MechStateImpaired".Translate();
            Color statusColor = statusReport.Accepted ? ColorLibrary.Green : ColorLibrary.Red;
            DrawKeyValueRow("Tolgrim_MechStatus".Translate(), statusStr, ref curY, width, statusReport.Accepted ? null : statusReport.Reason, rowIdx++, statusColor);

            // Ancho de banda total, usado, libre
            int totalBandwidth = tracker.TotalBandwidth;
            int usedBandwidth = tracker.UsedBandwidth;
            int freeBandwidth = totalBandwidth - usedBandwidth;
            string bwDisplay = $"{usedBandwidth} / {totalBandwidth} ({"Tolgrim_BandwidthAvailable".Translate()}: {freeBandwidth})";
            Color bwColor = freeBandwidth < 0 ? ColorLibrary.Red : (freeBandwidth == 0 ? ColorLibrary.Yellow : Color.white);
            
            // Tooltip nativo con desglose real desde StatWorker
            StatRequest bwReq = StatRequest.For(pawn);
            string bwExplanation = StatDefOf.MechBandwidth.Worker.GetExplanationFull(bwReq, StatDefOf.MechBandwidth.toStringNumberSense, totalBandwidth);
            TaggedString bwTip = "Bandwidth".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": " + $"{usedBandwidth} / {totalBandwidth}\n\n" + bwExplanation;
            if (tracker.UsedBandwidthFromSubjects > 0)
            {
                bwTip += $"\n\n{"BandwidthUsage".Translate()}: {tracker.UsedBandwidthFromSubjects}";
            }
            if (tracker.UsedBandwidthFromGestation > 0)
            {
                bwTip += $"\n\n{"MechGestationBandwidthUsed".Translate()}: {tracker.UsedBandwidthFromGestation}";
            }
            DrawKeyValueRow("Tolgrim_MechBandwidth".Translate(), bwDisplay, ref curY, width, bwTip, rowIdx++, bwColor);

            // Desglose de bonificaciones de Neurofibra, Sueros y otras fuentes
            float neuroBonus = BandwidthBonusRegistry.GetBonusBySource(pawn, "Neurofibre");
            if (neuroBonus > 0f)
            {
                DrawKeyValueRow("Tolgrim_NeurofibreBonus".Translate(), $"+{neuroBonus:0.##}", ref curY, width, "Tolgrim_NeurofibreBonusTip".Translate(), rowIdx++);
            }

            CompMechanitorBandwidth? bwComp = pawn.TryGetComp<CompMechanitorBandwidth>();
            int doses = bwComp?.bandnodeSerumDoses ?? 0;
            float serumBonus = BandwidthBonusRegistry.GetBonusBySource(pawn, "BandnodeSera");
            if (doses > 0 || S.enableBandnodeSera)
            {
                string serumVal = $"+{serumBonus:0.##} ({doses}/{S.maxBandnodeSeraDoses})";
                DrawKeyValueRow("Tolgrim_BandnodeSeraBonus".Translate(), serumVal, ref curY, width, "Tolgrim_BandnodeSeraDesc".Translate(), rowIdx++);
            }

            float otherBonus = BandwidthBonusRegistry.GetTotalBonus(pawn) - neuroBonus - serumBonus;
            if (otherBonus > 0.001f)
            {
                DrawKeyValueRow("Tolgrim_OtherBandwidthBonus".Translate(), $"+{otherBonus:0.##}", ref curY, width, null, rowIdx++);
            }

            // Radio de control de mecas (configurable)
            if (S.showCommandRange)
            {
                string rangeUnit = "Tolgrim_TilesUnit".Translate();
                float currentRange = S.baseCommandRange > 0f ? S.baseCommandRange : 24.9f;
                DrawKeyValueRow("Tolgrim_MechControlRange".Translate(), $"{currentRange:F1} {rangeUnit}", ref curY, width, "Tolgrim_MechControlRangeTip".Translate(), rowIdx++);
            }

            // Cantidad de mecas controlados / supervisados
            int mechsCount = tracker.OverseenPawns.Count;
            int activeMechsCount = tracker.ControlledPawns.Count;
            string mechsDisplay = $"{activeMechsCount} / {mechsCount}";
            string mechsTip = "Tolgrim_MechsControlledTip".Translate();
            DrawKeyValueRow("Tolgrim_MechsCount".Translate(), mechsDisplay, ref curY, width, mechsTip, rowIdx++);

            // Grupos de control disponibles (configurable)
            if (S.showControlGroups)
            {
                int totalControlGroups = tracker.TotalAvailableControlGroups;
                int activeControlGroups = tracker.controlGroups.Count;
                string groupsDisplay = $"{activeControlGroups} / {totalControlGroups}";
                string groupsTip = "Tolgrim_ControlGroupsTip".Translate();

                if (activeControlGroups > 0)
                {
                    groupsTip += "\n";
                    for (int i = 0; i < activeControlGroups; i++)
                    {
                        MechanitorControlGroup g = tracker.controlGroups[i];
                        string? custom = WorldComponent_ControlGroupNames.GetCustomName(g);
                        string gLabel = !string.IsNullOrWhiteSpace(custom) ? custom! : ("Group".Translate() + " " + g.Index);
                        groupsTip += $"\n• {gLabel}: {g.WorkMode.LabelCap} ({g.MechsForReading.Count} {"MechsLower".Translate()})";
                    }
                }

                Rect rowRect = new Rect(0f, curY, width, 24f);
                DrawKeyValueRow("Tolgrim_ControlGroups".Translate(), groupsDisplay, ref curY, width, groupsTip, rowIdx++);

                // Clic en la fila de grupos de control para desplegar menú rápido de renombrar o colorear grupos
                if (Mouse.IsOver(rowRect) && Event.current.type == EventType.MouseDown && Event.current.button == 0 && activeControlGroups > 0)
                {
                    Event.current.Use();
                    List<FloatMenuOption> groupMenu = new List<FloatMenuOption>();
                    for (int i = 0; i < activeControlGroups; i++)
                    {
                        MechanitorControlGroup g = tracker.controlGroups[i];
                        string? custom = WorldComponent_ControlGroupNames.GetCustomName(g);
                        string gLabel = !string.IsNullOrWhiteSpace(custom) ? custom! : ("Group".Translate() + " " + g.Index);

                        groupMenu.Add(new FloatMenuOption($"{"Tolgrim_RenameControlGroup".Translate()}: {gLabel}", () =>
                        {
                            Find.WindowStack.Add(new Dialog_RenameControlGroup(g));
                        }, TexButton.Rename, Color.white));

                        MechColorState? cState = MechColorState.For(g);
                        if (cState != null)
                        {
                            groupMenu.Add(new FloatMenuOption($"{"Tolgrim_SelectMechColor".Translate()}: {gLabel}", () =>
                            {
                                cState.DoDialog();
                            }, MechColorTextures.ChangeColor, Color.white));
                        }
                    }
                    Find.WindowStack.Add(new FloatMenu(groupMenu));
                }
            }

            curY += 6f;
        }

        private void DrawCapabilitiesSection(Pawn pawn, ref float curY, float width)
        {
            bool expanded = IsStatsExpanded;
            DrawSectionHeader("Tolgrim_MechStatsHeader".Translate(), ref expanded, ref curY, width);
            IsStatsExpanded = expanded;
            if (!expanded) return;

            int rowIdx = 0;
            DrawPawnStatRow(pawn, StatDefOf.MechBandwidth, ref curY, width, rowIdx++);
            DrawPawnStatRow(pawn, StatDefOf.MechControlGroups, ref curY, width, rowIdx++);

            StatDef? remoteRepairDist = DefDatabase<StatDef>.GetNamedSilentFail("MechRemoteRepairDistance");
            if (remoteRepairDist != null && (remoteRepairDist.showIfUndefined || pawn.GetStatValue(remoteRepairDist) > 0f))
            {
                DrawPawnStatRow(pawn, remoteRepairDist, ref curY, width, rowIdx++);
            }

            StatDef? remoteShieldDist = DefDatabase<StatDef>.GetNamedSilentFail("MechRemoteShieldDistance");
            if (remoteShieldDist != null && (remoteShieldDist.showIfUndefined || pawn.GetStatValue(remoteShieldDist) > 0f))
            {
                DrawPawnStatRow(pawn, remoteShieldDist, ref curY, width, rowIdx++);
            }

            StatDef? remoteShieldEnergy = DefDatabase<StatDef>.GetNamedSilentFail("MechRemoteShieldEnergy");
            if (remoteShieldEnergy != null && (remoteShieldEnergy.showIfUndefined || pawn.GetStatValue(remoteShieldEnergy) > 0f))
            {
                DrawPawnStatRow(pawn, remoteShieldEnergy, ref curY, width, rowIdx++);
            }

            StatDef? formingSpeed = DefDatabase<StatDef>.GetNamedSilentFail("MechFormingSpeed");
            if (formingSpeed != null)
            {
                DrawPawnStatRow(pawn, formingSpeed, ref curY, width, rowIdx++);
            }

            StatDef? repairSpeed = DefDatabase<StatDef>.GetNamedSilentFail("MechRepairSpeed");
            if (repairSpeed != null)
            {
                DrawPawnStatRow(pawn, repairSpeed, ref curY, width, rowIdx++);
            }

            StatDef? workSpeed = DefDatabase<StatDef>.GetNamedSilentFail("MechWorkSpeed");
            if (workSpeed != null && pawn.GetStatValue(workSpeed) > 0f)
            {
                DrawPawnStatRow(pawn, workSpeed, ref curY, width, rowIdx++);
            }

            curY += 6f;
        }

        private void DrawPawnStatRow(Pawn pawn, StatDef stat, ref float curY, float width, int rowIndex)
        {
            float val = pawn.GetStatValue(stat);
            string valStr = stat.ValueToString(val, stat.toStringNumberSense);
            string label = stat.LabelCap;

            StatRequest req = StatRequest.For(pawn);
            string tip = stat.description;
            if (!string.IsNullOrEmpty(tip))
            {
                tip += "\n\n" + stat.Worker.GetExplanationFull(req, stat.toStringNumberSense, val);
            }
            else
            {
                tip = stat.Worker.GetExplanationFull(req, stat.toStringNumberSense, val);
            }

            DrawKeyValueRow(label, valStr, ref curY, width, tip, rowIndex);
        }

        private void DrawImplantsSection(Pawn pawn, ref float curY, float width)
        {
            bool expanded = IsImplantsExpanded;
            DrawSectionHeader("Tolgrim_MechImplantsHeader".Translate(), ref expanded, ref curY, width);
            IsImplantsExpanded = expanded;
            if (!expanded) return;

            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            int count = 0;

            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff h = hediffs[i];
                if (!IsMechanitorRelatedHediff(h)) continue;

                DrawImplantRow(pawn, h, ref curY, width, count++);
            }

            if (count == 0)
            {
                DrawEmptyRow("Tolgrim_NoMechImplants".Translate(), ref curY, width);
            }

            curY += 6f;
        }

        private void DrawEquipmentSection(Pawn pawn, ref float curY, float width)
        {
            bool expanded = IsEquipmentExpanded;
            DrawSectionHeader("Tolgrim_MechEquipmentHeader".Translate(), ref expanded, ref curY, width);
            IsEquipmentExpanded = expanded;
            if (!expanded) return;

            int count = 0;

            // Ropa / Armaduras / Mochilas de ancho de banda
            if (pawn.apparel != null)
            {
                List<Apparel> worn = pawn.apparel.WornApparel;
                for (int i = 0; i < worn.Count; i++)
                {
                    Apparel app = worn[i];
                    if (HasMechanitorOffsets(app.def.equippedStatOffsets))
                    {
                        DrawItemRow(pawn, app, ref curY, width, count++);
                    }
                }
            }

            // Armas / Herramientas equipadas
            if (pawn.equipment != null)
            {
                List<ThingWithComps> eqList = pawn.equipment.AllEquipmentListForReading;
                for (int i = 0; i < eqList.Count; i++)
                {
                    ThingWithComps eq = eqList[i];
                    if (HasMechanitorOffsets(eq.def.equippedStatOffsets))
                    {
                        DrawItemRow(pawn, eq, ref curY, width, count++);
                    }
                }
            }

            if (count == 0)
            {
                DrawEmptyRow("Tolgrim_NoMechEquipment".Translate(), ref curY, width);
            }

            curY += 6f;
        }

        private bool IsMechanitorRelatedHediff(Hediff h)
        {
            if (h.def == HediffDefOf.MechlinkImplant) return true;
            if (h.def.defName == "Tolgrim_MechNeurofibre") return true;
            if (NeurofibreBonusProvider.GetNeurofibreTier(h.def.defName, out _) > 0) return true;
            if (h.def.defName.Contains("Sublink") || h.def.defName.Contains("RemoteRepairer") || 
                h.def.defName.Contains("RemoteShielder") || h.def.defName.Contains("RepairProbe") ||
                h.def.defName.Contains("MechFormfeeder") || h.def.defName.Contains("Mechlink"))
            {
                return true;
            }

            HediffStage stage = h.CurStage;
            if (stage != null && HasMechanitorOffsets(stage.statOffsets))
            {
                return true;
            }
            return false;
        }

        private bool HasMechanitorOffsets(List<StatModifier>? statOffsets)
        {
            if (statOffsets == null) return false;
            for (int i = 0; i < statOffsets.Count; i++)
            {
                StatDef stat = statOffsets[i].stat;
                if (stat == null) continue;
                if (stat.category?.defName == "Mechanitor" ||
                    stat == StatDefOf.MechBandwidth ||
                    stat == StatDefOf.MechControlGroups)
                {
                    return true;
                }
            }
            return false;
        }

        private void DrawSectionHeader(string label, ref bool expanded, ref float curY, float width)
        {
            Rect rect = new Rect(0f, curY, width, SectionHeaderHeight);
            Widgets.DrawBoxSolid(rect, HeaderBgColor);

            Rect toggleRect = new Rect(rect.x + 5f, rect.y + 4f, 18f, 18f);
            GUI.DrawTexture(toggleRect, expanded ? TexButton.Collapse : TexButton.Reveal);

            Rect labelRect = new Rect(rect.x + 28f, rect.y, rect.width - 28f, rect.height);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Tiny;
            Widgets.Label(labelRect, label.ToUpperInvariant());
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonInvisible(rect))
            {
                expanded = !expanded;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }

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
                Widgets.DrawBoxSolid(rowRect, HighlightRowColor);
            }

            if (!string.IsNullOrEmpty(tooltip))
            {
                TooltipHandler.TipRegion(rowRect, tooltip);
            }

            // Repartición equilibrada 55% clave / 45% valor
            Rect keyRect = new Rect(rowRect.x + 6f, rowRect.y, rowRect.width * 0.55f - 6f, rowRect.height);
            Rect valRect = new Rect(rowRect.x + rowRect.width * 0.55f, rowRect.y, rowRect.width * 0.45f - 6f, rowRect.height);

            Text.WordWrap = false;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(keyRect, key.Truncate(keyRect.width));

            Text.Anchor = TextAnchor.MiddleRight;
            if (valColor.HasValue)
            {
                GUI.color = valColor.Value;
            }
            Widgets.Label(valRect, value.Truncate(valRect.width));
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;

            curY += RowHeight;
        }

        private void DrawImplantRow(Pawn pawn, Hediff hediff, ref float curY, float width, int rowIndex)
        {
            Rect rowRect = new Rect(0f, curY, width, RowHeight);

            if (rowIndex % 2 == 1)
            {
                Widgets.DrawBoxSolid(rowRect, AlternateRowColor);
            }
            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawBoxSolid(rowRect, HighlightRowColor);
            }

            string tip = hediff.GetTooltip(pawn, Prefs.DevMode);
            TooltipHandler.TipRegion(rowRect, tip);

            // 52% nombre de implante / 48% bonificación compacta
            Rect labelRect = new Rect(rowRect.x + 6f, rowRect.y, rowRect.width * 0.52f - 6f, rowRect.height);
            Rect valRect = new Rect(rowRect.x + rowRect.width * 0.52f, rowRect.y, rowRect.width * 0.48f - 6f, rowRect.height);

            Text.WordWrap = false;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, hediff.LabelCap.Truncate(labelRect.width));

            // Resumen compacto de bonificación
            string bonusSummary = GetHediffBonusSummary(hediff);
            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = ColoredText.SubtleGrayColor;
            Widgets.Label(valRect, bonusSummary.Truncate(valRect.width));
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;

            curY += RowHeight;
        }

        private void DrawItemRow(Pawn pawn, Thing item, ref float curY, float width, int rowIndex)
        {
            Rect rowRect = new Rect(0f, curY, width, RowHeight);

            if (rowIndex % 2 == 1)
            {
                Widgets.DrawBoxSolid(rowRect, AlternateRowColor);
            }
            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawBoxSolid(rowRect, HighlightRowColor);
            }

            TooltipHandler.TipRegion(rowRect, item.GetTooltip());

            // Icono
            Rect iconRect = new Rect(rowRect.x + 4f, rowRect.y + 2f, 20f, 20f);
            Widgets.ThingIcon(iconRect, item);

            // Label de ítem y resumen compacto
            Rect labelRect = new Rect(rowRect.x + 28f, rowRect.y, rowRect.width * 0.48f - 28f, rowRect.height);
            Rect valRect = new Rect(rowRect.x + rowRect.width * 0.48f, rowRect.y, rowRect.width * 0.52f - 6f, rowRect.height);

            Text.WordWrap = false;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, item.LabelCap.Truncate(labelRect.width));

            string bonusSummary = GetStatOffsetsSummary(item.def.equippedStatOffsets);
            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = ColoredText.SubtleGrayColor;
            Widgets.Label(valRect, bonusSummary.Truncate(valRect.width));
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;

            curY += RowHeight;
        }

        private void DrawEmptyRow(string message, ref float curY, float width)
        {
            Rect rowRect = new Rect(0f, curY, width, RowHeight);
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = ColoredText.SubtleGrayColor;
            Widgets.Label(rowRect, message);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            curY += RowHeight;
        }

        private string GetHediffBonusSummary(Hediff hediff)
        {
            if (hediff.def.defName == "Tolgrim_MechNeurofibre")
            {
                int lvl = (hediff as Hediff_Level)?.level ?? (int)System.Math.Round(hediff.Severity);
                float mult = MechanitorExpandedMod.Settings?.neurofibreMultiplier ?? 1f;
                return $"+{lvl * 3f * mult:0.##} {"Tolgrim_ShortBandwidth".Translate()}";
            }

            if (NeurofibreBonusProvider.GetNeurofibreTier(hediff.def.defName, out float baseVal) > 0)
            {
                float mult = MechanitorExpandedMod.Settings?.neurofibreMultiplier ?? 1f;
                return $"+{baseVal * mult:0.##} {"Tolgrim_ShortBandwidth".Translate()}";
            }

            HediffStage stage = hediff.CurStage;
            if (stage == null || stage.statOffsets.NullOrEmpty())
            {
                return hediff.Part != null ? hediff.Part.LabelCap : "";
            }
            return GetStatOffsetsSummary(stage.statOffsets);
        }

        private string GetStatOffsetsSummary(List<StatModifier>? offsets)
        {
            if (offsets.NullOrEmpty()) return "";
            StringBuilder sb = new StringBuilder();
            List<StatModifier> list = offsets!;
            for (int i = 0; i < list.Count; i++)
            {
                StatModifier mod = list[i];
                if (mod == null) continue;
                StatDef? stat = mod.stat;
                if (stat == null) continue;
                if (stat.category?.defName == "Mechanitor" ||
                    stat == StatDefOf.MechBandwidth ||
                    stat == StatDefOf.MechControlGroups)
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append(mod.ValueToStringAsOffset);
                    sb.Append(" ");
                    sb.Append(GetCompactStatLabel(stat));
                }
            }
            return sb.ToString();
        }

        private string GetCompactStatLabel(StatDef stat)
        {
            if (stat == StatDefOf.MechBandwidth) return "Tolgrim_ShortBandwidth".Translate();
            if (stat == StatDefOf.MechControlGroups) return "Tolgrim_ShortControlGroups".Translate();
            if (stat.defName == "MechFormingSpeed") return "Tolgrim_ShortGestationSpeed".Translate();
            if (stat.defName == "MechRepairSpeed") return "Tolgrim_ShortRepairSpeed".Translate();
            if (stat.defName == "MechRemoteRepairDistance") return "Tolgrim_ShortRepairDist".Translate();
            if (stat.defName == "MechRemoteShieldDistance") return "Tolgrim_ShortShieldDist".Translate();
            if (stat.defName == "MechRemoteShieldEnergy") return "Tolgrim_ShortShieldEnergy".Translate();
            if (stat.defName == "MechWorkSpeed") return "Tolgrim_ShortWorkSpeed".Translate();
            return stat.LabelCap;
        }
    }
}
