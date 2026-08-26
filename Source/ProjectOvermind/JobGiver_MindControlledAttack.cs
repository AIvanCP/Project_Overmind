using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ProjectOvermind
{
    /// <summary>
    /// Job giver for Mind Spike victims.
    ///
    /// WHY THIS EXISTS
    /// ---------------
    /// Mind Spike used to put the victim into vanilla MentalStateDefOf.Berserk.
    /// Vanilla RimWorld's JobGiver_Berserk.TryGiveJob hardcodes JobDefOf.AttackMelee -
    /// it never looks at the pawn's equipped weapon. That is why a mind-controlled
    /// archer or pistol user would walk up to their own ally and throw punches
    /// instead of shooting.
    ///
    /// This job giver picks a target from the victim's OWN faction and then issues
    /// the job that matches whatever weapon they are actually holding:
    ///   - ranged weapon  -> JobDefOf.AttackStatic (they shoot from where they are)
    ///   - melee / unarmed -> JobDefOf.AttackMelee (unchanged behaviour)
    ///
    /// It is wired in through the ProjectOvermind_MindControlled mental state, which
    /// is added to the MentalStateCritical think tree by Patches/ThinkTree_MindControl.xml.
    /// </summary>
    public class JobGiver_MindControlledAttack : ThinkNode_JobGiver
    {
        /// <summary>How far the victim will look for someone of their own faction to attack.</summary>
        private const float MaxTargetSearchRadius = 40f;

        /// <summary>Re-evaluate reasonably often so they keep switching to live targets.</summary>
        private const int JobExpiryTicks = 240;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null || !pawn.Spawned || pawn.Map == null)
            {
                return null;
            }

            Pawn target = FindTraitorTarget(pawn);
            if (target == null)
            {
                return null;
            }

            // Ask the pawn for the verb it would really use against this target.
            // allowManualCastWeapons: false  - do not try to fire manually-aimed
            //                                  weapons (mortars etc.) while raving.
            // allowTurrets: false            - never pick a turret verb.
            Verb verb = pawn.TryGetAttackVerb(target, false, false);

            if (verb == null || verb.IsMeleeAttack)
            {
                // No usable ranged verb -> behave exactly like vanilla berserk.
                Job meleeJob = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
                meleeJob.maxNumMeleeAttacks = 1;
                meleeJob.expiryInterval = JobExpiryTicks;
                meleeJob.checkOverrideOnExpire = true;
                meleeJob.killIncappedTarget = false;
                return meleeJob;
            }

            // Ranged weapon: shoot from the current position rather than closing in.
            Job shootJob = JobMaker.MakeJob(JobDefOf.AttackStatic, target);
            shootJob.maxNumStaticAttacks = 2;
            shootJob.expiryInterval = JobExpiryTicks;
            shootJob.checkOverrideOnExpire = true;
            shootJob.endIfCantShootTargetFromCurPos = true;
            shootJob.killIncappedTarget = false;
            return shootJob;
        }

        /// <summary>
        /// Finds the closest valid pawn belonging to the victim's own faction.
        /// Falls back to any nearby non-player pawn if the victim has no faction,
        /// so faction-less raiders and animals still turn on whoever is next to them.
        /// </summary>
        private static Pawn FindTraitorTarget(Pawn pawn)
        {
            Faction ownFaction = pawn.Faction;
            Pawn best = null;
            float bestDistSq = MaxTargetSearchRadius * MaxTargetSearchRadius;

            // AllPawnsSpawned is IReadOnlyList<Pawn> in RimWorld 1.6 (was List<Pawn> before).
            IReadOnlyList<Pawn> candidates = pawn.Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < candidates.Count; i++)
            {
                Pawn other = candidates[i];

                if (other == pawn) continue;
                if (other.Dead || other.Downed) continue;

                // Other mind-controlled pawns stay valid targets on purpose - that is
                // what makes a spiked group tear itself apart.

                if (ownFaction != null)
                {
                    if (other.Faction != ownFaction) continue;
                }
                else
                {
                    // Faction-less victim: attack anything that is not the player's.
                    if (other.Faction != null && other.Faction.IsPlayer) continue;
                }

                float distSq = (other.Position - pawn.Position).LengthHorizontalSquared;
                if (distSq > bestDistSq) continue;

                if (!pawn.CanReach(other, PathEndMode.Touch, Danger.Deadly)
                    && !GenSight.LineOfSight(pawn.Position, other.Position, pawn.Map))
                {
                    // Neither reachable for melee nor visible for shooting.
                    continue;
                }

                best = other;
                bestDistSq = distSq;
            }

            return best;
        }
    }
}
