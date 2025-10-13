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

                // Use vanilla berserk but we'll handle it specially
                MentalStateDef berserkDef = MentalStateDefOf.Berserk;
                
                if (berserkDef != null && pawn.mindState != null)
                {
                    // Force berserk state
                    bool success = pawn.mindState.mentalStateHandler.TryStartMentalState(
                        berserkDef,
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

                // Remove mental state
                if (pawn.InMentalState && pawn.MentalStateDef == MentalStateDefOf.Berserk)
                {
                    pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref casterPawn, "casterPawn");
            Scribe_Values.Look(ref hasChained, "hasChained", false);
            Scribe_Values.Look(ref mentalStateApplied, "mentalStateApplied", false);
        }
    }
}
