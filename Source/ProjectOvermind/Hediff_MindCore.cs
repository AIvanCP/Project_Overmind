using System;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Hediff for Mind Core psycast - comprehensive psycast performance enhancement
    /// Provides psyfocus regeneration, max entropy increase, cost reduction, and cooldown reduction
    /// All effects scale with caster's psychic sensitivity
    /// Cross-mod compatible with any psycast system
    /// </summary>
    public class Hediff_MindCore : HediffWithComps
    {
        // Store CASTER's sensitivity (not recipient's)
        private float casterSensitivity = 1f;

        /// <summary>
        /// Public property for Harmony patches to access caster's sensitivity safely
        /// </summary>
        public float CasterSensitivity => casterSensitivity;

        // Base values and scaling
        private const float BasePsyfocusRegenPerSecond = 0.01f;  // 1% per second at 1.0 sensitivity
        private const float PsyfocusRegenPerSensitivity = 0.005f; // +0.5% per 0.1 sensitivity

        private const float BaseMaxEntropyIncrease = 30f;  // +30 max entropy at 1.0 sensitivity
        private const float MaxEntropyPerSensitivity = 3f; // +3 max entropy per 0.1 sensitivity

        private const float BaseCostReduction = 0.30f;  // 30% cost reduction at 1.0 sensitivity
        private const float CostReductionPerSensitivity = 0.03f; // +3% per 0.1 sensitivity
        private const float MaxCostReduction = 0.70f;  // Cap at 70% reduction (30% minimum cost)

        private const float BaseCooldownReduction = 0.25f;  // 25% cooldown reduction at 1.0 sensitivity
        private const float CooldownReductionPerSensitivity = 0.025f; // +2.5% per 0.1 sensitivity
        private const float MaxCooldownReduction = 0.60f;  // Cap at 60% reduction (40% minimum cooldown)

        private int lastRegenTick = 0;

        /// <summary>
        /// Initialize hediff with caster's sensitivity
        /// </summary>
        public void InitializeCasterSensitivity(float sensitivity)
        {
            casterSensitivity = Mathf.Max(0.01f, sensitivity); // Minimum 0.01 to avoid division issues
        }

        /// <summary>
        /// Get psyfocus regeneration amount per second (scaled)
        /// </summary>
        public float GetPsyfocusRegenPerSecond()
        {
            // Base regen + scaling
            return BasePsyfocusRegenPerSecond + (casterSensitivity / 0.1f * PsyfocusRegenPerSensitivity);
        }

        /// <summary>
        /// Get max entropy increase (scaled)
        /// </summary>
        public float GetMaxEntropyIncrease()
        {
            // Base increase + scaling
            return BaseMaxEntropyIncrease + (casterSensitivity / 0.1f * MaxEntropyPerSensitivity);
        }

        /// <summary>
        /// Get psyfocus cost multiplier (scaled, capped)
        /// Returns a value between 0.3 and 1.0 (1.0 - reduction)
        /// </summary>
        public float GetPsyfocusCostMultiplier()
        {
            float reduction = BaseCostReduction + (casterSensitivity / 0.1f * CostReductionPerSensitivity);
            reduction = Mathf.Min(reduction, MaxCostReduction); // Cap at 70% reduction
            return 1f - reduction; // Return multiplier (e.g., 0.7 = 70% cost, 30% reduction)
        }

        /// <summary>
        /// Get cooldown multiplier (scaled, capped)
        /// Returns a value between 0.4 and 1.0 (1.0 - reduction)
        /// </summary>
        public float GetCooldownMultiplier()
        {
            float reduction = BaseCooldownReduction + (casterSensitivity / 0.1f * CooldownReductionPerSensitivity);
            reduction = Mathf.Min(reduction, MaxCooldownReduction); // Cap at 60% reduction
            return 1f - reduction; // Return multiplier (e.g., 0.75 = 75% cooldown, 25% reduction)
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            lastRegenTick = Find.TickManager.TicksGame;

            if (Prefs.DevMode)
            {
                StringBuilder log = new StringBuilder();
                log.AppendLine($"[Mind Core] Applied to {pawn.LabelShort}");
                log.AppendLine($"  - Caster Sensitivity: {casterSensitivity:F2}");
                log.AppendLine($"  - Psyfocus Regen: {GetPsyfocusRegenPerSecond():P2}/sec");
                log.AppendLine($"  - Max Entropy +{GetMaxEntropyIncrease():F0}");
                log.AppendLine($"  - Cost Multiplier: {GetPsyfocusCostMultiplier():P0}");
                log.AppendLine($"  - Cooldown Multiplier: {GetCooldownMultiplier():P0}");
                Log.Message(log.ToString());
            }
        }

        /// <summary>
        /// Tick to apply passive psyfocus regeneration
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            if (pawn == null || pawn.Dead || !pawn.Spawned)
                return;

            // Check if pawn has psychic entropy tracker (can use psycasts)
            if (pawn.psychicEntropy == null)
                return;

            int currentTick = Find.TickManager.TicksGame;

            // Apply psyfocus regen every 60 ticks (~1 second)
            if (currentTick - lastRegenTick >= 60)
            {
                lastRegenTick = currentTick;

                float regenAmount = GetPsyfocusRegenPerSecond();
                
                // Add psyfocus (if not at max)
                float currentPsyfocus = pawn.psychicEntropy.CurrentPsyfocus;
                float maxPsyfocus = pawn.psychicEntropy.EntropyValue <= 0 ? 1f : 1f; // Max is always 1.0
                if (currentPsyfocus < maxPsyfocus)
                {
                    pawn.psychicEntropy.OffsetPsyfocusDirectly(regenAmount);
                }
            }
        }

        /// <summary>
        /// Modify max psychic entropy stat
        /// </summary>
        public override void PostTick()
        {
            base.PostTick();

            // Note: Max entropy is handled via StatPart_MindCore instead
            // to avoid stat caching issues
        }

        /// <summary>
        /// Show buff details in tooltip with duration and all effects
        /// </summary>
        public override string TipStringExtra
        {
            get
            {
                if (pawn == null) return base.TipStringExtra;

                try
                {
                    StringBuilder sb = new StringBuilder();

                    // Show caster sensitivity
                    sb.AppendLine($"Caster Sensitivity: {casterSensitivity:F1}");
                    sb.AppendLine();

                    // Effects with actual values
                    sb.AppendLine($"Psyfocus Regen: +{GetPsyfocusRegenPerSecond():P1}/sec (passive)");
                    sb.AppendLine($"Psyfocus Gain: +15% (meditation/neural supercharger)");
                    sb.AppendLine($"Max Entropy: +{GetMaxEntropyIncrease():F0}");
                    
                    // Show cost and cooldown as reductions
                    float costReduction = (1f - GetPsyfocusCostMultiplier()) * 100f;
                    float cooldownReduction = (1f - GetCooldownMultiplier()) * 100f;
                    sb.AppendLine($"Psycast Cost: -{costReduction:F0}% (all abilities including VPE)");
                    sb.AppendLine($"Cooldown Time: -{cooldownReduction:F0}% (all abilities including VPE)");

                    return sb.ToString().TrimEnd();
                }
                catch (Exception ex)
                {
                    Log.Error($"[Mind Core] Error in TipStringExtra: {ex}");
                    return base.TipStringExtra;
                }
            }
        }

        /// <summary>
        /// Show duration remaining in brackets
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref casterSensitivity, "casterSensitivity", 1f);
            Scribe_Values.Look(ref lastRegenTick, "lastRegenTick", 0);
        }
    }
}
