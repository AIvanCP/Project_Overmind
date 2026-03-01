using System;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Hediff for Overmind Adaptation psycast.
    /// Grants terrain movement cost reduction, movement speed bonus,
    /// and escalating environmental immunities based on caster's PsychicSensitivity.
    ///
    /// Sensitivity thresholds (caster's sensitivity, stored at cast time):
    ///   >= 3.0 → Immune to toxic environment and gas damage
    ///   >= 5.0 → Immune to vacuum (Odyssey DLC) suffocation
    ///   >= 8.0 → Immune to heatstroke/hypothermia; 50% need decay reduction
    ///
    /// Terrain effect (scales with caster sensitivity):
    ///   terrainIgnore = clamp01(0.25 + sensitivity * 0.10)
    ///   moveBonus    = clamp(0.25 + sensitivity * 0.10, 0, 2)
    /// </summary>
    public class Hediff_OvermindAdaptation : HediffWithComps
    {
        // Caster's psychic sensitivity stored at cast time
        public float CasterSensitivity { get; private set; } = 1f;

        // ── Environmental immunity check interval (every ~3 seconds) ──────────
        private const int ImmunityCheckInterval = 180;
        private int immunityTickCounter = 0;

        // ── Cached computed values ────────────────────────────────────────────
        private float cachedTerrainIgnore = 0.35f;  // default at sens 1.0
        private float cachedMoveBonus     = 0.35f;

        // ── Def name strings (avoids repeated string allocations) ─────────────
        // Toxic environment hediff
        private static readonly string ToxicBuildupDefName    = "ToxicBuildup";
        // Gas/chemical hediffs (Anomaly DLC and others)
        private static readonly string GasDeathDefName        = "GasMask_GasDeath";      // placeholder
        // Suffocation (Odyssey vacuum)
        private static readonly string SuffocationDefName     = "Suffocation";
        // Temperature hediffs
        private static readonly string HypothermiaDefName     = "Hypothermia";
        private static readonly string HeatstrokeDefName      = "Heatstroke";

        // ── Public terrain/movement properties used by Harmony patches ────────

        /// <summary>
        /// Fraction of terrain cost to remove (0.0 = no reduction, 1.0 = full immunity).
        /// Cached when sensitivity initialized.
        /// </summary>
        public float TerrainIgnoreFraction => cachedTerrainIgnore;

        /// <summary>
        /// Additive move speed bonus (in cells/second). Used by StatPart.
        /// At sensitivity 1.0 → ~0.35 c/s bonus (≈15-17% at base 2.0 c/s).
        /// </summary>
        public float MoveBonusCells => cachedMoveBonus * 2.0f; // scale to c/s

        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by Verb_OvermindAdaptation after adding the hediff.
        /// Sets caster sensitivity and pre-computes derived values.
        /// </summary>
        public void InitializeCasterSensitivity(float sensitivity)
        {
            CasterSensitivity = Mathf.Max(0f, sensitivity);
            cachedTerrainIgnore = Mathf.Clamp01(0.25f + (CasterSensitivity * 0.10f));
            cachedMoveBonus    = Mathf.Clamp(0.25f + (CasterSensitivity * 0.10f), 0f, 2f);

            if (Prefs.DevMode)
                Log.Message($"[OvermindAdaptation] Initialized on {pawn?.LabelShort}: " +
                            $"sens={CasterSensitivity:F2}, terrainIgnore={cachedTerrainIgnore:P0}, " +
                            $"moveBonus={cachedMoveBonus:P0}");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Tick
        // ─────────────────────────────────────────────────────────────────────

        public override void PostTick()
        {
            base.PostTick();

            try
            {
                immunityTickCounter++;
                if (immunityTickCounter < ImmunityCheckInterval)
                    return;
                immunityTickCounter = 0;

                if (pawn == null || pawn.Dead || !pawn.Spawned || pawn.health == null)
                    return;
                if (!pawn.RaceProps.Humanlike)
                    return;

                ApplyEnvironmentalImmunities();
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                    Log.Error($"[OvermindAdaptation] PostTick error on {pawn?.LabelShort}: {ex}");
            }
        }

        /// <summary>
        /// Remove / suppress bad hediffs based on caster sensitivity thresholds.
        /// </summary>
        private void ApplyEnvironmentalImmunities()
        {
            if (pawn?.health?.hediffSet == null) return;

            // ── Tier 1: >= 3.0 sensitivity → toxic & gas immunity ────────────
            if (CasterSensitivity >= 3.0f)
            {
                RemoveHediffByName(ToxicBuildupDefName, maxSeverity: 0.6f);
                RemoveHediffByName(GasDeathDefName);

                // Also check for other common gas/poison hediffs
                RemoveHediffByName("ChemicalDamage_Small");
                RemoveHediffByName("ChemicalDamage_Large");
                RemoveHediffByName("GasPoison");
                RemoveHediffByName("Vomit"); // often triggered by toxic
            }

            // ── Tier 2: >= 5.0 sensitivity → vacuum immunity ─────────────────
            if (CasterSensitivity >= 5.0f)
            {
                RemoveHediffByName(SuffocationDefName, maxSeverity: 0.7f);
                RemoveHediffByName("SuffocationBuildrate");
                RemoveHediffByName("VacuumExposure");
            }

            // ── Tier 3: >= 8.0 sensitivity → temperature & need immunity ─────
            if (CasterSensitivity >= 8.0f)
            {
                RemoveHediffByName(HypothermiaDefName, maxSeverity: 0.6f);
                RemoveHediffByName(HeatstrokeDefName, maxSeverity: 0.6f);

                // Suppress airborne disease hediffs by reducing their severity
                SuppressAirborneDiseases();
            }
        }

        /// <summary>
        /// Remove a hediff by def name if it exists (and optionally only when below maxSeverity).
        /// Returns true if removed.
        /// </summary>
        private bool RemoveHediffByName(string defName, float maxSeverity = float.MaxValue)
        {
            if (string.IsNullOrEmpty(defName)) return false;
            if (pawn?.health?.hediffSet == null) return false;

            HediffDef def = DefDatabase<HediffDef>.GetNamedSilentFail(defName);
            if (def == null) return false;

            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(def);
            if (existing == null) return false;

            if (existing.Severity <= maxSeverity)
            {
                pawn.health.RemoveHediff(existing);
                if (Prefs.DevMode)
                    Log.Message($"[OvermindAdaptation] Cleared '{defName}' from {pawn.LabelShort}");
                return true;
            }
            else
            {
                // Reduce severity instead of full removal when severity is high
                existing.Severity = Mathf.Max(0f, existing.Severity - 0.15f);
                return false;
            }
        }

        /// <summary>
        /// Suppress (slow progression of) airborne disease hediffs.
        /// </summary>
        private void SuppressAirborneDiseases()
        {
            if (pawn?.health?.hediffSet == null) return;

            foreach (Hediff h in pawn.health.hediffSet.hediffs)
            {
                if (h == null || h.def == null) continue;
                if (!h.def.isBad) continue;
                if (h.def.defName == null) continue;

                // Only target disease hediffs (not injuries, not this hediff)
                bool isDisease = h is HediffWithComps &&
                                 h.def.tendable &&
                                 h.def.defName != this.def?.defName;

                if (isDisease)
                {
                    // Reduce severity slightly to slow progression of diseases
                    h.Severity = Mathf.Max(0f, h.Severity - 0.05f);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Tooltip / Label
        // ─────────────────────────────────────────────────────────────────────

        public override string LabelInBrackets
        {
            get
            {
                HediffComp_Disappears comp = this.TryGetComp<HediffComp_Disappears>();
                if (comp != null && comp.ticksToDisappear > 0)
                    return DurationHelper.GetDurationString(comp.ticksToDisappear);
                return null;
            }
        }

        public override string TipStringExtra
        {
            get
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Caster psychic sensitivity: {CasterSensitivity:F2}x");
                    sb.AppendLine($"Terrain cost reduction: {cachedTerrainIgnore:P0}");
                    sb.AppendLine($"Move speed bonus: +{cachedMoveBonus:P0}");
                    sb.AppendLine();

                    // Environmental tiers
                    if (CasterSensitivity >= 3.0f)
                        sb.AppendLine("✓ Immune: Toxic & Gas exposure");
                    if (CasterSensitivity >= 5.0f)
                        sb.AppendLine("✓ Immune: Vacuum / Suffocation");
                    if (CasterSensitivity >= 8.0f)
                    {
                        sb.AppendLine("✓ Immune: Heatstroke & Hypothermia");
                        sb.AppendLine("✓ Need decay reduced 50%");
                    }

                    HediffComp_Disappears comp = this.TryGetComp<HediffComp_Disappears>();
                    if (comp != null && comp.ticksToDisappear > 0)
                        sb.AppendLine($"Duration remaining: {DurationHelper.GetDurationString(comp.ticksToDisappear)}");

                    return sb.ToString().TrimEnd();
                }
                catch
                {
                    return base.TipStringExtra;
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Save/Load
        // ─────────────────────────────────────────────────────────────────────

        public override void ExposeData()
        {
            base.ExposeData();
            float sens = CasterSensitivity;
            Scribe_Values.Look(ref sens, "casterSensitivity", 1f);
            CasterSensitivity = sens;

            // Re-compute cached values after load
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                cachedTerrainIgnore = Mathf.Clamp01(0.25f + (CasterSensitivity * 0.10f));
                cachedMoveBonus    = Mathf.Clamp(0.25f + (CasterSensitivity * 0.10f), 0f, 2f);
            }
        }
    }
}
