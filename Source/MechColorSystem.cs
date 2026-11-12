using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

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

    public class Dialog_ChooseMechColor : Dialog_ChooseColor
    {
        private class CallbackHolder
        {
            public Dialog_ChooseMechColor? dialog;
            public void Select(Color col) => dialog?.OnColorSelected(col);
        }

        private static readonly List<Color> cachedColors = DefDatabase<ColorDef>.AllDefsListForReading.Select(c => c.color).ToList();
        private readonly MechColorState state;
        private bool useDefault;
        private readonly Color curDefault;
        private float opacity;
        private static CallbackHolder callback = new CallbackHolder();
        private static readonly FieldInfo? SelectedColorField = typeof(Dialog_ChooseColor).GetField("selectedColor", BindingFlags.Instance | BindingFlags.NonPublic);

        public Dialog_ChooseMechColor(MechColorState state)
            : base(GetTitle(state), state.ActiveColor, cachedColors, callback.Select)
        {
            this.state = state;
            useDefault = !state.color.HasValue;
            curDefault = state.DefaultColor;
            opacity = state.SetOpacity ?? 0.5f;

            callback.dialog = this;
            callback = new CallbackHolder();
        }

        private static string GetTitle(MechColorState s)
        {
            string targetName = s.pawn?.LabelShortCap ?? (WorldComponent_ControlGroupNames.GetCustomName(s.group!) ?? ("Group".Translate() + " " + s.group!.Index));
            return "Tolgrim_ChooseMechColorTitle".Translate(targetName);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Color prevColor = GetSelectedColor();
            base.DoWindowContents(inRect);

            if (prevColor != GetSelectedColor())
            {
                useDefault = false;
            }

            // Checkbox "Usar color predeterminado"
            Rect checkRect = new Rect(inRect.x + 2f, inRect.y + 35f + 10f + 88f + 4f, 130f, 24f);
            Widgets.CheckboxLabeled(checkRect, "Tolgrim_DefaultColorCheck".Translate(), ref useDefault);
            if (useDefault)
            {
                SetSelectedColor(curDefault);
            }

            Vector2 closeSize = Window.CloseButSize;
            Rect sliderArea = new Rect(inRect.x + closeSize.x + 12f, inRect.height - closeSize.y, inRect.width - (closeSize.x * 2f) - 24f, closeSize.y);

            string defaultNotice = useDefault ? $" ({"Tolgrim_FromDefaultColor".Translate()})" : "";
            Widgets.Label(new Rect(sliderArea.x, sliderArea.y - 20f, sliderArea.width, 22f), $"{"Tolgrim_MechColorOpacity".Translate()} {opacity:P0}{defaultNotice}");

            if (!useDefault)
            {
                opacity = Widgets.HorizontalSlider(new Rect(sliderArea.x, sliderArea.y + 2f, sliderArea.width, 20f), opacity, 0f, 1f, roundTo: 0.05f);
            }
        }

        private Color GetSelectedColor()
        {
            if (SelectedColorField != null)
            {
                return (Color)SelectedColorField.GetValue(this);
            }
            return Color.white;
        }

        private void SetSelectedColor(Color col)
        {
            SelectedColorField?.SetValue(this, col);
        }

        public void OnColorSelected(Color col)
        {
            if (useDefault)
            {
                state.Reset();
            }
            else
            {
                state.Set(col, opacity);
            }
        }
    }
}
