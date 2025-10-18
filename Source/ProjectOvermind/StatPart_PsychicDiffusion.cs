using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// StatPart for Psychic Diffusion - applies dynamic stat modifications based on psychic sensitivity
    /// Handles movement, work speed, and threshold-based bonuses
    /// </summary>
    public class StatPart_PsychicDiffusion : StatPart
    {
        // Cache for hediff lookups (pawn → hediff)
        private static Dictionary<Pawn, Hediff_PsychicDiffusion> hediffCache = new Dictionary<Pawn, Hediff_PsychicDiffusion>();

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn)) return;

            Hediff_PsychicDiffusion hediff = GetPsychicDiffusionHediff(pawn);
            if (hediff == null) return;

            // SAFE: Read cached sensitivity from hediff instead of calling GetStatValue
            float sensitivity = hediff.CachedPsychicSensitivity;
            float bonus = 0f;

            // MoveSpeed: Base +10%, scales +1% per 0.1 sensitivity
            if (parentStat == StatDefOf.MoveSpeed)
            {
                float scaling = sensitivity * (Hediff_PsychicDiffusion.ScalingPerPoint / 0.1f);
                bonus = Hediff_PsychicDiffusion.BaseMoveSpeed + scaling;
            }
            // WorkSpeedGlobal: Base +10%, scales +1% per 0.1 sensitivity, threshold ≥5.0 adds +20%
            else if (parentStat == StatDefOf.WorkSpeedGlobal)
            {
                float scaling = sensitivity * (Hediff_PsychicDiffusion.ScalingPerPoint / 0.1f);
                bonus = Hediff_PsychicDiffusion.BaseWorkSpeed + scaling;

                // Threshold ≥5.0
                if (sensitivity >= Hediff_PsychicDiffusion.Threshold5)
                {
                    float thresholdBonus = Mathf.Floor((sensitivity - Hediff_PsychicDiffusion.Threshold5) / Hediff_PsychicDiffusion.ThresholdScalingStep) * Hediff_PsychicDiffusion.ThresholdScalingBonus;
                    bonus += Hediff_PsychicDiffusion.BaseWorkSpeedThreshold5 + thresholdBonus;
                }
            }
            // Medical Tend Quality: Threshold ≥3.0
            else if (parentStat == StatDefOf.MedicalTendQuality && sensitivity >= Hediff_PsychicDiffusion.Threshold3)
            {
                float thresholdBonus = Mathf.Floor((sensitivity - Hediff_PsychicDiffusion.Threshold3) / Hediff_PsychicDiffusion.ThresholdScalingStep) * Hediff_PsychicDiffusion.ThresholdScalingBonus;
                bonus = Hediff_PsychicDiffusion.BaseHealPowerBonus + thresholdBonus;
            }
            // IncomingDamageFactor: Threshold ≥5.0
            else if (parentStat == StatDefOf.IncomingDamageFactor && sensitivity >= Hediff_PsychicDiffusion.Threshold5)
            {
                float thresholdBonus = Mathf.Floor((sensitivity - Hediff_PsychicDiffusion.Threshold5) / Hediff_PsychicDiffusion.ThresholdScalingStep) * Hediff_PsychicDiffusion.ThresholdScalingBonus;
                float reduction = Hediff_PsychicDiffusion.BaseIncomingDamageReduction + thresholdBonus;
                val *= (1f - reduction); // Reduce incoming damage
                return; // Don't add bonus, we modified val directly
            }

            if (bonus > 0f)
            {
                val *= (1f + bonus);
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn)) return null;

            Hediff_PsychicDiffusion hediff = GetPsychicDiffusionHediff(pawn);
            if (hediff == null) return null;

            // SAFE: Read cached sensitivity from hediff instead of calling GetStatValue
            float sensitivity = hediff.CachedPsychicSensitivity;
            float bonus = 0f;
            string label = "Psychic Diffusion";

            if (parentStat == StatDefOf.MoveSpeed)
            {
                float scaling = sensitivity * (Hediff_PsychicDiffusion.ScalingPerPoint / 0.1f);
                bonus = Hediff_PsychicDiffusion.BaseMoveSpeed + scaling;
            }
            else if (parentStat == StatDefOf.WorkSpeedGlobal)
            {
                float scaling = sensitivity * (Hediff_PsychicDiffusion.ScalingPerPoint / 0.1f);
                bonus = Hediff_PsychicDiffusion.BaseWorkSpeed + scaling;

                if (sensitivity >= Hediff_PsychicDiffusion.Threshold5)
                {
                    float thresholdBonus = Mathf.Floor((sensitivity - Hediff_PsychicDiffusion.Threshold5) / Hediff_PsychicDiffusion.ThresholdScalingStep) * Hediff_PsychicDiffusion.ThresholdScalingBonus;
                    bonus += Hediff_PsychicDiffusion.BaseWorkSpeedThreshold5 + thresholdBonus;
                }
            }
            else if (parentStat == StatDefOf.MedicalTendQuality && sensitivity >= Hediff_PsychicDiffusion.Threshold3)
            {
                float thresholdBonus = Mathf.Floor((sensitivity - Hediff_PsychicDiffusion.Threshold3) / Hediff_PsychicDiffusion.ThresholdScalingStep) * Hediff_PsychicDiffusion.ThresholdScalingBonus;
                bonus = Hediff_PsychicDiffusion.BaseHealPowerBonus + thresholdBonus;
            }
            else if (parentStat == StatDefOf.IncomingDamageFactor && sensitivity >= Hediff_PsychicDiffusion.Threshold5)
            {
                float thresholdBonus = Mathf.Floor((sensitivity - Hediff_PsychicDiffusion.Threshold5) / Hediff_PsychicDiffusion.ThresholdScalingStep) * Hediff_PsychicDiffusion.ThresholdScalingBonus;
                float reduction = Hediff_PsychicDiffusion.BaseIncomingDamageReduction + thresholdBonus;
                return $"{label}: x{(1f - reduction):F2}";
            }

            if (bonus > 0f)
            {
                return $"{label}: +{(bonus * 100f):F1}%";
            }

            return null;
        }

        private Hediff_PsychicDiffusion GetPsychicDiffusionHediff(Pawn pawn)
        {
            if (pawn == null || pawn.health == null) return null;

            // Check cache first
            if (hediffCache.TryGetValue(pawn, out Hediff_PsychicDiffusion cachedHediff))
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
                if (hediff is Hediff_PsychicDiffusion diffusionHediff)
                {
                    hediffCache[pawn] = diffusionHediff;
                    return diffusionHediff;
                }
            }

            return null;
        }
    }
}
