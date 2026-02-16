using System;
using HarmonyLib;
using RimWorld;
using Verse;
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
    /// </summary>
    [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), "TryAddEntropy")]
    public static class PsychicEntropyTracker_TryAddEntropy_Patch
    {
        static void Prefix(Pawn_PsychicEntropyTracker __instance, ref float entropy, Pawn pawn)
        {
            try
            {
                if (pawn == null || entropy <= 0f)
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
                float originalEntropy = entropy;
                entropy *= multiplier;

                if (Prefs.DevMode)
                {
                    float reduction = (1f - multiplier) * 100f;
                    Log.Message($"[Mind Core] Reduced psyfocus cost by {reduction:F0}% for {pawn.LabelShort} ({originalEntropy:F2} → {entropy:F2})");
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
}
