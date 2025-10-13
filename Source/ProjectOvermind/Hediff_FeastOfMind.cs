using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace ProjectOvermind
{
    public class Hediff_FeastOfMind : HediffWithComps
    {
        private new const int TickInterval = 60; // Check every 60 ticks (1 second) for visual effects
        private const float BaseEffect = 0.5f; // 50% base effect
        private const float MinEffect = 0.1f; // 10% minimum
        private const float MaxEffect = 1.0f; // 100% maximum

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            try
            {
                if (pawn == null)
                {
                    if (Prefs.DevMode)
                        Log.Warning("[FeastOfMind] PostAdd: pawn is null");
                    return;
                }

                // Get psychic sensitivity and scale the effect
                float psychicSensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                
                // Calculate the scaling factor
                // Formula: clamp(BaseEffect * psychicSensitivity, MinEffect, MaxEffect)
                float scalingFactor = Mathf.Clamp(BaseEffect * psychicSensitivity, MinEffect, MaxEffect);
                
                // Set severity to the scaling factor
                // This will multiply the stat offsets defined in XML
                Severity = scalingFactor;

                if (Prefs.DevMode)
                {
                    Log.Message($"[FeastOfMind] PostAdd for {pawn.LabelShort}:");
                    Log.Message($"  - Psychic Sensitivity: {psychicSensitivity:F2}");
                    Log.Message($"  - Scaling Factor: {scalingFactor:F2} ({scalingFactor * 100:F0}%)");
                    Log.Message($"  - Eating Speed Bonus: +{scalingFactor * 50:F0}%");
                    Log.Message($"  - Hunger Rate Reduction: {(1f - scalingFactor) * 100:F0}% of normal");
                }

                // Spawn initial visual effect
                if (pawn.Spawned && pawn.Map != null)
                {
                    FleckMaker.ThrowDustPuffThick(
                        pawn.DrawPos,
                        pawn.Map,
                        1.5f,
                        new Color(0.3f, 1f, 0.7f) // cyan-green
                    );
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[FeastOfMind] PostAdd error for {pawn?.LabelShort ?? "null"}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public override void PostTick()
        {
            base.PostTick();

            try
            {
                if (pawn == null || !pawn.Spawned || pawn.Map == null)
                    return;

                // Periodic visual effect (every 60 ticks = 1 second)
                if (pawn.IsHashIntervalTick(TickInterval))
                {
                    // 20% chance to spawn a subtle glow each second
                    if (Rand.Chance(0.2f))
                    {
                        FleckMaker.ThrowDustPuffThick(
                            pawn.DrawPos + new Vector3(
                                Rand.Range(-0.3f, 0.3f),
                                0f,
                                Rand.Range(-0.3f, 0.3f)
                            ),
                            pawn.Map,
                            Rand.Range(0.3f, 0.6f),
                            new Color(0.3f, 1f, 0.7f, 0.5f) // cyan-green, semi-transparent
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                    Log.Error($"[FeastOfMind] PostTick error for {pawn?.LabelShort ?? "null"}: {ex.Message}");
            }
        }

        public override bool TryMergeWith(Hediff other)
        {
            // Don't allow stacking - duration will be refreshed in Verb instead
            return false;
        }

        public override void PostRemoved()
        {
            base.PostRemoved();

            try
            {
                if (Prefs.DevMode && pawn != null)
                    Log.Message($"[FeastOfMind] Buff expired for {pawn.LabelShort}");

                // Optional: spawn a fade-out effect
                if (pawn != null && pawn.Spawned && pawn.Map != null)
                {
                    FleckMaker.ThrowDustPuffThick(
                        pawn.DrawPos,
                        pawn.Map,
                        0.8f,
                        new Color(0.3f, 1f, 0.7f, 0.3f)
                    );
                }
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                    Log.Error($"[FeastOfMind] PostRemoved error: {ex.Message}");
            }
        }

        public override string LabelInBrackets
        {
            get
            {
                // Show the scaled percentage in the health tab
                float effectPercent = Severity * 100f;
                return $"{effectPercent:F0}% effect";
            }
        }
    }
}
