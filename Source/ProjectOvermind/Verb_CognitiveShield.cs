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
    /// Verb for "Cognitive Shield" psycast - applies defensive buff to all player pawns
    /// Provides mental protection and damage reduction scaled by psychic sensitivity
    /// </summary>
    public class Verb_CognitiveShield : Verb_CastAbility
    {
        private const int BuffDurationTicks = 1500; // 25 seconds
        private static readonly HediffDef CognitiveShieldHediffDef = HediffDef.Named("ProjectOvermind_CognitiveShield");

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
                    Log.Warning("[Cognitive Shield] Failed: No valid map");
                    Messages.Message("Cognitive Shield failed: No valid map.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                

                // Get all player-owned pawns on the map
                List<Pawn> playerPawns = GetPlayerPawnsOnMap();

                if (playerPawns.Count == 0)
                {
                    
                    Messages.Message("Cognitive Shield: No colonists found on map.", MessageTypeDefOf.NeutralEvent, false);
                    return true; // Still counts as successful cast
                }

                int buffedCount = 0;

                // Apply Cognitive Shield buff to all player pawns
                foreach (Pawn pawn in playerPawns)
                {
                    if (ApplyCognitiveShieldBuff(pawn))
                    {
                        buffedCount++;
                    }
                }

                // Success feedback
                Messages.Message($"Cognitive Shield activated! {buffedCount} colonists protected.", 
                    new LookTargets(playerPawns.Select(p => new TargetInfo(p)).ToList()), 
                    MessageTypeDefOf.PositiveEvent, false);

                // Sound effect
                SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(CasterPawn));

                
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Cognitive Shield cast failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        private List<Pawn> GetPlayerPawnsOnMap()
        {
            List<Pawn> result = new List<Pawn>();

            try
            {
                foreach (Pawn pawn in CasterPawn.Map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn == null || pawn.Dead || pawn.Destroyed) continue;
                    if (!pawn.IsColonist && pawn.Faction != Faction.OfPlayer) continue;
                    if (!pawn.RaceProps.Humanlike) continue; // Only humanlike pawns

                    result.Add(pawn);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] GetPlayerPawnsOnMap error: {ex.Message}");
            }

            return result;
        }

        private bool ApplyCognitiveShieldBuff(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || pawn.health == null) return false;

            try
            {
                // Check if pawn already has the buff
                Hediff existingBuff = pawn.health.hediffSet.GetFirstHediffOfDef(CognitiveShieldHediffDef);

                if (existingBuff != null)
                {
                    // Refresh duration instead of stacking
                    HediffComp_Disappears comp = existingBuff.TryGetComp<HediffComp_Disappears>();
                    if (comp != null)
                    {
                        comp.ticksToDisappear = BuffDurationTicks;
                    }
                }
                else
                {
                    // Add new buff
                    Hediff newBuff = HediffMaker.MakeHediff(CognitiveShieldHediffDef, pawn);
                    pawn.health.AddHediff(newBuff);
                }

                // Visual effect
                FleckMaker.AttachedOverlay(pawn, FleckDefOf.PsycastSkipInnerExit, Vector3.zero, 1f);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] ApplyCognitiveShieldBuff error for {pawn?.LabelShort}: {ex.Message}");
                return false;
            }
        }
    }
}
