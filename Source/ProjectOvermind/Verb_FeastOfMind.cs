using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ProjectOvermind
{
    public class Verb_FeastOfMind : Verb_CastAbility
    {
        private const int BuffDurationTicks = 5400; // 90 seconds
        private static HediffDef FeastOfMindHediffDef => HediffDef.Named("ProjectOvermind_FeastOfMind");

        protected override bool TryCastShot()
        {
            try
            {
                Pawn targetPawn = currentTarget.Thing as Pawn;
                
                if (targetPawn == null)
                {
                    if (Prefs.DevMode)
                        Log.Warning("[FeastOfMind] TryCastShot: target is not a pawn");
                    return false;
                }

                // Verify target is valid (humanlike, alive, friendly)
                if (!targetPawn.RaceProps.Humanlike)
                {
                    if (Prefs.DevMode)
                        Log.Warning($"[FeastOfMind] TryCastShot: target {targetPawn.LabelShort} is not humanlike");
                    return false;
                }

                if (targetPawn.Dead || targetPawn.Downed)
                {
                    if (Prefs.DevMode)
                        Log.Warning($"[FeastOfMind] TryCastShot: target {targetPawn.LabelShort} is dead or downed");
                    return false;
                }

                // Check if target is friendly
                if (CasterPawn != null && targetPawn.HostileTo(CasterPawn))
                {
                    if (Prefs.DevMode)
                        Log.Warning($"[FeastOfMind] TryCastShot: target {targetPawn.LabelShort} is hostile");
                    return false;
                }

                // Apply the buff
                ApplyFeastOfMindBuff(targetPawn);

                // Visual effect: cyan-green psychic fleck at target position
                FleckMaker.Static(targetPawn.DrawPos, targetPawn.Map, FleckDefOf.PsycastAreaEffect, 2f);
                
                // Additional subtle glow
                for (int i = 0; i < 3; i++)
                {
                    FleckMaker.ThrowDustPuffThick(
                        targetPawn.DrawPos + new Vector3(
                            Rand.Range(-0.5f, 0.5f), 
                            0f, 
                            Rand.Range(-0.5f, 0.5f)
                        ),
                        targetPawn.Map,
                        Rand.Range(0.5f, 1f),
                        new Color(0.3f, 1f, 0.7f) // cyan-green
                    );
                }

                // Sound effect
                SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(targetPawn));

                // Floating text above pawn
                MoteMaker.ThrowText(
                    targetPawn.DrawPos + new Vector3(0f, 0f, 0.5f),
                    targetPawn.Map,
                    "Feast of Mind",
                    new Color(0.3f, 1f, 0.7f), // cyan-green
                    3.5f
                );

                // Message to player
                Messages.Message(
                    $"Feast of Mind applied to {targetPawn.LabelShort}",
                    targetPawn,
                    MessageTypeDefOf.PositiveEvent,
                    historical: false
                );

                if (Prefs.DevMode)
                    Log.Message($"[FeastOfMind] applied to {targetPawn.LabelShort}");

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[FeastOfMind] TryCastShot error: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        private void ApplyFeastOfMindBuff(Pawn pawn)
        {
            try
            {
                if (pawn?.health?.hediffSet == null)
                {
                    if (Prefs.DevMode)
                        Log.Warning($"[FeastOfMind] ApplyBuff: pawn {pawn?.LabelShort ?? "null"} has null health or hediffSet");
                    return;
                }

                // Check if pawn already has the buff
                Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(FeastOfMindHediffDef);
                
                if (existingHediff != null)
                {
                    // Refresh duration instead of stacking
                    HediffComp_Disappears comp = existingHediff.TryGetComp<HediffComp_Disappears>();
                    if (comp != null)
                    {
                        comp.ticksToDisappear = BuffDurationTicks;
                        if (Prefs.DevMode)
                            Log.Message($"[FeastOfMind] Refreshed buff duration for {pawn.LabelShort}");
                    }
                }
                else
                {
                    // Add new hediff
                    Hediff hediff = HediffMaker.MakeHediff(FeastOfMindHediffDef, pawn);
                    pawn.health.AddHediff(hediff);
                    
                    if (Prefs.DevMode)
                        Log.Message($"[FeastOfMind] Added new buff to {pawn.LabelShort}");
                }

                // Spawn visual effect at pawn location
                FleckMaker.Static(pawn.DrawPos, pawn.Map, FleckDefOf.PsycastSkipInnerExit, 1.5f);
            }
            catch (Exception ex)
            {
                Log.Error($"[FeastOfMind] ApplyBuff error for {pawn?.LabelShort ?? "null"}: {ex.Message}");
            }
        }
    }
}
