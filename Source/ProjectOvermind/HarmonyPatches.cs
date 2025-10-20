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
                                   ability.def.defName == "ProjectOvermind_ISeeYou";

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
}
