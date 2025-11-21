using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ProjectOvermind
{
    /// <summary>
    /// Hediff for Soul Refill ability - regenerates needs over time scaled by psychic sensitivity.
    /// Includes tier bonuses for high sensitivity pawns.
    /// </summary>
    public class Hediff_SoulRefill : HediffWithComps
    {
        // Tick intervals
        private new const int TickInterval = 60; // Check every 60 ticks (1 second)
        private const int SensitivityCacheInterval = 300; // Refresh sensitivity cache every 5 seconds
        
        // Base regeneration settings
        private const float BaseRegenPercent = 0.01f; // 1% per second base
        private const float SensitivityScaling = 0.001f; // +0.1% per 0.1 sensitivity (0.001 per tick)
        
        // Tier thresholds
        private const float Tier1Sensitivity = 3.0f;
        private const float Tier2Sensitivity = 5.0f;
        private const float Tier3Sensitivity = 8.0f;
        
        // Cached values
        private float cachedSensitivity = 1.0f;
        private int ticksSinceLastSensitivityUpdate = 0;
        
        /// <summary>
        /// Main tick logic - regenerate needs every 60 ticks.
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            
            if (pawn == null || pawn.Dead || pawn.needs == null)
            {
                return;
            }
            
            // Update cached sensitivity periodically
            ticksSinceLastSensitivityUpdate++;
            if (ticksSinceLastSensitivityUpdate >= SensitivityCacheInterval)
            {
                UpdateCachedSensitivity();
                ticksSinceLastSensitivityUpdate = 0;
            }
            
            // Regenerate needs every TickInterval
            if (pawn.IsHashIntervalTick(TickInterval))
            {
                RegenerateNeeds();
            }
        }
        
        /// <summary>
        /// Update cached psychic sensitivity to avoid recursion issues.
        /// </summary>
        private void UpdateCachedSensitivity()
        {
            try
            {
                StatDef sensitivityStat = StatDefOf.PsychicSensitivity;
                if (sensitivityStat != null && pawn.GetStatValue != null)
                {
                    cachedSensitivity = pawn.GetStatValue(sensitivityStat);
                }
                else
                {
                    cachedSensitivity = 1.0f;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[ProjectOvermind] Error updating cached sensitivity for {pawn.LabelShort}: {ex}");
                cachedSensitivity = 1.0f;
            }
        }
        
        /// <summary>
        /// Regenerate all needs based on sensitivity and tier bonuses.
        /// </summary>
        private void RegenerateNeeds()
        {
            try
            {
                // Calculate base regen amount (per tick)
                float baseRegen = BaseRegenPercent / 60f; // Divide by 60 to get per-tick amount
                float sensitivityBonus = cachedSensitivity * SensitivityScaling;
                float totalRegenPerTick = baseRegen + sensitivityBonus;
                
                // Tier 3 bonus: +1% extra regeneration
                if (cachedSensitivity >= Tier3Sensitivity)
                {
                    totalRegenPerTick += 0.01f / 60f;
                }
                
                // Apply to standard needs
                RegenerateNeed(NeedDefOf.Food, totalRegenPerTick);
                RegenerateNeed(NeedDefOf.Rest, totalRegenPerTick);
                
                // Mood is added by mod, use safe access
                NeedDef moodDef = DefDatabase<NeedDef>.GetNamedSilentFail("Mood");
                if (moodDef != null)
                {
                    RegenerateNeed(moodDef, totalRegenPerTick);
                }
                
                // Tier bonuses
                if (cachedSensitivity >= Tier1Sensitivity)
                {
                    // +20% recreation gain
                    NeedDef recreationDef = DefDatabase<NeedDef>.GetNamedSilentFail("Recreation");
                    if (recreationDef != null)
                    {
                        RegenerateNeed(recreationDef, totalRegenPerTick * 1.2f);
                    }
                }
                
                if (cachedSensitivity >= Tier2Sensitivity)
                {
                    // +15% immunity gain (if pawn has immunity tracker)
                    if (pawn.health?.immunity != null)
                    {
                        // Boost immunity for all active diseases
                        try
                        {
                            var immunities = pawn.health.immunity.GetType().GetField("immunityList", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(pawn.health.immunity);
                            if (immunities is System.Collections.IEnumerable immunityEnum)
                            {
                                foreach (var immunity in immunityEnum)
                                {
                                    var immunityProp = immunity.GetType().GetProperty("immunity");
                                    if (immunityProp != null)
                                    {
                                        float current = (float)immunityProp.GetValue(immunity);
                                        immunityProp.SetValue(immunity, Math.Min(current + 0.0025f, 1.0f));
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Silently fail if reflection doesn't work
                        }
                    }
                }
                
                if (cachedSensitivity >= Tier3Sensitivity)
                {
                    // -30% rest need drain (handled by reducing rest fall rate)
                    Need_Rest restNeed = pawn.needs?.TryGetNeed<Need_Rest>();
                    if (restNeed != null)
                    {
                        // Compensate for 30% of rest drain (approximate)
                        restNeed.CurLevel += 0.0002f;
                    }
                }
                
                // Try to regenerate mod-added needs (safe API usage)
                RegenerateModNeeds(totalRegenPerTick);
            }
            catch (Exception ex)
            {
                Log.Error($"[ProjectOvermind] Error regenerating needs for {pawn.LabelShort}: {ex}");
            }
        }
        
        /// <summary>
        /// Regenerate a specific need by amount.
        /// </summary>
        private void RegenerateNeed(NeedDef needDef, float amount)
        {
            try
            {
                Need need = pawn.needs?.TryGetNeed(needDef);
                if (need != null && need.CurLevelPercentage < 1.0f)
                {
                    need.CurLevel += amount;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorOnce($"[ProjectOvermind] Error regenerating need {needDef?.defName}: {ex}", needDef.GetHashCode());
            }
        }
        
        /// <summary>
        /// Safely regenerate ALL needs dynamically (including mod-added needs).
        /// </summary>
        private void RegenerateModNeeds(float amount)
        {
            try
            {
                if (pawn.needs?.AllNeeds == null) return;
                
                // Regenerate ALL needs dynamically (catches all mod-added needs)
                foreach (Need need in pawn.needs.AllNeeds)
                {
                    if (need == null || need.def == null) continue;
                    
                    // Skip needs we already handled
                    if (need.def == NeedDefOf.Food || need.def == NeedDefOf.Rest) continue;
                    
                    // Skip mood if we already handled it
                    NeedDef moodDef = DefDatabase<NeedDef>.GetNamedSilentFail("Mood");
                    if (moodDef != null && need.def == moodDef) continue;
                    
                    // Skip recreation if we handled it in tier 1
                    NeedDef recreationDef = DefDatabase<NeedDef>.GetNamedSilentFail("Recreation");
                    if (recreationDef != null && need.def == recreationDef && cachedSensitivity >= Tier1Sensitivity) continue;
                    
                    // Regenerate all other needs
                    if (need.CurLevelPercentage < 1.0f)
                    {
                        need.CurLevel += amount;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorOnce($"[ProjectOvermind] Error regenerating mod needs: {ex}", "SoulRefill_ModNeeds".GetHashCode());
            }
        }
        
        /// <summary>
        /// Save/load state.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cachedSensitivity, "cachedSensitivity", 1.0f);
            Scribe_Values.Look(ref ticksSinceLastSensitivityUpdate, "ticksSinceLastSensitivityUpdate", 0);
        }
    }
}
