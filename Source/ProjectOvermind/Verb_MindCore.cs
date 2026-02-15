using System;
using RimWorld;
using Verse;
using Verse.Sound;

namespace ProjectOvermind
{
    /// <summary>
    /// Verb for "Mind Core" psycast - applies a scalable buff that enhances psycast performance
    /// Improves psyfocus regeneration, max entropy, reduces psyfocus cost and cooldowns
    /// Works with all psycasts including modded ones
    /// </summary>
    public class Verb_MindCore : Verb_CastAbility
    {
        private static readonly HediffDef MindCoreHediffDef = HediffDef.Named("ProjectOvermind_MindCore");

        /// <summary>
        /// Validate target - must be pawn, humanlike, from same faction, and capable of psycasting
        /// </summary>
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!base.ValidateTarget(target, showMessages))
                return false;

            // Must be a pawn
            if (target.Thing == null || !(target.Thing is Pawn targetPawn))
            {
                if (showMessages)
                    Messages.Message("Must target a pawn.", MessageTypeDefOf.RejectInput, false);
                return false;
            }

            // Must be humanlike
            if (!targetPawn.RaceProps.Humanlike)
            {
                if (showMessages)
                    Messages.Message("Target must be humanlike.", MessageTypeDefOf.RejectInput, false);
                return false;
            }

            // Must be from same faction as caster
            if (CasterPawn != null && targetPawn.Faction != CasterPawn.Faction)
            {
                if (showMessages)
                    Messages.Message("Can only target pawns from your faction.", MessageTypeDefOf.RejectInput, false);
                return false;
            }

            // Must have psychic sensitivity > 0 (can use psycasts)
            float sensitivity = targetPawn.GetStatValue(StatDefOf.PsychicSensitivity);
            if (sensitivity <= 0f)
            {
                if (showMessages)
                    Messages.Message($"{targetPawn.LabelShort} cannot use psycasts (psychic sensitivity = 0).", MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }

        protected override bool TryCastShot()
        {
            try
            {
                if (CasterPawn == null)
                {
                    Log.Warning("[Mind Core] Cast failed: No caster pawn.");
                    return false;
                }

                // Get target pawn
                Pawn targetPawn = currentTarget.Thing as Pawn;
                if (targetPawn == null)
                {
                    Messages.Message("Mind Core failed: Invalid target.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                // Calculate duration based on caster's psychic sensitivity
                int duration = CalculateMindCoreDuration(CasterPawn);

                // Check if target already has the buff
                Hediff existingHediff = targetPawn.health.hediffSet.GetFirstHediffOfDef(MindCoreHediffDef);
                if (existingHediff != null)
                {
                    // Refresh duration instead of stacking
                    HediffComp_Disappears disappearsComp = existingHediff.TryGetComp<HediffComp_Disappears>();
                    if (disappearsComp != null)
                    {
                        disappearsComp.ticksToDisappear = duration;
                    }
                    
                    Messages.Message(
                        $"Mind Core refreshed on {targetPawn.LabelShort} ({DurationHelper.GetDurationString(duration)}).",
                        targetPawn,
                        MessageTypeDefOf.NeutralEvent,
                        true
                    );
                }
                else
                {
                    // Apply new hediff
                    Hediff_MindCore hediff = (Hediff_MindCore)HediffMaker.MakeHediff(MindCoreHediffDef, targetPawn);
                    hediff.InitializeCasterSensitivity(CasterPawn.GetStatValue(StatDefOf.PsychicSensitivity));
                    
                    // Set duration
                    HediffComp_Disappears comp = hediff.TryGetComp<HediffComp_Disappears>();
                    if (comp != null)
                    {
                        comp.ticksToDisappear = duration;
                    }

                    targetPawn.health.AddHediff(hediff);

                    Messages.Message(
                        $"Mind Core applied to {targetPawn.LabelShort} ({DurationHelper.GetDurationString(duration)}).",
                        targetPawn,
                        MessageTypeDefOf.PositiveEvent,
                        true
                    );
                }

                // Visual and audio feedback
                FleckMaker.Static(targetPawn.Position, targetPawn.Map, FleckDefOf.PsycastAreaEffect, 2f);
                SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(targetPawn));

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Core] Error in TryCastShot: {ex}");
                Messages.Message("Mind Core failed due to an error.", MessageTypeDefOf.RejectInput, false);
                return false;
            }
        }

        /// <summary>
        /// Calculate Mind Core duration based on caster's psychic sensitivity
        /// Base: 3 hours (7500 ticks), +0.5 hour per 0.1 sensitivity
        /// </summary>
        private int CalculateMindCoreDuration(Pawn caster)
        {
            if (caster == null)
                return 7500; // 3 hours base

            float sensitivity = caster.GetStatValue(StatDefOf.PsychicSensitivity);

            // Base: 3 hours = 7500 ticks
            // Scaling: +0.5 hour (1250 ticks) per 0.1 sensitivity
            int bonusTicks = (int)((sensitivity / 0.1f) * 1250f);

            return 7500 + bonusTicks;
        }
    }
}
