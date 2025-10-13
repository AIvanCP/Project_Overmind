using System;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Hediff for Hallucination psycast - causes panic attacks and visual effects
    /// Periodically forces erratic behavior in affected enemies
    /// </summary>
    public class Hediff_Hallucination : HediffWithComps
    {
        private const int CheckInterval = 60; // Check every 60 ticks (~1 second)
        private const float PanicChance = 0.25f; // 25% chance per second
        private int tickCounter = 0;

        /// <summary>
        /// Tick logic for panic attacks and visual effects
        /// </summary>
        public override void PostTick()
        {
            base.PostTick();

            try
            {
                tickCounter++;
                
                // Only process every CheckInterval ticks for performance
                if (tickCounter >= CheckInterval)
                {
                    tickCounter = 0;
                    
                    if (pawn == null || !pawn.Spawned || pawn.Dead || pawn.Downed)
                        return;

                    // Spawn purple shimmer effect periodically
                    if (Rand.Chance(0.4f)) // 40% chance each interval
                    {
                        FleckMaker.ThrowDustPuffThick(pawn.DrawPos, pawn.Map, 0.5f, new Color(0.8f, 0.3f, 0.8f));
                    }

                    // Trigger panic attack with 25% chance each second
                    if (Rand.Chance(PanicChance))
                    {
                        TriggerPanicAttack();
                    }
                }
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"[Hallucination] Error in PostTick: {ex}");
                }
            }
        }

        /// <summary>
        /// Trigger a panic attack - pawn attacks empty tile or moves erratically
        /// </summary>
        private void TriggerPanicAttack()
        {
            try
            {
                if (pawn == null || pawn.Map == null || pawn.Dead || pawn.Downed)
                    return;

                // Skip if pawn is in mental break or already has a forced job
                if (pawn.MentalStateDef != null || pawn.InMentalState)
                    return;

                // 50/50 chance: attack empty tile or move erratically
                if (Rand.Bool)
                {
                    // Attack empty tile (simulate hallucination)
                    IntVec3 randomCell = pawn.Position + IntVec3Utility.RandomHorizontalOffset(3f);
                    if (randomCell.InBounds(pawn.Map) && randomCell.Walkable(pawn.Map))
                    {
                        // Force melee attack at empty cell
                        Job panicJob = JobMaker.MakeJob(JobDefOf.AttackMelee, randomCell);
                        panicJob.expiryInterval = 60; // Short duration
                        panicJob.canBashDoors = false;
                        panicJob.canBashFences = false;
                        
                        if (pawn.jobs != null)
                        {
                            pawn.jobs.StartJob(panicJob, JobCondition.InterruptForced, null, false, true);
                        }

                        if (Prefs.DevMode)
                        {
                            Log.Message($"[Hallucination] {pawn.LabelShort} panic attacks empty cell");
                        }
                    }
                }
                else
                {
                    // Move erratically (wander to random nearby cell)
                    IntVec3 randomDest = pawn.Position + IntVec3Utility.RandomHorizontalOffset(5f);
                    if (randomDest.InBounds(pawn.Map) && randomDest.Walkable(pawn.Map))
                    {
                        Job wanderJob = JobMaker.MakeJob(JobDefOf.Goto, randomDest);
                        wanderJob.expiryInterval = 120; // Short wander
                        wanderJob.locomotionUrgency = LocomotionUrgency.Sprint;
                        
                        if (pawn.jobs != null)
                        {
                            pawn.jobs.StartJob(wanderJob, JobCondition.InterruptForced, null, false, true);
                        }

                        if (Prefs.DevMode)
                        {
                            Log.Message($"[Hallucination] {pawn.LabelShort} wanders erratically");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Warning($"[Hallucination] Error triggering panic attack: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Prevent stacking hallucination hediffs
        /// </summary>
        public override bool TryMergeWith(Hediff other)
        {
            // Don't allow stacking - only one Hallucination at a time
            return false;
        }

        /// <summary>
        /// Cleanup when hediff is removed
        /// </summary>
        public override void PostRemoved()
        {
            base.PostRemoved();

            try
            {
                if (pawn != null && pawn.Spawned && !pawn.Dead)
                {
                    // Return pawn to normal AI behavior
                    if (pawn.jobs != null && pawn.jobs.curJob != null)
                    {
                        // Don't force-end jobs, let them expire naturally
                        if (Prefs.DevMode)
                        {
                            Log.Message($"[Hallucination] Removed from {pawn.LabelShort}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"[Hallucination] Error in PostRemoved: {ex}");
                }
            }
        }
    }
}
