using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Harmony patches to fix global self-cast abilities.
    /// 
    /// PROBLEM: Custom Ability class approach doesn't work because RimWorld's gizmo
    /// (Command_Ability) checks targeting BEFORE calling any ability methods.
    /// 
    /// SOLUTION: Patch the gizmo's ProcessInput method to skip targeting for our abilities.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("rimworld.projectovermind");
            harmony.PatchAll();
            Log.Message("[Project Overmind] Harmony patches applied");
        }
    }

    /// <summary>
    /// Patch Command_Ability.ProcessInput to bypass targeting for global self-cast abilities.
    /// This runs BEFORE the targeting UI is shown.
    /// </summary>
    [HarmonyPatch(typeof(Command_Ability), "ProcessInput")]
    public static class Command_Ability_ProcessInput_Patch
    {
        static bool Prefix(Command_Ability __instance, Event ev)
        {
            try
            {
                // Access the ability field (might be private)
                var abilityField = typeof(Command_Ability).GetField("ability", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (abilityField == null)
                {
                    // Fallback: try public property
                    var abilityProp = typeof(Command_Ability).GetProperty("Ability");
                    if (abilityProp != null)
                    {
                        var ability = abilityProp.GetValue(__instance) as Ability;
                        return HandleAbility(ability, ev);
                    }
                    return true; // Let original run if we can't access ability
                }

                Ability abilityObj = abilityField.GetValue(__instance) as Ability;
                return HandleAbility(abilityObj, ev);
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Error in Command_Ability patch: {ex}");
                return true; // Let original method run on error
            }
        }

        private static bool HandleAbility(Ability ability, Event ev)
        {
            if (ability == null) return true;

            // Check if this is one of our global self-cast abilities
            bool isGlobalSelfCast = ability.def.defName == "ProjectOvermind_Inspiration" ||
                                   ability.def.defName == "ProjectOvermind_Hallucination" ||
                                   ability.def.defName == "ProjectOvermind_CognitiveShield" ||
                                   ability.def.defName == "ProjectOvermind_PsychicDiffusion" ||
                                   ability.def.defName == "ProjectOvermind_ISeeYou" ||
                                   ability.def.defName == "ProjectOvermind_SoulRefill" ||
                                   ability.def.defName == "ProjectOvermind_AuraClean" ||
                                   ability.def.defName == "ProjectOvermind_OvermindAdaptation";

            if (!isGlobalSelfCast)
            {
                return true; // Let original method handle normal abilities
            }

            // Bypass targeting entirely - cast immediately on caster
            if (ability.pawn != null)
            {
                // Queue the casting job with caster as both target and destination
                ability.QueueCastingJob(ability.pawn, ability.pawn);
            }

            // Return false to prevent original ProcessInput from running
            return false;
        }
    }

    /// <summary>
    /// Patch Pawn_PsychicEntropyTracker.TryAddEntropy to reduce psyfocus cost for pawns with Mind Core buff
    /// Prefix: Modify entropy amount BEFORE it's added (better VPE compatibility)
    /// NOTE: RimWorld 1.6 renamed parameter from "entropy" to "value"
    /// </summary>
    [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), "TryAddEntropy")]
    public static class PsychicEntropyTracker_TryAddEntropy_Patch
    {
        static void Prefix(Pawn_PsychicEntropyTracker __instance, ref float value)
        {
            try
            {
                // Get pawn from instance
                var pawnField = typeof(Pawn_PsychicEntropyTracker).GetField("pawn", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Pawn pawn = pawnField?.GetValue(__instance) as Pawn;

                if (pawn == null || value <= 0f)
                    return;

                // Check if pawn has Mind Core buff
                HediffDef mindCoreDef = HediffDef.Named("ProjectOvermind_MindCore");
                if (mindCoreDef == null)
                    return;

                Hediff hediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(mindCoreDef);
                if (hediff == null)
                    return;

                Hediff_MindCore mindCore = hediff as Hediff_MindCore;
                if (mindCore == null)
                    return;

                // Get cost multiplier (e.g., 0.7 = 70% cost)
                float multiplier = mindCore.GetPsyfocusCostMultiplier();
                
                // Reduce entropy cost BEFORE it's added
                float originalValue = value;
                value *= multiplier;

                if (Prefs.DevMode)
                {
                    float reduction = (1f - multiplier) * 100f;
                    Log.Message($"[Mind Core] Reduced psyfocus cost by {reduction:F0}% for {pawn.LabelShort} ({originalValue:F2} â†’ {value:F2})");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Core] Error in TryAddEntropy patch: {ex}");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // REMOVED 2026-08-14: three patches whose targets do not exist in RimWorld 1.6.
    //
    //   1. [HarmonyPatch(typeof(Ability), "GetEntropyUsedByPawn")]
    //   2. [HarmonyPatch(typeof(Ability))] + ("EntropyUsed", MethodType.Getter)
    //        RimWorld.Ability has NO member containing "Entropy" at all. Both of these
    //        belong to VPE's VanillaPsycastsExpanded.Psycast, not to vanilla Ability,
    //        so `typeof(Ability)` resolved to the wrong type.
    //
    //   3. [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), "GainPsyfocus")]
    //        Real signature is GainPsyfocus(Thing focus) - there is no `amount`
    //        parameter, so Harmony could not bind `ref float amount` either.
    //
    // WHY THIS MATTERED: Harmony.PatchAll() throws on the first bad patch class and
    // aborts the whole loop, so every patch declared after #1 never applied - the
    // movement bonus and damage prevention included. Deleting these three is what
    // makes the rest of the mod actually work.
    //
    // All three only implemented psyfocus/entropy cost reduction. Their replacement
    // is Ability_FinalPsyfocusCost_Patch directly below, which hooks the method that
    // actually exists in 1.6.
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Mind Core: reduce the PSYFOCUS cost of casting.
    ///
    /// This is the patch the two removed VPE patches were trying to be. The real
    /// 1.6 entry point is Ability.FinalPsyfocusCost(LocalTargetInfo) - verified to
    /// be called from Psycast.Activate, so reducing __result reduces both the cost
    /// shown in the UI and the psyfocus actually spent.
    ///
    /// Applies to EVERY psycast the pawn owns, not just Project Overmind's, because
    /// Ability is the shared base class for vanilla and VPE psycasts alike.
    /// </summary>
    [HarmonyPatch(typeof(Ability), nameof(Ability.FinalPsyfocusCost))]
    public static class Ability_FinalPsyfocusCost_Patch
    {
        public static bool Prepare()
        {
            return AccessTools.Method(typeof(Ability), "FinalPsyfocusCost") != null;
        }

        public static void Postfix(Ability __instance, ref float __result)
        {
            try
            {
                if (__result <= 0f) return;

                Pawn pawn = __instance?.pawn;
                if (pawn == null) return;

                HediffDef mindCoreDef = DefDatabase<HediffDef>.GetNamedSilentFail("ProjectOvermind_MindCore");
                if (mindCoreDef == null) return;

                Hediff_MindCore mindCore =
                    pawn.health?.hediffSet?.GetFirstHediffOfDef(mindCoreDef) as Hediff_MindCore;
                if (mindCore == null) return;

                __result *= mindCore.GetPsyfocusCostMultiplier();
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Core] Error in FinalPsyfocusCost patch: {ex}");
            }
        }
    }

    /// <summary>
    /// Patch Ability.StartCooldown to reduce cooldown duration for pawns with Mind Core buff
    /// Prefix: Modify the cooldown duration before it's applied
    /// </summary>
    [HarmonyPatch(typeof(Ability), "StartCooldown")]
    public static class Ability_StartCooldown_Patch
    {
        static void Prefix(Ability __instance, ref int ticks)
        {
            try
            {
                Pawn pawn = __instance?.pawn;
                if (pawn == null)
                    return;

                // Check if pawn has Mind Core buff
                HediffDef mindCoreDef = HediffDef.Named("ProjectOvermind_MindCore");
                if (mindCoreDef == null)
                    return;

                Hediff hediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(mindCoreDef);
                if (hediff == null)
                    return;

                Hediff_MindCore mindCore = hediff as Hediff_MindCore;
                if (mindCore == null)
                    return;

                // Get cooldown multiplier (e.g., 0.75 = 75% cooldown, 25% faster)
                float multiplier = mindCore.GetCooldownMultiplier();
                
                // Apply multiplier to cooldown ticks
                int originalTicks = ticks;
                ticks = (int)(ticks * multiplier);

                if (Prefs.DevMode)
                {
                    Log.Message($"[Mind Core] Reduced cooldown by {(1f - multiplier):P0} for {pawn.LabelShort} ({originalTicks} â†’ {ticks} ticks)");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Core] Error in StartCooldown patch: {ex}");
            }
        }
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Overmind Adaptation â€“ terrain movement cost reduction (safe Postfix only)
    // Compatible with DMC mod's movement patches (both use Postfix, stack safely).
    //
    // We patch SetupMoveIntoNextCell and directly scale internal movement cost
    // fields (nextCellCostTotal / nextCellCostLeft). This is the most reliable
    // hook even when lower-level CostToMoveIntoCell methods are JIT-inlined.
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [HarmonyPatch(typeof(Pawn_PathFollower), "SetupMoveIntoNextCell")]
    public static class OvermindAdaptation_MovementCost_Patch
    {
        // Cached HediffDef to avoid per-tick string lookups
        private static HediffDef _adaptDef;

        private static HediffDef AdaptDef
        {
            get
            {
                if (_adaptDef == null)
                    _adaptDef = DefDatabase<HediffDef>.GetNamedSilentFail("ProjectOvermind_OvermindAdaptation");
                return _adaptDef;
            }
        }

        // (removed _lastDevLogTick - the per-tick dev logging it throttled is gone)

        /// <summary>
        /// Postfix on SetupMoveIntoNextCell. Scale next-cell movement costs by
        /// (1 - TerrainIgnoreFraction), with a minimum cost floor of 1 tick.
        /// ___pawn / ___nextCellCostTotal / ___nextCellCostLeft are private
        /// field injections from Pawn_PathFollower.
        /// </summary>
        public static void Postfix(
            Pawn_PathFollower __instance,
            Pawn ___pawn,
            ref float ___nextCellCostTotal,
            ref float ___nextCellCostLeft)
        {
            try
            {
                Pawn pawn = ___pawn;
                if (pawn == null) return;
                if (pawn.RaceProps == null || !pawn.RaceProps.Humanlike) return;
                if (pawn.health?.hediffSet == null) return;

                HediffDef def = AdaptDef;
                if (def == null) return;

                Hediff_OvermindAdaptation hediff =
                    pawn.health.hediffSet.GetFirstHediffOfDef(def) as Hediff_OvermindAdaptation;
                if (hediff == null) return;

                float ignore = hediff.TerrainIgnoreFraction; // 0..1
                if (ignore <= 0f) return;

                float multiplier = 1f - ignore;

                ___nextCellCostTotal *= multiplier;
                ___nextCellCostLeft *= multiplier;

                // Clamp to minimum of 1 (1 tick minimum cost) to avoid zero/negative
                if (___nextCellCostTotal < 1f) ___nextCellCostTotal = 1f;
                if (___nextCellCostLeft < 1f) ___nextCellCostLeft = 1f;

                // NO LOGGING HERE. This runs from Pawn_PathFollower.SetupMoveIntoNextCell,
                // i.e. every time any adapted pawn steps into a cell. The old code logged
                // once per 120 ticks behind Prefs.DevMode, which still produced 8,250 lines
                // in a single session - over half the entire Player.log - and buried every
                // other mod's messages. Dev mode is left on permanently in this setup, so it
                // is not a usable gate for a per-tick hot path.
                //
                // If this ever needs debugging again, log from a one-shot place (the ability
                // cast, or Hediff_OvermindAdaptation.PostAdd) instead of from the mover.
            }
            catch
            {
                // Swallow exceptions silently to never break pathfinding
            }
        }
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Overmind Adaptation â€“ environmental damage absorption (Postfix on PreApplyDamage)
    // Handles gas and vacuum direct damage. Temperature + disease handled in Hediff Tick.
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [HarmonyPatch(typeof(Pawn_HealthTracker), "PreApplyDamage")]
    public static class OvermindAdaptation_DamagePrevention_Patch
    {
        // Known environmental damage def names
        // Using string sets with case-insensitive comparison for mod compat
        private static readonly HashSet<string> ToxicGasDefs = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            "ToxicGas",      "GasToxic",       "ChemicalBurn",
            "Smoke",         "GasSmoke",       "ToxicDamage",
            "Toxic",         "GasDamage",      "GasPoison",
        };

        private static readonly HashSet<string> VacuumDefs = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            "Vacuum",        "VacuumDamage",   "Suffocation",
            "VacuumExposure","AirDepleted",
        };

        private static HediffDef _adaptDef;

        private static HediffDef AdaptDef
        {
            get
            {
                if (_adaptDef == null)
                    _adaptDef = DefDatabase<HediffDef>.GetNamedSilentFail("ProjectOvermind_OvermindAdaptation");
                return _adaptDef;
            }
        }

        /// <summary>
        /// Postfix: set absorbed = true for gas/vacuum damage on pawns with Overmind Adaptation
        /// at the required sensitivity threshold.
        /// Does NOT skip original method. Does NOT return false.
        /// ___pawn uses Harmony's field injection to access private Pawn_HealthTracker.pawn field.
        /// </summary>
        public static void Postfix(Pawn_HealthTracker __instance, ref DamageInfo dinfo, ref bool absorbed, Pawn ___pawn)
        {
            try
            {
                // Already absorbed by something else, no need to re-process
                if (absorbed) return;

                Pawn pawn = ___pawn;
                if (pawn == null || pawn.Dead) return;
                if (pawn.RaceProps == null || !pawn.RaceProps.Humanlike) return;
                if (pawn.health?.hediffSet == null) return;

                HediffDef def = AdaptDef;
                if (def == null) return;

                Hediff_OvermindAdaptation hediff =
                    pawn.health.hediffSet.GetFirstHediffOfDef(def) as Hediff_OvermindAdaptation;
                if (hediff == null) return;

                float sens = hediff.CasterSensitivity;
                string damageName = dinfo.Def?.defName;
                if (string.IsNullOrEmpty(damageName)) return;

                // Toxic / gas (threshold 3.0)
                if (sens >= 3.0f && ToxicGasDefs.Contains(damageName))
                {
                    absorbed = true;
                    if (Prefs.DevMode)
                        Log.Message($"[OvermindAdaptation] Absorbed gas/toxic damage '{damageName}' on {pawn.LabelShort}");
                    return;
                }

                // Vacuum / suffocation (threshold 5.0)
                if (sens >= 5.0f && VacuumDefs.Contains(damageName))
                {
                    absorbed = true;
                    if (Prefs.DevMode)
                        Log.Message($"[OvermindAdaptation] Absorbed vacuum damage '{damageName}' on {pawn.LabelShort}");
                }
            }
            catch
            {
                // Swallow silently â€“ never crash health system
            }
        }
    }
}
