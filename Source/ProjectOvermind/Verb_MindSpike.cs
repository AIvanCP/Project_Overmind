using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace ProjectOvermind
{
    public class Verb_MindSpike : Verb_CastAbility
    {
        private const float ChainRange = 6f;

        protected override bool TryCastShot()
        {
            try
            {
                if (currentTarget.HasThing && currentTarget.Thing is Pawn targetPawn)
                {
                    // Validate target
                    if (!IsValidTarget(targetPawn))
                    {
                        Messages.Message("Cannot use Mind Spike on this target.", MessageTypeDefOf.RejectInput, false);
                        return false;
                    }

                    // Apply Mind Spike effect
                    ApplyMindSpike(targetPawn, CasterPawn, false);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Spike] Error in TryCastShot: {ex}");
            }

            return false;
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

                // Must be humanlike (no mechanoids or animals)
                if (!target.RaceProps.Humanlike)
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

        public static void ApplyMindSpike(Pawn target, Pawn caster, bool isChain)
        {
            try
            {
                if (target == null || target.Dead)
                    return;

                // Add the Mind Spike hediff
                Hediff existingHediff = target.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ProjectOvermind_MindSpikeControlled);
                if (existingHediff != null)
                {
                    // Already under control, refresh duration
                    existingHediff.Severity = 1f;
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
                }

                // Visual effects
                FleckMaker.ThrowMetaPuff(target.Position.ToVector3(), target.Map);
                
                // Psychic effect fleck
                FleckMaker.Static(target.Position, target.Map, FleckDefOf.PsycastAreaEffect, 1f);

                // Floating text
                MoteMaker.ThrowText(target.DrawPos + Vector3.up, target.Map, "SEIZED!", new Color(0.7f, 0.2f, 1f), 3.5f);

                // Message
                string chainText = isChain ? " (Chained)" : "";
                Messages.Message(
                    $"[Mind Spike] {caster.LabelShort} seized {target.LabelShort}'s mind!{chainText}", 
                    target, 
                    MessageTypeDefOf.NeutralEvent, 
                    false
                );
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Spike] Error in ApplyMindSpike: {ex}");
            }
        }

        public static void TryChainToNearbyEnemy(Pawn deadPawn, Pawn caster)
        {
            try
            {
                if (deadPawn == null || caster == null || deadPawn.Map == null)
                    return;

                // Find nearest valid enemy within chain range
                List<Pawn> nearbyPawns = deadPawn.Map.mapPawns.AllPawnsSpawned
                    .Where(p => p != deadPawn 
                        && !p.Dead 
                        && !p.Downed
                        && p.HostileTo(caster)
                        && p.RaceProps.Humanlike
                        && p.Position.DistanceTo(deadPawn.Position) <= ChainRange
                        && p.health.capacities.CapableOf(PawnCapacityDefOf.Consciousness))
                    .OrderBy(p => p.Position.DistanceTo(deadPawn.Position))
                    .ToList();

                if (nearbyPawns.Any())
                {
                    Pawn chainTarget = nearbyPawns.First();
                    
                    // Visual effect for chain
                    FleckMaker.ThrowLightningGlow(deadPawn.DrawPos, deadPawn.Map, 1.5f);
                    FleckMaker.ThrowLightningGlow(chainTarget.DrawPos, chainTarget.Map, 1.5f);

                    // Apply Mind Spike to new target
                    ApplyMindSpike(chainTarget, caster, true);
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
