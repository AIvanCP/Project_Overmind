using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Verb for "Spatial Anchor" psycast - creates a gravitational anchor at target location
    /// Affects hostile pawns within 10-tile radius with movement debuff and pull effect
    /// </summary>
    public class Verb_SpatialAnchor : Verb_CastAbility
    {
        private const int AnchorDurationTicks = 1200; // 20 seconds
        private const float AnchorRadius = 10f;

        protected override bool TryCastShot()
        {
            Log.Message("[Spatial Anchor] TryCastShot called");
            try
            {
                if (CasterPawn == null || CasterPawn.Map == null)
                {
                    Log.Warning("[Spatial Anchor] Failed: No valid map");
                    Messages.Message("Spatial Anchor failed: No valid map.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                IntVec3 targetCell = currentTarget.Cell;

                if (!targetCell.IsValid || !targetCell.InBounds(CasterPawn.Map))
                {
                    Log.Warning($"[Spatial Anchor] Failed: Invalid target location {targetCell}");
                    Messages.Message("Spatial Anchor: Invalid target location.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                Log.Message($"[Spatial Anchor] Creating anchor at {targetCell}");

                // Create the spatial anchor thing
                Thing_SpatialAnchor anchor = (Thing_SpatialAnchor)ThingMaker.MakeThing(ThingDef.Named("ProjectOvermind_SpatialAnchor"));
                anchor.Setup(targetCell, CasterPawn.Map, AnchorRadius, AnchorDurationTicks);
                
                Log.Message($"[Spatial Anchor] Spawning anchor at {targetCell}");
                GenSpawn.Spawn(anchor, targetCell, CasterPawn.Map);

                // Enhanced visual effects
                FleckMaker.Static(targetCell.ToVector3Shifted(), CasterPawn.Map, FleckDefOf.PsycastAreaEffect, 5f);
                
                // Create multiple ring effects to show the area of effect
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * 45f * Mathf.Deg2Rad;
                    IntVec3 edgeCell = targetCell + new IntVec3(
                        Mathf.RoundToInt(AnchorRadius * Mathf.Cos(angle)),
                        0,
                        Mathf.RoundToInt(AnchorRadius * Mathf.Sin(angle))
                    );
                    if (edgeCell.InBounds(CasterPawn.Map))
                    {
                        FleckMaker.ThrowDustPuffThick(edgeCell.ToVector3Shifted(), CasterPawn.Map, 1f, new Color(0.5f, 0.3f, 1f));
                    }
                }
                
                // Sound effect
                SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(targetCell, CasterPawn.Map));

                // Success message
                Messages.Message($"Spatial Anchor created at {targetCell}. Hostile pawns within {AnchorRadius} tiles will be affected.", 
                    new LookTargets(targetCell, CasterPawn.Map), MessageTypeDefOf.PositiveEvent, false);

                Log.Message($"[Spatial Anchor] Successfully created anchor at {targetCell}, duration: {AnchorDurationTicks} ticks");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Spatial Anchor cast failed: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }
    }

    /// <summary>
    /// Spatial Anchor thing - invisible anchor that applies debuff to hostile pawns in radius
    /// </summary>
    public class Thing_SpatialAnchor : Thing
    {
        private IntVec3 centerPosition;
        private float radius;
        private int expiryTick;
        private int lastCheckTick;
        private const int CheckInterval = 60; // Check every second
        private static readonly HediffDef GraviticPullHediff = HediffDef.Named("ProjectOvermind_GraviticPull");

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref centerPosition, "centerPosition");
            Scribe_Values.Look(ref radius, "radius", 10f);
            Scribe_Values.Look(ref expiryTick, "expiryTick");
            Scribe_Values.Look(ref lastCheckTick, "lastCheckTick");
        }

        public void Setup(IntVec3 center, Map map, float rad, int durationTicks)
        {
            centerPosition = center;
            radius = rad;
            expiryTick = Find.TickManager.TicksGame + durationTicks;
            lastCheckTick = Find.TickManager.TicksGame;
            
            if (Prefs.DevMode)
            {
                Log.Message($"[Spatial Anchor] Setup: center={center}, radius={rad}, duration={durationTicks} ticks ({durationTicks/60}s)");
            }
        }

        protected override void Tick()
        {
            base.Tick();

            if (!Spawned || Map == null)
            {
                Log.Message("[Spatial Anchor] Thing not spawned or map is null, destroying");
                Destroy(DestroyMode.Vanish);
                return;
            }

            int currentTick = Find.TickManager.TicksGame;

            // Check if anchor has expired
            if (currentTick >= expiryTick)
            {
                Log.Message($"[Spatial Anchor] Anchor expired at tick {currentTick}, destroying");
                Destroy(DestroyMode.Vanish);
                return;
            }

            // Process gravitic pull effects every CheckInterval ticks
            if (currentTick - lastCheckTick >= CheckInterval)
            {
                lastCheckTick = currentTick;
                ProcessGraviticEffects();
            }
            
            // Continuous visual effects every tick (show anchor is active)
            if (currentTick % 10 == 0) // Every 10 ticks (~0.17 seconds)
            {
                // Center pulsing effect
                FleckMaker.ThrowDustPuffThick(centerPosition.ToVector3Shifted(), Map, 1.5f, new Color(0.5f, 0.3f, 1f, 0.8f));
                
                // Radius ring effect - show 8 points around the circumference
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * 45f * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(radius * Mathf.Cos(angle), 0f, radius * Mathf.Sin(angle));
                    Vector3 ringPos = centerPosition.ToVector3Shifted() + offset;
                    
                    if (new IntVec3(ringPos).InBounds(Map))
                    {
                        FleckMaker.ThrowDustPuffThick(ringPos, Map, 0.8f, new Color(0.5f, 0.7f, 1f, 0.6f));
                    }
                }
            }
        }

        private void ProcessGraviticEffects()
        {
            if (Map == null) return;

            try
            {
                // Find all hostile pawns in radius
                List<Pawn> affectedPawns = new List<Pawn>();
                
                foreach (Pawn pawn in Map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn == null || pawn.Dead || pawn.Destroyed) continue;
                    if (!pawn.HostileTo(Faction.OfPlayer)) continue; // Only affect hostiles
                    
                    float distance = pawn.Position.DistanceTo(centerPosition);
                    if (distance <= radius)
                    {
                        affectedPawns.Add(pawn);
                    }
                }

                if (affectedPawns.Count > 0)
                {
                    Log.Message($"[Spatial Anchor] Found {affectedPawns.Count} hostile pawns in radius at {centerPosition}");
                }

                // Apply gravitic pull hediff to affected pawns
                foreach (Pawn pawn in affectedPawns)
                {
                    ApplyGraviticPull(pawn);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] Spatial Anchor ProcessGraviticEffects error: {ex.Message}");
            }
        }

        private void ApplyGraviticPull(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || pawn.health == null) return;

            try
            {
                // Apply or refresh the gravitic pull hediff
                Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(GraviticPullHediff);
                
                if (existingHediff != null)
                {
                    // Refresh duration
                    HediffComp_Disappears comp = existingHediff.TryGetComp<HediffComp_Disappears>();
                    if (comp != null)
                    {
                        comp.ticksToDisappear = 120; // 2 seconds buffer
                    }
                }
                else
                {
                    // Add new hediff
                    Hediff newHediff = HediffMaker.MakeHediff(GraviticPullHediff, pawn);
                    pawn.health.AddHediff(newHediff);
                    Log.Message($"[Spatial Anchor] Applied gravitic pull to {pawn.LabelShort}");
                }

                // 10% chance to pull toward center (every second)
                if (Rand.Chance(0.1f))
                {
                    IntVec3 pawnPos = pawn.Position;
                    
                    // Find the adjacent cell closest to center
                    IntVec3 bestCell = pawnPos;
                    float bestDistance = float.MaxValue;
                    
                    foreach (IntVec3 adjCell in GenAdj.CardinalDirections.Select(dir => pawnPos + dir))
                    {
                        if (!adjCell.InBounds(Map) || !adjCell.Walkable(Map)) continue;
                        
                        float distance = adjCell.DistanceTo(centerPosition);
                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            bestCell = adjCell;
                        }
                    }
                    
                    if (bestCell != pawnPos && pawn.pather != null)
                    {
                        pawn.pather.TryRecoverFromUnwalkablePosition(false);
                        pawn.Position = bestCell;
                        pawn.Notify_Teleported(false, false);
                        
                        // Small mote to show pull effect
                        FleckMaker.ThrowAirPuffUp(pawn.DrawPos, Map);
                        Log.Message($"[Spatial Anchor] Pulled {pawn.LabelShort} from {pawnPos} to {bestCell}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Overmind] ApplyGraviticPull error for {pawn?.LabelShort}: {ex.Message}");
            }
        }
    }
}
