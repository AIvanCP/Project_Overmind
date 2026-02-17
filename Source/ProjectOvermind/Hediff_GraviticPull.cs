using RimWorld;
using Verse;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Hediff for gravitic pull effect from Spatial Anchor
    /// Reduces movement speed and dodge chance
    /// </summary>
    public class Hediff_GraviticPull : HediffWithComps
    {
        // This hediff's stat effects are defined in XML
        // The pull mechanic is handled by the Thing_SpatialAnchor class

        /// <summary>
        /// Show duration in brackets on health tab
        /// </summary>
        public override string LabelInBrackets
        {
            get
            {
                HediffComp_Disappears comp = this.TryGetComp<HediffComp_Disappears>();
                if (comp != null && comp.ticksToDisappear > 0)
                {
                    return DurationHelper.GetDurationString(comp.ticksToDisappear);
                }
                return null;
            }
        }
    }
}
