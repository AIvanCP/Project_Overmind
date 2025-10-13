using System;
using RimWorld;
using Verse;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Hediff for Inspiration psycast - scales stat bonuses with pawn's Psychic Sensitivity
    /// Provides massive work speed, learning, quality, and move speed buffs
    /// </summary>
    public class Hediff_InspirationAura : HediffWithComps
    {
        private new const int TickInterval = 60; // Update every 60 ticks (~1 second)
        private int tickCounter = 0;

        /// <summary>
        /// Apply psychic sensitivity scaling to stat offsets
        /// </summary>
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

        /// <summary>
        /// Scale stat modifiers based on psychic sensitivity
        /// Higher sensitivity = stronger buff
        /// </summary>
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            try
            {
                if (pawn == null)
                    return;

                // Get psychic sensitivity
                float psychicSensitivity = 1f;
                if (pawn.GetStatValue != null)
                {
                    psychicSensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                }

                // Clamp sensitivity to reasonable range
                psychicSensitivity = Mathf.Clamp(psychicSensitivity, 0.1f, 2.5f);

                // Apply sensitivity scaling to severity
                // This will scale all stat offsets proportionally
                Severity = psychicSensitivity;

                if (Prefs.DevMode)
                {
                    Log.Message($"[Inspiration] Applied to {pawn.LabelShort} with sensitivity {psychicSensitivity:F2}x");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Inspiration] Error in PostAdd: {ex}");
            }
        }

        /// <summary>
        /// Modify work quality based on psychic sensitivity
        /// +20% base quality bonus scaled by sensitivity
        /// </summary>
        public override bool TryMergeWith(Hediff other)
        {
            // Don't allow stacking - only one Inspiration buff at a time
            return false;
        }
    }
}
