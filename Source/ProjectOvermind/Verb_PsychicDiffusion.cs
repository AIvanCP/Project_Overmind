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
    /// Verb for "Psychic Diffusion" psycast - applies global buff to all player pawns AND debuff to all enemies
    /// Provides movement, work speed, and healing spread scaled by psychic sensitivity (player)
    /// Applies combat penalties to hostile pawns (enemies)
    /// </summary>
    public class Verb_PsychicDiffusion : Verb_CastAbility
    {
        private const int BuffDurationTicks = 1200; // 20 seconds
        private static readonly HediffDef PsychicDiffusionBuffHediffDef = HediffDef.Named("ProjectOvermind_PsychicDiffusion");
        private static readonly HediffDef PsychicDiffusionDebuffHediffDef = HediffDef.Named("ProjectOvermind_PsychicDiffusionDebuff");

        /// <summary>
        /// Override to prevent targeting UI and cast immediately on self
        /// This is called when the ability button is clicked
        /// </summary>
        public override void OrderForceTarget(LocalTargetInfo target)
        {
            
            
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
            
            try
            {
                if (CasterPawn == null || CasterPawn.Map == null)
                {
                    Log.Warning("[Psychic Diffusion] Failed: No valid map");
                    Messages.Message("Psychic Diffusion failed: No valid map.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                

                // Get all player-owned pawns on the map
                List<Pawn> playerPawns = GetPlayerPawnsOnMap();
                // Get all hostile pawns on the map
                List<Pawn> hostilePawns = GetHostilePawnsOnMap();

                if (playerPawns.Count == 0 && hostilePawns.Count == 0)
                {
                    
                    Messages.Message("Psychic Diffusion: No colonists or enemies found on map.", MessageTypeDefOf.NeutralEvent, false);
                    return true; // Still counts as successful cast
                }

                int buffedCount = 0;
                int debuffedCount = 0;

                // Apply Psychic Diffusion BUFF to all player pawns
                foreach (Pawn pawn in playerPawns)
                {
                    if (ApplyPsychicDiffusionBuff(pawn))
                    {
                        buffedCount++;
                    }
                }

                // Apply Psychic Diffusion DEBUFF to all hostile pawns
                foreach (Pawn pawn in hostilePawns)
                {
                    if (ApplyPsychicDiffusionDebuff(pawn))
                    {
                        debuffedCount++;
                    }
                }

                // Success feedback
                string message = "";
                if (buffedCount > 0 && debuffedCount > 0)
                {
                    message = $"Psychic Diffusion: {buffedCount} colonist{(buffedCount == 1 ? "" : "s")} buffed, {debuffedCount} enem{(debuffedCount == 1 ? "y" : "ies")} debuffed!";
                }
                else if (buffedCount > 0)
                {
                    message = $"Psychic Diffusion: {buffedCount} colonist{(buffedCount == 1 ? "" : "s")} connected to the network.";
                }
                else if (debuffedCount > 0)
                {
                    message = $"Psychic Diffusion: {debuffedCount} enem{(debuffedCount == 1 ? "y" : "ies")} disrupted!";
                }

                if (!string.IsNullOrEmpty(message))
                {
                    Messages.Message(message, 
                        new LookTargets(playerPawns.Select(p => new TargetInfo(p)).ToList()), 
                        MessageTypeDefOf.PositiveEvent, false);
                }

                // Sound effect
                SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(CasterPawn));

                
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Psychic Diffusion cast failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        private List<Pawn> GetPlayerPawnsOnMap()
        {
            List<Pawn> result = new List<Pawn>();

            try
            {
                foreach (Pawn pawn in CasterPawn.Map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn == null || pawn.Dead || pawn.Destroyed) continue;
                    if (!pawn.IsColonist && pawn.Faction != Faction.OfPlayer) continue;
                    if (!pawn.RaceProps.Humanlike) continue; // Only humanlike pawns

                    result.Add(pawn);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] GetPlayerPawnsOnMap error: {ex.Message}");
            }

            return result;
        }

        private bool ApplyPsychicDiffusionBuff(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || pawn.health == null) return false;

            try
            {
                // Check if pawn already has the buff
                Hediff existingBuff = pawn.health.hediffSet.GetFirstHediffOfDef(PsychicDiffusionBuffHediffDef);

                if (existingBuff != null)
                {
                    // Refresh duration instead of stacking
                    HediffComp_Disappears comp = existingBuff.TryGetComp<HediffComp_Disappears>();
                    if (comp != null)
                    {
                        comp.ticksToDisappear = BuffDurationTicks;
                    }
                }
                else
                {
                    // Add new buff
                    Hediff newBuff = HediffMaker.MakeHediff(PsychicDiffusionBuffHediffDef, pawn);
                    pawn.health.AddHediff(newBuff);
                }

                // Visual effect (green aura for allies)
                FleckMaker.AttachedOverlay(pawn, FleckDefOf.PsycastSkipInnerExit, Vector3.zero, 1f);
                FleckMaker.ThrowDustPuffThick(pawn.DrawPos, pawn.Map, 1.5f, new Color(0.6f, 1f, 0.9f));

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] ApplyPsychicDiffusionBuff error for {pawn?.LabelShort}: {ex.Message}");
                return false;
            }
        }

        private List<Pawn> GetHostilePawnsOnMap()
        {
            List<Pawn> result = new List<Pawn>();

            try
            {
                foreach (Pawn pawn in CasterPawn.Map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn == null || pawn.Dead || pawn.Destroyed) continue;
                    if (!pawn.HostileTo(Faction.OfPlayer)) continue; // Only hostile pawns
                    if (pawn.RaceProps.IsMechanoid) continue; // Exclude mechanoids
                    if (!pawn.RaceProps.Humanlike && !pawn.RaceProps.Animal) continue; // Only humanlike or animals

                    result.Add(pawn);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] GetHostilePawnsOnMap error: {ex.Message}");
            }

            return result;
        }

        private bool ApplyPsychicDiffusionDebuff(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || pawn.health == null) return false;

            try
            {
                // Check if pawn already has the debuff
                Hediff existingDebuff = pawn.health.hediffSet.GetFirstHediffOfDef(PsychicDiffusionDebuffHediffDef);

                if (existingDebuff != null)
                {
                    // Refresh duration instead of stacking
                    HediffComp_Disappears comp = existingDebuff.TryGetComp<HediffComp_Disappears>();
                    if (comp != null)
                    {
                        comp.ticksToDisappear = BuffDurationTicks;
                    }
                }
                else
                {
                    // Add new debuff
                    Hediff newDebuff = HediffMaker.MakeHediff(PsychicDiffusionDebuffHediffDef, pawn);
                    pawn.health.AddHediff(newDebuff);
                }

                // Visual effect (purple/dark aura for enemies)
                FleckMaker.ThrowDustPuffThick(pawn.DrawPos, pawn.Map, 1.5f, new Color(0.7f, 0.3f, 0.9f));

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] ApplyPsychicDiffusionDebuff error for {pawn?.LabelShort}: {ex.Message}");
                return false;
            }
        }
    }
}
