using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Hediff for Cognitive Shield - defensive buff that scales with psychic sensitivity
    /// Provides base defenses plus threshold-based bonuses
    /// </summary>
    public class Hediff_CognitiveShield : HediffWithComps
    {
        // Store CASTER's sensitivity (not recipient's)
        private float casterSensitivity = 1f;

        /// <summary>
        /// Public property for StatParts to access caster's sensitivity safely
        /// </summary>
        public float CachedPsychicSensitivity => casterSensitivity;

        /// <summary>
        /// Initialize hediff with caster's sensitivity
        /// </summary>
        public void SetCasterSensitivity(float sensitivity)
        {
            casterSensitivity = sensitivity;
        }

        /// <summary>
        /// Returns caster's sensitivity (not recipient's)
        /// </summary>
        private float GetCachedSensitivity()
        {
            return casterSensitivity;
        }

        // Base effects (always active)
        public const float BasePsychicSensitivityBonus = 0.25f;
        public const float BaseMentalDamageReduction = 0.30f;
        public const float BaseStunReduction = 0.20f;
        public const float BaseMentalBreakThresholdBonus = 0.15f;

        // Scaling
        public const float ScalingPerPoint = 0.1f; // 0.1 sensitivity = +1% to base effects

        // Thresholds
        public const float Threshold3 = 3.0f;
        public const float Threshold5 = 5.0f;
        public const float Threshold8 = 8.0f;

        // Threshold base bonuses (scale with ThresholdScalingStep)
        public const float BaseIncomingDamageReduction = 0.10f; // ≥3.0
        public const float BaseConsciousnessBonus = 0.10f;      // ≥3.0
        public const float BaseMentalImmunity = 1.0f;            // ≥5.0 (100% immunity)
        public const float BaseReflectChance = 0.25f;            // ≥8.0
        public const float BaseHealRateBonus = 0.50f;            // ≥8.0

        // Threshold scaling
        public const float ThresholdScalingStep = 0.2f; // Each 0.2 over threshold
        public const float ThresholdScalingBonus = 0.01f; // Adds +1%

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
        }

        public override void Tick()
        {
            base.Tick();

            // Handle mental immunity at threshold 5.0
            if (pawn != null && GetCachedSensitivity() >= Threshold5)
            {
                TryRemoveMentalHediffs();
            }

            // Handle reflect damage at threshold 8.0 (handled via HediffComp in separate tick interval)
        }

        private void TryRemoveMentalHediffs()
        {
            if (pawn == null || pawn.health == null) return;

            try
            {
                // Remove mental break hediffs if at threshold 5.0+
                List<Hediff> hediffsToRemove = new List<Hediff>();

                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff == null) continue;
                    
                    // Check if it's a mental hediff
                    string defName = hediff.def.defName.ToLower();
                    if (defName.Contains("panic") || defName.Contains("confusion") || 
                        defName.Contains("hallucination") || defName.Contains("berserk") ||
                        defName.Contains("manhunter") || defName.Contains("mental"))
                    {
                        hediffsToRemove.Add(hediff);
                    }
                }

                foreach (Hediff hediff in hediffsToRemove)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] TryRemoveMentalHediffs error: {ex.Message}");
            }
        }

        /// <summary>
        /// Show duration in brackets
        /// </summary>
        public override string LabelInBrackets
        {
            get
            {
                HediffComp_Disappears comp = this.TryGetComp<HediffComp_Disappears>();
                if (comp != null && comp.ticksToDisappear > 0)
                {
                    return DurationHelper.GetDurationString(comp.ticksToDisappear);
                }
                return null;
            }
        }

        /// <summary>
        /// Show buff details in tooltip
        /// </summary>
        public override string TipStringExtra
        {
            get
            {
                if (pawn == null) return base.TipStringExtra;

                try
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    float sensitivity = GetCachedSensitivity();

                    // Show caster sensitivity
                    sb.AppendLine($"Caster Sensitivity: {sensitivity:F1}");
                    sb.AppendLine();

                    // Base effects
                    sb.AppendLine("Base Effects:");
                    float psyBonus = BasePsychicSensitivityBonus + (ScalingPerPoint * sensitivity);
                    sb.AppendLine($"• Psychic Sensitivity: +{psyBonus * 100:F0}%");
                    
                    float mentalDmg = BaseMentalDamageReduction + (ScalingPerPoint * sensitivity);
                    sb.AppendLine($"• Mental Damage Reduction: {mentalDmg * 100:F0}%");
                    
                    float stunRed = BaseStunReduction + (ScalingPerPoint * sensitivity);
                    sb.AppendLine($"• Stun Reduction: {stunRed * 100:F0}%");
                    
                    float breakThreshold = BaseMentalBreakThresholdBonus + (ScalingPerPoint * sensitivity);
                    sb.AppendLine($"• Mental Break Threshold: +{breakThreshold * 100:F0}%");

                    // Threshold perks
                    if (sensitivity >= Threshold3)
                    {
                        sb.AppendLine();
                        sb.AppendLine("Threshold Perks (≥3.0):");
                        float dmgReduction = BaseIncomingDamageReduction + Mathf.Floor((sensitivity - Threshold3) / ThresholdScalingStep) * ThresholdScalingBonus;
                        sb.AppendLine($"• Incoming Damage Reduction: {dmgReduction * 100:F0}%");
                        
                        float consciousness = BaseConsciousnessBonus + Mathf.Floor((sensitivity - Threshold3) / ThresholdScalingStep) * ThresholdScalingBonus;
                        sb.AppendLine($"• Consciousness: +{consciousness * 100:F0}%");
                    }

                    if (sensitivity >= Threshold5)
                    {
                        sb.AppendLine();
                        sb.AppendLine("Threshold Perks (≥5.0):");
                        sb.AppendLine($"• Mental Break Immunity: 100%");
                    }

                    if (sensitivity >= Threshold8)
                    {
                        sb.AppendLine();
                        sb.AppendLine("Threshold Perks (≥8.0):");
                        float reflectChance = BaseReflectChance + Mathf.Floor((sensitivity - Threshold8) / ThresholdScalingStep) * ThresholdScalingBonus;
                        sb.AppendLine($"• Reflect Damage Chance: {reflectChance * 100:F0}%");
                        
                        float healRate = BaseHealRateBonus + Mathf.Floor((sensitivity - Threshold8) / ThresholdScalingStep) * ThresholdScalingBonus;
                        sb.AppendLine($"• Healing Rate: +{healRate * 100:F0}%");
                    }

                    return sb.ToString().TrimEnd();
                }
                catch (Exception ex)
                {
                    Log.Error($"[CognitiveShield] Error in TipStringExtra: {ex}");
                    return base.TipStringExtra;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref casterSensitivity, "casterSensitivity", 1f);
        }
    }
}
