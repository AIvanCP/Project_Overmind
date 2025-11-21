using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ProjectOvermind
{
    public class Verb_SoulRefill : Verb_CastAbility
    {
        /// <summary>
        /// Forces the ability to target self immediately, bypassing targeting UI.
        /// </summary>
        public override void OrderForceTarget(LocalTargetInfo target)
        {
            ability.QueueCastingJob(new LocalTargetInfo(CasterPawn), new LocalTargetInfo(CasterPawn));
        }

        /// <summary>
        /// Validates that only player faction pawns can use this ability.
        /// </summary>
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (CasterPawn?.Faction != Faction.OfPlayer)
            {
                if (showMessages)
                {
                    Messages.Message("ProjectOvermind_SoulRefill_PlayerOnly".Translate(), MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Casts Soul Refill on all player pawns on the map, applying the need regeneration buff.
        /// </summary>
        protected override bool TryCastShot()
        {
            if (CasterPawn == null || CasterPawn.Faction != Faction.OfPlayer || CasterPawn.Map == null)
            {
                return false;
            }

            // Get hediff def
            HediffDef soulRefillDef = DefDatabase<HediffDef>.GetNamed("ProjectOvermind_SoulRefillBuff", false);
            if (soulRefillDef == null)
            {
                Log.Error("[ProjectOvermind] Soul Refill hediff def not found!");
                return false;
            }

            // Duration: 120 seconds = 7200 ticks
            int durationTicks = 7200;
            int buffedCount = 0;

            // Get all player-owned pawns on the map
            List<Pawn> playerPawns = CasterPawn.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
            
            foreach (Pawn pawn in playerPawns)
            {
                if (pawn == null || pawn.Dead || !pawn.RaceProps.Humanlike)
                {
                    continue;
                }

                // Apply or refresh buff
                Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(soulRefillDef);
                if (existingHediff != null)
                {
                    // Refresh duration if already active
                    HediffComp_Disappears disappearComp = existingHediff.TryGetComp<HediffComp_Disappears>();
                    if (disappearComp != null)
                    {
                        disappearComp.ticksToDisappear = durationTicks;
                        buffedCount++;
                    }
                }
                else
                {
                    // Apply new hediff
                    Hediff hediff = HediffMaker.MakeHediff(soulRefillDef, pawn);
                    HediffComp_Disappears comp = hediff.TryGetComp<HediffComp_Disappears>();
                    if (comp != null)
                    {
                        comp.ticksToDisappear = durationTicks;
                    }
                    pawn.health.AddHediff(hediff);
                    buffedCount++;
                }
            }

            // Success feedback
            Messages.Message(
                $"Soul Refill: {buffedCount} colonist{(buffedCount == 1 ? "" : "s")} sustained!",
                CasterPawn,
                MessageTypeDefOf.PositiveEvent,
                true
            );

            // Visual effects
            FleckMaker.Static(CasterPawn.Position, CasterPawn.Map, FleckDefOf.PsycastAreaEffect, 2f);
            
            return true;
        }
    }
}
