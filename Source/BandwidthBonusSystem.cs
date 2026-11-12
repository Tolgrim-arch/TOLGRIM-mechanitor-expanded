using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    /// <summary>
    /// Represents a single source of permanent or dynamic MechBandwidth bonus.
    /// </summary>
    public class BandwidthBonusEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public float Value { get; set; }
        public string Source { get; set; } = string.Empty;
        public bool IsCumulative { get; set; }
    }

    /// <summary>
    /// Interface for extensible bandwidth bonus providers.
    /// </summary>
    public interface IBandwidthBonusProvider
    {
        IEnumerable<BandwidthBonusEntry> GetBonuses(Pawn pawn);
    }

    /// <summary>
    /// Per-pawn component persisting mechanitor progression data (e.g. Bandnode Sera doses).
    /// </summary>
    public class CompMechanitorBandwidth : ThingComp
    {
        public int bandnodeSerumDoses = 0;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref bandnodeSerumDoses, "tolgrim_bandnodeSerumDoses", 0);
        }
    }

    public class CompProperties_MechanitorBandwidth : CompProperties
    {
        public CompProperties_MechanitorBandwidth()
        {
            compClass = typeof(CompMechanitorBandwidth);
        }
    }

    /// <summary>
    /// Central registry and calculator for all Mechanitor Expanded bandwidth bonuses.
    /// </summary>
    public static class BandwidthBonusRegistry
    {
        private static readonly List<IBandwidthBonusProvider> Providers = new List<IBandwidthBonusProvider>();

        static BandwidthBonusRegistry()
        {
            RegisterProvider(new NeurofibreBonusProvider());
            RegisterProvider(new BandnodeSerumBonusProvider());
        }

        public static void RegisterProvider(IBandwidthBonusProvider provider)
        {
            if (provider != null && !Providers.Contains(provider))
            {
                Providers.Add(provider);
            }
        }

        public static List<BandwidthBonusEntry> GetAllBonuses(Pawn pawn)
        {
            List<BandwidthBonusEntry> results = new List<BandwidthBonusEntry>();
            if (pawn == null || !MechanitorUtility.IsMechanitor(pawn))
            {
                return results;
            }

            HashSet<string> seenNonCumulativeIds = new HashSet<string>();
            for (int i = 0; i < Providers.Count; i++)
            {
                foreach (BandwidthBonusEntry entry in Providers[i].GetBonuses(pawn))
                {
                    if (entry == null || Math.Abs(entry.Value) < 0.001f) continue;

                    if (!entry.IsCumulative)
                    {
                        if (seenNonCumulativeIds.Contains(entry.Id)) continue;
                        seenNonCumulativeIds.Add(entry.Id);
                    }

                    results.Add(entry);
                }
            }

            return results;
        }

        public static float GetTotalBonus(Pawn pawn)
        {
            float total = 0f;
            List<BandwidthBonusEntry> entries = GetAllBonuses(pawn);
            for (int i = 0; i < entries.Count; i++)
            {
                total += entries[i].Value;
            }
            return total;
        }

        public static float GetBonusBySource(Pawn pawn, string source)
        {
            float total = 0f;
            List<BandwidthBonusEntry> entries = GetAllBonuses(pawn);
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Source == source)
                {
                    total += entries[i].Value;
                }
            }
            return total;
        }
    }

    public class NeurofibreBonusProvider : IBandwidthBonusProvider
    {
        public IEnumerable<BandwidthBonusEntry> GetBonuses(Pawn pawn)
        {
            MechanitorExpandedSettings settings = MechanitorExpandedMod.Settings;
            if (settings == null || !settings.enableNeurofibre) yield break;

            List<Hediff>? hediffs = pawn.health?.hediffSet?.hediffs;
            if (hediffs == null) yield break;

            // Highest tier wins if multiple somehow exist (non-cumulative)
            int bestTier = 0;
            Hediff? bestHediff = null;
            float baseVal = 0f;

            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff h = hediffs[i];
                if (h.def.defName == "Tolgrim_MechNeurofibre")
                {
                    int lvl = (h as Hediff_Level)?.level ?? (int)System.Math.Round(h.Severity);
                    if (lvl > 0 && lvl > bestTier)
                    {
                        bestTier = lvl;
                        bestHediff = h;
                        baseVal = lvl * 3f;
                    }
                    continue;
                }

                int tier = GetNeurofibreTier(h.def.defName, out float val);
                if (tier > bestTier)
                {
                    bestTier = tier;
                    bestHediff = h;
                    baseVal = val;
                }
            }

            if (bestHediff != null && bestTier > 0)
            {
                float finalVal = baseVal * settings.neurofibreMultiplier;
                yield return new BandwidthBonusEntry
                {
                    Id = "Tolgrim_Neurofibre",
                    Label = bestHediff.LabelCap,
                    Description = bestHediff.def.description ?? string.Empty,
                    Value = finalVal,
                    Source = "Neurofibre",
                    IsCumulative = false
                };
            }
        }

        public static int GetNeurofibreTier(string defName, out float baseBandwidth)
        {
            switch (defName)
            {
                case "Tolgrim_NeurofibreImplantI":
                    baseBandwidth = 3f;
                    return 1;
                case "Tolgrim_NeurofibreImplantII":
                    baseBandwidth = 6f;
                    return 2;
                case "Tolgrim_NeurofibreImplantIII":
                    baseBandwidth = 9f;
                    return 3;
                case "Tolgrim_NeurofibreImplantIV":
                    baseBandwidth = 12f;
                    return 4;
                default:
                    baseBandwidth = 0f;
                    return 0;
            }
        }
    }

    public class BandnodeSerumBonusProvider : IBandwidthBonusProvider
    {
        public IEnumerable<BandwidthBonusEntry> GetBonuses(Pawn pawn)
        {
            MechanitorExpandedSettings settings = MechanitorExpandedMod.Settings;
            if (settings == null || !settings.enableBandnodeSera) yield break;

            CompMechanitorBandwidth? comp = pawn.TryGetComp<CompMechanitorBandwidth>();
            if (comp == null || comp.bandnodeSerumDoses <= 0) yield break;

            int effectiveDoses = Math.Min(comp.bandnodeSerumDoses, settings.maxBandnodeSeraDoses);
            float val = effectiveDoses * 1f * settings.serumMultiplier;

            yield return new BandwidthBonusEntry
            {
                Id = "Tolgrim_BandnodeSera",
                Label = "Tolgrim_BandnodeSeraBreakdown".Translate(effectiveDoses),
                Description = "Tolgrim_BandnodeSeraDesc".Translate(),
                Value = val,
                Source = "BandnodeSera",
                IsCumulative = true
            };
        }
    }

    /// <summary>
    /// Native StatPart hooked into StatDefOf.MechBandwidth so all game systems
    /// (Gizmos, StatWorker, InfoCard, and ITab) automatically use the exact total.
    /// </summary>
    public class StatPart_MechanitorExpandedBandwidth : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn && MechanitorUtility.IsMechanitor(pawn))
            {
                val += BandwidthBonusRegistry.GetTotalBonus(pawn);
            }
        }

        public override string? ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is Pawn pawn) || !MechanitorUtility.IsMechanitor(pawn))
            {
                return null;
            }

            List<BandwidthBonusEntry> entries = BandwidthBonusRegistry.GetAllBonuses(pawn);
            if (entries.Count == 0)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < entries.Count; i++)
            {
                BandwidthBonusEntry e = entries[i];
                sb.AppendLine($"{e.Label}: +{e.Value:0.##}");
            }
            return sb.ToString().TrimEndNewlines();
        }
    }
}
