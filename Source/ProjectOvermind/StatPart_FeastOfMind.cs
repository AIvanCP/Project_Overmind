using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ProjectOvermind
{
    /// <summary>
    /// StatPart that applies Feast of Mind eating speed and hunger rate modifications
    /// Optimized: only checks hediff presence, calculates once per hediff application
    /// </summary>
    public class StatPart_FeastOfMind : StatPart
    {
        // Cached calculation to avoid recalculating every stat check
        private Dictionary<Pawn, float> cachedEatingSpeed = new Dictionary<Pawn, float>();
        private Dictionary<Pawn, float> cachedHungerRate = new Dictionary<Pawn, float>();
        
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn))
                return;

            // Quick check: does pawn have the hediff?
            Hediff_FeastOfMind feastHediff = GetFeastOfMindHediff(pawn);
            if (feastHediff == null)
            {
                // Clean cache if hediff removed
                cachedEatingSpeed.Remove(pawn);
                cachedHungerRate.Remove(pawn);
                return;
            }

            // Apply stat modification based on stat type
            if (parentStat == StatDefOf.EatingSpeed)
            {
                // Get or calculate eating speed bonus
                if (!cachedEatingSpeed.TryGetValue(pawn, out float bonus))
                {
                    bonus = CalculateEatingSpeedBonus(pawn);
                    cachedEatingSpeed[pawn] = bonus;
                }
                val += bonus;
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn))
                return null;

            Hediff_FeastOfMind feastHediff = GetFeastOfMindHediff(pawn);
            if (feastHediff == null)
                return null;

            if (parentStat == StatDefOf.EatingSpeed)
            {
                float bonus = cachedEatingSpeed.TryGetValue(pawn, out float val) ? val : CalculateEatingSpeedBonus(pawn);
                if (bonus > 0)
                    return $"Feast of Mind: +{bonus * 100:F0}%";
            }

            return null;
        }

        private Hediff_FeastOfMind GetFeastOfMindHediff(Pawn pawn)
        {
            if (pawn?.health?.hediffSet?.hediffs == null)
                return null;

            // Fast lookup - only check hediffs list once
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff is Hediff_FeastOfMind feast && !feast.ShouldRemove)
                    return feast;
            }
            return null;
        }

        private float CalculateEatingSpeedBonus(Pawn pawn)
        {
            if (pawn == null)
                return 0f;

            // SAFE: Read cached sensitivity from hediff instead of calling GetStatValue
            // This prevents recursion during stat calculation
            var feastHediff = GetFeastOfMindHediff(pawn);
            if (feastHediff == null)
                return 0f;

            // Access the cached sensitivity from the hediff
            float sensitivity = feastHediff.CachedPsychicSensitivity;
            
            // Base: +25% at sensitivity 0, +1% per 0.1 sensitivity
            return Hediff_FeastOfMind.BaseEatingSpeed + (Hediff_FeastOfMind.ScalingPerPoint * sensitivity);
        }

        // Clear cache when hediff is removed (called by hediff PostRemoved)
        public static void ClearCache(Pawn pawn)
        {
            // This would need to be called from Hediff_FeastOfMind.PostRemoved
            // For now, cache auto-clears when hediff not found
        }
    }

    /// <summary>
    /// StatPart for hunger rate factor modification
    /// Applies as multiplier to base hunger rate
    /// </summary>
    public class StatPart_FeastOfMindHunger : StatPart
    {
        private Dictionary<Pawn, float> cachedHungerFactor = new Dictionary<Pawn, float>();

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn))
                return;

            Hediff_FeastOfMind feastHediff = GetFeastOfMindHediff(pawn);
            if (feastHediff == null)
            {
                cachedHungerFactor.Remove(pawn);
                return;
            }

            // Apply hunger rate reduction as multiplier
            if (!cachedHungerFactor.TryGetValue(pawn, out float factor))
            {
                factor = CalculateHungerFactor(pawn);
                cachedHungerFactor[pawn] = factor;
            }

            val *= factor;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn))
                return null;

            Hediff_FeastOfMind feastHediff = GetFeastOfMindHediff(pawn);
            if (feastHediff == null)
                return null;

            float factor = cachedHungerFactor.TryGetValue(pawn, out float val) ? val : CalculateHungerFactor(pawn);
            float reduction = (1f - factor) * 100f;
            
            if (reduction > 0)
                return $"Feast of Mind: -{reduction:F0}% hunger rate";

            return null;
        }

        private Hediff_FeastOfMind GetFeastOfMindHediff(Pawn pawn)
        {
            if (pawn?.health?.hediffSet?.hediffs == null)
                return null;

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff is Hediff_FeastOfMind feast && !feast.ShouldRemove)
                    return feast;
            }
            return null;
        }

        private float CalculateHungerFactor(Pawn pawn)
        {
            if (pawn == null)
                return 1f;

            // SAFE: Read cached sensitivity from hediff instead of calling GetStatValue
            var feastHediff = GetFeastOfMindHediff(pawn);
            if (feastHediff == null)
                return 1f;

            float sensitivity = feastHediff.CachedPsychicSensitivity;
            
            // Calculate hunger reduction: base 10% + (sensitivity * 0.1), capped at 99%
            float hungerReduction = Mathf.Clamp(
                Hediff_FeastOfMind.BaseHungerReduction + (Hediff_FeastOfMind.ScalingPerPoint * sensitivity),
                Hediff_FeastOfMind.BaseHungerReduction,
                Hediff_FeastOfMind.MaxHungerReduction
            );
            
            // Return factor: 1.0 - reduction (e.g., 20% reduction = 0.8 factor)
            return 1f - hungerReduction;
        }
    }
}
