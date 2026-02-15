using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Area effect Mind Spike - controls all hostile humanlike pawns in radius
    /// Radius scales with caster's psychic sensitivity
    /// </summary>
    public class Verb_MindSpike : Verb_CastAbility
    {
        // Base radius at 0 sensitivity: 3 tiles
        private const float BaseRadius = 3f;
        // Each 0.1 sensitivity adds 0.3 tiles radius
        private const float RadiusPerStep = 0.3f;
        // Chain range when controlled pawn dies
        private const float ChainRange = 6f;

        /// <summary>
        /// Draw targeting radius ring
        /// </summary>
        public override void DrawHighlight(LocalTargetInfo target)
        {
            if (target.IsValid && CasterPawn != null)
            {
                float radius = DurationHelper.CalculateRadius(CasterPawn, BaseRadius, RadiusPerStep);
                GenDraw.DrawRadiusRing(target.Cell, radius);
            }
        }

        /// <summary>
        /// Show radius field around target location
        /// </summary>
        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            if (CasterPawn != null)
            {
                return DurationHelper.CalculateRadius(CasterPawn, BaseRadius, RadiusPerStep);
            }
            return BaseRadius;
        }

        /// <summary>
        /// Calculate are effect cells for targeting overlay
        /// </summary>
        public override void OrderForceTarget(LocalTargetInfo target)
        {
            base.OrderForceTarget(target);
        }

        protected override bool TryCastShot()
        {
            try
            {
                if (CasterPawn == null || CasterPawn.Map == null)
                {
                    return false;
                }

                // Get target position (can be pawn or ground)
                IntVec3 targetPos;
                if (currentTarget.HasThing && currentTarget.Thing is Pawn)
                {
                    targetPos = currentTarget.Thing.Position;
                }
                else
                {
                    targetPos = currentTarget.Cell;
                }

                // Calculate radius based on caster's sensitivity
                float radius = DurationHelper.CalculateRadius(CasterPawn, BaseRadius, RadiusPerStep);

                // Get all pawns in radius
                List<Pawn> affectedPawns = GetAffectedPawns(targetPos, radius);

                if (affectedPawns.Count == 0)
                {
                    Messages.Message("Mind Spike: No valid targets in area.", MessageTypeDefOf.NeutralEvent, false);
                    return true; // Still counts as cast (cooldown applies)
                }

                // Apply Mind Spike to all valid targets
                int controlledCount = 0;
                foreach (Pawn pawn in affectedPawns)
                {
                    if (ApplyMindSpike(pawn, CasterPawn, false))
                    {
                        controlledCount++;
                    }
                }

                // Success message
                Messages.Message(
                    $"Mind Spike: {controlledCount} mind{(controlledCount == 1 ? "" : "s")} seized!",
                    new TargetInfo(targetPos, CasterPawn.Map),
                    MessageTypeDefOf.NeutralEvent,
                    true
                );

                // Area visual effect
                FleckMaker.Static(targetPos, CasterPawn.Map, FleckDefOf.PsycastAreaEffect, radius * 2f);
                
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Spike] Error in TryCastShot: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Get all valid hostile humanlike pawns in radius
        /// </summary>
        private List<Pawn> GetAffectedPawns(IntVec3 center, float radius)
        {
            List<Pawn> result = new List<Pawn>();
            
            try
            {
                if (CasterPawn?.Map == null)
                    return result;

                foreach (Pawn pawn in CasterPawn.Map.mapPawns.AllPawnsSpawned)
                {
                    if (IsValidTarget(pawn) && pawn.Position.DistanceTo(center) <= radius)
                    {
                        result.Add(pawn);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Spike] Error in GetAffectedPawns: {ex}");
            }

            return result;
        }

        private bool IsValidTarget(Pawn target)
        {
            try
            {
                if (target == null || target.Dead || target.Downed)
                    return false;

                // Must be hostile
                if (!target.HostileTo(CasterPawn))
                    return false;

                // Allow humanlike OR animals (no mechanoids)
                if (target.RaceProps.IsMechanoid)
                    return false;

                if (!target.RaceProps.Humanlike && !target.RaceProps.Animal)
                    return false;

                // Must have consciousness
                if (!target.health.capacities.CapableOf(PawnCapacityDefOf.Consciousness))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Spike] Error in IsValidTarget: {ex}");
                return false;
            }
        }

        public static bool ApplyMindSpike(Pawn target, Pawn caster, bool isChain)
        {
            try
            {
                if (target == null || target.Dead)
                    return false;

                // Add the Mind Spike hediff with dynamic duration
                Hediff existingHediff = target.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ProjectOvermind_MindSpikeControlled);
                if (existingHediff != null)
                {
                    // Already under control, refresh duration
                    HediffComp_Disappears comp = existingHediff.TryGetComp<HediffComp_Disappears>();
                    if (comp != null)
                    {
                        comp.ticksToDisappear = DurationHelper.CalculateDuration(caster);
                    }
                }
                else
                {
                    Hediff_MindSpikeControlled hediff = (Hediff_MindSpikeControlled)HediffMaker.MakeHediff(
                        HediffDefOf.ProjectOvermind_MindSpikeControlled, 
                        target
                    );
                    hediff.casterPawn = caster;
                    hediff.hasChained = isChain;
                    target.health.AddHediff(hediff);
                    
                    // Set dynamic duration based on caster sensitivity
                    HediffComp_Disappears comp = hediff.TryGetComp<HediffComp_Disappears>();
                    if (comp != null)
                    {
                        comp.ticksToDisappear = DurationHelper.CalculateDuration(caster);
                    }
                }

                // Visual effects
                FleckMaker.ThrowMetaPuff(target.Position.ToVector3(), target.Map);
                FleckMaker.Static(target.Position, target.Map, FleckDefOf.PsycastAreaEffect, 1f);

                // Floating text
                MoteMaker.ThrowText(target.DrawPos + Vector3.up, target.Map, "SEIZED!", new Color(0.7f, 0.2f, 1f), 3.5f);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Spike] Error in ApplyMindSpike: {ex}");
                return false;
            }
        }

        public static void TryChainToNearbyEnemy(Pawn deadPawn, Pawn caster)
        {
            try
            {
                if (deadPawn == null || caster == null || deadPawn.Map == null)
                    return;

                // Find nearest valid enemy within chain range
                Pawn chainTarget = deadPawn.Map.mapPawns.AllPawnsSpawned
                    .Where(p => p != deadPawn 
                        && !p.Dead 
                        && !p.Downed
                        && p.HostileTo(caster)
                        && p.RaceProps.Humanlike
                        && p.Position.DistanceTo(deadPawn.Position) <= ChainRange
                        && p.health.capacities.CapableOf(PawnCapacityDefOf.Consciousness))
                    .OrderBy(p => p.Position.DistanceTo(deadPawn.Position))
                    .FirstOrDefault();

                if (chainTarget != null)
                {
                    // Visual effect for chain
                    FleckMaker.ThrowLightningGlow(deadPawn.DrawPos, deadPawn.Map, 1.5f);
                    FleckMaker.ThrowLightningGlow(chainTarget.DrawPos, chainTarget.Map, 1.5f);

                    // Apply Mind Spike to new target
                    ApplyMindSpike(chainTarget, caster, true);
                    
                    Messages.Message(
                        $"Mind Spike: Chained to {chainTarget.LabelShort}!", 
                        chainTarget, 
                        MessageTypeDefOf.NeutralEvent, 
                        false
                    );
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Spike] Error in TryChainToNearbyEnemy: {ex}");
            }
        }
    }

    // Custom HediffDef holder for compile-time safety
    [DefOf]
    public static class HediffDefOf
    {
        public static HediffDef ProjectOvermind_MindSpikeControlled;
        public static HediffDef ProjectOvermind_MindSpikeDisoriented;

        static HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
        }
    }
}
