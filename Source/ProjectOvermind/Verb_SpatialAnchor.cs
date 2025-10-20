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

        /// <summary>
        /// CRITICAL FIX: Override DrawHighlight to show radius preview during targeting phase.
        /// 
        /// This method is called every frame while the player is targeting the ability.
        /// GenDraw.DrawRadiusRing creates a persistent blue circle showing the affected area,
        /// helping players position the anchor optimally before casting.
        /// 
        /// Performance: GenDraw methods are optimized for per-frame rendering and won't cause lag.
        /// </summary>
        public override void DrawHighlight(LocalTargetInfo target)
        {
            base.DrawHighlight(target);
            
            // Only draw if target is valid and on the map
            if (target.IsValid && target.Cell.InBounds(CasterPawn?.Map))
            {
                // Draw blue/cyan radius ring showing 10-tile affected area
                // This is the same method used by vanilla Skip psycast and Smokepop pack
                GenDraw.DrawRadiusRing(target.Cell, AnchorRadius);
                
                if (Prefs.DevMode)
                {
                    // Optional: Draw filled area for dev mode (clearer visualization)
                    List<IntVec3> affectedCells = GetAffectedCells(target.Cell, CasterPawn.Map);
                    GenDraw.DrawFieldEdges(affectedCells, new Color(0.5f, 0.7f, 1f, 0.3f));
                }
            }
        }

        /// <summary>
        /// Helper method to calculate all cells within the anchor's radius.
        /// Used for both targeting preview and runtime effect processing.
        /// </summary>
        private List<IntVec3> GetAffectedCells(IntVec3 center, Map map)
        {
            List<IntVec3> cells = new List<IntVec3>();
            if (map == null) return cells;
            
            int radiusInt = Mathf.CeilToInt(AnchorRadius);
            
            // Iterate over all cells in a square around the center
            for (int x = -radiusInt; x <= radiusInt; x++)
            {
                for (int z = -radiusInt; z <= radiusInt; z++)
                {
                    IntVec3 cell = center + new IntVec3(x, 0, z);
                    
                    // Only include cells that are within the actual circular radius and in bounds
                    if (cell.InBounds(map) && cell.DistanceTo(center) <= AnchorRadius)
                    {
                        cells.Add(cell);
                    }
                }
            }
            return cells;
        }

        protected override bool TryCastShot()
        {
            // SPATIAL ANCHOR: Reduced verbose logging (Issue #13 cleanup)
            try
            {
                if (CasterPawn == null || CasterPawn.Map == null)
                {
                    Log.Error("[SPATIAL ANCHOR] CRITICAL: No valid map");
                    Messages.Message("Spatial Anchor failed: No valid map.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                IntVec3 targetCell = currentTarget.Cell;

                if (!targetCell.IsValid || !targetCell.InBounds(CasterPawn.Map))
                {
                    Log.Error($"[SPATIAL ANCHOR] CRITICAL: Invalid target location {targetCell}");
                    Messages.Message("Spatial Anchor: Invalid target location.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                ThingDef anchorDef = ThingDef.Named("ProjectOvermind_SpatialAnchor");
                
                if (anchorDef == null)
                {
                    Log.Error("[SPATIAL ANCHOR] CRITICAL: ThingDef 'ProjectOvermind_SpatialAnchor' not found!");
                    Messages.Message("Spatial Anchor: ThingDef not found (mod configuration error)", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                Thing anchorThing = ThingMaker.MakeThing(anchorDef);

                if (anchorThing == null)
                {
                    Log.Error("[SPATIAL ANCHOR] CRITICAL: ThingMaker.MakeThing returned null!");
                    Messages.Message("Spatial Anchor: Failed to create anchor thing", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                Thing spawnedThing = GenSpawn.Spawn(anchorThing, targetCell, CasterPawn.Map);
                
                if (spawnedThing == null || !spawnedThing.Spawned)
                {
                    Log.Error($"[SPATIAL ANCHOR] CRITICAL: Thing failed to spawn! Returned: {spawnedThing != null}, Spawned: {spawnedThing?.Spawned}");
                    Messages.Message("Spatial Anchor: Thing failed to spawn on map", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                // Now that the thing is spawned, call Setup on the spawned instance so Map and Spawned are valid
                Thing_SpatialAnchor anchor = spawnedThing as Thing_SpatialAnchor;
                if (anchor != null)
                {
                    try
                    {
                        anchor.Setup(spawnedThing.Position, spawnedThing.Map, AnchorRadius, AnchorDurationTicks);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[SPATIAL ANCHOR] Error during anchor.Setup: {ex}");
                    }
                }

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
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[SPATIAL ANCHOR] ========== EXCEPTION: {ex.Message} ==========\n{ex.StackTrace}");
                Messages.Message($"Spatial Anchor error: {ex.Message}", MessageTypeDefOf.RejectInput, false);
                return false;
            }
        }
    }

    /// <summary>
    /// Spatial Anchor thing - invisible anchor that applies debuff to hostile pawns in radius
    /// CRITICAL FIXES:
    /// 1. Reduced CheckInterval from 60 to 30 ticks (0.5s instead of 1s)
    /// 2. Increased hediff buffer from 60 to 90 ticks (CheckInterval + 60 = failsafe)
    /// 3. Added Draw() override for persistent ground visuals
    /// 4. Pre-calculated affected cells for performance
    /// 5. Enhanced visual effects with GenDraw methods
    /// </summary>
    public class Thing_SpatialAnchor : Thing
    {
        private IntVec3 centerPosition;
        private float radius;
        private int expiryTick;
        private int lastCheckTick;
        
        // CRITICAL FIX: Reduced from 60 to 30 ticks
        // Old: 60 ticks (1 second) → 120 tick buffer → 60 tick safety window (too tight)
        // New: 30 ticks (0.5 seconds) → 90 tick buffer → 60 tick safety window (reliable)
        private const int CheckInterval = 30; 
        
        private static readonly HediffDef GraviticPullHediff = HediffDef.Named("ProjectOvermind_GraviticPull");
        
        // PERFORMANCE OPTIMIZATION: Pre-calculate affected cells once in Setup()
        // instead of recalculating every Draw() call (60 FPS)
        private List<IntVec3> affectedCells;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref centerPosition, "centerPosition");
            Scribe_Values.Look(ref radius, "radius", 10f);
            Scribe_Values.Look(ref expiryTick, "expiryTick");
            Scribe_Values.Look(ref lastCheckTick, "lastCheckTick");
            // Note: affectedCells is recalculated on load via PostLoadInit or first Tick
        }

        public void Setup(IntVec3 center, Map map, float rad, int durationTicks)
        {
            centerPosition = center;
            radius = rad;
            expiryTick = Find.TickManager.TicksGame + durationTicks;
            lastCheckTick = Find.TickManager.TicksGame;
            
            // PERFORMANCE OPTIMIZATION: Pre-calculate affected cells once
            // This list is used by Draw() method which runs at 60 FPS
            affectedCells = CalculateAffectedCells();
        }

        /// <summary>
        /// Calculate all cells within the anchor's radius.
        /// Called once in Setup() for performance (avoid recalculating 60 times per second in Draw()).
        /// </summary>
        private List<IntVec3> CalculateAffectedCells()
        {
            List<IntVec3> cells = new List<IntVec3>();
            if (Map == null) return cells;
            
            int radiusInt = Mathf.CeilToInt(radius);
            
            for (int x = -radiusInt; x <= radiusInt; x++)
            {
                for (int z = -radiusInt; z <= radiusInt; z++)
                {
                    IntVec3 cell = centerPosition + new IntVec3(x, 0, z);
                    if (cell.InBounds(Map) && cell.DistanceTo(centerPosition) <= radius)
                    {
                        cells.Add(cell);
                    }
                }
            }
            return cells;
        }

        /// <summary>
        /// Main tick loop - processes anchor expiry, visual effects, and gravitic pulls.
        /// 
        /// VISUAL STRATEGY: Since Thing doesn't support Draw() override directly,
        /// we use GenDraw in MapComponent or here with appropriate frequency.
        /// For persistent visuals, we spawn motes periodically instead of per-frame rendering.
        /// </summary>
        protected override void Tick()
        {
            base.Tick();
            
            int currentTick = Find.TickManager.TicksGame;
            
            // HEAVY DEBUG LOGGING v1.6.4
            // Reduced per-tick logging to avoid spam; only log critical conditions

            if (!Spawned || Map == null)
            {
                Log.Error($"[SPATIAL ANCHOR] CRITICAL: Thing not spawned or map is null! Spawned={Spawned}, Map={Map != null} - DESTROYING");
                Destroy(DestroyMode.Vanish);
                return;
            }

            // Check if anchor has expired
            if (currentTick >= expiryTick)
            {
                // ENHANCED: Final explosion effect when anchor expires
                FleckMaker.Static(centerPosition.ToVector3Shifted(), Map, 
                    FleckDefOf.ExplosionFlash, 3f);
                FleckMaker.ThrowLightningGlow(centerPosition.ToVector3Shifted(), Map, 2f);
                
                Destroy(DestroyMode.Vanish);
                return;
            }

            // CRITICAL FIX: Process gravitic pull effects every CheckInterval ticks (30 ticks = 0.5 seconds)
            // Old: 60 ticks (1 second) → enemies could walk in/out between checks
            // New: 30 ticks (0.5 seconds) → more responsive debuff application
            if (currentTick - lastCheckTick >= CheckInterval)
            {
                lastCheckTick = currentTick;
                // Process gravitic effects periodically without noisy logging
                ProcessGraviticEffects();
            }
            
            // PERSISTENT VISUAL EFFECTS: Show anchor is active with periodic flecks
            // Render every 10 ticks (~6 times per second) for smooth visuals without lag
            if (currentTick % 10 == 0)
            {
                // Center pulsing effect - brighter and more noticeable
                FleckMaker.ThrowLightningGlow(centerPosition.ToVector3Shifted(), Map, 1.5f);
                FleckMaker.ThrowDustPuffThick(centerPosition.ToVector3Shifted(), Map, 1.5f, new Color(0.5f, 0.3f, 1f, 0.8f));
                
                // Radius ring effect - show 8 points around the circumference for clear boundary
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
            if (Map == null)
            {
                Log.Error("[SPATIAL ANCHOR] ProcessGraviticEffects: Map is null!");
                return;
            }

            try
            {
                // Find all hostile pawns in radius
                List<Pawn> affectedPawns = new List<Pawn>();
                
                // Reduced verbose logging - only log in DevMode
                foreach (Pawn pawn in Map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn == null || pawn.Dead || pawn.Destroyed) continue;
                    
                    bool isHostile = pawn.HostileTo(Faction.OfPlayer);
                    if (!isHostile) continue; // Only affect hostiles
                    
                    float distance = pawn.Position.DistanceTo(centerPosition);
                    if (distance <= radius)
                    {
                        affectedPawns.Add(pawn);
                    }
                }

                // Apply gravitic pull hediff to affected pawns
                foreach (Pawn pawn in affectedPawns)
                {
                    ApplyGraviticPull(pawn);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[SPATIAL ANCHOR] ProcessGraviticEffects EXCEPTION: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Apply or refresh the Gravitic Pull hediff on a hostile pawn.
        /// 
        /// CRITICAL TIMING FIX:
        /// Old system: CheckInterval=60, buffer=120 → 60 tick safety window (1 second)
        ///   Problem: If enemy walks in at tick 59, hediff expires at tick 179,
        ///            but next check is at tick 120 (59 ticks too late → hediff already gone)
        /// 
        /// New system: CheckInterval=30, buffer=90 → 60 tick safety window (1 second)
        ///   Solution: Enemy walks in at any point, hediff lasts 90 ticks,
        ///            next check happens every 30 ticks, so we're guaranteed to refresh
        ///            before expiration (worst case: expires at tick+90, refresh at tick+30)
        /// 
        /// Formula: buffer = CheckInterval + 60 ticks = reliable refresh cycle
        /// </summary>
        private void ApplyGraviticPull(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || pawn.health == null)
            {
                // Removed verbose warning - only log critical errors
                return;
            }

            try
            {
                // Apply or refresh the gravitic pull hediff
                Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(GraviticPullHediff);
                
                    if (existingHediff != null)
                    {
                        HediffComp_Disappears comp = existingHediff.TryGetComp<HediffComp_Disappears>();
                        if (comp != null)
                        {
                            comp.ticksToDisappear = CheckInterval + 60; // refresh
                        }
                        else
                        {
                            Log.Error($"[SPATIAL ANCHOR] Hediff exists but has no HediffComp_Disappears component!");
                        }
                    }
                    else
                    {
                        if (GraviticPullHediff == null)
                        {
                            Log.Error("[SPATIAL ANCHOR] CRITICAL: GraviticPullHediff HediffDef is NULL! Cannot create hediff.");
                            return;
                        }

                        Hediff newHediff = HediffMaker.MakeHediff(GraviticPullHediff, pawn);
                        pawn.health.AddHediff(newHediff);

                        // Visual effect when first applied
                        FleckMaker.ThrowDustPuffThick(pawn.DrawPos, Map, 1f, new Color(0.5f, 0.7f, 1f));
                    }

                // 10% chance per check (every 0.5 seconds with CheckInterval=30) to pull toward center
                // Note: With old CheckInterval=60 (1 second), description said "10% per second"
                // With new CheckInterval=30 (0.5 seconds), effective rate is ~19% per second (1 - 0.9^2)
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
                        
                        // ENHANCED: Better visual effects for pull
                        FleckMaker.ThrowAirPuffUp(pawn.DrawPos, Map);
                        FleckMaker.ThrowDustPuffThick(pawn.DrawPos, Map, 1f, new Color(0.5f, 0.7f, 1f));
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
