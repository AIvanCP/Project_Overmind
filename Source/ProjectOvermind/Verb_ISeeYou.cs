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
    /// Verb for "I See You" psycast - reveals all hidden entities on the map
    /// </summary>
    public class Verb_ISeeYou : Verb_CastAbility
    {
        private const int RevealDurationTicks = 3600; // 60 seconds

        /// <summary>
        /// Override to prevent targeting UI and cast immediately on self
        /// This is called when the ability button is clicked
        /// </summary>
        public override void OrderForceTarget(LocalTargetInfo target)
        {
            Log.Message("[I See You] OrderForceTarget called - executing immediately without targeting");
            
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
            Log.Message("[I See You] TryCastShot called");
            try
            {
                if (CasterPawn == null || CasterPawn.Map == null)
                {
                    Log.Warning("[I See You] Failed: No valid map");
                    Messages.Message("I See You failed: No valid map.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                Log.Message($"[I See You] Executing on map {CasterPawn.Map}");

                // Start reveal effect and get hostile count
                int hostileCount = 0;
                MapComponent_ISeeYou component = CasterPawn.Map.GetComponent<MapComponent_ISeeYou>();
                if (component != null)
                {
                    hostileCount = component.StartReveal(CasterPawn, RevealDurationTicks);
                    Log.Message($"[I See You] Component found, hostile count: {hostileCount}");
                }
                else
                {
                    Log.Warning("[I See You] MapComponent_ISeeYou not found on map - reveal will not work.");
                    Messages.Message("I See You: Component missing, effect inactive.", MessageTypeDefOf.RejectInput, false);
                }

                // Play alert sound ONLY if hostile invisible entities detected
                if (hostileCount > 0)
                {
                    PlayAlertSound();
                    
                    // Success message with count
                    Messages.Message(
                        $"I See You: {hostileCount} hostile hidden {(hostileCount == 1 ? "entity" : "entities")} revealed!", 
                        CasterPawn, 
                        MessageTypeDefOf.PositiveEvent, 
                        true
                    );
                }
                else
                {
                    // No hostile entities found - silent feedback
                    Messages.Message(
                        "I See You: No hostile hidden entities detected.", 
                        CasterPawn, 
                        MessageTypeDefOf.NeutralEvent, 
                        false
                    );
                }

                // Visual feedback (always show psycast effect)
                FleckMaker.Static(CasterPawn.Position, CasterPawn.Map, FleckDefOf.PsycastAreaEffect, 3f);

                Log.Message("[I See You] Cast completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[I See You] Error in TryCastShot: {ex}");
                Messages.Message("I See You failed due to an error.", MessageTypeDefOf.RejectInput, false);
                return false;
            }
        }

        private void PlayAlertSound()
        {
            try
            {
                SoundDef alertSound = DefDatabase<SoundDef>.GetNamedSilentFail("ProjectOvermind_ISeeYou_Alert");
                if (alertSound != null)
                {
                    alertSound.PlayOneShot(new TargetInfo(CasterPawn.Position, CasterPawn.Map));
                }
                else
                {
                    // Fallback to vanilla sound
                    if (Prefs.DevMode)
                    {
                        Log.Warning("[I See You] Custom alert sound not found, using fallback.");
                    }
                    SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(CasterPawn.Position, CasterPawn.Map));
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[I See You] Error playing sound: {ex}");
            }
        }
    }
}
