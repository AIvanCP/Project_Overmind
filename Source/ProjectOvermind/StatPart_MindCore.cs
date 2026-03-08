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

            if (parentStat == StatDefOf.PsychicEntropyMax)
            {
                float increase = hediff.GetMaxEntropyIncrease();
                val += increase;
            }
            else if (parentStat == StatDefOf.Ability_PsyfocusCost)
            {
                // Multiply the cost by the Mind Core multiplier so it shows in the stat tab
                // e.g. multiplier 0.5 → val becomes 50% of base (means 50% cost reduction)
                val *= hediff.GetPsyfocusCostMultiplier();
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
            else if (parentStat == StatDefOf.Ability_PsyfocusCost)
            {
                float reduction = (1f - hediff.GetPsyfocusCostMultiplier()) * 100f;
                return $"Mind Core: -{reduction:F0}% psyfocus cost";
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
