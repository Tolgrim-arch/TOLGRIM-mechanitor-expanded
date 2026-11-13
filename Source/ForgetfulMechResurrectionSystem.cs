using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    public class MechanitorMechsRecord : IExposable
    {
        public Dictionary<Pawn, int> mechs = new Dictionary<Pawn, int>();
        private List<Pawn> tmpMech = new List<Pawn>();
        private List<int> tmpGroupIndex = new List<int>();

        public bool IsEmpty => mechs == null || mechs.Count == 0;

        public MechanitorMechsRecord()
        {
        }

        public MechanitorMechsRecord(Pawn mech)
        {
            Add(mech);
        }

        public void Add(Pawn mech)
        {
            if (mech == null) return;
            MechanitorControlGroup? group = MechanitorUtility.GetMechControlGroup(mech);
            int groupIndex = (group != null) ? (group.Index - 1) : 0;
            mechs[mech] = groupIndex;
        }

        public bool Contains(Pawn mech)
        {
            return mech != null && mechs.ContainsKey(mech);
        }

        public int MechGroup(Pawn mech)
        {
            if (mech != null && mechs.TryGetValue(mech, out int val))
            {
                return val;
            }
            return 0;
        }

        public void Remove(Pawn mech)
        {
            if (mech != null)
            {
                mechs.Remove(mech);
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref mechs, "mechs", LookMode.Reference, LookMode.Value, ref tmpMech, ref tmpGroupIndex);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && mechs == null)
            {
                mechs = new Dictionary<Pawn, int>();
            }
        }
    }

    public class WorldComponent_ForgetfulMechResurrection : RimWorld.Planet.WorldComponent
    {
        private Dictionary<Pawn, MechanitorMechsRecord> mechanitorMechs = new Dictionary<Pawn, MechanitorMechsRecord>();
        private Dictionary<Pawn, Pawn> mechRefs = new Dictionary<Pawn, Pawn>();
        private List<Pawn> tmpMechanitors = new List<Pawn>();
        private List<MechanitorMechsRecord> tmpMechs = new List<MechanitorMechsRecord>();

        public static WorldComponent_ForgetfulMechResurrection? Instance => Find.World?.GetComponent<WorldComponent_ForgetfulMechResurrection>();

        public WorldComponent_ForgetfulMechResurrection(RimWorld.Planet.World world) : base(world)
        {
        }

        public void AddMech(Pawn mechanitor, Pawn mech)
        {
            if (mechanitor == null || mech == null) return;

            if (mechanitorMechs.TryGetValue(mechanitor, out MechanitorMechsRecord record))
            {
                record.Add(mech);
            }
            else
            {
                mechanitorMechs[mechanitor] = new MechanitorMechsRecord(mech);
            }
            mechRefs[mech] = mechanitor;
        }

        public void RemoveMech(Pawn mech)
        {
            if (mech == null) return;

            if (mechRefs.TryGetValue(mech, out Pawn overseer))
            {
                if (mechanitorMechs.TryGetValue(overseer, out MechanitorMechsRecord record))
                {
                    record.Remove(mech);
                    if (record.IsEmpty)
                    {
                        mechanitorMechs.Remove(overseer);
                    }
                }
            }
            mechRefs.Remove(mech);
        }

        public Pawn? GetMechOverseer(Pawn mech)
        {
            if (mech != null && mechRefs.TryGetValue(mech, out Pawn overseer))
            {
                return overseer;
            }
            return null;
        }

        public bool ContainsMech(Pawn mech)
        {
            return mech != null && mechRefs.ContainsKey(mech);
        }

        public bool CanMechanitorReviveMech(Pawn mechanitor, Pawn mech)
        {
            if (!MechanitorExpandedMod.Settings.enableForgetfulMechResurrection)
            {
                return true;
            }

            if (mech != null && mechRefs.TryGetValue(mech, out Pawn boundOverseer))
            {
                return boundOverseer == mechanitor;
            }
            return true;
        }

        public bool CanMechRememberMechanitor(Pawn mechanitor, Pawn mech)
        {
            if (mech != null && mechRefs.TryGetValue(mech, out Pawn boundOverseer))
            {
                return boundOverseer == mechanitor;
            }
            return false;
        }

        public int MechGroup(Pawn mech)
        {
            if (mech != null && mechRefs.TryGetValue(mech, out Pawn overseer) &&
                mechanitorMechs.TryGetValue(overseer, out MechanitorMechsRecord record))
            {
                return record.MechGroup(mech);
            }
            return 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                List<Pawn> deadMechs = new List<Pawn>(mechRefs.Keys);
                for (int i = 0; i < deadMechs.Count; i++)
                {
                    Pawn m = deadMechs[i];
                    if (m == null || (m.Destroyed && !m.SpawnedOrAnyParentSpawned))
                    {
                        if (m != null)
                        {
                            RemoveMech(m);
                        }
                    }
                }

                List<Pawn> overseers = new List<Pawn>(mechanitorMechs.Keys);
                for (int i = 0; i < overseers.Count; i++)
                {
                    Pawn ov = overseers[i];
                    if (ov == null || ov.Destroyed)
                    {
                        if (ov != null)
                        {
                            mechanitorMechs.Remove(ov);
                        }
                    }
                }
            }

            Scribe_Collections.Look(ref mechanitorMechs, "tolgrim_mechanitorMechs", LookMode.Reference, LookMode.Deep, ref tmpMechanitors, ref tmpMechs);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (mechanitorMechs == null)
                {
                    mechanitorMechs = new Dictionary<Pawn, MechanitorMechsRecord>();
                }
                mechRefs.Clear();
                foreach (KeyValuePair<Pawn, MechanitorMechsRecord> kvp in mechanitorMechs)
                {
                    if (kvp.Value?.mechs != null)
                    {
                        foreach (Pawn mech in kvp.Value.mechs.Keys)
                        {
                            if (mech != null)
                            {
                                mechRefs[mech] = kvp.Key;
                            }
                        }
                    }
                }
            }
        }
    }
}
