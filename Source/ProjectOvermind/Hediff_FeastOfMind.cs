using RimWorld;
using System;
using System.Text;
using UnityEngine;
using Verse;

namespace ProjectOvermind
{
    public class Hediff_FeastOfMind : HediffWithComps
    {
        private new const int TickInterval = 60; // Check every 60 ticks (1 second) for visual effects
        
        // Base values at sensitivity 0 - PUBLIC for StatPart access
        public const float BaseHungerReduction = 0.10f; // 10% hunger reduction at s=0
        public const float BaseEatingSpeed = 0.25f; // +25% eating speed at s=0
        
        // Scaling: each 0.1 sensitivity adds 1% to effects - PUBLIC for StatPart access
        public const float ScalingPerPoint = 0.1f; // multiply by sensitivity
        
        // Hunger cap: max 99% reduction (min 1% hunger rate) - PUBLIC for StatPart access
        public const float MaxHungerReduction = 0.99f;
        
        // Threshold levels for special perks - PUBLIC for StatPart access
        public const float ThresholdLearning = 3.0f;
        public const float ThresholdDamageReduction = 5.0f;
        public const float ThresholdTirednessReduction = 8.0f;
        
        // Base values for threshold perks - PUBLIC for StatPart access
        public const float BaseLearningBonus = 0.10f; // +10% learning at 3.0
        public const float BaseDamageReduction = 0.05f; // +5% damage reduction at 5.0
        public const float BaseTirednessReduction = 0.05f; // -5% rest need at 8.0
        
