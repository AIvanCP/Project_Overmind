using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Custom verb for the Translocate Target psycast.
    /// Uses two-stage targeting: first select a pawn, then select a destination cell.
    /// </summary>
    public class Verb_TranslocateTarget : Verb_CastAbility
    {
        private Pawn selectedTarget;
        private const float MaxDestinationRange = 10f;

        protected override bool TryCastShot()
        {
            try
            {
                // First stage: get the target pawn
                if (currentTarget.HasThing && currentTarget.Thing is Pawn targetPawn)
                {
                    if (!ValidateTarget(targetPawn))
                    {
                        return false;
                    }

                    selectedTarget = targetPawn;

                    // Second stage: request destination cell
                    // Create action for when destination is selected
                    Action<LocalTargetInfo> onDestinationSelected = delegate(LocalTargetInfo dest)
                    {
                        if (dest.IsValid && dest.Cell.IsValid)
                        {
                            ExecuteTranslocation(selectedTarget, dest.Cell);
                        }
                    };

                    // Start targeting for destination
                    Find.Targeter.BeginTargeting(GetDestinationTargetingParameters(), onDestinationSelected, CasterPawn);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Error in TryCastShot: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Validate that the target pawn can be translocated
        /// </summary>
        private bool ValidateTarget(Pawn target)
        {
            try
            {
                if (target == null)
                {
                    Messages.Message("Cannot translocate: Invalid target.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                if (target.Dead)
                {
                    Messages.Message("Cannot translocate: Target is dead.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                if (target.Downed)
                {
                    Messages.Message("Cannot translocate: Target is downed.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                if (target.RaceProps.IsMechanoid)
                {
                    Messages.Message("Cannot translocate: Mechanoids are immune to translocation.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Error in ValidateTarget: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Get targeting parameters for destination cell selection
        /// </summary>
        private TargetingParameters GetDestinationTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetLocations = true,
                canTargetPawns = false,
                canTargetBuildings = false,
                canTargetItems = false,
                validator = delegate (TargetInfo targ)
                {
                    if (!targ.IsValid || !targ.Cell.IsValid)
                    {
                        return false;
                    }

                    IntVec3 cell = targ.Cell;
                    Map map = targ.Map ?? CasterPawn.Map;

                    // Check if destination is within range of the target
                    if (selectedTarget != null && selectedTarget.Position.DistanceTo(cell) > MaxDestinationRange)
                    {
                        return false;
                    }

                    // Check if cell is walkable and not impassable
                    if (!cell.Walkable(map))
                    {
                        return false;
                    }

                    // Check if cell is not fogged
                    if (cell.Fogged(map))
                    {
                        return false;
                    }

                    // Check for roof if caster can't be under roofs
                    if (cell.Roofed(map) && selectedTarget != null && !selectedTarget.RaceProps.ToolUser)
                    {
                        return false;
                    }

                    return true;
                }
            };
        }

        /// <summary>
        /// Execute the translocation effect
        /// </summary>
        private void ExecuteTranslocation(Pawn target, IntVec3 destinationCell)
        {
            try
            {
                if (target == null || !target.Spawned)
                {
                    Messages.Message("Translocation failed: Target is no longer available.", MessageTypeDefOf.RejectInput, false);
                    return;
                }

                Map map = target.Map;
                IntVec3 sourceCell = target.Position;

                // Validate destination one more time
                if (!destinationCell.Walkable(map) || destinationCell.Fogged(map))
                {
                    Messages.Message("Translocation failed: Destination is not accessible.", MessageTypeDefOf.RejectInput, false);
                    return;
                }

                // Play exit effect at source
                PlayTranslocateEffect(sourceCell, map, true);

                // Teleport the pawn
                target.DeSpawn();
                GenSpawn.Spawn(target, destinationCell, map);
                target.jobs?.EndCurrentJob(JobCondition.InterruptForced);
                target.stances?.CancelBusyStanceHard();

                // Play entry effect at destination
                PlayTranslocateEffect(destinationCell, map, false);

                // Apply spatial daze hediff
                ApplySpatialDaze(target);

                // Play sound
                SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(destinationCell, map));

                // Success message
                Messages.Message($"Translocated {target.NameShortColored} to {destinationCell}.", 
                    new LookTargets(new TargetInfo(destinationCell, map)), MessageTypeDefOf.NeutralEvent, true);

                // Optional: Flash the destination to draw attention
                FlashCell(destinationCell, map);
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Error in ExecuteTranslocation: {ex}");
                Messages.Message("Translocation failed due to an error.", MessageTypeDefOf.RejectInput, false);
            }
        }

        /// <summary>
        /// Play translocation visual effect
        /// </summary>
        private void PlayTranslocateEffect(IntVec3 cell, Map map, bool isExit)
        {
            try
            {
                FleckCreationData dataStatic = FleckMaker.GetDataStatic(cell.ToVector3Shifted(), map, 
                    isExit ? FleckDefOf.PsycastSkipFlashEntry : FleckDefOf.PsycastSkipFlashEntry, 1f);
                dataStatic.rotation = Rand.Range(0, 360);
                map.flecks.CreateFleck(dataStatic);

                // Add additional visual particles
                for (int i = 0; i < 6; i++)
                {
                    FleckCreationData dataParticle = FleckMaker.GetDataStatic(
                        cell.ToVector3Shifted() + Gen.RandomHorizontalVector(1f), 
                        map, 
                        FleckDefOf.PsycastSkipInnerExit, 
                        Rand.Range(0.5f, 1f));
                    dataParticle.rotationRate = Rand.Range(-30, 30);
                    dataParticle.velocityAngle = Rand.Range(0, 360);
                    dataParticle.velocitySpeed = Rand.Range(0.1f, 0.8f);
                    map.flecks.CreateFleck(dataParticle);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Error in PlayTranslocateEffect: {ex}");
            }
        }

        /// <summary>
        /// Apply spatial daze hediff to the target
        /// </summary>
        private void ApplySpatialDaze(Pawn target)
        {
            try
            {
                HediffDef spatialDazeDef = DefDatabase<HediffDef>.GetNamedSilentFail("ProjectOvermind_SpatialDaze");
                if (spatialDazeDef != null && target.health != null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(spatialDazeDef, target);
                    target.health.AddHediff(hediff);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Error in ApplySpatialDaze: {ex}");
            }
        }

        /// <summary>
        /// Flash the cell to highlight the destination
        /// </summary>
        private void FlashCell(IntVec3 cell, Map map)
        {
            try
            {
                map.debugDrawer.FlashCell(cell, 0.5f, "translocate", 50);
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Error in FlashCell: {ex}");
            }
        }
    }
}
