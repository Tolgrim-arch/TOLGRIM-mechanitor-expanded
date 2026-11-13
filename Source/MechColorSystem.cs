using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Tolgrim_MechanitorExpanded
{
    [StaticConstructorOnStartup]
    public static class MechColorTextures
    {
        public static readonly Texture2D Circle = ContentFinder<Texture2D>.Get("UI/Icons/Tolgrim_MechColorCircle", true);
        public static readonly Texture2D ChangeColor = ContentFinder<Texture2D>.Get("UI/Commands/ChangeColor", true);
    }

    public class MechColorState
    {
        private static readonly ConditionalWeakTable<Pawn, MechColorState> pawnTable = new ConditionalWeakTable<Pawn, MechColorState>();
        private static readonly ConditionalWeakTable<MechanitorControlGroup, MechColorState> groupTable = new ConditionalWeakTable<MechanitorControlGroup, MechColorState>();

        public Color? color;
        public float? opacity;

        public readonly Pawn? pawn;
        public readonly MechanitorControlGroup? group;

        public IEnumerable<Pawn> AffectedMechs
        {
            get
            {
                if (group != null)
                {
                    foreach (AssignedMech assignedMech in group.AssignedMechs)
                    {
                        yield return assignedMech.pawn;
                    }
                }
                else if (pawn != null && pawn.IsColonyMech)
                {
                    yield return pawn;
                }
                else if (pawn?.mechanitor != null)
                {
                    foreach (Pawn overseen in pawn.mechanitor.OverseenPawns)
                    {
                        yield return overseen;
                    }
                }
            }
        }

        public Color? SetColor => color ?? Next?.SetColor;
        public Color ActiveColor => SetColor ?? FactionMechColor;
        public Color DefaultColor => Next?.ActiveColor ?? FactionMechColor;
        private Color FactionMechColor => Find.FactionManager?.OfPlayer?.AllegianceColor ?? Color.white;
        public float? SetOpacity => opacity ?? Next?.SetOpacity;

        private MechColorState? Next
        {
            get
            {
                if (group?.Tracker?.Pawn != null)
                {
                    return For(group.Tracker.Pawn);
                }
                if (pawn != null && pawn.IsColonyMech)
                {
                    MechanitorControlGroup? g = MechanitorUtility.GetMechControlGroup(pawn);
                    if (g != null) return For(g);
                }
                return null;
            }
        }

        public IEnumerable<FloatMenuOption> Menu
        {
            get
            {
                yield return new FloatMenuOption("Tolgrim_SelectMechColor".Translate(), DoDialog);
                yield return new FloatMenuOption("Tolgrim_CopyMechColor".Translate(), StartCopy);
                yield return new FloatMenuOption("Tolgrim_DefaultMechColor".Translate(), Reset);
            }
        }

        private TargetingParameters TargetParams => new TargetingParameters
        {
            canTargetPawns = true,
            canTargetBuildings = false,
            canTargetHumans = false,
            canTargetAnimals = false,
            onlyTargetFactions = new List<Faction>(1) { Find.FactionManager.OfPlayer }
        };

        public static MechColorState? For(Pawn? p)
        {
            if (p == null || !HasState(p)) return null;
            return pawnTable.GetValue(p, Create);
        }

        public static MechColorState? For(MechanitorControlGroup? g)
        {
            if (g == null) return null;
            return groupTable.GetValue(g, Create);
        }

        private static bool HasState(Pawn p)
        {
            if (p.IsColonyMech) return true;
            if (MechanitorUtility.IsMechanitor(p) && p.IsColonist) return true;
            return false;
        }

        private static MechColorState Create(Pawn p) => new MechColorState(p);
        private static MechColorState Create(MechanitorControlGroup g) => new MechColorState(g);

        private MechColorState(Pawn p) { pawn = p; }
        private MechColorState(MechanitorControlGroup g) { group = g; }
        protected MechColorState(MechColorState copy)
        {
            pawn = copy.pawn;
            group = copy.group;
            color = copy.color;
            opacity = copy.opacity;
        }

        public virtual MechColorState Merge(MechColorState other)
        {
            return new MergedMechColorState(this).Merge(other);
        }

        public void ExposeData()
        {
            if (Scribe.EnterNode("tolgrim_mechColor"))
            {
                Scribe_Values.Look(ref color, "color");
                Scribe_Values.Look(ref opacity, "opacity");
                Scribe.ExitNode();
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                // Retrocompatibilidad con guardados que usaban Mech Colors original
                if (Scribe.EnterNode("kathanon.MechColors"))
                {
                    Scribe_Values.Look(ref color, "color");
                    Scribe_Values.Look(ref opacity, "opacity");
                    Scribe.ExitNode();
                }
            }
        }

        public Gizmo Gizmo() => new Command_MechColor(this);

        public void DrawColorIndicator(Rect rect, bool low)
        {
            if (!color.HasValue) return;

            float size = rect.height * 0.4f;
            rect.xMin += rect.width - size;
            if (low)
            {
                rect.yMin += rect.height - size;
            }
            else
            {
                rect.yMax -= rect.height - size;
            }

            rect = rect.ContractedBy(6f);
            GUI.color = color.Value;
            GUI.DrawTexture(rect, MechColorTextures.Circle);
            GUI.color = Color.white;
        }

        public void OpenMenu()
        {
            Find.WindowStack.Add(new FloatMenu(Menu.ToList()));
        }

        public void DoDialog()
        {
            Find.WindowStack.Add(new Dialog_ChooseMechColor(this));
        }

        private void StartCopy()
        {
            Find.Targeter.BeginTargeting(TargetParams, CopyFrom);
        }

        private void CopyFrom(LocalTargetInfo target)
        {
            if (target.Thing is Pawn targetPawn)
            {
                MechColorState? st = For(targetPawn);
                if (st != null)
                {
                    Set(st.SetColor, st.SetOpacity);
                }
            }
        }

        public void Reset()
        {
            Set(null, null);
        }

        public virtual void Set(Color? newColor, float? newOpacity)
        {
            color = newColor;
            opacity = newOpacity;
            foreach (Pawn mech in AffectedMechs)
            {
                PortraitsCache.SetDirty(mech);
            }
        }
    }

    public class MergedMechColorState : MechColorState
    {
        private readonly List<MechColorState> parts = new List<MechColorState>();

        public MergedMechColorState(MechColorState main) : base(main)
        {
            parts.Add(main);
        }

        public override MechColorState Merge(MechColorState other)
        {
            parts.Add(other);
            return this;
        }

        public override void Set(Color? newColor, float? newOpacity)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                parts[i].Set(newColor, newOpacity);
            }
        }
    }

    public class Command_MechColor : Command_Action
    {
        private MechColorState state;

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => state.Menu;

        public Command_MechColor(MechColorState state)
        {
            this.state = state;
            bool isMech = state.pawn != null && state.pawn.IsColonyMech;
            defaultLabel = isMech ? "Tolgrim_SetMechColorLabel".Translate() : "Tolgrim_SetDefaultMechColorLabel".Translate();
            defaultDesc = isMech ? "Tolgrim_SetMechColorDesc".Translate() : "Tolgrim_SetDefaultMechColorDesc".Translate();
            icon = MechColorTextures.ChangeColor;
            action = state.DoDialog;
            alsoClickIfOtherInGroupClicked = false;
        }

        public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
        {
            base.DrawIcon(rect, buttonMat, parms);
            state.DrawColorIndicator(rect, low: false);
        }

        public override bool GroupsWith(Gizmo other) => other is Command_MechColor;

        public override void MergeWith(Gizmo other)
        {
            if (other is Command_MechColor otherColor)
            {
                state = state.Merge(otherColor.state);
            }
        }
    }

    public class Dialog_ChooseMechColor : Window
    {
        private static readonly List<Color> cachedColors = DefDatabase<ColorDef>.AllDefsListForReading.Select(c => c.color).ToList();
        private readonly MechColorState state;
        private readonly string title;
        private Color curColor;
        private readonly Color defaultColor;
        private bool useDefault;
        private float opacity;
        private Vector2 scrollPosition = Vector2.zero;

        public override Vector2 InitialSize => new Vector2(480f, 430f);

        public Dialog_ChooseMechColor(MechColorState state)
        {
            this.state = state;
            title = GetTitle(state);
            useDefault = !state.color.HasValue;
            defaultColor = state.DefaultColor;
            curColor = state.ActiveColor;
            opacity = state.SetOpacity ?? 0.5f;

            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            doCloseX = true;
        }

        private static string GetTitle(MechColorState s)
        {
            string targetName = s.pawn?.LabelShortCap ?? (WorldComponent_ControlGroupNames.GetCustomName(s.group!) ?? ("Group".Translate() + " " + s.group!.Index));
            return "Tolgrim_ChooseMechColorTitle".Translate(targetName);
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Título
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width - 30f, 32f);
            Widgets.Label(titleRect, title);
            Text.Font = GameFont.Small;

            float curY = inRect.y + 38f;

            // Fila de muestra de color y checkbox de color por defecto
            Rect previewRect = new Rect(inRect.x, curY, 52f, 52f);
            Widgets.DrawBoxSolid(previewRect, new Color(0.1f, 0.1f, 0.1f, 1f));
            Widgets.DrawBoxSolid(previewRect.ContractedBy(3f), useDefault ? defaultColor : curColor);
            Widgets.DrawBox(previewRect);

            Rect defaultCheckRect = new Rect(inRect.x + 62f, curY + 12f, 220f, 26f);
            bool prevUseDefault = useDefault;
            Widgets.CheckboxLabeled(defaultCheckRect, "Tolgrim_DefaultColorCheck".Translate(), ref useDefault);
            if (useDefault && !prevUseDefault)
            {
                curColor = defaultColor;
            }

            curY += 60f;

            // Paleta de colores en cuadrícula ordenada
            Rect paletteOuterRect = new Rect(inRect.x, curY, inRect.width, 180f);
            Widgets.DrawBoxSolid(paletteOuterRect, new Color(0f, 0f, 0f, 0.2f));

            int boxSize = 22;
            int boxPad = 4;
            int totalBox = boxSize + boxPad;
            int columns = Mathf.FloorToInt((paletteOuterRect.width - 16f) / totalBox);
            if (columns < 1) columns = 1;
            int rows = Mathf.CeilToInt((float)cachedColors.Count / columns);
            Rect viewRect = new Rect(0f, 0f, paletteOuterRect.width - 16f, rows * totalBox + 6f);

            Widgets.BeginScrollView(paletteOuterRect, ref scrollPosition, viewRect);
            for (int i = 0; i < cachedColors.Count; i++)
            {
                int col = i % columns;
                int row = i / columns;
                Rect boxRect = new Rect(col * totalBox + 2f, row * totalBox + 2f, boxSize, boxSize);
                Color c = cachedColors[i];

                Widgets.DrawBoxSolid(boxRect, c);
                if (curColor.IndistinguishableFrom(c) && !useDefault)
                {
                    Widgets.DrawBox(boxRect.ExpandedBy(2f), 2);
                }

                if (Mouse.IsOver(boxRect))
                {
                    Widgets.DrawHighlight(boxRect);
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        Event.current.Use();
                        curColor = c;
                        useDefault = false;
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    }
                }
            }
            Widgets.EndScrollView();

            curY += 190f;

            // Control deslizante de opacidad
            string opacityText = $"{"Tolgrim_MechColorOpacity".Translate()} {opacity:P0}" + (useDefault ? $" ({"Tolgrim_FromDefaultColor".Translate()})" : "");
            Widgets.Label(new Rect(inRect.x, curY, inRect.width, 22f), opacityText);
            curY += 24f;

            if (!useDefault)
            {
                opacity = Widgets.HorizontalSlider(new Rect(inRect.x, curY, inRect.width, 22f), opacity, 0.1f, 1f, roundTo: 0.05f);
            }
            else
            {
                GUI.color = ColoredText.SubtleGrayColor;
                Widgets.HorizontalSlider(new Rect(inRect.x, curY, inRect.width, 22f), opacity, 0.1f, 1f, roundTo: 0.05f);
                GUI.color = Color.white;
            }

            curY += 32f;

            // Botones inferiores: Guardar / Cancelar
            float btnWidth = 130f;
            float btnHeight = 32f;
            Rect closeRect = new Rect(inRect.x, inRect.height - btnHeight, btnWidth, btnHeight);
            Rect okRect = new Rect(inRect.width - btnWidth, inRect.height - btnHeight, btnWidth, btnHeight);

            if (Widgets.ButtonText(closeRect, "CloseButton".Translate()))
            {
                Close();
            }

            if (Widgets.ButtonText(okRect, "OK".Translate()))
            {
                if (useDefault)
                {
                    state.Reset();
                }
                else
                {
                    state.Set(curColor, opacity);
                }
                Close();
            }
        }
    }
}
