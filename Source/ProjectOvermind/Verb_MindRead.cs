using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace ProjectOvermind
{
    /// <summary>
    /// Verb for "Mind Read" psycast - reveals complete mental and physical profile of target pawn
    /// Shows traits, skills, passions, mood, health, and quest intentions (betrayal, leaving, etc.)
    /// </summary>
    public class Verb_MindRead : Verb_CastAbility
    {
        protected override bool TryCastShot()
        {
            try
            {
                if (CasterPawn == null)
                {
                    Log.Warning("[Mind Read] Cast failed: No caster pawn.");
                    return false;
                }

                // Get target pawn
                Pawn targetPawn = currentTarget.Thing as Pawn;
                if (targetPawn == null)
                {
                    Messages.Message("Mind Read failed: Invalid target.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                // Validate target is humanlike
                if (targetPawn.RaceProps == null || targetPawn.RaceProps.intelligence != Intelligence.Humanlike)
                {
                    Messages.Message("Mind Read failed: Target must be a humanlike.", MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                // Check for Psychically Dull trait - 50% resistance
                if (HasPsychicallyDullTrait(targetPawn))
                {
                    if (Rand.Chance(0.5f))
                    {
                        // Resistance success - cast fails
                        Messages.Message(
                            $"{targetPawn.LabelShort}'s mind resists intrusion.", 
                            targetPawn, 
                            MessageTypeDefOf.NeutralEvent, 
                            true
                        );
                        
                        // Show failure mote
                        FleckMaker.Static(targetPawn.DrawPos, targetPawn.Map, FleckDefOf.IncapIcon, 1.5f);
                        
                        // Play soft sound
                        SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(targetPawn));
                        
                        return true; // Ability was used (counts as success for cooldown)
                    }
                }

                // Successful cast - read mind
                string mindReadData = ReadPawnMind(targetPawn);

                // Display information in dialog
                Find.WindowStack.Add(new Dialog_MessageBox(
                    mindReadData,
                    "Close",
                    null,
                    null,
                    null,
                    $"Mind Read: {targetPawn.LabelShort}",
                    false,
                    null,
                    null
                ));

                // Success feedback
                Messages.Message(
                    $"Mind reading complete: {targetPawn.LabelShort}'s thoughts revealed.", 
                    targetPawn, 
                    MessageTypeDefOf.PositiveEvent, 
                    true
                );

                // Show success mote
                MoteText moteText = (MoteText)ThingMaker.MakeThing(ThingDefOf.Mote_Text);
                moteText.exactPosition = targetPawn.DrawPos + new Vector3(0f, 0f, 1f);
                moteText.text = "Your mind is open to me...";
                moteText.textColor = new Color(0.6f, 0.8f, 1f); // Light blue
                GenSpawn.Spawn(moteText, targetPawn.Position, targetPawn.Map);

                // Play psychic sound
                SoundDefOf.PsycastPsychicPulse.PlayOneShot(new TargetInfo(targetPawn));

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Read] Error in TryCastShot: {ex}");
                Messages.Message("Mind Read failed due to an error.", MessageTypeDefOf.RejectInput, false);
                return false;
            }
        }

        /// <summary>
        /// Check if pawn has Psychically Dull trait
        /// </summary>
        private bool HasPsychicallyDullTrait(Pawn pawn)
        {
            try
            {
                if (pawn?.story?.traits?.allTraits == null)
                    return false;

                return pawn.story.traits.allTraits.Any(t => 
                    t?.def?.defName != null && 
                    (t.def.defName == "PsychicSensitivity" && t.Degree < 0 || 
                     t.def.defName.ToLower().Contains("psychicdull") ||
                     t.def.defName.ToLower().Contains("psychically") && t.def.defName.ToLower().Contains("dull"))
                );
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Warning($"[Mind Read] Error checking PsychicallyDull: {ex.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Read all information from target pawn's mind
        /// </summary>
        private string ReadPawnMind(Pawn pawn)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                sb.AppendLine($"<b>=== Mind Read: {pawn.LabelShort} ===</b>\n");

                // === TRAITS ===
                sb.AppendLine("<b>Traits:</b>");
                if (pawn.story?.traits?.allTraits != null && pawn.story.traits.allTraits.Count > 0)
                {
                    foreach (Trait trait in pawn.story.traits.allTraits)
                    {
                        if (trait?.def != null)
                        {
                            string traitLabel = trait.LabelCap;
                            string traitDesc = trait.TipString(pawn);
                            sb.AppendLine($"  • {traitLabel}");
                            if (!string.IsNullOrEmpty(traitDesc))
                            {
                                sb.AppendLine($"    {traitDesc}");
                            }
                        }
                    }
                }
                else
                {
                    sb.AppendLine("  (No traits)");
                }
                sb.AppendLine();

                // === SKILLS & PASSIONS ===
                sb.AppendLine("<b>Skills & Passions:</b>");
                if (pawn.skills?.skills != null)
                {
                    var sortedSkills = pawn.skills.skills.OrderByDescending(s => s.Level);
                    foreach (SkillRecord skill in sortedSkills)
                    {
                        if (skill?.def != null)
                        {
                            string passionIcon = "";
                            if (skill.passion == Passion.Minor)
                                passionIcon = "[+]";
                            else if (skill.passion == Passion.Major)
                                passionIcon = "[++]";

                            sb.AppendLine($"  • {skill.def.LabelCap}: Level {skill.Level} {passionIcon}");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("  (No skill data)");
                }
                sb.AppendLine();

                // === MOOD & MENTAL STATE ===
                sb.AppendLine("<b>Mental State:</b>");
                if (pawn.needs?.mood != null)
                {
                    float moodLevel = pawn.needs.mood.CurLevel;
                    string moodDesc = GetMoodDescription(moodLevel);
                    sb.AppendLine($"  • Mood: {(moodLevel * 100f):F0}% ({moodDesc})");

                    // Current thoughts - get distinct memory thoughts
                    if (pawn.needs.mood.thoughts?.memories?.Memories != null)
                    {
                        var recentThoughts = pawn.needs.mood.thoughts.memories.Memories
                            .Take(5)
                            .ToList();
                        
                        if (recentThoughts.Count > 0)
                        {
                            sb.AppendLine("  • Recent thoughts:");
                            foreach (var thought in recentThoughts)
                            {
                                if (thought?.def != null)
                                {
                                    sb.AppendLine($"    - {thought.def.LabelCap}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    sb.AppendLine("  (No mood data)");
                }

                // Mental state
                if (pawn.MentalStateDef != null)
                {
                    sb.AppendLine($"  • Mental Break: {pawn.MentalStateDef.LabelCap}");
                }
                sb.AppendLine();

                // === HEALTH SUMMARY ===
                sb.AppendLine("<b>Health Summary:</b>");
                if (pawn.health != null)
                {
                    // Overall health
                    float healthPercent = pawn.health.summaryHealth.SummaryHealthPercent * 100f;
                    sb.AppendLine($"  • Overall Health: {healthPercent:F0}%");

                    // Major injuries or conditions
                    if (pawn.health.hediffSet?.hediffs != null)
                    {
                        var significantHediffs = pawn.health.hediffSet.hediffs
                            .Where(h => h.Visible || h.def.makesSickThought)
                            .Take(5)
                            .ToList();

                        if (significantHediffs.Count > 0)
                        {
                            sb.AppendLine("  • Conditions:");
                            foreach (Hediff hediff in significantHediffs)
                            {
                                if (hediff?.def != null)
                                {
                                    sb.AppendLine($"    - {hediff.LabelCap}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    sb.AppendLine("  (No health data)");
                }
                sb.AppendLine();

                // === QUEST INTENTIONS (Hidden Storyteller Info) ===
                sb.AppendLine("<b>Hidden Intentions:</b>");
                string questInfo = ReadQuestIntentions(pawn);
                sb.AppendLine(questInfo);

            }
            catch (Exception ex)
            {
                Log.Error($"[Mind Read] Error building mind read data: {ex}");
                sb.AppendLine("\n[Error reading some data - see log for details]");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Attempt to read hidden quest intentions or storyteller flags
        /// </summary>
        private string ReadQuestIntentions(Pawn pawn)
        {
            try
            {
                StringBuilder intentSb = new StringBuilder();
                bool foundIntentions = false;

                // Check if pawn is part of a quest
                List<Quest> activeQuests = Find.QuestManager.QuestsListForReading;
                if (activeQuests != null)
                {
                    foreach (Quest quest in activeQuests)
                    {
                        if (quest == null || quest.State != QuestState.Ongoing)
                            continue;

                        // Check if this pawn is involved in the quest
                        if (IsPawnInvolvedInQuest(pawn, quest))
                        {
                            intentSb.AppendLine($"  • Involved in quest: \"{quest.name}\"");
                            
                            // Try to extract betrayal or leaving intentions
                            string questDetails = AnalyzeQuestForIntentions(quest, pawn);
                            if (!string.IsNullOrEmpty(questDetails))
                            {
                                intentSb.AppendLine($"    {questDetails}");
                                foundIntentions = true;
                            }
                        }
                    }
                }

                // Check faction relationship
                if (pawn.Faction != null && pawn.Faction != Faction.OfPlayer)
                {
                    intentSb.AppendLine($"  • Faction: {pawn.Faction.Name} ({pawn.Faction.PlayerGoodwill} goodwill)");
                    
                    if (pawn.Faction.HostileTo(Faction.OfPlayer))
                    {
                        intentSb.AppendLine("    [Hostile - intends to attack]");
                        foundIntentions = true;
                    }
                    else if (pawn.Faction.PlayerGoodwill < -50)
                    {
                        intentSb.AppendLine("    [Poor relations - may turn hostile]");
                        foundIntentions = true;
                    }
                }

                // Check guest status
                if (pawn.guest != null)
                {
                    GuestStatus status = pawn.guest.GuestStatus;
                    intentSb.AppendLine($"  • Guest Status: {status}");
                    
                    if (pawn.guest.Released)
                    {
                        intentSb.AppendLine("    [Will leave when able]");
                        foundIntentions = true;
                    }
                }

                if (!foundIntentions)
                {
                    intentSb.AppendLine("  Intentions unclear - no active quests or hidden agendas detected.");
                }

                return intentSb.ToString();
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Warning($"[Mind Read] Error reading quest intentions: {ex.Message}");
                }
                return "  [Unable to read intentions - data protected or unavailable]";
            }
        }

        /// <summary>
        /// Check if pawn is involved in a quest
        /// </summary>
        private bool IsPawnInvolvedInQuest(Pawn pawn, Quest quest)
        {
            try
            {
                if (quest?.PartsListForReading == null)
                    return false;

                // Check quest parts for pawn references
                foreach (var part in quest.PartsListForReading)
                {
                    if (part == null)
                        continue;

                    // Use reflection-like checks for common quest part types that involve pawns
                    var partType = part.GetType();
                    var fields = partType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    foreach (var field in fields)
                    {
                        var value = field.GetValue(part);
                        
                        // Direct pawn reference
                        if (value is Pawn p && p == pawn)
                            return true;
                        
                        // List of pawns
                        if (value is List<Pawn> pawnList && pawnList.Contains(pawn))
                            return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Warning($"[Mind Read] Error checking quest involvement: {ex.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Analyze quest for betrayal or leaving intentions
        /// </summary>
        private string AnalyzeQuestForIntentions(Quest quest, Pawn pawn)
        {
            try
            {
                if (quest?.PartsListForReading == null)
                    return "";

                StringBuilder intentSb = new StringBuilder();

                // Look for quest parts that indicate betrayal, leaving, or joining
                foreach (var part in quest.PartsListForReading)
                {
                    if (part == null)
                        continue;

                    string partTypeName = part.GetType().Name.ToLower();

                    // Common quest part patterns
                    if (partTypeName.Contains("betray"))
                    {
                        intentSb.AppendLine("    [WARNING: May betray you]");
                    }
                    else if (partTypeName.Contains("leave") || partTypeName.Contains("depart"))
                    {
                        intentSb.AppendLine("    [Will leave soon]");
                    }
                    else if (partTypeName.Contains("join") || partTypeName.Contains("recruit"))
                    {
                        intentSb.AppendLine("    [May join your colony]");
                    }
                    else if (partTypeName.Contains("attack") || partTypeName.Contains("hostile"))
                    {
                        intentSb.AppendLine("    [WARNING: May turn hostile]");
                    }
                }

                return intentSb.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Warning($"[Mind Read] Error analyzing quest: {ex.Message}");
                }
                return "";
            }
        }

        /// <summary>
        /// Get mood description from mood level
        /// </summary>
        private string GetMoodDescription(float moodLevel)
        {
            if (moodLevel >= 0.8f)
                return "Very Happy";
            else if (moodLevel >= 0.6f)
                return "Content";
            else if (moodLevel >= 0.4f)
                return "Neutral";
            else if (moodLevel >= 0.2f)
                return "Stressed";
            else
                return "Breaking";
        }
    }
}
