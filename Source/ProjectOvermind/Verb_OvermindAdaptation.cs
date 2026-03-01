using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Verb for "Overmind Adaptation" psycast.
    /// Global self-cast (like Inspiration) — applies to ALL player colonists and the caster.
    /// Effect strength (terrain reduction, environmental immunity tiers) depends on CASTER's
    /// PsychicSensitivity at the moment of casting.
    /// </summary>
    public class Verb_OvermindAdaptation : Verb_CastAbility
    {
        private static readonly HediffDef AdaptationHediffDef =
            HediffDef.Named("ProjectOvermind_OvermindAdaptation");

        // ── Targeting bypass ─────────────────────────────────────────────────

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            return true; // Self-cast, no targeting needed
        }

        public override void OrderForceTarget(LocalTargetInfo target)
        {
            if (ability != null && CasterPawn != null)
                ability.QueueCastingJob(CasterPawn, CasterPawn);
        }

        // ── Cast logic ───────────────────────────────────────────────────────

        protected override bool TryCastShot()
        {
            try
            {
                if (CasterPawn == null || CasterPawn.Map == null)
                {
                    Messages.Message("Overmind Adaptation failed: No valid map.",
                        MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                if (AdaptationHediffDef == null)
                {
                    Log.Error("[OvermindAdaptation] HediffDef 'ProjectOvermind_OvermindAdaptation' not found!");
                    return false;
                }

                float casterSensitivity = CasterPawn.GetStatValue(StatDefOf.PsychicSensitivity);
                int durationTicks = DurationHelper.CalculateDuration(CasterPawn);

                List<Pawn> targets = GetPlayerPawnsOnMap();
                if (targets.Count == 0)
                {
                    Messages.Message("Overmind Adaptation: No colonists found on map.",
                        MessageTypeDefOf.NeutralEvent, false);
                    return true;
                }

                int affected = 0;
                foreach (Pawn pawn in targets)
                {
                    if (ApplyAdaptation(pawn, casterSensitivity, durationTicks))
                        affected++;
                }

                // Feedback
                string sensitivityTier = GetSensitivityTierLabel(casterSensitivity);
                Messages.Message(
                    $"Overmind Adaptation ({sensitivityTier}): {affected} colonist{(affected == 1 ? "" : "s")} adapted!",
                    CasterPawn, MessageTypeDefOf.PositiveEvent, true);

                // Visuals / sound
                FleckMaker.Static(CasterPawn.Position, CasterPawn.Map, FleckDefOf.PsycastAreaEffect, 6f);
                SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(CasterPawn));

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[OvermindAdaptation] Error in TryCastShot: {ex}");
                Messages.Message("Overmind Adaptation failed due to an error.",
                    MessageTypeDefOf.RejectInput, false);
                return false;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private List<Pawn> GetPlayerPawnsOnMap()
        {
            var result = new List<Pawn>();
            try
            {
                if (CasterPawn?.Map == null) return result;

                foreach (Pawn p in CasterPawn.Map.mapPawns.AllPawnsSpawned)
                {
                    if (p == null || p.Dead || p.Downed) continue;
                    if (!p.RaceProps.Humanlike) continue;
                    if (p.Faction != Faction.OfPlayer) continue;
                    result.Add(p);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[OvermindAdaptation] Error collecting pawns: {ex}");
            }
            return result;
        }

        private bool ApplyAdaptation(Pawn pawn, float casterSensitivity, int durationTicks)
        {
            try
            {
                if (pawn == null || pawn.Dead || pawn.health == null)
                    return false;

                // Refresh existing hediff if present
                Hediff existing = pawn.health.hediffSet?.GetFirstHediffOfDef(AdaptationHediffDef);
                if (existing != null)
                {
                    HediffComp_Disappears comp = existing.TryGetComp<HediffComp_Disappears>();
                    if (comp != null)
                        comp.ticksToDisappear = durationTicks;

                    // Refresh caster sensitivity (caster may have changed gear/traits)
                    if (existing is Hediff_OvermindAdaptation adaptHediff)
                        adaptHediff.InitializeCasterSensitivity(casterSensitivity);

                    return true;
                }

                // Add new hediff
                Hediff newHediff = HediffMaker.MakeHediff(AdaptationHediffDef, pawn);
                pawn.health.AddHediff(newHediff);

                // Set duration AFTER adding (PostAdd initializes comps)
                HediffComp_Disappears disappearsComp = newHediff.TryGetComp<HediffComp_Disappears>();
                if (disappearsComp != null)
                    disappearsComp.ticksToDisappear = durationTicks;

                // Initialize sensitivity
                if (newHediff is Hediff_OvermindAdaptation newAdapt)
                    newAdapt.InitializeCasterSensitivity(casterSensitivity);

                // Small visual cue on each affected pawn
                if (pawn.Spawned && pawn.Map != null)
                    FleckMaker.ThrowDustPuffThick(pawn.DrawPos, pawn.Map, 0.8f,
                        new Color(0.4f, 0.8f, 1.0f));

                return true;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                    Log.Error($"[OvermindAdaptation] Error applying to {pawn?.LabelShort}: {ex}");
                return false;
            }
        }

        private static string GetSensitivityTierLabel(float sensitivity)
        {
            if (sensitivity >= 8.0f) return "Apex";
            if (sensitivity >= 5.0f) return "Void";
            if (sensitivity >= 3.0f) return "Refined";
            return "Basic";
        }
    }
}
