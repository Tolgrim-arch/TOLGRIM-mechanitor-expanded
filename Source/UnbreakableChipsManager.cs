using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    [StaticConstructorOnStartup]
    public static class UnbreakableChipsManager
    {
        private static bool isApplied = false;

        private class ChipBackup
        {
            public bool originalUseHitPoints;
            public float? originalFlammability;
            public float? originalDeteriorationRate;
        }

        private class ChipConfig
        {
            public string defName = "";
            public bool zeroFlammability;
            public bool zeroDeterioration;
        }

        // Vanilla Biotech + mod support (DMS, Alpha Mechs) exact match to workshop 2883982082
        private static readonly List<ChipConfig> TargetChips = new List<ChipConfig>
        {
            // Vanilla Biotech
            new ChipConfig { defName = "SignalChip", zeroFlammability = false, zeroDeterioration = true },
            new ChipConfig { defName = "PowerfocusChip", zeroFlammability = true, zeroDeterioration = true },
            new ChipConfig { defName = "NanostructuringChip", zeroFlammability = true, zeroDeterioration = true },

            // Dead Man's Switch (Aoba.DeadManSwitch.Core)
            new ChipConfig { defName = "Neurocomputer", zeroFlammability = false, zeroDeterioration = true },
            new ChipConfig { defName = "ComponentTransmitter", zeroFlammability = true, zeroDeterioration = true },
            new ChipConfig { defName = "ComponentControlChip", zeroFlammability = false, zeroDeterioration = true },
            new ChipConfig { defName = "ComponentFireControl", zeroFlammability = false, zeroDeterioration = true },
            new ChipConfig { defName = "DMS_EncryptedTransmitter", zeroFlammability = true, zeroDeterioration = true },
            new ChipConfig { defName = "PulsarEmitterUnit", zeroFlammability = false, zeroDeterioration = true },

            // Alpha Mechs (sarg.alphamechs)
            new ChipConfig { defName = "AM_HyperLinkageChip", zeroFlammability = true, zeroDeterioration = true },
            new ChipConfig { defName = "AM_StellarProcessingChip", zeroFlammability = true, zeroDeterioration = true },
            new ChipConfig { defName = "AM_QuantumMatrixChip", zeroFlammability = true, zeroDeterioration = true }
        };

        private static readonly Dictionary<ThingDef, ChipBackup> OriginalValues = new Dictionary<ThingDef, ChipBackup>();

        static UnbreakableChipsManager()
        {
            ApplySetting(MechanitorExpandedMod.Settings.unbreakableMechChips);
        }

        public static void ApplySetting(bool makeUnbreakable)
        {
            if (isApplied == makeUnbreakable) return;

            for (int i = 0; i < TargetChips.Count; i++)
            {
                ChipConfig config = TargetChips[i];
                ThingDef? def = DefDatabase<ThingDef>.GetNamedSilentFail(config.defName);
                if (def == null) continue;

                if (!OriginalValues.TryGetValue(def, out ChipBackup? backup))
                {
                    backup = new ChipBackup
                    {
                        originalUseHitPoints = def.useHitPoints,
                        originalFlammability = GetStatBaseValue(def, StatDefOf.Flammability),
                        originalDeteriorationRate = GetStatBaseValue(def, StatDefOf.DeteriorationRate)
                    };
                    OriginalValues[def] = backup;
                }

                if (makeUnbreakable)
                {
                    def.useHitPoints = false;
                    if (config.zeroFlammability) SetStatBaseValue(def, StatDefOf.Flammability, 0f);
                    if (config.zeroDeterioration) SetStatBaseValue(def, StatDefOf.DeteriorationRate, 0f);
                }
                else
                {
                    def.useHitPoints = backup.originalUseHitPoints;
                    RestoreStatBaseValue(def, StatDefOf.Flammability, backup.originalFlammability);
                    RestoreStatBaseValue(def, StatDefOf.DeteriorationRate, backup.originalDeteriorationRate);
                }
            }

            isApplied = makeUnbreakable;
        }

        private static float? GetStatBaseValue(ThingDef def, StatDef stat)
        {
            if (def.statBases == null) return null;
            for (int i = 0; i < def.statBases.Count; i++)
            {
                if (def.statBases[i].stat == stat) return def.statBases[i].value;
            }
            return null;
        }

        private static void SetStatBaseValue(ThingDef def, StatDef stat, float value)
        {
            if (def.statBases == null) def.statBases = new List<StatModifier>();

            for (int i = 0; i < def.statBases.Count; i++)
            {
                if (def.statBases[i].stat == stat)
                {
                    def.statBases[i].value = value;
                    return;
                }
            }

            def.statBases.Add(new StatModifier { stat = stat, value = value });
        }

        private static void RestoreStatBaseValue(ThingDef def, StatDef stat, float? originalValue)
        {
            if (def.statBases == null) return;

            for (int i = def.statBases.Count - 1; i >= 0; i--)
            {
                if (def.statBases[i].stat == stat)
                {
                    if (originalValue.HasValue)
                    {
                        def.statBases[i].value = originalValue.Value;
                    }
                    else
                    {
                        def.statBases.RemoveAt(i);
                    }
                    return;
                }
            }

            if (originalValue.HasValue)
            {
                def.statBases.Add(new StatModifier { stat = stat, value = originalValue.Value });
            }
        }
    }
}
