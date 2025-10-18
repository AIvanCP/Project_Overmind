using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ProjectOvermind
{
    /// <summary>
    /// Comprehensive StatPart for Inspiration buff
    /// Handles all base stats and threshold perks efficiently
    /// Optimized: caches hediff lookup, calculates bonus once per pawn
    /// </summary>
    public class StatPart_Inspiration : StatPart
    {
        // Cache to avoid repeated hediff lookups (cleared when hediff removed)
        private static Dictionary<Pawn, Hediff_InspirationAura> cachedHediffs = new Dictionary<Pawn, Hediff_InspirationAura>();

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn))
                return;

            Hediff_InspirationAura inspiration = GetInspirationHediff(pawn);
            if (inspiration == null)
            {
                cachedHediffs.Remove(pawn);
                return;
            }

            // SAFE: Read cached sensitivity from hediff instead of calling GetStatValue
            float sensitivity = inspiration.CachedPsychicSensitivity;
            float bonus = 0f;

            // Base stats (always active)
            if (parentStat == StatDefOf.WorkSpeedGlobal)
            {
                bonus = Hediff_InspirationAura.BaseWorkSpeed + (Hediff_InspirationAura.ScalingPerPoint * sensitivity);
            }
            else if (parentStat == StatDefOf.GlobalLearningFactor)
            {
                bonus = Hediff_InspirationAura.BaseLearning + (Hediff_InspirationAura.ScalingPerPoint * sensitivity);
            }
            else if (parentStat == StatDefOf.MoveSpeed)
            {
                bonus = Hediff_InspirationAura.BaseMoveSpeed + (Hediff_InspirationAura.ScalingPerPoint * sensitivity);
            }
            // Threshold 3: Farming & Production
            else if (sensitivity >= Hediff_InspirationAura.Threshold3)
            {
                float thresholdBonus = Mathf.Floor((sensitivity - Hediff_InspirationAura.Threshold3) / Hediff_InspirationAura.ThresholdScalingStep) * Hediff_InspirationAura.ThresholdScalingBonus;
                
                if (parentStat == StatDefOf.PlantWorkSpeed)
                {
                    bonus = Hediff_InspirationAura.BasePlantWorkSpeed + thresholdBonus;
                }
                else if (parentStat == StatDefOf.PlantHarvestYield)
                {
                    bonus = Hediff_InspirationAura.BaseHarvestYield + thresholdBonus;
                }
                else if (parentStat.defName == "DrugCookingSpeed")
                {
                    bonus = Hediff_InspirationAura.BaseDrugCookSpeed + thresholdBonus;
                }
            }

            // Threshold 5: Combat & Resources
            if (sensitivity >= Hediff_InspirationAura.Threshold5)
            {
                float thresholdBonus = Mathf.Floor((sensitivity - Hediff_InspirationAura.Threshold5) / Hediff_InspirationAura.ThresholdScalingStep) * Hediff_InspirationAura.ThresholdScalingBonus;
                
                if (parentStat == StatDefOf.HuntingStealth)
                {
                    bonus = Hediff_InspirationAura.BaseHuntingStealth + thresholdBonus;
                }
                else if (parentStat.defName == "ButcheryFleshSpeed")
                {
                    bonus = Hediff_InspirationAura.BaseButcherSpeed + thresholdBonus;
                }
                else if (parentStat == StatDefOf.MiningSpeed)
                {
                    bonus = Hediff_InspirationAura.BaseMiningSpeed + thresholdBonus;
                }
                else if (parentStat == StatDefOf.MiningYield)
                {
                    bonus = Hediff_InspirationAura.BaseMiningYield + thresholdBonus;
                }
            }

            // Threshold 8: Advanced Crafting
            if (sensitivity >= Hediff_InspirationAura.Threshold8)
            {
                float thresholdBonus = Mathf.Floor((sensitivity - Hediff_InspirationAura.Threshold8) / Hediff_InspirationAura.ThresholdScalingStep) * Hediff_InspirationAura.ThresholdScalingBonus;
                
                if (parentStat.defName == "SmithingSpeed")
                {
                    bonus = Hediff_InspirationAura.BaseSmithingSpeed + thresholdBonus;
                }
                else if (parentStat == StatDefOf.ConstructionSpeed)
                {
                    bonus = Hediff_InspirationAura.BaseConstructionSpeed + thresholdBonus;
                }
                else if (parentStat == StatDefOf.GeneralLaborSpeed)
                {
                    bonus = Hediff_InspirationAura.BaseCraftingSpeed + thresholdBonus;
                }
                else if (parentStat == StatDefOf.MedicalSurgerySuccessChance)
                {
                    bonus = Hediff_InspirationAura.BaseSurgerySuccess + thresholdBonus;
                }
            }

            if (bonus > 0)
                val += bonus;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !(req.Thing is Pawn pawn))
                return null;

            Hediff_InspirationAura inspiration = GetInspirationHediff(pawn);
            if (inspiration == null)
                return null;

            // SAFE: Read cached sensitivity from hediff instead of calling GetStatValue
            float sensitivity = inspiration.CachedPsychicSensitivity;
            float bonus = 0f;
            string label = "Inspiration";

            // Calculate bonus for current stat
            if (parentStat == StatDefOf.WorkSpeedGlobal)
            {
                bonus = Hediff_InspirationAura.BaseWorkSpeed + (Hediff_InspirationAura.ScalingPerPoint * sensitivity);
            }
            else if (parentStat == StatDefOf.GlobalLearningFactor)
            {
                bonus = Hediff_InspirationAura.BaseLearning + (Hediff_InspirationAura.ScalingPerPoint * sensitivity);
            }
            else if (parentStat == StatDefOf.MoveSpeed)
            {
                bonus = Hediff_InspirationAura.BaseMoveSpeed + (Hediff_InspirationAura.ScalingPerPoint * sensitivity);
            }
            else if (sensitivity >= Hediff_InspirationAura.Threshold3)
            {
                float thresholdBonus = Mathf.Floor((sensitivity - Hediff_InspirationAura.Threshold3) / Hediff_InspirationAura.ThresholdScalingStep) * Hediff_InspirationAura.ThresholdScalingBonus;
                
                if (parentStat == StatDefOf.PlantWorkSpeed)
                    bonus = Hediff_InspirationAura.BasePlantWorkSpeed + thresholdBonus;
                else if (parentStat == StatDefOf.PlantHarvestYield)
                    bonus = Hediff_InspirationAura.BaseHarvestYield + thresholdBonus;
                else if (parentStat.defName == "DrugCookingSpeed")
                    bonus = Hediff_InspirationAura.BaseDrugCookSpeed + thresholdBonus;
            }

            if (sensitivity >= Hediff_InspirationAura.Threshold5)
            {
                float thresholdBonus = Mathf.Floor((sensitivity - Hediff_InspirationAura.Threshold5) / Hediff_InspirationAura.ThresholdScalingStep) * Hediff_InspirationAura.ThresholdScalingBonus;
                
                if (parentStat == StatDefOf.HuntingStealth)
                    bonus = Hediff_InspirationAura.BaseHuntingStealth + thresholdBonus;
                else if (parentStat.defName == "ButcheryFleshSpeed")
                    bonus = Hediff_InspirationAura.BaseButcherSpeed + thresholdBonus;
                else if (parentStat == StatDefOf.MiningSpeed)
                    bonus = Hediff_InspirationAura.BaseMiningSpeed + thresholdBonus;
                else if (parentStat == StatDefOf.MiningYield)
                    bonus = Hediff_InspirationAura.BaseMiningYield + thresholdBonus;
            }

            if (sensitivity >= Hediff_InspirationAura.Threshold8)
            {
                float thresholdBonus = Mathf.Floor((sensitivity - Hediff_InspirationAura.Threshold8) / Hediff_InspirationAura.ThresholdScalingStep) * Hediff_InspirationAura.ThresholdScalingBonus;
                
                if (parentStat.defName == "SmithingSpeed")
                    bonus = Hediff_InspirationAura.BaseSmithingSpeed + thresholdBonus;
                else if (parentStat == StatDefOf.ConstructionSpeed)
                    bonus = Hediff_InspirationAura.BaseConstructionSpeed + thresholdBonus;
                else if (parentStat == StatDefOf.GeneralLaborSpeed)
                    bonus = Hediff_InspirationAura.BaseCraftingSpeed + thresholdBonus;
                else if (parentStat == StatDefOf.MedicalSurgerySuccessChance)
                    bonus = Hediff_InspirationAura.BaseSurgerySuccess + thresholdBonus;
            }

            if (bonus > 0)
                return $"{label}: +{bonus * 100:F0}%";

            return null;
        }

        private Hediff_InspirationAura GetInspirationHediff(Pawn pawn)
        {
            // Check cache first
            if (cachedHediffs.TryGetValue(pawn, out Hediff_InspirationAura cached))
            {
                if (cached != null && !cached.ShouldRemove)
                    return cached;
                else
                    cachedHediffs.Remove(pawn);
            }

            // Lookup and cache
            if (pawn?.health?.hediffSet?.hediffs == null)
                return null;

            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff is Hediff_InspirationAura inspiration && !inspiration.ShouldRemove)
                {
                    cachedHediffs[pawn] = inspiration;
                    return inspiration;
                }
            }

            return null;
        }
    }
}
