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

        protected override bool TryCastShot()
        {
            try
            {
                if (CasterPawn == null || CasterPawn.Map == null)
                {
                    Messages.Message("I See You failed: No valid map.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                // Start reveal effect and get hostile count
                int hostileCount = 0;
                MapComponent_ISeeYou component = CasterPawn.Map.GetComponent<MapComponent_ISeeYou>();
                if (component != null)
                {
                    hostileCount = component.StartReveal(CasterPawn, RevealDurationTicks);
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
