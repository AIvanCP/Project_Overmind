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
        // Cache for psychic sensitivity to prevent stack overflow
        private float cachedSensitivity = 1f;
        private int lastCacheTick = -9999;
        private const int CacheRefreshInterval = 300; // Refresh every 5 seconds (safe)

        /// <summary>
        /// Public property for StatParts to access cached sensitivity safely
        /// </summary>
        public float CachedPsychicSensitivity => cachedSensitivity;

        /// <summary>
        /// SAFE: Returns only cached value, NEVER calls GetStatValue to prevent recursion
        /// </summary>
        private float GetCachedSensitivity()
        {
            if (pawn == null) return 1f;
            return cachedSensitivity;
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
            
            if (pawn != null)
            {
                cachedSensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                lastCacheTick = Find.TickManager.TicksGame;
                Log.Message($"[Overmind] Cognitive Shield applied to {pawn.LabelShort} (sensitivity: {cachedSensitivity:F2})");
            }
        }

        public override void Tick()
        {
            base.Tick();

            // SAFE: Refresh cache every 5 seconds (not during stat calculation)
            if (pawn != null)
            {
                int currentTick = Find.TickManager.TicksGame;
                if (currentTick - lastCacheTick >= CacheRefreshInterval)
                {
                    cachedSensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                    lastCacheTick = currentTick;
                }
            }

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

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
