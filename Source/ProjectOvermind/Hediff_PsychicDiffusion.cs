using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Hediff for Psychic Diffusion - aura buff that scales with psychic sensitivity
    /// Provides movement, work speed, healing spread, and threshold-based bonuses
    /// </summary>
    public class Hediff_PsychicDiffusion : HediffWithComps
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
        public const float BaseMoveSpeed = 0.10f;
        public const float BaseWorkSpeed = 0.10f;
        public const float BaseMoodBonus = 5f;
        public const float BaseHealingSpread = 0.30f; // 30% of healing spreads

        // Scaling
        public const float ScalingPerPoint = 0.1f; // 0.1 sensitivity = +1% to base effects

        // Thresholds
        public const float Threshold3 = 3.0f;
        public const float Threshold5 = 5.0f;
        public const float Threshold8 = 8.0f;

        // Threshold base bonuses
        public const float BaseHealPowerBonus = 0.10f;           // ≥3.0
        public const float BaseSpreadRadiusBonus = 2f;           // ≥3.0 (+2 tiles)
        public const float BaseWorkSpeedThreshold5 = 0.20f;     // ≥5.0 (+20% extra work speed)
        public const float BaseIncomingDamageReduction = 0.20f; // ≥5.0
        public const float BaseMiniHealAmount = 5f;              // ≥8.0 (5 HP every 5 seconds)
        public const float BaseBuffTransferRate = 1.0f;          // ≥8.0 (100% transfer)

        // Threshold scaling
        public const float ThresholdScalingStep = 0.2f; // Each 0.2 over threshold
        public const float ThresholdScalingBonus = 0.01f; // Adds +1%

        // Tick tracking
        private int lastHealPulseTick = 0;
        private const int HealPulseInterval = 300; // 5 seconds

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            
            if (pawn != null)
            {
                cachedSensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                lastCacheTick = Find.TickManager.TicksGame;
                Log.Message($"[Overmind] Psychic Diffusion applied to {pawn.LabelShort} (sensitivity: {cachedSensitivity:F2})");
            }

            lastHealPulseTick = Find.TickManager.TicksGame;
        }

        public override void Tick()
        {
            base.Tick();

            if (pawn == null || pawn.Dead || !pawn.Spawned) return;

            int currentTick = Find.TickManager.TicksGame;

            // SAFE: Refresh cache every 5 seconds (not during stat calculation)
            if (currentTick - lastCacheTick >= CacheRefreshInterval)
            {
                cachedSensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                lastCacheTick = currentTick;
            }

            // Handle mini-heal pulse at threshold 8.0 (every 5 seconds)
            float sensitivity = GetCachedSensitivity();
            if (sensitivity >= Threshold8)
            {
                if (currentTick - lastHealPulseTick >= HealPulseInterval)
                {
                    lastHealPulseTick = currentTick;
                    ApplyMiniHealPulse();
                }
            }
        }

        private void ApplyMiniHealPulse()
        {
            if (pawn == null || pawn.Map == null) return;

            try
            {
                float sensitivity = GetCachedSensitivity();
                
                // Calculate scaling bonus
                float thresholdBonus = Mathf.Floor((sensitivity - Threshold8) / ThresholdScalingStep) * ThresholdScalingBonus;
                float healAmount = BaseMiniHealAmount * (1f + thresholdBonus);

                // Find allies within 10 tiles
                List<Pawn> nearbyAllies = new List<Pawn>();
                foreach (Pawn ally in pawn.Map.mapPawns.AllPawnsSpawned)
                {
                    if (ally == null || ally.Dead) continue;
                    if (!ally.IsColonist && ally.Faction != Faction.OfPlayer) continue;
                    if (!ally.RaceProps.Humanlike) continue;
                    
                    float distance = ally.Position.DistanceTo(pawn.Position);
                    if (distance <= 10f)
                    {
                        nearbyAllies.Add(ally);
                    }
                }

                // Apply healing to all nearby allies (including self)
                foreach (Pawn ally in nearbyAllies)
                {
                    if (ally.health != null && ally.health.hediffSet != null)
                    {
                        // Find first injury to heal
                        List<Hediff_Injury> injuries = new List<Hediff_Injury>();
                        ally.health.hediffSet.GetHediffs(ref injuries, null);
                        Hediff_Injury injury = injuries.FirstOrDefault();
                        
                        if (injury != null)
                        {
                            injury.Heal(healAmount);
                            
                            // Small visual effect
                            FleckMaker.ThrowMetaIcon(ally.Position, ally.Map, FleckDefOf.HealingCross);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] ApplyMiniHealPulse error: {ex.Message}");
            }
        }

        /// <summary>
        /// Override tooltip generation to prevent NullReferenceException
        /// Vanilla HediffStatsUtility can't handle our dynamic stat modifications properly
        /// </summary>
        public override string TipStringExtra
        {
            get
            {
                if (pawn == null) return base.TipStringExtra;

                try
                {
                    float sensitivity = GetCachedSensitivity();
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    
                    // Base effects
                    sb.AppendLine($"Mood: +{BaseMoodBonus:F0}");
                    
                    float moveSpeedScaling = sensitivity * (ScalingPerPoint / 0.1f);
                    float moveSpeedBonus = BaseMoveSpeed + moveSpeedScaling;
                    sb.AppendLine($"Movement Speed: +{(moveSpeedBonus * 100f):F1}%");
                    
                    float workSpeedScaling = sensitivity * (ScalingPerPoint / 0.1f);
                    float workSpeedBonus = BaseWorkSpeed + workSpeedScaling;
                    if (sensitivity >= Threshold5)
                    {
                        float thresholdBonus = Mathf.Floor((sensitivity - Threshold5) / ThresholdScalingStep) * ThresholdScalingBonus;
                        workSpeedBonus += BaseWorkSpeedThreshold5 + thresholdBonus;
                    }
                    sb.AppendLine($"Work Speed: +{(workSpeedBonus * 100f):F1}%");
                    
                    // Threshold bonuses
                    if (sensitivity >= Threshold3)
                    {
                        float thresholdBonus = Mathf.Floor((sensitivity - Threshold3) / ThresholdScalingStep) * ThresholdScalingBonus;
                        float healPower = BaseHealPowerBonus + thresholdBonus;
                        sb.AppendLine($"Medical Tend Quality: +{(healPower * 100f):F1}%");
                    }
                    
                    if (sensitivity >= Threshold5)
                    {
                        float thresholdBonus = Mathf.Floor((sensitivity - Threshold5) / ThresholdScalingStep) * ThresholdScalingBonus;
                        float damageReduction = BaseIncomingDamageReduction + thresholdBonus;
                        sb.AppendLine($"Damage Reduction: -{(damageReduction * 100f):F1}%");
                    }
                    
                    if (sensitivity >= Threshold8)
                    {
                        float thresholdBonus = Mathf.Floor((sensitivity - Threshold8) / ThresholdScalingStep) * ThresholdScalingBonus;
                        float healAmount = BaseMiniHealAmount * (1f + thresholdBonus);
                        sb.AppendLine($"Regeneration Pulse: {healAmount:F1} HP every 5s");
                    }
                    
                    sb.AppendLine($"Healing Spread: {(BaseHealingSpread * 100f):F0}%");
                    sb.AppendLine($"\nPsychic Sensitivity: {sensitivity:F2}");
                    
                    return sb.ToString().TrimEnd();
                }
                catch (Exception ex)
                {
                    Log.Error($"[ProjectOvermind] Error generating Psychic Diffusion tooltip: {ex.Message}");
                    return "Psychic Diffusion buff";
                }
            }
        }

        /// <summary>
        /// Don't provide SpecialDisplayStats - we handle tooltip via TipStringExtra
        /// This prevents vanilla HediffStatsUtility from causing NullReferenceException
        /// </summary>
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            // Return empty - all display handled in TipStringExtra
            yield break;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastHealPulseTick, "lastHealPulseTick", 0);
        }
    }
}
