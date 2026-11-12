using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    public class WorldComponent_ControlGroupNames : WorldComponent
    {
        private List<string> keys = new List<string>();
        private List<string> values = new List<string>();
        public Dictionary<string, string> customNames = new Dictionary<string, string>();

        public static WorldComponent_ControlGroupNames? Instance => Find.World?.GetComponent<WorldComponent_ControlGroupNames>();

        public WorldComponent_ControlGroupNames(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref customNames, "tolgrim_controlGroupNames", LookMode.Value, LookMode.Value, ref keys, ref values);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && customNames == null)
            {
                customNames = new Dictionary<string, string>();
            }
        }

        public static string GetKey(MechanitorControlGroup group)
        {
            if (group?.Tracker?.Pawn == null) return string.Empty;
            return $"{group.Tracker.Pawn.ThingID}_{group.Index}";
        }

        public static string? GetCustomName(MechanitorControlGroup group)
        {
            if (group == null) return null;
            string key = GetKey(group);
            if (string.IsNullOrEmpty(key)) return null;

            WorldComponent_ControlGroupNames? comp = Instance;
            if (comp != null && comp.customNames.TryGetValue(key, out string name) && !string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            return null;
        }

        public static void SetCustomName(MechanitorControlGroup group, string newName)
        {
            if (group == null) return;
            string key = GetKey(group);
            if (string.IsNullOrEmpty(key)) return;

            WorldComponent_ControlGroupNames? comp = Instance;
            if (comp == null) return;

            if (string.IsNullOrWhiteSpace(newName))
            {
                comp.customNames.Remove(key);
            }
            else
            {
                comp.customNames[key] = newName.Trim();
            }
        }
    }

    public class ControlGroupRenameAdapter : IRenameable
    {
        private readonly MechanitorControlGroup group;

        public ControlGroupRenameAdapter(MechanitorControlGroup group)
        {
            this.group = group;
        }

        public string RenamableLabel
        {
            get => WorldComponent_ControlGroupNames.GetCustomName(group) ?? BaseLabel;
            set => WorldComponent_ControlGroupNames.SetCustomName(group, value);
        }

        public string BaseLabel => ("Group".Translate()) + " " + group.Index;

        public string InspectLabel => RenamableLabel;
    }

    public class Dialog_RenameControlGroup : Dialog_Rename<ControlGroupRenameAdapter>
    {
        public Dialog_RenameControlGroup(MechanitorControlGroup group)
            : base(new ControlGroupRenameAdapter(group))
        {
        }

        protected override AcceptanceReport NameIsValid(string name)
        {
            AcceptanceReport baseReport = base.NameIsValid(name);
            if (!baseReport.Accepted) return baseReport;
            return true;
        }

        protected override void OnRenamed(string name)
        {
            Messages.Message("Tolgrim_ControlGroupRenamed".Translate(name), MessageTypeDefOf.TaskCompletion, historical: false);
        }
    }
}
