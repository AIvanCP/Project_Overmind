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
                                   ability.def.defName == "ProjectOvermind_AuraClean";

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
                    Log.Message($"[Mind Core] Reduced psyfocus cost by {reduction:F0}% for {pawn.LabelShort} ({originalValue:F2} → {value:F2})");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Core] Error in TryAddEntropy patch: {ex}");
            }
        }
    }

    /// <summary>
    /// Additional patch for VPE compatibility - patches the ability's GetEntropyUsedByCurrentPawn method
    /// This ensures VPE abilities also get cost reduction
    /// </summary>
    [HarmonyPatch(typeof(Ability), "GetEntropyUsedByPawn")]
    public static class Ability_GetEntropyUsedByPawn_Patch
    {
        static void Postfix(Ability __instance, Pawn caster, ref float __result)
        {
            try
            {
                if (caster == null || __result <= 0f)
                    return;

                // Check if pawn has Mind Core buff
                HediffDef mindCoreDef = HediffDef.Named("ProjectOvermind_MindCore");
                if (mindCoreDef == null)
                    return;

                Hediff hediff = caster.health?.hediffSet?.GetFirstHediffOfDef(mindCoreDef);
                if (hediff == null)
                    return;

                Hediff_MindCore mindCore = hediff as Hediff_MindCore;
                if (mindCore == null)
                    return;

                // Apply cost multiplier to the result
                float multiplier = mindCore.GetPsyfocusCostMultiplier();
                float originalCost = __result;
                __result *= multiplier;

                if (Prefs.DevMode)
                {
                    float reduction = (1f - multiplier) * 100f;
                    Log.Message($"[Mind Core] GetEntropyUsedByPawn reduced by {reduction:F0}% for {caster.LabelShort}: {originalCost:F2} → {__result:F2}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Core] Error in GetEntropyUsedByPawn patch: {ex}");
            }
        }
    }

    /// <summary>
    /// Patch Ability.EntropyUsed property to show reduced cost in UI (VPE compatibility)
    /// This makes the gizmo tooltip display the correct reduced cost
    /// </summary>
    [HarmonyPatch(typeof(Ability))]
    [HarmonyPatch("EntropyUsed", MethodType.Getter)]
    public static class Ability_EntropyUsed_Patch
    {
        static void Postfix(Ability __instance, ref float __result)
        {
            try
            {
                Pawn pawn = __instance?.pawn;
                if (pawn == null || __result <= 0f)
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

                // Apply cost multiplier
                float multiplier = mindCore.GetPsyfocusCostMultiplier();
                __result *= multiplier;
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Core] Error in EntropyUsed patch: {ex}");
            }
        }
    }

    /// <summary>
    /// Patch psyfocus recovery to boost it with Mind Core
    /// This enhances the natural psyfocus regeneration
    /// </summary>
    [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), "GainPsyfocus")]
    public static class PsychicEntropyTracker_GainPsyfocus_Patch
    {
        static void Prefix(ref float amount, Pawn ___pawn)
        {
            try
            {
                if (___pawn == null || amount <= 0f)
                    return;

                // Check if pawn has Mind Core buff
                HediffDef mindCoreDef = HediffDef.Named("ProjectOvermind_MindCore");
                if (mindCoreDef == null)
                    return;

                Hediff hediff = ___pawn.health?.hediffSet?.GetFirstHediffOfDef(mindCoreDef);
                if (hediff == null)
                    return;

                Hediff_MindCore mindCore = hediff as Hediff_MindCore;
                if (mindCore == null)
                    return;

                // Boost psyfocus gain (this is separate from the passive tick regen)
                // Apply a moderate 15% boost to meditation/neural supercharger gains
                float originalAmount = amount;
                amount *= 1.15f;

                if (Prefs.DevMode && (originalAmount > 0.01f)) // Only log significant gains
                {
                    Log.Message($"[Mind Core] Boosted psyfocus gain for {___pawn.LabelShort}: {originalAmount:F3} → {amount:F3}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Core] Error in GainPsyfocus patch: {ex}");
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
                    Log.Message($"[Mind Core] Reduced cooldown by {(1f - multiplier):P0} for {pawn.LabelShort} ({originalTicks} → {ticks} ticks)");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Core] Error in StartCooldown patch: {ex}");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Overmind Adaptation – terrain movement cost reduction (safe Postfix only)
    // Compatible with DMC mod's movement patches (both use Postfix, stack safely)
    // ─────────────────────────────────────────────────────────────────────────

    [HarmonyPatch(typeof(Pawn_PathFollower), "CostToMoveIntoCell",
        new[] { typeof(Pawn), typeof(IntVec3) })]
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

        /// <summary>
        /// Postfix: reduce terrain movement cost by TerrainIgnoreFraction.
        /// Only modifies __result – never skips original. Safe with DMC and other mods.
        /// RimWorld 1.6 returns float.
        /// </summary>
        public static void Postfix(Pawn pawn, IntVec3 c, ref float __result)
        {
            try
            {
                // Quick bail-outs (performance-sensitive, called every pathfinding tick)
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

                __result *= (1f - ignore);

                // Clamp to minimum of 1 (1 tick minimum cost) to avoid zero/negative
                if (__result < 1f) __result = 1f;
            }
            catch
            {
                // Swallow exceptions silently to never break pathfinding
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Overmind Adaptation – environmental damage absorption (Postfix on PreApplyDamage)
    // Handles gas and vacuum direct damage. Temperature + disease handled in Hediff Tick.
    // ─────────────────────────────────────────────────────────────────────────

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
                // Swallow silently – never crash health system
            }
        }
    }
}
