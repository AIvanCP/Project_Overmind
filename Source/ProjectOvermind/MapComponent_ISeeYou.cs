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
    /// Uses safe overlay system to avoid modifying other mods' invisibility implementations
    /// </summary>
    public class MapComponent_ISeeYou : MapComponent
    {
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

                if (Prefs.DevMode)
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
        /// Find all hidden entities on the map and mark them as revealed
        /// Uses safe detection methods that don't modify other mods' code
        /// </summary>
        /// <returns>Number of hostile entities detected</returns>
        private int FindAndRevealHiddenEntities()
        {
            int hostileCount = 0;
            
            try
            {
                if (map == null || activeSession == null)
                    return 0;

                // Check all pawns and things on the map
                List<Thing> allThings = new List<Thing>();
                allThings.AddRange(map.mapPawns.AllPawnsSpawned.Cast<Thing>());
                
                // Also check for things that might be hidden (non-pawn entities)
                foreach (IntVec3 cell in map.AllCells)
                {
                    List<Thing> thingsAtCell = map.thingGrid.ThingsListAtFast(cell);
                    if (thingsAtCell != null)
                    {
                        allThings.AddRange(thingsAtCell);
                    }
                }

                foreach (Thing thing in allThings.Distinct())
                {
                    if (thing == null || thing.Destroyed)
                        continue;

                    // Skip friendly pawns (player faction)
                    if (thing is Pawn pawn && pawn.Faction == Faction.OfPlayer)
                        continue;

                    // Check if thing is hidden using safe detection
                    if (IsThingHidden(thing))
                    {
                        RevealEntity(thing);
                        
                        // Count hostile entities
                        if (thing is Pawn p && p.HostileTo(Faction.OfPlayer))
                        {
                            hostileCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[I See You] Error in FindAndRevealHiddenEntities: {ex}");
            }
            
            return hostileCount;
        }

        /// <summary>
        /// Safe detection of hidden entities - checks common patterns without modifying other mods
        /// </summary>
        private bool IsThingHidden(Thing thing)
        {
            try
            {
                if (thing == null)
                    return false;

                // Check 1: Hediffs with "hidden", "invisible", "stealth", "concealed" in defName
                if (thing is Pawn pawn)
                {
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
                                if (Prefs.DevMode)
                                {
                                    Log.Message($"[I See You] Detected hidden hediff: {hediff.def.defName} on {pawn.LabelShort}");
                                }
                                return true;
                            }
                        }
                    }
                }

                // Check 2: Thing comps with "hidden", "invisible", "stealth" in type name
                if (thing is ThingWithComps thingWithComps && thingWithComps.AllComps != null)
                {
                    foreach (ThingComp comp in thingWithComps.AllComps)
                    {
                        if (comp == null)
                            continue;

                        string typeName = comp.GetType().Name.ToLower();
                        if (typeName.Contains("hidden") || 
                            typeName.Contains("invisible") || 
                            typeName.Contains("stealth") ||
                            typeName.Contains("concealed") ||
                            typeName.Contains("cloaked"))
                        {
                            if (Prefs.DevMode)
                            {
                                Log.Message($"[I See You] Detected hidden comp: {comp.GetType().Name} on {thing.Label}");
                            }
                            return true;
                        }
                    }
                }

                // Check 3: Thing def tags
                if (thing.def?.defName != null)
                {
                    string defNameLower = thing.def.defName.ToLower();
                    if (defNameLower.Contains("hidden") || 
                        defNameLower.Contains("invisible") || 
                        defNameLower.Contains("stealth"))
                    {
                        if (Prefs.DevMode)
                        {
                            Log.Message($"[I See You] Detected hidden thing def: {thing.def.defName}");
                        }
                        return true;
                    }
                }

                // Check 4: Vanilla Anomaly DLC entities (Ghoul, Revenant, Shambler, etc.)
                // These often have special reveal mechanics or scripted invisibility
                if (thing is Pawn p && p.RaceProps?.intelligence == Intelligence.Humanlike)
                {
                    // Check for anomaly-specific hediffs or states
                    if (p.health?.hediffSet?.hediffs != null)
                    {
                        foreach (Hediff h in p.health.hediffSet.hediffs)
                        {
                            if (h?.def?.defName != null && 
                                (h.def.defName.Contains("Revenant") || 
                                 h.def.defName.Contains("Ghoul") ||
                                 h.def.defName.Contains("Shambler") ||
                                 h.def.defName.Contains("Anomaly")))
                            {
                                if (Prefs.DevMode)
                                {
                                    Log.Message($"[I See You] Detected anomaly entity: {p.LabelShort}");
                                }
                                return true;
                            }
                        }
                    }
                }

                // Check 5: Scripted invisibility - check for pawns with special rendering flags
                // Some entities use RenderFlags or DrawPos manipulation for invisibility
                if (thing is Pawn pawn2)
                {
                    // Check if pawn has "Unobserved" or similar mental states
                    if (pawn2.MentalStateDef != null)
                    {
                        string mentalStateDefName = pawn2.MentalStateDef.defName.ToLower();
                        if (mentalStateDefName.Contains("unobserved") || 
                            mentalStateDefName.Contains("hidden") ||
                            mentalStateDefName.Contains("lurking"))
                        {
                            if (Prefs.DevMode)
                            {
                                Log.Message($"[I See You] Detected hidden mental state: {pawn2.LabelShort}");
                            }
                            return true;
                        }
                    }

                    // Check for scripted invisibility via faction or special pawn kinds
                    if (pawn2.kindDef?.defName != null)
                    {
                        string kindDefName = pawn2.kindDef.defName.ToLower();
                        if (kindDefName.Contains("shambler") || 
                            kindDefName.Contains("lurker") ||
                            kindDefName.Contains("creeper") ||
                            kindDefName.Contains("stalker"))
                        {
                            if (Prefs.DevMode)
                            {
                                Log.Message($"[I See You] Detected stealth pawn kind: {pawn2.LabelShort}");
                            }
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Warning($"[I See You] Error checking if thing is hidden: {ex.Message}");
                }
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
                activeSession.revealedEntities.Add(new RevealedEntity
                {
                    thing = thing,
                    exclamationMote = mote
                });

                // Flash cell to highlight
                map.debugDrawer.FlashCell(thing.Position, 0.8f, "reveal", 50);

                if (Prefs.DevMode)
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
        /// End the reveal session
        /// </summary>
        private void EndReveal(bool showMessage)
        {
            try
            {
                if (activeSession == null)
                    return;

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

                if (Prefs.DevMode)
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
