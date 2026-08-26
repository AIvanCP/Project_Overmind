using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace ProjectOvermind
{
    public class Hediff_MindSpikeControlled : HediffWithComps
    {
        public Pawn casterPawn;
        public bool hasChained = false;
        private bool mentalStateApplied = false;

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            
            try
            {
                // Apply berserk mental state that targets allies
                ApplyBerserkState();
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Spike] Error in PostAdd: {ex}");
            }
        }

        public override void Tick()
        {
            base.Tick();

            try
            {
                // Ensure mental state stays active
                if (!mentalStateApplied && pawn != null && !pawn.Dead)
                {
                    ApplyBerserkState();
                }

                // Check if hediff is about to expire
                if (ShouldRemove)
                {
                    OnRemoved();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Spike] Error in Tick: {ex}");
            }
        }

        private void ApplyBerserkState()
        {
            try
            {
                if (pawn == null || pawn.Dead || pawn.InMentalState)
                    return;

                // Use our own mental state, NOT vanilla Berserk.
                //
                // Vanilla Berserk routes through JobGiver_Berserk, whose TryGiveJob
                // hardcodes JobDefOf.AttackMelee and never looks at the equipped
                // weapon. That is why spiked archers and gunners used to walk up and
                // punch their allies. ProjectOvermind_MindControlled is routed to
                // JobGiver_MindControlledAttack instead (see
                // Patches/ThinkTree_MindControl.xml), which shoots with ranged weapons
                // and only melees when the pawn has no usable ranged verb.
                //
                // Falls back to Berserk if the def somehow failed to load, so the
                // ability still does something rather than silently doing nothing.
                MentalStateDef controlDef =
                    DefDatabase<MentalStateDef>.GetNamedSilentFail("ProjectOvermind_MindControlled")
                    ?? MentalStateDefOf.Berserk;

                if (controlDef != null && pawn.mindState != null)
                {
                    bool success = pawn.mindState.mentalStateHandler.TryStartMentalState(
                        controlDef,
                        null,
                        false,
                        false
                    );

                    if (success)
                    {
                        mentalStateApplied = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Spike] Error in ApplyBerserkState: {ex}");
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            OnRemoved();
        }

        private void OnRemoved()
        {
            try
            {
                if (pawn == null || pawn.Dead)
                {
                    // If pawn died while controlled, try to chain
                    if (casterPawn != null && !hasChained)
                    {
                        Verb_MindSpike.TryChainToNearbyEnemy(pawn, casterPawn);
                    }
                    return;
                }

                // Remove mental state. Must accept BOTH our own state and vanilla
                // Berserk, because the fallback in ApplyBerserkState can still start
                // Berserk, and saves made before this change hold Berserk victims.
                if (pawn.InMentalState)
                {
                    MentalStateDef cur = pawn.MentalStateDef;
                    MentalStateDef controlDef =
                        DefDatabase<MentalStateDef>.GetNamedSilentFail("ProjectOvermind_MindControlled");

                    if (cur == MentalStateDefOf.Berserk || (controlDef != null && cur == controlDef))
                    {
                        pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
                    }
                }

                // Apply disorientation debuff
                Hediff disorientedHediff = HediffMaker.MakeHediff(
                    HediffDefOf.ProjectOvermind_MindSpikeDisoriented, 
                    pawn
                );
                pawn.health.AddHediff(disorientedHediff);

                // Visual feedback
                MoteMaker.ThrowText(pawn.DrawPos + UnityEngine.Vector3.up, pawn.Map, "Released", UnityEngine.Color.white, 3f);
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Spike] Error in OnRemoved: {ex}");
            }
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);

            try
            {
                // Chain effect on death
                if (casterPawn != null && !hasChained && pawn != null && pawn.Corpse != null)
                {
                    Verb_MindSpike.TryChainToNearbyEnemy(pawn, casterPawn);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Spike] Error in Notify_PawnDied: {ex}");
            }
        }

        /// <summary>
        /// Show duration in brackets on health tab
        /// </summary>
        public override string LabelInBrackets
        {
            get
            {
                HediffComp_Disappears comp = this.TryGetComp<HediffComp_Disappears>();
                if (comp != null && comp.ticksToDisappear > 0)
                {
                    return DurationHelper.GetDurationString(comp.ticksToDisappear);
                }
                return null;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref casterPawn, "casterPawn");
            Scribe_Values.Look(ref hasChained, "hasChained", false);
            Scribe_Values.Look(ref mentalStateApplied, "mentalStateApplied", false);
        }
    }
}
