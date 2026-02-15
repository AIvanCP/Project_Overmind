using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ProjectOvermind
{
    /// <summary>
    /// StatPart for Mind Core - increases max psychic entropy
    /// Provides dynamic stat modification based on caster's sensitivity
    /// </summary>
    public class StatPart_MindCore : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn)) return;

            Hediff_MindCore hediff = GetMindCoreHediff(pawn);
            if (hediff == null) return;

            // Only affects PsychicEntropyMax
            if (parentStat == StatDefOf.PsychicEntropyMax)
            {
                float increase = hediff.GetMaxEntropyIncrease();
                val += increase;
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn)) return null;

            Hediff_MindCore hediff = GetMindCoreHediff(pawn);
            if (hediff == null) return null;

            if (parentStat == StatDefOf.PsychicEntropyMax)
            {
                float increase = hediff.GetMaxEntropyIncrease();
                return $"Mind Core: +{increase:F0}";
            }

            return null;
        }

        private Hediff_MindCore GetMindCoreHediff(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
                return null;

            HediffDef hediffDef = HediffDef.Named("ProjectOvermind_MindCore");
            if (hediffDef == null)
                return null;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            return hediff as Hediff_MindCore;
        }
    }
}
