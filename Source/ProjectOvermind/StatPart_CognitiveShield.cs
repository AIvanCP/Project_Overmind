using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// StatPart for Cognitive Shield - applies dynamic stat modifications based on psychic sensitivity
    /// Handles base psychic sensitivity bonus and threshold-based bonuses
    /// </summary>
    public class StatPart_CognitiveShield : StatPart
    {
        // Cache for hediff lookups (pawn → hediff)
        private static Dictionary<Pawn, Hediff_CognitiveShield> hediffCache = new Dictionary<Pawn, Hediff_CognitiveShield>();

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn)) return;

            Hediff_CognitiveShield hediff = GetCognitiveShieldHediff(pawn);
            if (hediff == null) return;

            // SAFE: Read cached sensitivity from hediff instead of calling GetStatValue
            float sensitivity = hediff.CachedPsychicSensitivity;
            float bonus = 0f;

            // NOTE: PsychicSensitivity base bonus now handled via hediff statOffsets to avoid infinite recursion
            // IncomingDamageFactor: Threshold ≥3.0 (reduce incoming damage)
            if (parentStat == StatDefOf.IncomingDamageFactor && sensitivity >= Hediff_CognitiveShield.Threshold3)
            {
                float thresholdBonus = Mathf.Floor((sensitivity - Hediff_CognitiveShield.Threshold3) / Hediff_CognitiveShield.ThresholdScalingStep) * Hediff_CognitiveShield.ThresholdScalingBonus;
                float reduction = Hediff_CognitiveShield.BaseIncomingDamageReduction + thresholdBonus;
                val *= (1f - reduction); // Reduce incoming damage
                return; // Don't add bonus, we modified val directly
            }
            // InjuryHealingFactor: Threshold ≥8.0
            else if (parentStat == StatDefOf.InjuryHealingFactor && sensitivity >= Hediff_CognitiveShield.Threshold8)
            {
                float thresholdBonus = Mathf.Floor((sensitivity - Hediff_CognitiveShield.Threshold8) / Hediff_CognitiveShield.ThresholdScalingStep) * Hediff_CognitiveShield.ThresholdScalingBonus;
                bonus = Hediff_CognitiveShield.BaseHealRateBonus + thresholdBonus;
            }

            if (bonus > 0f)
            {
                val *= (1f + bonus);
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn)) return null;

            Hediff_CognitiveShield hediff = GetCognitiveShieldHediff(pawn);
            if (hediff == null) return null;

            // SAFE: Read cached sensitivity from hediff instead of calling GetStatValue
            float sensitivity = hediff.CachedPsychicSensitivity;
            float bonus = 0f;
            string label = "Cognitive Shield";

            // NOTE: PsychicSensitivity base bonus shown via hediff, not here
            if (parentStat == StatDefOf.IncomingDamageFactor && sensitivity >= Hediff_CognitiveShield.Threshold3)
            {
                float thresholdBonus = Mathf.Floor((sensitivity - Hediff_CognitiveShield.Threshold3) / Hediff_CognitiveShield.ThresholdScalingStep) * Hediff_CognitiveShield.ThresholdScalingBonus;
                float reduction = Hediff_CognitiveShield.BaseIncomingDamageReduction + thresholdBonus;
                return $"{label}: x{(1f - reduction):F2}";
            }
            else if (parentStat == StatDefOf.InjuryHealingFactor && sensitivity >= Hediff_CognitiveShield.Threshold8)
            {
                float thresholdBonus = Mathf.Floor((sensitivity - Hediff_CognitiveShield.Threshold8) / Hediff_CognitiveShield.ThresholdScalingStep) * Hediff_CognitiveShield.ThresholdScalingBonus;
                bonus = Hediff_CognitiveShield.BaseHealRateBonus + thresholdBonus;
            }

            if (bonus > 0f)
            {
                return $"{label}: +{(bonus * 100f):F1}%";
            }

            return null;
        }

        private Hediff_CognitiveShield GetCognitiveShieldHediff(Pawn pawn)
        {
            if (pawn == null || pawn.health == null) return null;

            // Check cache first
            if (hediffCache.TryGetValue(pawn, out Hediff_CognitiveShield cachedHediff))
            {
                if (cachedHediff != null && cachedHediff.pawn != null && !cachedHediff.pawn.Dead)
                {
                    return cachedHediff;
                }
                else
                {
                    // Cached hediff is discarded, remove from cache
                    hediffCache.Remove(pawn);
                }
            }

            // Search for hediff
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff is Hediff_CognitiveShield shieldHediff)
                {
                    hediffCache[pawn] = shieldHediff;
                    return shieldHediff;
                }
            }

            return null;
        }
    }
}
