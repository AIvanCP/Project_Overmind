using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Verb for "Hallucination" psycast - applies global debuff to all hostile pawns/animals on map
    /// Does NOT affect mechanoids
    /// </summary>
    public class Verb_Hallucination : Verb_CastAbility
    {
        private const int DebuffDurationTicks = 2400; // 40 seconds
        private static readonly HediffDef HallucinationHediffDef = HediffDef.Named("ProjectOvermind_Hallucination");

        /// <summary>
        /// Override to prevent targeting UI and cast immediately on self
        /// This is called when the ability button is clicked
        /// </summary>
        public override void OrderForceTarget(LocalTargetInfo target)
        {
            
            
            // Cast immediately on caster without showing targeting UI
            if (CasterPawn != null)
            {
                ability.QueueCastingJob(CasterPawn, CasterPawn);
            }
        }

        /// <summary>
        /// Override to always return available (no target validation needed)
        /// </summary>
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            // Always return true - this is a self-cast ability
            return true;
        }

        protected override bool TryCastShot()
        {
            
            try
            {
                if (CasterPawn == null || CasterPawn.Map == null)
                {
                    Log.Warning("[Hallucination] Failed: No valid map");
                    Messages.Message("Hallucination failed: No valid map.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                

                // Get all hostile pawns and animals on the map (not mechs)
                List<Pawn> hostilePawns = GetHostilePawnsOnMap();

                if (hostilePawns.Count == 0)
                {
                    
                    Messages.Message("Hallucination: No hostile creatures found on map.", MessageTypeDefOf.NeutralEvent, false);
                    return true; // Still counts as successful cast
                }

                int debuffedCount = 0;

                // Apply Hallucination debuff to all hostile pawns
                foreach (Pawn pawn in hostilePawns)
                {
                    if (ApplyHallucinationDebuff(pawn))
                    {
                        debuffedCount++;
                    }
                }

                // Success feedback
                Messages.Message(
                    $"Hallucination: {debuffedCount} hostile creature{(debuffedCount == 1 ? "" : "s")} afflicted!", 
                    CasterPawn, 
                    MessageTypeDefOf.PositiveEvent, 
                    true
                );

                // Visual effect at caster position
                FleckMaker.Static(CasterPawn.Position, CasterPawn.Map, FleckDefOf.PsycastAreaEffect, 5f);

                // Play sound effect
                SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(CasterPawn));

                
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Hallucination] Error in TryCastShot: {ex}");
                Messages.Message("Hallucination failed due to an error.", MessageTypeDefOf.RejectInput, false);
                return false;
            }
        }

        /// <summary>
        /// Get all hostile pawns and animals on the current map (excluding mechanoids)
        /// </summary>
        private List<Pawn> GetHostilePawnsOnMap()
        {
            List<Pawn> result = new List<Pawn>();

            try
            {
                if (CasterPawn?.Map == null)
                    return result;

                // Get all spawned pawns on the map
                IEnumerable<Pawn> allPawns = CasterPawn.Map.mapPawns.AllPawnsSpawned;
                
                foreach (Pawn pawn in allPawns)
                {
                    if (pawn == null || pawn.Dead || pawn.Downed)
                        continue;

                    // Skip mechanoids
                    if (pawn.RaceProps != null && pawn.RaceProps.IsMechanoid)
                        continue;

                    // Only affect hostile pawns (hostile to player faction)
                    if (pawn.HostileTo(Faction.OfPlayer))
                    {
                        result.Add(pawn);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Hallucination] Error getting hostile pawns: {ex}");
            }

            return result;
        }

        /// <summary>
        /// Apply Hallucination debuff to a single pawn
        /// Prevents duplicate hediffs
        /// </summary>
        private bool ApplyHallucinationDebuff(Pawn pawn)
        {
            try
            {
                if (pawn == null || pawn.Dead || pawn.health == null)
                    return false;

                // Check for existing Hallucination debuff
                Hediff existingDebuff = pawn.health.hediffSet.GetFirstHediffOfDef(HallucinationHediffDef);
                if (existingDebuff != null)
                {
                    // Refresh duration by accessing the disappears comp
                    HediffComp_Disappears disappearsComp = existingDebuff.TryGetComp<HediffComp_Disappears>();
                    if (disappearsComp != null)
                    {
                        disappearsComp.ticksToDisappear = DebuffDurationTicks;
                    }
                    
                    if (Prefs.DevMode)
                    {
                        
                    }
                    
                    return true;
                }

                // Add new Hallucination hediff
                Hediff newDebuff = HediffMaker.MakeHediff(HallucinationHediffDef, pawn);
                pawn.health.AddHediff(newDebuff);

                // Spawn visual effect at pawn position
                if (pawn.Spawned && pawn.Map != null)
                {
                    FleckMaker.ThrowDustPuffThick(pawn.DrawPos, pawn.Map, 1f, new Color(0.8f, 0.3f, 0.8f));
                }

                if (Prefs.DevMode)
                {
                    
                }

                return true;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"[Hallucination] Error applying debuff to {pawn?.LabelShort}: {ex}");
                }
                return false;
            }
        }
    }
}
