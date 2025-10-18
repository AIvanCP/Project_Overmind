using System;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Hediff for Inspiration psycast - comprehensive scaling with psychic sensitivity
    /// Base buffs at s=0, scaling per 0.1 sensitivity, threshold perks at 3.0/5.0/8.0
    /// </summary>
    public class Hediff_InspirationAura : HediffWithComps
    {
        private new const int TickInterval = 60; // Update every 60 ticks (~1 second)
        private int tickCounter = 0;

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

        // Base values at sensitivity 0 - PUBLIC for StatPart access
        public const float BaseWorkSpeed = 0.50f; // +50% work speed at s=0
        public const float BaseLearning = 0.60f; // +60% learning at s=0
        public const float BaseMoveSpeed = 0.10f; // +10% move speed at s=0
        public const float BaseQuality = 0.15f; // +15% quality at s=0
        
        // Scaling: each 0.1 sensitivity adds 1% - PUBLIC for StatPart access
        public const float ScalingPerPoint = 0.1f;
        
        // Threshold levels - PUBLIC for StatPart access
        public const float Threshold3 = 3.0f;
        public const float Threshold5 = 5.0f;
        public const float Threshold8 = 8.0f;
        
        // Threshold 3 base bonuses (farming & production) - PUBLIC for StatPart access
        public const float BasePlantWorkSpeed = 0.10f; // +10% plant work at 3.0
        public const float BaseHarvestYield = 0.10f; // +10% harvest yield at 3.0
        public const float BaseDrugCookSpeed = 0.10f; // +10% drug production at 3.0
        
        // Threshold 5 base bonuses (combat & resource gathering) - PUBLIC for StatPart access
        public const float BaseHuntingStealth = 0.10f; // +10% hunting stealth at 5.0
        public const float BaseButcherSpeed = 0.10f; // +10% butcher speed at 5.0
        public const float BaseMiningSpeed = 0.10f; // +10% mining speed at 5.0
        public const float BaseMiningYield = 0.10f; // +10% mining yield at 5.0
        
        // Threshold 8 base bonuses (advanced crafting & construction) - PUBLIC for StatPart access
        public const float BaseSmithingSpeed = 0.10f; // +10% smithing at 8.0
        public const float BaseConstructionSpeed = 0.10f; // +10% construction at 8.0
        public const float BaseCraftingSpeed = 0.10f; // +10% general crafting at 8.0
        public const float BaseSurgerySuccess = 0.10f; // +10% surgery success at 8.0
        
        // Threshold perk scaling: each 0.2 over threshold adds 1% - PUBLIC for StatPart access
        public const float ThresholdScalingStep = 0.2f;
        public const float ThresholdScalingBonus = 0.01f;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            try
            {
                if (pawn == null)
                {
                    if (Prefs.DevMode)
                        Log.Warning("[Inspiration] PostAdd: pawn is null");
                    return;
                }

                // Get cached psychic sensitivity
                float sensitivity = GetCachedSensitivity();
                
                // Calculate base stat bonuses
                float workSpeed = BaseWorkSpeed + (ScalingPerPoint * sensitivity);
                float learning = BaseLearning + (ScalingPerPoint * sensitivity);
                float moveSpeed = BaseMoveSpeed + (ScalingPerPoint * sensitivity);
                float quality = BaseQuality + (ScalingPerPoint * sensitivity);
                
                // Set severity for XML compatibility
                Severity = 1.0f;

                if (Prefs.DevMode)
                {
                    StringBuilder log = new StringBuilder();
                    log.AppendLine($"[Inspiration] Applied to {pawn.LabelShort}");
                    log.AppendLine($"  - Psychic Sensitivity: {sensitivity:F2} ({sensitivity * 100:F0}%)");
                    log.AppendLine($"  - Work Speed: +{workSpeed * 100:F0}%");
                    log.AppendLine($"  - Learning: +{learning * 100:F0}%");
                    log.AppendLine($"  - Move Speed: +{moveSpeed * 100:F0}%");
                    log.AppendLine($"  - Quality: +{quality * 100:F0}%");
                    
                    // Log threshold 3 perks (farming & production)
                    if (sensitivity >= Threshold3)
                    {
                        float over = Mathf.Floor((sensitivity - Threshold3) / ThresholdScalingStep) * ThresholdScalingBonus;
                        log.AppendLine($"  - [≥3.0 Farming & Production]:");
                        log.AppendLine($"    • Plant Work Speed: +{(BasePlantWorkSpeed + over) * 100:F0}%");
                        log.AppendLine($"    • Harvest Yield: +{(BaseHarvestYield + over) * 100:F0}%");
                        log.AppendLine($"    • Drug Production: +{(BaseDrugCookSpeed + over) * 100:F0}%");
                    }
                    
                    // Log threshold 5 perks (combat & resources)
                    if (sensitivity >= Threshold5)
                    {
                        float over = Mathf.Floor((sensitivity - Threshold5) / ThresholdScalingStep) * ThresholdScalingBonus;
                        log.AppendLine($"  - [≥5.0 Combat & Resources]:");
                        log.AppendLine($"    • Hunting Stealth: +{(BaseHuntingStealth + over) * 100:F0}%");
                        log.AppendLine($"    • Butcher Speed: +{(BaseButcherSpeed + over) * 100:F0}%");
                        log.AppendLine($"    • Mining Speed: +{(BaseMiningSpeed + over) * 100:F0}%");
                        log.AppendLine($"    • Mining Yield: +{(BaseMiningYield + over) * 100:F0}%");
                    }
                    
                    // Log threshold 8 perks (advanced crafting)
                    if (sensitivity >= Threshold8)
                    {
                        float over = Mathf.Floor((sensitivity - Threshold8) / ThresholdScalingStep) * ThresholdScalingBonus;
                        log.AppendLine($"  - [≥8.0 Advanced Crafting]:");
                        log.AppendLine($"    • Smithing Speed: +{(BaseSmithingSpeed + over) * 100:F0}%");
                        log.AppendLine($"    • Construction Speed: +{(BaseConstructionSpeed + over) * 100:F0}%");
                        log.AppendLine($"    • Crafting Speed: +{(BaseCraftingSpeed + over) * 100:F0}%");
                        log.AppendLine($"    • Surgery Success: +{(BaseSurgerySuccess + over) * 100:F0}%");
                    }
                    
                    Log.Message(log.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Inspiration] PostAdd error for {pawn?.LabelShort ?? "null"}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// SAFE cache refresh every 5 seconds
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            if (pawn == null) return;

            int currentTick = Find.TickManager.TicksGame;
            if (currentTick - lastCacheTick >= CacheRefreshInterval)
            {
                cachedSensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                lastCacheTick = currentTick;
            }
        }

        public override void PostTick()
        {
            base.PostTick();

            try
            {
                tickCounter++;
                
                // Only process every TickInterval ticks for performance
                if (tickCounter >= TickInterval)
                {
                    tickCounter = 0;
                    
                    if (pawn != null && pawn.Spawned && !pawn.Dead)
                    {
                        // Spawn blue aura effect periodically
                        if (Rand.Chance(0.3f)) // 30% chance each interval
                        {
                            FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.PsycastAreaEffect, 1.5f);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"[Inspiration] Error in PostTick: {ex}");
                }
            }
        }

        public override bool TryMergeWith(Hediff other)
        {
            // Don't allow stacking - only one Inspiration buff at a time
            return false;
        }

        public override string LabelInBrackets
        {
            get
            {
                if (pawn == null)
                    return "unknown";
                    
                float sensitivity = GetCachedSensitivity();
                float workSpeed = BaseWorkSpeed + (ScalingPerPoint * sensitivity);
                
                // Show main work speed bonus
                return $"+{workSpeed * 100:F0}% work speed";
            }
        }
    }
}
