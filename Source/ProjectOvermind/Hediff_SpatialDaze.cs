using System;
using RimWorld;
using Verse;

namespace ProjectOvermind
{
    /// <summary>
    /// Hediff for Spatial Daze - applied after translocation
    /// Handles the disorientation effect when a pawn is teleported
    /// </summary>
    public class Hediff_SpatialDaze : HediffWithComps
    {
        // The hediff stages are defined in XML, so this class mainly exists
        // for future extensibility and custom behavior if needed.

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            
            try
            {
                // Optional: Add custom behavior when the hediff is first applied
                // For example, you could make the pawn stagger or drop items
                
                if (pawn != null && pawn.Spawned)
                {
                    // Make the pawn stagger slightly
                    pawn.stances?.stagger?.StaggerFor(95);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Error in Hediff_SpatialDaze.PostAdd: {ex}");
            }
        }

        public override void Tick()
        {
            base.Tick();
            
            try
            {
                // Optional: Add periodic effects while the hediff is active
                // Currently, the XML-defined capacity modifiers handle the main effect
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Error in Hediff_SpatialDaze.Tick: {ex}");
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            
            try
            {
                // Optional: Add custom behavior when the hediff expires
                // For example, display a message or play a sound
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Error in Hediff_SpatialDaze.PostRemoved: {ex}");
            }
        }
    }
}
