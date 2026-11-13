using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    public class Recipe_InstallNeurofibre : Recipe_InstallImplant
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord? part = null)
        {
            if (!MechanitorExpandedMod.Settings.enableNeurofibre)
            {
                return false;
            }
            if (!(thing is Pawn pawn) || !MechanitorUtility.IsMechanitor(pawn))
            {
                return false;
            }
            return base.AvailableOnNow(thing, part);
        }

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate(BodyPartRecord record)
            {
                if (!pawn.health.hediffSet.GetNotMissingParts().Contains(record))
                {
                    return false;
                }
                if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record))
                {
                    return false;
                }

                Hediff? existing = pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.Part == record && x.def == recipe.addsHediff);
                if (existing is Hediff_Level lvl && lvl.level >= (int)recipe.addsHediff.maxSeverity)
                {
                    return false;
                }
                return true;
            });
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            }

            Hediff_Level? existing = pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.Part == part && x.def == recipe.addsHediff) as Hediff_Level;
            if (existing != null)
            {
                existing.ChangeLevel(1);
                existing.Severity = existing.level;
            }
            else
            {
                pawn.health.AddHediff(recipe.addsHediff, part);
            }

            pawn.mechanitor?.Notify_BandwidthChanged();
        }
    }
}
