using RimWorld;
using Verse;

namespace Tolgrim_MechanitorExpanded
{
    public class CompUseEffect_BandnodeSerum : CompUseEffect
    {
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            MechanitorExpandedSettings settings = MechanitorExpandedMod.Settings;
            if (!settings.enableBandnodeSera)
            {
                return "Tolgrim_BandnodeSeraDisabled".Translate();
            }

            if (p == null || !MechanitorUtility.IsMechanitor(p))
            {
                return "Tolgrim_RequiresMechanitor".Translate();
            }

            CompMechanitorBandwidth? comp = p.TryGetComp<CompMechanitorBandwidth>();
            if (comp == null)
            {
                return false;
            }

            if (comp.bandnodeSerumDoses >= settings.maxBandnodeSeraDoses)
            {
                return "Tolgrim_BandnodeSeraMaxReached".Translate(settings.maxBandnodeSeraDoses);
            }

            return true;
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            CompMechanitorBandwidth? comp = usedBy.TryGetComp<CompMechanitorBandwidth>();
            if (comp == null) return;

            comp.bandnodeSerumDoses++;
            usedBy.mechanitor?.Notify_BandwidthChanged();

            Messages.Message(
                "Tolgrim_BandnodeSerumUsedMsg".Translate(usedBy.LabelShortCap, comp.bandnodeSerumDoses, MechanitorExpandedMod.Settings.maxBandnodeSeraDoses),
                usedBy,
                MessageTypeDefOf.PositiveEvent
            );
        }
    }
}
