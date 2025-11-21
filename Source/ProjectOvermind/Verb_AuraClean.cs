using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ProjectOvermind
{
    public class Verb_AuraClean : Verb_CastAbility
    {
        /// <summary>
        /// Forces the ability to target self immediately, bypassing targeting UI.
        /// </summary>
        public override void OrderForceTarget(LocalTargetInfo target)
        {
            ability.QueueCastingJob(new LocalTargetInfo(CasterPawn), new LocalTargetInfo(CasterPawn));
        }

        /// <summary>
        /// Always return true for global self-cast validation.
        /// </summary>
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (CasterPawn?.Faction != Faction.OfPlayer)
            {
                if (showMessages)
                {
                    Messages.Message("ProjectOvermind_AuraClean_PlayerOnly".Translate(), MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Casts Aura Clean on all player pawns on the map, applying the filth-cleaning aura buff.
        /// </summary>
        protected override bool TryCastShot()
        {
            if (CasterPawn == null || CasterPawn.Faction != Faction.OfPlayer || CasterPawn.Map == null)
            {
                return false;
            }

            // Get hediff def
            HediffDef auraCleanDef = DefDatabase<HediffDef>.GetNamed("ProjectOvermind_AuraCleanBuff", false);
            if (auraCleanDef == null)
            {
                Log.Error("[ProjectOvermind] Aura Clean hediff def not found!");
                return false;
            }

            // Duration: 45 seconds = 2700 ticks
            int durationTicks = 2700;
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
                Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(auraCleanDef);
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
                    Hediff hediff = HediffMaker.MakeHediff(auraCleanDef, pawn);
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
                $"Aura Clean: {buffedCount} colonist{(buffedCount == 1 ? "" : "s")} protected!",
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
