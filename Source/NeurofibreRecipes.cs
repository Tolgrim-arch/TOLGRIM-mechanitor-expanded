using System.Collections.Generic;
using RimWorld;
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

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            // Remove any existing Neurofibre tier so levels are mutually exclusive
            if (pawn.health?.hediffSet != null)
            {
                List<Hediff> toRemove = new List<Hediff>();
                List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
                for (int i = 0; i < hediffs.Count; i++)
                {
                    if (NeurofibreBonusProvider.GetNeurofibreTier(hediffs[i].def.defName, out _) > 0)
                    {
                        toRemove.Add(hediffs[i]);
                    }
                }
                for (int i = 0; i < toRemove.Count; i++)
                {
                    pawn.health.RemoveHediff(toRemove[i]);
                }
            }

            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);
            pawn.mechanitor?.Notify_BandwidthChanged();
        }
    }
}
