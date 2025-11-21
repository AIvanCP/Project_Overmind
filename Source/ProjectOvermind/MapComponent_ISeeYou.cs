using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// MapComponent that manages "I See You" reveal sessions
    /// Upgraded to detect anomalies (Sightstealer, CompAnomaly, etc.) while excluding player buildings/pawns and conduits
    /// </summary>
    public class MapComponent_ISeeYou : MapComponent
    {
        // Debug flag - set to true only when troubleshooting
        private const bool enableRevealDebug = false;

        private class RevealSession
        {
            public Pawn caster;
            public int expiryTick;
            public List<RevealedEntity> revealedEntities = new List<RevealedEntity>();
        }

        private class RevealedEntity
        {
            public Thing thing;
            public Mote exclamationMote;
            // Store original hiding state for restoration
            public List<ThingComp> hidingComps = new List<ThingComp>();
            public List<Hediff> hidingHediffs = new List<Hediff>();
            public Dictionary<ThingComp, object> originalCompStates = new Dictionary<ThingComp, object>();
            // Track damage during reveal
            public bool wasDamaged = false;
            public float initialHealth = 0f;
        }

        private RevealSession activeSession;

        public MapComponent_ISeeYou(Map map) : base(map)
        {
        }

        /// <summary>
        /// Start a new reveal session
        /// </summary>
        /// <returns>Number of hostile entities detected</returns>
        public int StartReveal(Pawn caster, int durationTicks)
        {
            try
            {
                if (caster == null || map == null)
                {
                    Log.Warning("[I See You] StartReveal called with null caster or map.");
                    return 0;
                }

                // End any existing session
                if (activeSession != null)
                {
                    EndReveal(false);
                }

                // Create new session
                activeSession = new RevealSession
                {
                    caster = caster,
                    expiryTick = Find.TickManager.TicksGame + durationTicks
                };

                // Find and reveal hidden entities
                int hostileCount = FindAndRevealHiddenEntities();

                if (enableRevealDebug)
                {
                    Log.Message($"[I See You] Started reveal session: {activeSession.revealedEntities.Count} entities revealed ({hostileCount} hostile) for {durationTicks} ticks.");
                }

                return hostileCount;
            }
            catch (Exception ex)
            {
                Log.Error($"[I See You] Error in StartReveal: {ex}");
                return 0;
            }
        }

        /// <summary>
        /// Find all hidden anomaly entities on the map and mark them as revealed
        /// Filters for: pawns with anomaly comps, creatures with hiding mechanics
        /// Excludes: player buildings/pawns, conduits, utility objects
        /// </summary>
        /// <returns>Number of hostile entities detected</returns>
        private int FindAndRevealHiddenEntities()
        {
            int hostileCount = 0;
            
            try
            {
                if (map == null || activeSession == null)
                    return 0;

                // Strategy: scan only pawns (no buildings/conduits)
                List<Pawn> allPawns = map.mapPawns.AllPawnsSpawned.ToList();
                
                if (allPawns == null || allPawns.Count == 0)
                    return 0;

                foreach (Pawn pawn in allPawns)
                {
                    if (pawn == null || pawn.Destroyed)
                        continue;

                    // FILTER 1: Skip player faction pawns
                    if (IsPlayerThing(pawn))
                        continue;

                    // FILTER 2: Check if pawn is a revealable anomaly/hidden creature
                    if (IsRevealableAnomaly(pawn))
                    {
                        RevealEntity(pawn);
                        
                        // Count hostile entities
                        if (pawn.HostileTo(Faction.OfPlayer))
                        {
                            hostileCount++;
                        }
                    }
                }

                if (enableRevealDebug)
                {
                    Log.Message($"[I See You] Scan complete: {hostileCount} hostile anomalies detected.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[I See You] Error in FindAndRevealHiddenEntities: {ex}");
            }
            
            return hostileCount;
        }

        /// <summary>
        /// Check if a revealed entity has taken damage
        /// </summary>
        private void CheckForDamage(RevealedEntity revealed)
        {
            try
            {
                if (revealed == null || revealed.thing == null || revealed.wasDamaged)
                    return;

                Pawn pawn = revealed.thing as Pawn;
                if (pawn == null || pawn.health == null)
                    return;

                // Check if health decreased
                float currentHealth = pawn.health.summaryHealth.SummaryHealthPercent;
                if (currentHealth < revealed.initialHealth - 0.01f) // Small threshold to account for rounding
                {
                    revealed.wasDamaged = true;
                    
                    if (enableRevealDebug)
                    {
                        Log.Message($"[I See You] {pawn.LabelShort} was damaged during reveal - will remain visible");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[I See You] Error checking damage: {ex}");
            }
        }

        /// <summary>
        /// Check if a pawn is a revealable anomaly or hidden creature
        /// Targets: Sightstealer, CompAnomaly, CompHider, scripted invisibility
        /// </summary>
        private bool IsRevealableAnomaly(Pawn pawn)
        {
            try
            {
                if (pawn == null)
                    return false;

                // Check 1: CompAnomaly (Anomaly DLC entities)
                // Use reflection to check for CompAnomaly since it might not be available without DLC
                ThingComp anomalyComp = pawn.AllComps?.FirstOrDefault(c => c.GetType().Name == "CompAnomaly");
                if (anomalyComp != null)
                {
                    if (enableRevealDebug)
                        Log.Message($"[I See You] Detected CompAnomaly on {pawn.LabelShort}");
                    return true;
                }

                // Check 2: CompSightstealer (specific Anomaly DLC enemy)
                ThingComp sightstealerComp = pawn.AllComps?.FirstOrDefault(c => c.GetType().Name.Contains("Sightstealer"));
                if (sightstealerComp != null)
                {
                    if (enableRevealDebug)
                        Log.Message($"[I See You] Detected Sightstealer on {pawn.LabelShort}");
                    return true;
                }

                // Check 3: CompHider or custom hiding comps
                ThingComp hiderComp = pawn.AllComps?.FirstOrDefault(c => 
                    c.GetType().Name.Contains("Hider") || 
                    c.GetType().Name.Contains("Invisible") ||
                    c.GetType().Name.Contains("Stealth"));
                if (hiderComp != null)
                {
                    if (enableRevealDebug)
                        Log.Message($"[I See You] Detected hiding comp {hiderComp.GetType().Name} on {pawn.LabelShort}");
                    return true;
                }

                // Check 4: Hediffs with "hidden", "invisible", "stealth", "concealed" in defName
                if (pawn.health?.hediffSet?.hediffs != null)
                {
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                    {
                        if (hediff?.def?.defName == null)
                            continue;

                        string defNameLower = hediff.def.defName.ToLower();
                        if (defNameLower.Contains("hidden") || 
                            defNameLower.Contains("invisible") || 
                            defNameLower.Contains("stealth") ||
                            defNameLower.Contains("concealed") ||
                            defNameLower.Contains("cloaked"))
                        {
                            if (enableRevealDebug)
                                Log.Message($"[I See You] Detected hidden hediff: {hediff.def.defName} on {pawn.LabelShort}");
                            return true;
                        }

                        // Anomaly-specific hediffs
                        if (defNameLower.Contains("revenant") || 
                            defNameLower.Contains("ghoul") ||
                            defNameLower.Contains("shambler") ||
                            defNameLower.Contains("anomaly"))
                        {
                            if (enableRevealDebug)
                                Log.Message($"[I See You] Detected anomaly hediff: {hediff.def.defName} on {pawn.LabelShort}");
                            return true;
                        }
                    }
                }

                // Check 5: Mental states (Unobserved, lurking, etc.)
                if (pawn.MentalStateDef != null)
                {
                    string mentalStateDefName = pawn.MentalStateDef.defName.ToLower();
                    if (mentalStateDefName.Contains("unobserved") || 
                        mentalStateDefName.Contains("hidden") ||
                        mentalStateDefName.Contains("lurking"))
                    {
                        if (enableRevealDebug)
                            Log.Message($"[I See You] Detected hidden mental state: {pawn.LabelShort}");
                        return true;
                    }
                }

                // Check 6: Pawn kind (Shambler, Lurker, Creeper, Stalker)
                if (pawn.kindDef?.defName != null)
                {
                    string kindDefName = pawn.kindDef.defName.ToLower();
                    if (kindDefName.Contains("shambler") || 
                        kindDefName.Contains("lurker") ||
                        kindDefName.Contains("creeper") ||
                        kindDefName.Contains("stalker") ||
                        kindDefName.Contains("sightstealer"))
                    {
                        if (enableRevealDebug)
                            Log.Message($"[I See You] Detected stealth pawn kind: {pawn.LabelShort}");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                if (enableRevealDebug)
                    Log.Warning($"[I See You] Error checking if pawn is revealable anomaly: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a thing belongs to the player (building or pawn)
        /// </summary>
        private bool IsPlayerThing(Thing thing)
        {
            if (thing == null)
                return false;

            // Player faction check
            if (thing.Faction == Faction.OfPlayer)
                return true;

            return false;
        }

        /// <summary>
        /// Check if a thing is a hidden utility object (conduit, power comp, optimization object)
        /// These should NEVER be revealed
        /// </summary>
        private bool IsHiddenUtility(Thing thing)
        {
            if (thing == null || thing.def == null)
                return false;

            try
            {
                // Filter 1: Buildings with low altitude (conduits, underground)
                if (thing.def.category == ThingCategory.Building && 
                    (thing.def.altitudeLayer == AltitudeLayer.Item || 
                     thing.def.altitudeLayer == AltitudeLayer.LowPlant))
                {
                    return true;
                }

                // Filter 2: DefName contains "Conduit"
                if (thing.def.defName.Contains("Conduit"))
                {
                    return true;
                }

                // Filter 3: Hidden/underground buildings
                if (thing.def.building != null && thing.def.building.isInert)
                {
                    return true;
                }

                // Filter 4: Power transmission components
                if (thing.TryGetComp<CompPowerTransmitter>() != null)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reveal an entity by spawning visual markers (non-invasive overlay method)
        /// </summary>
        private void RevealEntity(Thing thing)
        {
            try
            {
                if (thing == null || !thing.Spawned || activeSession == null)
                    return;

                // Create exclamation mote above entity
                MoteText moteText = (MoteText)ThingMaker.MakeThing(ThingDefOf.Mote_Text);
                moteText.exactPosition = thing.DrawPos + new Vector3(0f, 0f, 1f);
                moteText.text = "!";
                moteText.textColor = new Color(1f, 0.2f, 0.2f); // Red
                
                GenSpawn.Spawn(moteText, thing.Position, map);
                
                Mote mote = moteText;

                // Add to revealed list
                RevealedEntity revealedEntity = new RevealedEntity
                {
                    thing = thing,
                    exclamationMote = mote
                };
                
                // Store initial health for damage tracking
                if (thing is Pawn pawn && pawn.health != null)
                {
                    revealedEntity.initialHealth = pawn.health.summaryHealth.SummaryHealthPercent;
                }
                
                activeSession.revealedEntities.Add(revealedEntity);

                // Force reveal - disable hiding mechanisms
                ForceReveal(thing, revealedEntity);

                // Flash cell to highlight
                map.debugDrawer.FlashCell(thing.Position, 0.8f, "reveal", 50);

                if (enableRevealDebug)
                {
                    Log.Message($"[I See You] Revealed: {thing.Label} at {thing.Position}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[I See You] Error revealing entity: {ex}");
            }
        }

        /// <summary>
        /// Force reveal a hidden entity by disabling its hiding components/hediffs
        /// Uses reflection for mod compatibility - safely disables hiding mechanisms
        /// </summary>
        private void ForceReveal(Thing thing, RevealedEntity revealedEntity)
        {
            try
            {
                if (thing == null || !(thing is Pawn pawn))
                    return;

                // Find and disable hiding components
                if (pawn.AllComps != null)
                {
                    foreach (ThingComp comp in pawn.AllComps)
                    {
                        if (comp == null)
                            continue;

                        string typeName = comp.GetType().Name;
                        
                        // Check if this is a hiding/invisibility comp
                        if (typeName.Contains("Hider") || 
                            typeName.Contains("Invisible") || 
                            typeName.Contains("Stealth") ||
                            typeName.Contains("Sightstealer") ||
                            typeName.Contains("Concealed"))
                        {
                            // Store this comp for restoration
                            revealedEntity.hidingComps.Add(comp);

                            // Try to disable using reflection
                            TryDisableHidingComp(comp, revealedEntity);

                            if (enableRevealDebug)
                            {
                                Log.Message($"[I See You] Disabled hiding comp: {typeName} on {pawn.LabelShort}");
                            }
                        }
                    }
                }

                // Find and remove hiding hediffs temporarily
                if (pawn.health?.hediffSet?.hediffs != null)
                {
                    List<Hediff> hediffsToRemove = new List<Hediff>();
                    
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                    {
                        if (hediff?.def?.defName == null)
                            continue;

                        string defNameLower = hediff.def.defName.ToLower();
                        
                        // Check if this is a hiding hediff
                        if (defNameLower.Contains("hidden") || 
                            defNameLower.Contains("invisible") || 
                            defNameLower.Contains("stealth") ||
                            defNameLower.Contains("concealed") ||
                            defNameLower.Contains("cloaked"))
                        {
                            hediffsToRemove.Add(hediff);
                            revealedEntity.hidingHediffs.Add(hediff);

                            if (enableRevealDebug)
                            {
                                Log.Message($"[I See You] Removing hiding hediff: {hediff.def.defName} from {pawn.LabelShort}");
                            }
                        }
                    }

                    // Remove hiding hediffs
                    foreach (Hediff hediff in hediffsToRemove)
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[I See You] Error in ForceReveal: {ex}");
            }
        }

        /// <summary>
        /// Try to disable a hiding component using reflection
        /// Looks for common fields/properties: isHidden, hidden, enabled, active
        /// </summary>
        private void TryDisableHidingComp(ThingComp comp, RevealedEntity revealedEntity)
        {
            try
            {
                var compType = comp.GetType();

                // Common field/property names for hiding state
                string[] hidingFieldNames = { "isHidden", "hidden", "enabled", "active", "invisible" };

                foreach (string fieldName in hidingFieldNames)
                {
                    // Try field
                    var field = compType.GetField(fieldName, 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance);

                    if (field != null && field.FieldType == typeof(bool))
                    {
                        // Store original value
                        object originalValue = field.GetValue(comp);
                        revealedEntity.originalCompStates[comp] = originalValue;

                        // Disable (set to false or true depending on field name)
                        bool newValue = (fieldName == "enabled" || fieldName == "active") ? false : false;
                        field.SetValue(comp, newValue);

                        if (enableRevealDebug)
                        {
                            Log.Message($"[I See You] Set {fieldName} = {newValue} on {comp.GetType().Name}");
                        }
                        return;
                    }

                    // Try property
                    var property = compType.GetProperty(fieldName, 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance);

                    if (property != null && property.PropertyType == typeof(bool) && property.CanWrite)
                    {
                        // Store original value
                        object originalValue = property.GetValue(comp);
                        revealedEntity.originalCompStates[comp] = originalValue;

                        // Disable
                        bool newValue = (fieldName == "enabled" || fieldName == "active") ? false : false;
                        property.SetValue(comp, newValue);

                        if (enableRevealDebug)
                        {
                            Log.Message($"[I See You] Set {fieldName} = {newValue} on {comp.GetType().Name}");
                        }
                        return;
                    }
                }

                // Try calling Disable/Reveal methods if they exist
                var disableMethod = compType.GetMethod("Disable", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (disableMethod != null && disableMethod.GetParameters().Length == 0)
                {
                    disableMethod.Invoke(comp, null);
                    if (enableRevealDebug)
                    {
                        Log.Message($"[I See You] Called Disable() on {comp.GetType().Name}");
                    }
                    return;
                }

                var revealMethod = compType.GetMethod("Reveal", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (revealMethod != null && revealMethod.GetParameters().Length == 0)
                {
                    revealMethod.Invoke(comp, null);
                    if (enableRevealDebug)
                    {
                        Log.Message($"[I See You] Called Reveal() on {comp.GetType().Name}");
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                if (enableRevealDebug)
                {
                    Log.Warning($"[I See You] Could not disable hiding comp {comp.GetType().Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Tick the component to manage active reveals
        /// </summary>
        public override void MapComponentTick()
        {
            base.MapComponentTick();

            try
            {
                if (activeSession == null)
                    return;

                // Check if caster is still valid
                if (activeSession.caster == null || 
                    activeSession.caster.Dead || 
                    activeSession.caster.Map != map)
                {
                    EndReveal(true);
                    return;
                }

                // Check if duration expired
                if (Find.TickManager.TicksGame >= activeSession.expiryTick)
                {
                    EndReveal(true);
                    return;
                }

                // Update mote positions to follow moving entities
                UpdateRevealedEntities();
            }
            catch (Exception ex)
            {
                Log.Error($"[I See You] Error in MapComponentTick: {ex}");
            }
        }

        /// <summary>
        /// Update positions of reveal markers to follow entities
        /// </summary>
        private void UpdateRevealedEntities()
        {
            try
            {
                if (activeSession == null || activeSession.revealedEntities == null)
                    return;

                // Remove destroyed entities and update mote positions
                for (int i = activeSession.revealedEntities.Count - 1; i >= 0; i--)
                {
                    RevealedEntity revealed = activeSession.revealedEntities[i];
                    
                    if (revealed.thing == null || revealed.thing.Destroyed || !revealed.thing.Spawned)
                    {
                        // Entity destroyed or despawned
                        if (revealed.exclamationMote != null && !revealed.exclamationMote.Destroyed)
                        {
                            revealed.exclamationMote.Destroy();
                        }
                        activeSession.revealedEntities.RemoveAt(i);
                        continue;
                    }

                    // Check if entity was damaged
                    CheckForDamage(revealed);

                    // Periodically spawn new exclamation motes (since they fade quickly)
                    if (Find.TickManager.TicksGame % 120 == 0) // Every 2 seconds
                    {
                        RevealEntity(revealed.thing);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[I See You] Error updating revealed entities: {ex}");
            }
        }

        /// <summary>
        /// Restore original hiding state for a revealed entity
        /// </summary>
        private void RestoreHidden(RevealedEntity revealed)
        {
            try
            {
                if (revealed == null || revealed.thing == null)
                    return;

                Pawn pawn = revealed.thing as Pawn;
                if (pawn == null)
                    return;

                // Do NOT restore hiding if pawn was damaged during reveal
                // This keeps attacked enemies visible
                if (revealed.wasDamaged)
                {
                    if (enableRevealDebug)
                    {
                        Log.Message($"[I See You] Skipping restoration for {pawn.LabelShort} - was damaged during reveal");
                    }
                    return;
                }

                // Restore hiding component states
                foreach (ThingComp comp in revealed.hidingComps)
                {
                    if (comp == null)
                        continue;

                    // Try to restore original state
                    if (revealed.originalCompStates.ContainsKey(comp))
                    {
                        object originalValue = revealed.originalCompStates[comp];
                        TryRestoreHidingComp(comp, originalValue);
                    }
                    else
                    {
                        // Try to re-enable using common methods
                        var compType = comp.GetType();
                        var enableMethod = compType.GetMethod("Enable", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (enableMethod != null && enableMethod.GetParameters().Length == 0)
                        {
                            enableMethod.Invoke(comp, null);
                        }
                    }
                }

                // Re-add hiding hediffs
                if (pawn.health != null && revealed.hidingHediffs != null)
                {
                    foreach (Hediff hediff in revealed.hidingHediffs)
                    {
                        if (hediff?.def != null)
                        {
                            // Re-add the hediff
                            pawn.health.AddHediff(hediff.def);
                            
                            if (enableRevealDebug)
                            {
                                Log.Message($"[I See You] Restored hiding hediff: {hediff.def.defName} to {pawn.LabelShort}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[I See You] Error restoring hidden state: {ex}");
            }
        }

        /// <summary>
        /// Try to restore a hiding component's original state
        /// </summary>
        private void TryRestoreHidingComp(ThingComp comp, object originalValue)
        {
            try
            {
                if (!(originalValue is bool boolValue))
                    return;

                var compType = comp.GetType();
                string[] hidingFieldNames = { "isHidden", "hidden", "enabled", "active", "invisible" };

                foreach (string fieldName in hidingFieldNames)
                {
                    // Try field
                    var field = compType.GetField(fieldName, 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance);

                    if (field != null && field.FieldType == typeof(bool))
                    {
                        field.SetValue(comp, boolValue);
                        if (enableRevealDebug)
                        {
                            Log.Message($"[I See You] Restored {fieldName} = {boolValue} on {comp.GetType().Name}");
                        }
                        return;
                    }

                    // Try property
                    var property = compType.GetProperty(fieldName, 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance);

                    if (property != null && property.PropertyType == typeof(bool) && property.CanWrite)
                    {
                        property.SetValue(comp, boolValue);
                        if (enableRevealDebug)
                        {
                            Log.Message($"[I See You] Restored {fieldName} = {boolValue} on {comp.GetType().Name}");
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                if (enableRevealDebug)
                {
                    Log.Warning($"[I See You] Could not restore hiding comp: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// End the reveal session
        /// </summary>
        private void EndReveal(bool showMessage)
        {
            try
            {
                if (activeSession == null)
                    return;

                // Restore hiding state for all revealed entities
                if (activeSession.revealedEntities != null)
                {
                    foreach (RevealedEntity revealed in activeSession.revealedEntities)
                    {
                        RestoreHidden(revealed);
                    }
                }

                // Clean up motes
                if (activeSession.revealedEntities != null)
                {
                    foreach (RevealedEntity revealed in activeSession.revealedEntities)
                    {
                        if (revealed.exclamationMote != null && !revealed.exclamationMote.Destroyed)
                        {
                            revealed.exclamationMote.Destroy();
                        }
                    }
                }

                if (showMessage && activeSession.caster != null)
                {
                    Messages.Message(
                        "I See You effect ended.", 
                        MessageTypeDefOf.NeutralEvent, 
                        false
                    );
                }

                if (enableRevealDebug)
                {
                    Log.Message("[I See You] Reveal session ended.");
                }

                activeSession = null;
            }
            catch (Exception ex)
            {
                Log.Error($"[I See You] Error in EndReveal: {ex}");
                activeSession = null;
            }
        }

        /// <summary>
        /// Save/load support
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            
            try
            {
                if (Scribe.mode == LoadSaveMode.Saving && activeSession != null)
                {
                    // Save active session data
                    Pawn caster = activeSession.caster;
                    int expiryTick = activeSession.expiryTick;
                    
                    Scribe_References.Look(ref caster, "ISeeYou_caster");
                    Scribe_Values.Look(ref expiryTick, "ISeeYou_expiryTick");
                }
                else if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    // Load session data
                    Pawn caster = null;
                    int expiryTick = 0;
                    
                    Scribe_References.Look(ref caster, "ISeeYou_caster");
                    Scribe_Values.Look(ref expiryTick, "ISeeYou_expiryTick");
                    
                    if (caster != null && expiryTick > Find.TickManager.TicksGame)
                    {
                        // Restore session
                        int remainingTicks = expiryTick - Find.TickManager.TicksGame;
                        StartReveal(caster, remainingTicks);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[I See You] Error in ExposeData: {ex}");
            }
        }
    }
}
