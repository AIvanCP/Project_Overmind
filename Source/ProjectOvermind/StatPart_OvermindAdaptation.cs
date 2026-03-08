using RimWorld;
using Verse;

namespace ProjectOvermind
{
    /// <summary>
    /// StatPart for Overmind Adaptation – adds move speed bonus to the MoveSpeed stat.
    /// Terrain cost reduction is handled by the Harmony patch on CostToMoveIntoCell.
    /// This StatPart makes the bonus visible in the Stats tab.
    /// </summary>
    public class StatPart_OvermindAdaptation : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn))
                return;

            Hediff_OvermindAdaptation hediff = GetHediff(pawn);
            if (hediff == null)
                return;

            if (parentStat == StatDefOf.MoveSpeed)
            {
                val += hediff.MoveBonusCells;
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn))
                return null;

            Hediff_OvermindAdaptation hediff = GetHediff(pawn);
            if (hediff == null)
                return null;

            if (parentStat == StatDefOf.MoveSpeed)
                return $"Overmind Adaptation: +{hediff.MoveBonusCells:F2} c/s";

            return null;
        }

        private static Hediff_OvermindAdaptation GetHediff(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null) return null;
            HediffDef def = DefDatabase<HediffDef>.GetNamedSilentFail("ProjectOvermind_OvermindAdaptation");
            if (def == null) return null;
            return pawn.health.hediffSet.GetFirstHediffOfDef(def) as Hediff_OvermindAdaptation;
        }
    }
}