        // Threshold perk scaling: each 0.2 over threshold adds 1% - PUBLIC for StatPart access
        public const float ThresholdScalingStep = 0.2f;
        public const float ThresholdScalingBonus = 0.01f; // 1% per step

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            try
            {
                if (pawn == null)
                {
                    if (Prefs.DevMode)
                        Log.Warning("[FeastOfMind] PostAdd: pawn is null");
                    return;
                }

                // Get psychic sensitivity
                float sensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                
                // Calculate hunger reduction: base + (sensitivity * 0.1), capped at 99%
                float hungerReduction = Mathf.Clamp(
                    BaseHungerReduction + (ScalingPerPoint * sensitivity),
                    BaseHungerReduction,
                    MaxHungerReduction
                );
                
                // Calculate eating speed bonus: base + (sensitivity * 0.1), no cap
                float eatingSpeedBonus = BaseEatingSpeed + (ScalingPerPoint * sensitivity);
                
                // Set severity to encode the hunger reduction factor for XML stage use
                // Severity will be used by XML hungerRateFactor
                // We'll store hunger reduction value in severity
                Severity = hungerReduction;

                if (Prefs.DevMode)
                {
                    StringBuilder log = new StringBuilder();
                    log.AppendLine($"[FeastOfMind] PostAdd for {pawn.LabelShort}:");
                    log.AppendLine($"  - Psychic Sensitivity: {sensitivity:F2} ({sensitivity * 100:F0}%)");
                    log.AppendLine($"  - Hunger Reduction: {hungerReduction * 100:F0}% (hunger rate: {(1f - hungerReduction) * 100:F0}%)");
                    log.AppendLine($"  - Eating Speed Bonus: +{eatingSpeedBonus * 100:F0}%");
                    
                    // Log threshold perks
                    if (sensitivity >= ThresholdLearning)
                    {
                        float learningBonus = BaseLearningBonus + 
                            (Mathf.Floor((sensitivity - ThresholdLearning) / ThresholdScalingStep) * ThresholdScalingBonus);
                        log.AppendLine($"  - Learning Bonus (≥3.0): +{learningBonus * 100:F0}%");
                    }
                    if (sensitivity >= ThresholdDamageReduction)
                    {
                        float damageReduction = BaseDamageReduction + 
                            (Mathf.Floor((sensitivity - ThresholdDamageReduction) / ThresholdScalingStep) * ThresholdScalingBonus);
                        log.AppendLine($"  - Damage Reduction (≥5.0): +{damageReduction * 100:F0}%");
                    }
                    if (sensitivity >= ThresholdTirednessReduction)
                    {
                        float tirednessReduction = BaseTirednessReduction + 
                            (Mathf.Floor((sensitivity - ThresholdTirednessReduction) / ThresholdScalingStep) * ThresholdScalingBonus);
                        log.AppendLine($"  - Tiredness Reduction (≥8.0): +{tirednessReduction * 100:F0}%");
                    }
                    
                    Log.Message(log.ToString());
                }

                // Spawn initial visual effect
                if (pawn.Spawned && pawn.Map != null)
                {
                    FleckMaker.ThrowDustPuffThick(
                        pawn.DrawPos,
                        pawn.Map,
                        1.5f,
                        new Color(0.3f, 1f, 0.7f) // cyan-green
                    );
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[FeastOfMind] PostAdd error for {pawn?.LabelShort ?? "null"}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public override void PostTick()
        {
            base.PostTick();

            try
            {
                if (pawn == null || !pawn.Spawned || pawn.Map == null)
                    return;

                // Periodic visual effect (every 60 ticks = 1 second)
                if (pawn.IsHashIntervalTick(TickInterval))
                {
                    // 20% chance to spawn a subtle glow each second
                    if (Rand.Chance(0.2f))
                    {
                        FleckMaker.ThrowDustPuffThick(
                            pawn.DrawPos + new Vector3(
                                Rand.Range(-0.3f, 0.3f),
                                0f,
                                Rand.Range(-0.3f, 0.3f)
                            ),
                            pawn.Map,
                            Rand.Range(0.3f, 0.6f),
                            new Color(0.3f, 1f, 0.7f, 0.5f) // cyan-green, semi-transparent
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                    Log.Error($"[FeastOfMind] PostTick error for {pawn?.LabelShort ?? "null"}: {ex.Message}");
            }
        }

        public override bool TryMergeWith(Hediff other)
        {
            // Don't allow stacking - duration will be refreshed in Verb instead
            return false;
        }

        public override void PostRemoved()
        {
            base.PostRemoved();

            try
            {
                if (Prefs.DevMode && pawn != null)
                    Log.Message($"[FeastOfMind] Buff expired for {pawn.LabelShort}");

                // Optional: spawn a fade-out effect
                if (pawn != null && pawn.Spawned && pawn.Map != null)
                {
                    FleckMaker.ThrowDustPuffThick(
                        pawn.DrawPos,
                        pawn.Map,
                        0.8f,
                        new Color(0.3f, 1f, 0.7f, 0.3f)
                    );
                }
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                    Log.Error($"[FeastOfMind] PostRemoved error: {ex.Message}");
            }
        }

        // Custom stat offset methods
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            
            // Apply damage reduction if threshold reached
            if (pawn != null && dinfo.Def != null)
            {
                float sensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                if (sensitivity >= ThresholdDamageReduction)
                {
                    float damageReduction = BaseDamageReduction + 
                        (Mathf.Floor((sensitivity - ThresholdDamageReduction) / ThresholdScalingStep) * ThresholdScalingBonus);
                    
                    // Note: RimWorld damage is already applied, this is for logging only
                    // Actual damage reduction should be via StatPart or ArmorUtility
                    if (Prefs.DevMode)
                        Log.Message($"[FeastOfMind] Damage reduction active: {damageReduction * 100:F0}%");
                }
            }
        }

        public override string LabelInBrackets
        {
            get
            {
                if (pawn == null)
                    return "unknown";
                    
                float sensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                
                // Calculate actual hunger reduction
                float hungerReduction = Mathf.Clamp(
                    BaseHungerReduction + (ScalingPerPoint * sensitivity),
                    BaseHungerReduction,
                    MaxHungerReduction
                );
                
                // Show hunger reduction percentage
                return $"{hungerReduction * 100:F0}% hunger reduction";
            }
        }

        // Override CurStage to provide dynamic stage with calculated hungerRateFactor
        public override HediffStage CurStage
        {
            get
            {
                if (pawn == null || def.stages == null || def.stages.Count == 0)
                    return base.CurStage;

                // Get base stage from XML
                HediffStage stage = base.CurStage ?? def.stages[0];
                
                // Calculate dynamic hunger factor
                float sensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                float hungerReduction = Mathf.Clamp(
                    BaseHungerReduction + (ScalingPerPoint * sensitivity),
                    BaseHungerReduction,
                    MaxHungerReduction
                );
                
                // Create dynamic stage copy with calculated hunger factor
                HediffStage dynamicStage = new HediffStage();
                dynamicStage.hungerRateFactor = 1f - hungerReduction;
                
                return dynamicStage;
            }
        }
    }
}
