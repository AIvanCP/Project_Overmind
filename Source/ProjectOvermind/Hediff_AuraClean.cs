using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Hediff for Aura Clean ability - removes filth in an area around the caster.
    /// Radius and cleaning rate scale with psychic sensitivity.
    /// </summary>
    public class Hediff_AuraClean : HediffWithComps
    {
        // Tick intervals
        private new const int TickInterval = 90; // Clean every 90 ticks (1.5 seconds)
        private const int SensitivityCacheInterval = 300; // Refresh sensitivity cache every 5 seconds
        
        // Base cleaning settings
        private const float BaseRadius = 3.0f; // 3 tile radius base
        private const float RadiusScaling = 0.05f; // +0.05 radius per 0.1 sensitivity
        private const float MaxRadius = 12f; // Hard cap to prevent performance issues
        private const int MaxFilthPerInterval = 10; // Max filth to remove per interval (performance limit)
        
        // Tier thresholds
        private const float Tier1Sensitivity = 3.0f;
        private const float Tier2Sensitivity = 5.0f;
        private const float Tier3Sensitivity = 8.0f;
        
        // Cached values
        private float cachedSensitivity = 1.0f;
        private int ticksSinceLastSensitivityUpdate = 0;
        
        /// <summary>
        /// Main tick logic - clean filth every 90 ticks.
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            
            if (pawn == null || pawn.Dead || pawn.Map == null)
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
            
            // Clean filth every TickInterval
            if (pawn.IsHashIntervalTick(TickInterval))
            {
                CleanFilthInRadius();
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
        /// Calculate effective cleaning radius based on sensitivity and tier bonuses.
        /// </summary>
        private float GetEffectiveRadius()
        {
            float radius = BaseRadius + (cachedSensitivity * RadiusScaling * 10f); // Multiply by 10 because scaling is per 0.1
            
            // Tier bonuses
            if (cachedSensitivity >= Tier1Sensitivity)
            {
                radius += 1.0f; // +1 tile
            }
            if (cachedSensitivity >= Tier2Sensitivity)
            {
                radius += 1.0f; // +1 tile
            }
            if (cachedSensitivity >= Tier3Sensitivity)
            {
                radius += 1.5f; // +1.5 tiles
            }
            
            return Mathf.Min(radius, MaxRadius);
        }
        
        /// <summary>
        /// Calculate effective cleaning rate (filth per interval).
        /// </summary>
        private int GetEffectiveCleanRate()
        {
            int baseRate = 1;
            
            // Tier 3: double cleaning rate
            if (cachedSensitivity >= Tier3Sensitivity)
            {
                baseRate = 2;
            }
            
            return Math.Min(baseRate, MaxFilthPerInterval);
        }
        
        /// <summary>
        /// Clean filth in the area around the pawn.
        /// </summary>
        private void CleanFilthInRadius()
        {
            try
            {
                if (pawn?.Position == null || pawn.Map == null)
                {
                    return;
                }
                
                float radius = GetEffectiveRadius();
                int cleanRate = GetEffectiveCleanRate();
                int filthCleaned = 0;
                
                // Get all cells in radius
                IEnumerable<IntVec3> cells = GenRadial.RadialCellsAround(pawn.Position, radius, true);
                
                foreach (IntVec3 cell in cells)
                {
                    if (filthCleaned >= cleanRate)
                    {
                        break; // Hit performance limit
                    }
                    
                    if (!cell.InBounds(pawn.Map))
                    {
                        continue;
                    }
                    
                    // Get filth at this cell - use ToList to avoid collection modification issues
                    List<Thing> thingsAtCell = pawn.Map.thingGrid.ThingsListAt(cell).ToList();
                    for (int i = thingsAtCell.Count - 1; i >= 0; i--)
                    {
                        Thing thing = thingsAtCell[i];
                        if (thing is Filth filth && !filth.Destroyed && filth.Spawned)
                        {
                            // Check if we can clean this type of filth
                            if (CanCleanFilth(filth))
                            {
                                // Cancel any jobs targeting this filth to prevent NullReferenceException
                                if (pawn.Map?.mapPawns?.AllPawnsSpawned != null)
                                {
                                    foreach (Pawn otherPawn in pawn.Map.mapPawns.AllPawnsSpawned)
                                    {
                                        if (otherPawn?.CurJob?.targetA.Thing == filth)
                                        {
                                            otherPawn.jobs?.EndCurrentJob(Verse.AI.JobCondition.Incompletable, false);
                                        }
                                    }
                                }
                                
                                filth.Destroy(DestroyMode.Vanish);
                                filthCleaned++;
                                
                                if (filthCleaned >= cleanRate)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                
                // Apply tier 3 bonuses (movement and immunity buff)
                if (cachedSensitivity >= Tier3Sensitivity)
                {
                    // Small temporary buffs are already handled in HediffDef stages
                    // Additional immunity boost
                    if (pawn.health?.immunity != null && filthCleaned > 0)
                    {
                        // Tiny immunity boost when actively cleaning
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
                                        immunityProp.SetValue(immunity, Math.Min(current + 0.001f, 1.0f));
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
            }
            catch (Exception ex)
            {
                Log.Error($"[ProjectOvermind] Error cleaning filth for {pawn.LabelShort}: {ex}");
            }
        }
        
        /// <summary>
        /// Check if the ability can clean this type of filth based on tier.
        /// </summary>
        private bool CanCleanFilth(Filth filth)
        {
            if (filth == null || filth.def == null)
            {
                return false;
            }
            
            string defName = filth.def.defName.ToLower();
            
            // Base: can clean basic dirt, trash, rubble
            if (defName.Contains("dirt") || defName.Contains("rubble") || defName.Contains("sand"))
            {
                return true;
            }
            
            // Tier 1+: can clean medium blood and stains
            if (cachedSensitivity >= Tier1Sensitivity)
            {
                if (defName.Contains("blood") || defName.Contains("stain"))
                {
                    return true;
                }
            }
            
            // Tier 2+: can clean vomit and animal filth
            if (cachedSensitivity >= Tier2Sensitivity)
            {
                if (defName.Contains("vomit") || defName.Contains("animal"))
                {
                    return true;
                }
            }
            
            // Tier 3+: can clean almost everything (but not toxic/polluted)
            if (cachedSensitivity >= Tier3Sensitivity)
            {
                // Exclude toxic/pollution by default
                if (!defName.Contains("toxic") && !defName.Contains("pollut"))
                {
                    return true;
                }
            }
            
            return false;
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
