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
    /// Verb for "Inspiration" psycast - applies global buff to all player pawns on map
    /// Provides massive productivity bonuses scaled by psychic sensitivity
    /// </summary>
    public class Verb_Inspiration : Verb_CastAbility
    {
        private const int BuffDurationTicks = 3600; // 60 seconds
        private static readonly HediffDef InspirationHediffDef = HediffDef.Named("ProjectOvermind_InspirationAura");

        /// <summary>
        /// Override to prevent targeting UI and cast immediately on self
        /// This is called when the ability button is clicked
        /// </summary>
        public override void OrderForceTarget(LocalTargetInfo target)
        {
            Log.Message("[Inspiration] OrderForceTarget called - executing immediately without targeting");
            
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
            Log.Message("[Inspiration] TryCastShot called");
            try
            {
                if (CasterPawn == null || CasterPawn.Map == null)
                {
                    Log.Warning("[Inspiration] Failed: No valid map");
                    Messages.Message("Inspiration failed: No valid map.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                Log.Message($"[Inspiration] Executing on map {CasterPawn.Map}");

                // Get all player-owned pawns on the map
                List<Pawn> playerPawns = GetPlayerPawnsOnMap();

                if (playerPawns.Count == 0)
                {
                    Log.Message("[Inspiration] No colonists found");
                    Messages.Message("Inspiration: No colonists found on map.", MessageTypeDefOf.NeutralEvent, false);
                    return true; // Still counts as successful cast (cooldown applies)
                }

                int buffedCount = 0;

                // Apply Inspiration buff to all player pawns
                foreach (Pawn pawn in playerPawns)
                {
                    if (ApplyInspirationBuff(pawn))
                    {
                        buffedCount++;
                    }
                }

                // Success feedback
                Messages.Message(
                    $"Inspiration: {buffedCount} colonist{(buffedCount == 1 ? "" : "s")} inspired!", 
                    CasterPawn, 
                    MessageTypeDefOf.PositiveEvent, 
                    true
                );

                // Visual effect at caster position
                FleckMaker.Static(CasterPawn.Position, CasterPawn.Map, FleckDefOf.PsycastAreaEffect, 5f);

                // Play sound effect
                SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(CasterPawn));

                Log.Message($"[Inspiration] Buffed {buffedCount} pawns on map");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Inspiration] Error in TryCastShot: {ex}");
                Messages.Message("Inspiration failed due to an error.", MessageTypeDefOf.RejectInput, false);
                return false;
            }
        }

        /// <summary>
        /// Get all player-owned pawns on the current map
        /// </summary>
        private List<Pawn> GetPlayerPawnsOnMap()
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
                    // Only buff player faction pawns
                    if (pawn != null && 
                        !pawn.Dead && 
                        !pawn.Downed &&
                        pawn.Faction == Faction.OfPlayer &&
                        pawn.RaceProps != null &&
                        pawn.RaceProps.Humanlike)
                    {
                        result.Add(pawn);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Inspiration] Error getting player pawns: {ex}");
            }

            return result;
        }

        /// <summary>
        /// Apply Inspiration buff to a single pawn
        /// Prevents duplicate hediffs and handles psychic sensitivity scaling
        /// </summary>
        private bool ApplyInspirationBuff(Pawn pawn)
        {
            try
            {
                if (pawn == null || pawn.Dead || pawn.health == null)
                    return false;

                // Check for existing Inspiration buff
                Hediff existingBuff = pawn.health.hediffSet.GetFirstHediffOfDef(InspirationHediffDef);
                if (existingBuff != null)
                {
                    // Refresh duration by accessing the disappears comp
                    HediffComp_Disappears disappearsComp = existingBuff.TryGetComp<HediffComp_Disappears>();
                    if (disappearsComp != null)
                    {
                        disappearsComp.ticksToDisappear = BuffDurationTicks;
                    }
                    
                    if (Prefs.DevMode)
                    {
                        Log.Message($"[Inspiration] Refreshed buff on {pawn.LabelShort}");
                    }
                    
                    return true;
                }

                // Add new Inspiration hediff
                Hediff newBuff = HediffMaker.MakeHediff(InspirationHediffDef, pawn);
                pawn.health.AddHediff(newBuff);

                // Spawn visual effect at pawn position
                if (pawn.Spawned && pawn.Map != null)
                {
                    FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.PsycastAreaEffect, 2f);
                }

                if (Prefs.DevMode)
                {
                    float sensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                    Log.Message($"[Inspiration] Applied buff to {pawn.LabelShort} (sensitivity: {sensitivity:F2}x)");
                }

                return true;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"[Inspiration] Error applying buff to {pawn?.LabelShort}: {ex}");
                }
                return false;
            }
        }
    }
}
