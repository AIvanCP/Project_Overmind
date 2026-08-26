using RimWorld;
using Verse;

namespace ProjectOvermind
{
    /// <summary>
    /// Helper class for calculating dynamic ability durations based on caster's psychic sensitivity
    /// </summary>
    public static class DurationHelper
    {
        // RimWorld time constants
        private const int TicksPerHour = 2500;
        private const int TicksPerDay = 60000;
        
        // Base duration: 12 in-game hours (half day)
        private const int BaseDurationTicks = TicksPerHour * 12; // 30,000 ticks
        
        // Scaling: Each 0.1 psychic sensitivity adds 1 in-game hour
        private const float SensitivityStep = 0.1f;
        private const int TicksPerStep = TicksPerHour; // 2500 ticks per 0.1 sensitivity
        
        /// <summary>
        /// Calculate buff/debuff duration based on caster's psychic sensitivity
        /// Base: 12 hours, +1 hour per 0.1 sensitivity
        /// </summary>
        /// <param name="caster">Pawn casting the ability</param>
        /// <returns>Duration in ticks</returns>
        public static int CalculateDuration(Pawn caster)
        {
            if (caster == null)
                return BaseDurationTicks;
            
            float sensitivity = caster.GetStatValue(StatDefOf.PsychicSensitivity);
            
            // Calculate bonus ticks: (sensitivity / 0.1) * 2500
            int bonusTicks = (int)((sensitivity / SensitivityStep) * TicksPerStep);
            
            return BaseDurationTicks + bonusTicks;
        }
        
        /// <summary>
        /// Calculate Feast of Mind duration (shorter base: 6 hours)
        /// Base: 6 hours, +0.5 hour per 0.1 sensitivity
        /// </summary>
        /// <param name="caster">Pawn casting the ability</param>
        /// <returns>Duration in ticks</returns>
        public static int CalculateFeastDuration(Pawn caster)
        {
            if (caster == null)
                return TicksPerHour * 6; // 15,000 ticks (6 hours)
            
            float sensitivity = caster.GetStatValue(StatDefOf.PsychicSensitivity);
            
            // Calculate bonus ticks: (sensitivity / 0.1) * 1250 (0.5 hour per step)
            int bonusTicks = (int)((sensitivity / SensitivityStep) * (TicksPerStep / 2));
            
            return (TicksPerHour * 6) + bonusTicks;
        }
        
        /// <summary>
        /// Calculate radius for area abilities based on caster's psychic sensitivity
        /// </summary>
        /// <param name="caster">Pawn casting the ability</param>
        /// <param name="baseRadius">Base radius at 0 sensitivity</param>
        /// <param name="radiusPerStep">Radius increase per 0.1 sensitivity</param>
        /// <returns>Calculated radius</returns>
        public static float CalculateRadius(Pawn caster, float baseRadius, float radiusPerStep = 0.5f)
        {
            if (caster == null)
                return baseRadius;
            
            float sensitivity = caster.GetStatValue(StatDefOf.PsychicSensitivity);
            
            // Calculate bonus radius
            float bonusRadius = (sensitivity / SensitivityStep) * radiusPerStep;
            
            return baseRadius + bonusRadius;
        }
        
        /// <summary>
        /// Get human-readable duration string.
        ///
        /// FIXED 2026-08-15. The old version only divided by TicksPerHour (2500) using
        /// integer division, so ANY buff shorter than one in-game hour displayed as
        /// "0h". Nearly every ability here is that short - Cognitive Shield is 1500
        /// ticks (25s), Psychic Diffusion 1200 (20s), Mind Spike 600 (10s) - so freshly
        /// cast buffs looked expired in the health tab.
        ///
        /// Anything under an hour now reports in seconds/minutes, which is the useful
        /// unit for combat-length buffs anyway.
        /// </summary>
        public static string GetDurationString(int ticks)
        {
            if (ticks <= 0)
                return "0s";

            if (ticks < TicksPerHour)
            {
                // 60 ticks = 1 second at normal speed.
                int seconds = ticks / 60;
                if (seconds < 1)
                    return "<1s";
                if (seconds < 60)
                    return $"{seconds}s";

                int minutes = seconds / 60;
                int remainingSeconds = seconds % 60;
                return remainingSeconds > 0 ? $"{minutes}m {remainingSeconds}s" : $"{minutes}m";
            }

            int hours = ticks / TicksPerHour;
            int days = hours / 24;
            int remainingHours = hours % 24;

            if (days > 0)
                return $"{days}d {remainingHours}h";

            return $"{hours}h";
        }
    }
}
