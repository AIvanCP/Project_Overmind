# Project Overmind - Mindshift Abilities

A RimWorld mod that adds new psionic abilities.

## Features

### Translocate Target Psycast
- **Description**: Instantly relocate any pawn (ally or enemy) to a new location
- **Target**: Any living pawn (ally or enemy, humanlike or animal)
- **Range**: 15 tiles (base) + scales with sensitivity (base + 0.5 per 0.1)
- **Destination Range**: 10 tiles (base) + scales with sensitivity (from target)
- **Cast Time**: 1 second
- **Cooldown**: 15 seconds
- **Psyfocus Cost**: 0.10
- **Heat Cost**: 0.05
- **Required Psycast Level**: 3
- **Special**: Non-violent ability with no damage or relationship penalties
  - Applies "Spatial Daze" debuff (5 seconds) causing brief disorientation
  - Works on both friendly and hostile pawns (including animals)
  - Cannot affect mechanoids or downed pawns
  - Range scales with caster's psychic sensitivity

### Mind Spike Psycast
- **Description**: Unleash psychic dominance in an area, seizing all enemy minds within range
- **Target**: Ground location (area effect)
- **Range**: 20 tiles (base) + scales with sensitivity
- **Cast Time**: 1.5 seconds
- **Cooldown**: 45 seconds
- **Duration**: 12 in-game hours (base) + 1 hour per 0.1 Psychic Sensitivity
  - Example: Sensitivity 3.0 → 42 hours duration
- **Psyfocus Cost**: 0.20
- **Heat Cost**: 0.12
- **Required Psycast Level**: 3
- **Special**: Tactical area mind control ability
  - **Area Effect**: Affects all valid enemies and hostile animals within radius
  - **Base Radius**: 3 tiles + 0.3 tiles per 0.1 Sensitivity
    - Sensitivity 1.0 → 6 tile radius
    - Sensitivity 3.0 → 12 tile radius
    - Sensitivity 5.0 → 18 tile radius
  - **Visual Radius Ring**: Shows targeting area when casting
  - Forces enemies into berserk state targeting their allies
  - After effect ends, applies "Disoriented" debuff (-50% move speed, -20% aim for 5s)
  - If target dies while controlled, chains to nearest enemy within 6 tiles (once per cast)
  - Works on humanlike pawns AND animals (excludes mechanoids)
  - *"The mind is a weapon — sharpen it well."*

### Mind Read Psycast
- **Description**: Probe a target's mind to reveal all thoughts, traits, skills, and hidden intentions
- **Target**: Any humanlike pawn (enemies, visitors, guests, prisoners, quest pawns)
- **Range**: 10 tiles
- **Cast Time**: 1.0 seconds
- **Cooldown**: 30 seconds
- **Psyfocus Cost**: 0.30
- **Heat Cost**: 0.08
- **Required Psycast Level**: 3
- **Special**: Intelligence gathering ability with complete mental profile reveal
  - All traits (including hidden ones)
  - All skills and passions
  - Current mood and thoughts
  - Health summary
  - Quest intentions (betrayal, leaving, joining, etc.)
  - Does NOT affect faction goodwill or relationships
  - Psychically Dull pawns have 50% resistance chance
  - *"The mind speaks louder than words."*

### Feast of Mind Psycast
- **Description**: Channel psychic energy to sustain a target, reducing hunger and boosting eating speed
- **Target**: Self or friendly pawn
- **Range**: 5 tiles
- **Cast Time**: 1 second
- **Cooldown**: 60 seconds
- **Duration**: 6 in-game hours (base) + 0.5 hour per 0.1 Psychic Sensitivity
  - Example: Sensitivity 3.0 → 21 hours duration
- **Psyfocus Cost**: 0.15
- **Heat Cost**: 0.25
- **Required Psycast Level**: 3
- **Special**: Comprehensive scaling with Psychic Sensitivity
  - **Base effects at 0 sensitivity**: 10% hunger reduction, +25% eating speed
  - **Scaling**: Each 0.1 sensitivity adds +1% to both effects
  - **Examples**:
    - Sensitivity 1.0 → 20% hunger reduction, +35% eating speed
    - Sensitivity 2.0 → 30% hunger reduction, +45% eating speed
    - Sensitivity 5.0 → 60% hunger reduction, +75% eating speed
  - **Hunger cap**: Maximum 99% hunger reduction (always need some food)
  - **Eating speed**: No cap (scales infinitely)
  - **Threshold Perks** (extra bonuses at high sensitivity):
    - **≥3.0**: +10% learning speed (scales: +1% per 0.2 over 3.0)
    - **≥5.0**: +5% damage reduction (scales: +1% per 0.2 over 5.0)
    - **≥8.0**: +5% tiredness reduction (scales: +1% per 0.2 over 8.0)
  - Does not stack - recasting refreshes duration
  - Perfect for sustained operations and long crafting sessions
  - *"The mind nourishes the body."*

### Inspiration Psycast
- **Description**: Flood the minds of all allies with powerful psychic motivation
- **Target**: Self (map-wide buff to all player pawns)
- **Range**: Entire map
- **Cast Time**: 4 seconds
- **Cooldown**: 12 in-game hours (base) + 1 hour per 0.1 Psychic Sensitivity
  - Example: Sensitivity 3.0 → 42 hours durations (~3.3 minutes)
- **Duration**: 60 seconds
- **Psyfocus Cost**: 0.40
- **Heat Cost**: 0.20
- **Required Psycast Level**: 4
- **Special**: Comprehensive work buff scaling with Psychic Sensitivity
  - **Base effects at 0 sensitivity**:
    - +50% work speed (all tasks)
    - +60% learning speed
    - +10% movement speed
    - +15% crafting quality
  - **Scaling**: Each 0.1 sensitivity adds +1% to all effects (no cap)
  - **Examples**:
    - Sensitivity 1.0 → +60% work, +70% learning, +20% move, +25% quality
    - Sensitivity 2.0 → +70% work, +80% learning, +30% move, +35% quality
    - Sensitivity 5.0 → +100% work, +110% learning, +60% move, +65% quality
  - **Threshold Perks** (specialized work bonuses):
    - **≥3.0 Farming & Production**: +10% plant work speed, +10% harvest yield, +10% drug production (each scales: +1% per 0.2)
    - **≥5.0 Combat & Resources**: +10% hunting stealth, +10% butcher speed, +10% mining speed, +10% mining yield (each scales: +1% per 0.2)
    - **≥8.0 Advanced Crafting**: +10% smithing speed, +10% construction speed, +10% general crafting, +10% surgery success (each scales: +1% per 0.2)
  - Blue aura visual effect on affected pawns
  - Does NOT stack - refreshes duration if recast
  - *"Unlock the mind's full potential."*

### I See You Psycast
- **Target**: Self (map-wide effect)
- **Range**: Entire map
- **Cast Time**: 1.5 seconds
- **Cooldown**: 120 seconds (2 minutes)
- **Duration**: 12 in-game hours (base) + 1 hour per 0.1 Psychic Sensitivity
  - Example: Sensitivity 3.0 → 42 hours durations (2 minutes)
- **Duration**: 60 seconds
- **Psyfocus Cost**: 0.25
- **Heat Cost**: 0.12
- **Required Psycast Level**: 5
- **Special**: Anti-ambush and anomaly detection ability
  - Detects invisible creatures, stealthed entities, and anomalies
  - Works with vanilla and modded invisibility systems
  - Safe overlay system - doesn't break other mods
  - Exclamation mark indicators on hidden creatures
  - Player-only reveal: enemies don't gain targeting information
  - Effect ends if caster dies or leaves map
  - Alert sound plays ONLY when hostile invisible entities detected
  - *"Nothing stays hidden from the mind's eye."*

### Hallucination Psycast
- **Description**: Shatter enemy minds with terrifying psychic illusions
- **Target**: Self (map-wide debuff to all hostiles)
- **Range**: Entire map
- **Cast Time**:12 in-game hours (base) + 1 hour per 0.1 Psychic Sensitivity
  - Example: Sensitivity 3.0 → 42 hours duration
- **Cooldown**: 300 seconds (~5 minutes)
- **Duration**: 40 seconds
- **Psyfocus Cost**: 0.50
- **Heat Cost**: 0.30
- **Required Psycast Level**: 5
- **Special**: Psychic terror weapon
  - Afflicts all hostile pawns and animals (excluding mechanoids)
  - -40% shooting accuracy
  - -15% movement speed
  - 25% chance per second to trigger panic attack
  - Panic behavior: attacks empty space or moves erratically for 1-2 seconds
  - Purple shimmer visual effect
  - Does NOT stack with itself - refreshes duration if recast
  - No faction goodwill penalty
  - Perfect for defending against large raids
  - *"Let them fear what isn't there."*

### Soul Refill Psycast

- **Description**: Channel psychic energy to sustain all colonists on the map
- **Target**: Self (map-wide buff to all player pawns)
- **Range**: Self only (affects entire map)
- **Cast Time**: 2.5 seconds
- **Cooldown**: 400 seconds (~6.7 minutes)
- **Duration**: 12 in-game hours (base) + 1 hour per 0.1 Psychic Sensitivity
  - Example: Sensitivity 3.0 → 42 hours duration
- **Psyfocus Cost**: 0.30
- **Heat Cost**: 0.15
- **Required Psycast Level**: 3
- **Special**: Persistent need regeneration ability
  - **Affects ALL player-owned pawns on the map**
  - **Base effects**: +1% per second to all needs (food, rest, mood, recreation, etc.)
  - **Scaling**: +0.1% per 0.1 Psychic Sensitivity
  - **Examples**:
    - Sensitivity 1.0 → +2% per second
    - Sensitivity 2.0 → +3% per second
    - Sensitivity 5.0 → +6% per second
  - **Threshold Perks**:
    - **≥3.0**: +20% recreation gain
    - **≥5.0**: +15% immunity gain for all active diseases
    - **≥8.0**: -30% rest need drain, +1% extra regeneration
  - **Automatically regenerates ALL needs** including mod-added needs (Hygiene, Bladder, Comfort, etc.)
  - Works with any mod that adds needs - no hardcoded list
  - Effect continues even if caster is downed
  - Perfect for sustaining pawns during long operations or extended work shifts
  - *"The mind sustains the body."*

### Aura Clean Psycast

- **Description**: Create a psychic cleaning aura around all colonists on the map
- **Target**: Self (map-wide buff to all player pawns)
- **Range**: Self only (affects entire map)
- **Cast Time**: 1.5 seconds
- **Cooldown**: 180 seconds (3 minutes)
- **Duration**: 12 in-game hours (base) + 1 hour per 0.1 Psychic Sensitivity
  - Example: Sensitivity 3.0 → 42 hours duration
- **Psyfocus Cost**: 0.20
- **Heat Cost**: 0.10
- **Required Psycast Level**: 2
- **Special**: Area cleaning ability with intelligent filth filtering
  - **Affects ALL player-owned pawns on the map - each gets their own aura**
  - **Base effects**: 3-tile radius, removes 1 filth per 1.5 seconds
  - **Radius scaling**: +0.05 per 0.1 Sensitivity (max 12 tiles)
  - **Examples**:
    - Sensitivity 1.0 → 3.5 tile radius
    - Sensitivity 3.0 → 4.5 tile radius
    - Sensitivity 5.0 → 5.5 tile radius
    - Sensitivity 8.0 → 7.0 tile radius
  - **Threshold Perks**:
    - **≥3.0**: +1 tile radius, can clean blood and stains
    - **≥5.0**: +1 tile radius, can clean vomit and animal filth
    - **≥8.0**: +1.5 tile radius, double cleaning rate, +10% immunity +5% move speed
  - Only cleans filth types appropriate for tier level (base: dirt/rubble)
  - Does NOT clean toxic or polluted tiles by default
  - **Safe cleaning**: Automatically cancels pawn cleaning jobs to prevent conflicts
  - Performance-optimized with configurable filth limit per interval
  - Perfect for keeping workshops, hospitals, and dining areas clean
  - *"Cleanliness is clarity of mind."*

### Spatial Anchor Psycast
- **Description**: Create a gravitational anomaly that slows and pulls hostile pawns
- **Target**: Ground location
- **Range**: 10 tiles (base) + scales with sensitivity
- **Cast Time**: 1 second
- **Cooldown**: 90 seconds
- **Duration**: 20 seconds
- **Psyfocus Cost**: 0.35
- **Heat Cost**: 0.25
- **Required Psycast Level**: 4
- **Special**: Area control ability
  - Creates invisible anchor with 10-tile effect radius
  - Casting range scales with psychic sensitivity (base + 0.5 per 0.1)
  - Applies "Gravitic Pull" debuff to enemies in radius
  - -40% movement speed, -20% dodge chance
  - 10% chance per second to pull enemy 1 tile toward center
  - Perfect for raid defense and controlling chokepoints
  - *"Pin them in place and watch them crumble."*

### Cognitive Shield Psycast
- **Description**: Protect all colonists with a mental barrier scaling with psychic sensitivity
- **Target**: Self (map-wide buff)
- **Range**: Entire map
- **Cast Time**: 2 seconds
- **Cooldown**: 12 in-game hours (base) + 1 hour per 0.1 Psychic Sensitivity
  - Example: Sensitivity 3.0 → 42 hours durations
- **Duration**: 25 seconds
- **Psyfocus Cost**: 0.45
- **Heat Cost**: 0.30
- **Required Psycast Level**: 5
- **Special**: Defensive buff with comprehensive scaling
  - **Base effects at 0 sensitivity**: +25% psychic sensitivity, -30% incoming mental damage
  - **Scaling**: Each 0.1 sensitivity adds +1% to all effects
  - **Threshold Perks**:
    - **≥3.0**: +10% consciousness (scales: +1% per 0.2 over 3.0)
    - **≥5.0**: Mental immunity to all mental breaks
    - **≥8.0**: +20% injury healing rate (scales: +1% per 0.2 over 8.0)
  - Blue shimmer visual effect
  - Does NOT stack - refreshes duration if recast
  - *"A fortified mind cannot be broken."*

### Psychic Diffusion Psycast
- **Description**: Create a global psychic network that enhances all colonists and disrupts all enemies
- **Target**: Self (map-wide effect - affects ALL pawns on map)
- **Range**: Entire map
- **Cast Time**: 4 seconds
- **Cooldown**: 150 seconds (~2.5 minutes)
- **Duration**: 12 in-game hours (base) + 1 hour per 0.1 Psychic Sensitivity
  - Example: Sensitivity 3.0 → 42 hours duration
- **Psyfocus Cost**: 0.50
- **Heat Cost**: 0.35
- **Required Psycast Level**: 6
- **Special**: Global dual-effect ability with comprehensive benefits
  - **ALLY BUFFS (all colonists on map)**:
    - Base at sensitivity 0: +10% move speed, +10% work speed, +5 mood
    - Scaling: Each 0.1 sensitivity adds +1% to all effects
    - Examples:
      - Sensitivity 1.0 → +20% move, +20% work, +5 mood
      - Sensitivity 3.0 → +40% move, +40% work, +5 mood
      - Sensitivity 5.0 → +60% move, +60% work, +5 mood
    - **Threshold Perks** (specialized bonuses):
      - **≥3.0**: +10% heal power, buff spread radius +2 tiles (scales: +1% per 0.2)
      - **≥5.0**: +20% work speed, -20% incoming damage (scales: +1% per 0.2)
      - **≥8.0**: Mini-heal pulse (5 HP every 5 seconds), 100% buff transfer rate (scales: +1% per 0.2)
  - **ENEMY DEBUFFS (all hostile pawns/animals on map)**:
    - -30% shooting accuracy
    - -25% melee hit chance
    - -20% movement speed
    - -15% consciousness
  - Green aura visual effect for allies, purple aura for enemies
  - Does NOT stack - refreshes duration if recast
  - *"Together, we are stronger than the sum of our parts."*

### Mind Core Psycast
- **Description**: Empower a pawn's psycast capabilities with comprehensive performance enhancements
- **Target**: Any friendly pawn (same faction, can target self)
- **Range**: 15 tiles
- **Cast Time**: 2.5 seconds
- **Cooldown**: 300 seconds (5 minutes)
- **Duration**: 3 in-game hours (base) + 0.5 hour per 0.1 Psychic Sensitivity
  - Example: Sensitivity 1.0 → 8 hours duration
  - Example: Sensitivity 3.0 → 18 hours duration
  - Example: Sensitivity 5.0 → 28 hours duration
- **Psyfocus Cost**: 0.35
- **Heat Cost**: 0.25
- **Required Psycast Level**: 5
- **Special**: Cross-mod compatible psycast enhancement framework
  - **Effects (all scale with CASTER's psychic sensitivity)**:
    - **Passive Psyfocus Regeneration**: 1% per second (base at 1.0 sensitivity)
      - Scaling: +0.5% per 0.1 sensitivity
      - Example: Sensitivity 3.0 → 2.5% per second
    - **Max Psychic Entropy Increase**: +30 (base at 1.0 sensitivity)
      - Scaling: +3 per 0.1 sensitivity
      - Example: Sensitivity 3.0 → +120 max entropy
    - **Psycast Cost Reduction**: 30% cost reduction (base at 1.0 sensitivity)
      - Scaling: +3% per 0.1 sensitivity
      - Capped at 70% reduction (minimum 30% cost)
      - Example: Sensitivity 3.0 → 60% reduction (pay only 40%)
    - **Cooldown Reduction**: 25% faster cooldowns (base at 1.0 sensitivity)
      - Scaling: +2.5% per 0.1 sensitivity
      - Capped at 60% reduction (minimum 40% cooldown)
      - Example: Sensitivity 3.0 → 50% reduction (50% faster)
  - **Cross-Mod Compatibility**: Works with ALL psycasts
    - Vanilla psycasts
    - Vanilla Psycasts Expanded
    - Any mod using standard Ability/CompAbilityEffect system
  - Uses Harmony patches to modify cost/cooldown dynamically
  - Does NOT permanently alter ability definitions
  - Buff shows duration timer and current effects in tooltip
  - Can be refreshed on same target (resets duration)
  - *"Unlock the mind's true potential."*

### Overmind Adaptation Psycast
- **Description**: Psychically reshape all colonists' biology to master any terrain and environment
- **Target**: Self (global effect — applies to ALL colonists on map)
- **Cast Time**: 5.0 seconds
- **Cooldown**: 300 seconds (5 minutes)
- **Duration**: 12 in-game hours (base) + 1 hour per 0.1 Psychic Sensitivity
  - Example: Sensitivity 1.0 → 13h duration
  - Example: Sensitivity 5.0 → 17h duration
- **Psyfocus Cost**: 0.55
- **Heat Cost**: 0.35
- **Required Psycast Level**: 5
- **Special**: Terrain mastery + environmental immunity (all effects scale with CASTER's sensitivity)
  - **Terrain Movement**: `terrainIgnore = clamp01(0.25 + sensitivity × 0.10)`
    - Sensitivity 1.0 → 35% terrain cost reduction
    - Sensitivity 5.0 → 75% terrain cost reduction
    - Sensitivity 9.0 → 100% (full terrain immunity)
  - **Environmental Immunity Tiers (based on caster sensitivity)**:
    - **≥ 3.0** – Immune to Toxic Environment and Gas/Chemical exposure
    - **≥ 5.0** – Immune to Vacuum suffocation (Odyssey DLC safe, null-checked)
    - **≥ 8.0** – Immune to Heatstroke & Hypothermia; airborne diseases suppressed
  - Implemented via safe Harmony Postfix patches (never returns false, never skips original)
  - Fully compatible with DMC mod and any other movement patches
  - Shows active tier and remaining duration in buff tooltip
  - *"Where others see hostile terrain, the Overmind sees a path."*

## Requirements

- RimWorld 1.5 or 1.6
- Royalty DLC (for psycasting framework)

## Compatibility

- Compatible with Vanilla Psycasts Expanded
- Compatible with Combat Extended (no conflicts)
- Compatible with other psycast mods
- Load after Royalty and VPE if installed

## Installation

1. Subscribe on Steam Workshop (when published) or download manually
2. Enable in mod list, making sure it loads after Royalty
3. Start a new game or load an existing save

## How to Obtain

### Traders
Available from Orbital Bulk Goods and Exotic Goods traders (rare)

### Market Value
- Translocate Target Psytrainer: 1,200 silver (level 3)
- Mind Spike Psytrainer: 1,800 silver (level 3)
- Mind Read Psytrainer: 1,600 silver (level 3)
- Feast of Mind Psytrainer: 1,500 silver (level 3)
- Soul Refill Psytrainer: 1,400 silver (level 4)
- Aura Clean Psytrainer: 1,100 silver (level 3)
- Inspiration Psytrainer: 2,000 silver (level 4)
- Spatial Anchor Psytrainer: 1,800 silver (level 4)
- I See You Psytrainer: 2,200 silver (level 5)
- Hallucination Psytrainer: 2,400 silver (level 5)
- Cognitive Shield Psytrainer: 2,200 silver (level 5)
- Psychic Diffusion Psytrainer: 2,600 silver (level 6)
- Mind Core Psytrainer: 1,800 silver (level 5)
- Overmind Adaptation Psytrainer: 2,800 silver (level 5)

### Quest Rewards
Can appear as a reward in some quests

## Building from Source

### Prerequisites
- .NET Framework 4.7.2 or higher
- RimWorld installed (for assembly references)

### Build Steps
1. Clone or download this repository
2. Open `Source/ProjectOvermind/ProjectOvermind.csproj` in Visual Studio or your IDE
3. Update assembly reference paths in the .csproj file to match your RimWorld installation
4. Build the solution (Release configuration recommended)
5. The compiled DLL will be placed in `Assemblies/ProjectOvermind.dll`

### Using PowerShell (if dotnet SDK installed):
```powershell
cd "d:\0-tugas-IK-D\projek-gabut\Project_Overmind\Source\ProjectOvermind"
dotnet build -c Release
```

## Troubleshooting

### Psycast doesn't appear
- Make sure you have the Royalty DLC enabled
- Check that the mod is loaded after Royalty in the mod list
- Verify the pawn has psycasting ability

### Can't target certain pawns
- Mechanoids cannot be translocated (by design)
- Downed pawns cannot be translocated (by design)
- Dead pawns cannot be translocated

### Destination targeting issues
- Make sure the destination cell is walkable
- Destination must not be fogged
- Destination must be within 10 tiles of the target

## Credits

- Designed for RimWorld by Ludeon Studios
- Uses vanilla psycast framework from Royalty DLC
- Visual effects adapted from vanilla Skip psycast

## License

This mod is provided as-is for personal use. Feel free to modify for your own games.

## Changelog

### Version 1.9.0 (Current – New Ability + Bug Fix)
- **🆕 NEW ABILITY: Overmind Adaptation (Level 5)**
  - Global self-cast (like Inspiration) — applies to ALL colonists on map
  - Grants terrain movement cost reduction that scales with caster sensitivity (25% base + 10% per 0.1 sense)
  - Environmental immunity tiers based on caster's Psychic Sensitivity:
    - **≥ 3.0**: Immune to toxic environment and gas exposure (ToxicBuildup suppressed each tick)
    - **≥ 5.0**: Immune to vacuum/suffocation (Odyssey DLC safe — null-checked)
    - **≥ 8.0**: Immune to Heatstroke & Hypothermia; airborne diseases suppressed
  - Harmony Postfix on `CostToMoveIntoCell` (safe, never returns false, coexists with DMC mod)
  - Postfix on `PreApplyDamage` for gas/vacuum direct damage absorption (never skips original)
  - Psytrainer available from Exotic traders (2,800 silver market value)
  - Duration scales with caster sensitivity (12h base + 1h per 0.1 sensitivity)

- **🐛 FIXED: Hallucination showing 0h duration**
  - When applying to new enemy pawns, `ticksToDisappear` was never set (only the refresh path set it)
  - Fix: Set `HediffComp_Disappears.ticksToDisappear = DurationHelper.CalculateDuration(CasterPawn)` after `AddHediff`
  - All enemy pawns now correctly show duration (e.g., "13h 20m")

- **🔧 FIXED: RimWorld 1.6 Harmony TryAddEntropy crash**
  - Parameter `entropy` renamed to `value` in RimWorld 1.6
  - Patch now uses `ref float value` and accesses pawn via field injection
  - Mind Core cost/cooldown reduction now works correctly in RimWorld 1.6

- **⚙️ TECHNICAL:**
  - Added `using Verse.AI` to HarmonyPatches.cs
  - New Harmony patch classes: `OvermindAdaptation_MovementCost_Patch`, `OvermindAdaptation_DamagePrevention_Patch`
  - All environmental immunity implemented WITHOUT skipping original methods
  - All sensitivity thresholds check caster's stored sensitivity (not target pawn's)

- **Build Status:** 0 errors, 25 warnings (debug code only)

### Version 1.8.2 (Critical Fixes & Consistency)
- **🐛 FIXED: Duration Display**
  - Added LabelInBrackets to: Psychic Diffusion, Mind Spike Controlled, Spatial Daze, Gravitic Pull
  - All hediffs now show time remaining in brackets on health tab (e.g., "4d 23h")
  
- **🔧 FIXED: Stat Detail Tab Display**
  - Standardized ALL buffs to use **CASTER's** psychic sensitivity (not recipient's)
  - Cognitive Shield, Psychic Diffusion, and Feast of Mind now consistently check caster power
  - **IMPORTANT**: Damage reduction shows ONLY if caster meets threshold requirements:
    - Cognitive Shield: caster sensitivity ≥ 3.0
    - Psychic Diffusion: caster sensitivity ≥ 5.0
    - Feast of Mind: caster sensitivity ≥ 5.0
  - Buff tooltips now display "Caster Sensitivity: X.X" for clarity

- **📚 DOCUMENTATION: Mind Core Clarification**
  - Created `IMPORTANT_MIND_CORE_CLARIFICATION.md`
  - **Mind Core DOES affect**: Psycast costs (-30%), cooldowns (-20%), meditation gains (+15%)
  - **Mind Core DOES NOT affect**: Buff durations from OTHER abilities (e.g., VPE - Arknights 2h buff)
  - Duration of buffs is hardcoded in each ability's XML, not controlled by psycast system
  - Added testing guide with dev mode log examples

- **📚 DOCUMENTATION: Testing Guide**
  - Created `STAT_DETAIL_TAB_TESTING_GUIDE.md`
  - Comprehensive threshold table and troubleshooting steps
  - Explains why buffs may not show damage reduction in detail tab
  - Includes example scenarios with different caster sensitivity levels

- **⚙️ TECHNICAL:**
  - Feast of Mind now caches caster sensitivity when buff applied (consistent with other buffs)
  - Updated all Verb classes to refresh caster sensitivity when hediff duration refreshed
  - All VPE Harmony patches verified functional (5 patches active)

- **Build Status:** 0 errors, 25 warnings (debug code only)

### Version 1.8.0 (Mind Core Ability)
- **🆕 NEW ABILITY: Mind Core**
  - Targetable buff that enhances all psycast performance
  - Provides passive psyfocus regeneration (1% per second base, scalable)
  - Increases max psychic entropy (+30 base, scalable)
  - Reduces psyfocus cost of all psycasts (30% reduction base, scalable, capped at 70%)
  - Reduces cooldown of all psycasts (25% reduction base, scalable, capped at 60%)
  - All effects scale with CASTER's psychic sensitivity
  - Duration: 3 hours base + 0.5 hour per 0.1 sensitivity
  - **Cross-Mod Compatible**: Works with ALL psycasts (vanilla, VPE, and any mod using standard ability system)
  - Uses Harmony patches to modify cost/cooldown dynamically without altering ability definitions
  - Buff tooltip shows duration timer and current effect values
  - Can target self or friendly pawns (same faction only)
  - Level 5 psycast, 5-minute cooldown
  - Added psytrainer to exotic goods traders (rare)

- **✅ VERIFICATION: I See You sound effect**
  - Confirmed alert sound implementation is correct
  - Sound plays when hostile hidden entities are detected
  - Fallback to vanilla sound if custom sound not found
  - Sound file: Sounds/ISeeYou/alert.ogg

- **Build Status:** Ready for testing

### Version 1.7.2 (Feature Expansion & UX)
- **🎯 ENHANCEMENT: Mind Spike now affects animals**
  - Works on all hostile animals (vanilla and modded)
  - Excludes only mechanoids
  - Controlled animals attack their pack members
  
- **📏 ENHANCEMENT: Scalable ranges added**
  - Translocate Target: Both initial range (15 base) and destination range (10 base) scale with sensitivity
  - Spatial Anchor: Casting range (10 base) scales with sensitivity  
  - Formula: BaseRange + (Sensitivity / 0.1) × 0.5 tiles
  - Example at 3.0 sensitivity: +15 tiles = 25/40/25 total range
  
- **📝 UX: All ability descriptions simplified**
  - Shortened hover tooltips for better readability
  - Removed flavor text, kept core mechanics
  - All descriptions now 1-2 lines maximum
  
- **✅ VERIFICATION: Hallucination duration confirmed scaling**
  - Uses DurationHelper.CalculateDuration() correctly
  - 12 hours base + 1 hour per 0.1 sensitivity
  
- **Build Status:** 0 errors, 25 warnings (unreachable debug code)

### Version 1.7.1 (Critical Bug Fixes)
- **🔧 CRITICAL FIX: Diffusion & Cognitive Shield buff distribution**
  - Fixed buffs using recipient's sensitivity instead of caster's sensitivity
  - Added `SetCasterSensitivity()` method to store caster's stats
  - Now all buffed pawns receive benefits scaled by the CASTER's psychic sensitivity
  - Example: Caster with 5.0 sensitivity buffs ALL allies with 5.0-level effects
  
- **🔧 CRITICAL FIX: Mind Spike control duration too short**
  - Fixed hediff not applying dynamic duration on creation
  - Control duration now properly scales: 12 hours base + 1 hour per 0.1 caster sensitivity
  - Example: 3.0 sensitivity → 42 hours of mind control
  
- **🎯 ENHANCEMENT: Mind Spike targeting visibility**
  - Added `HighlightFieldRadiusAroundTarget()` override
  - Radius circle now shows continuously while targeting
  - Players can see affected area before committing to cast location
  
- **⚖️ BALANCE: Feast of Mind duration reduced**
  - Changed from 12 hours base to 6 hours base
  - Scaling: 6 hours + 0.5 hour per 0.1 sensitivity
  - More balanced for food sustenance ability
  - Example: 3.0 sensitivity → 21 hours duration
  
- **✅ VERIFICATION: Aura Clean threshold perks confirmed working**
  - Tier 1 (≥3.0): +1 radius, cleans blood/stains
  - Tier 2 (≥5.0): +1 radius, cleans vomit/animal filth
  - Tier 3 (≥8.0): +1.5 radius, double cleaning rate, immunity boost
  
- **📊 Duration Calculation Reference:**
  - Formula: `BaseTicks + (Sensitivity / 0.1) * TicksPerStep`
  - Standard abilities: 30,000 + (Sen / 0.1) * 2,500 ticks
  - Feast of Mind: 15,000 + (Sen / 0.1) * 1,250 ticks
  - Examples at 1000% sensitivity (10.0):
    * Standard: 30,000 + 100 * 2,500 = 280,000 ticks (~112 hours)
    * Feast: 15,000 + 100 * 1,250 = 140,000 ticks (~56 hours)

- **Build Status:** Compiles successfully with 0 errors, 25 warnings (unreachable debug code)

### Version 1.7.0 (Major Enhancement Update)
- **🎯 MAJOR FEATURE: Dynamic Duration Scaling**
  - ALL buff/debuff abilities now scale duration with caster's Psychic Sensitivity
  - Base duration: 12 in-game hours (30,000 ticks)
  - Scaling: +1 hour (2,500 ticks) per 0.1 Psychic Sensitivity
  - Examples:
    - Sensitivity 1.0 → 22 hours duration
    - Sensitivity 3.0 → 42 hours duration
    - Sensitivity 5.0 → 62 hours duration
  - Affected abilities: Feast of Mind, Inspiration, I See You, Hallucination, Soul Refill, Aura Clean, Cognitive Shield, Psychic Diffusion
  - Created `DurationHelper.cs` for centralized duration/radius calculations

- **🎯 MAJOR FEATURE: Mind Spike Area Effect**
  - Converted from single-target to area-of-effect ability
  - Base radius: 3 tiles + 0.3 tiles per 0.1 Sensitivity
  - Visual radius ring during targeting (cyan highlight)
  - Can target ground location or pawn - affects all valid enemies in radius
  - Range increased from 12 to 20 tiles
  - Psyfocus cost increased 0.15→0.20, entropy 0.08→0.12
  - Now uses `CastAbilityOnThingOrPosition` job for area targeting
  - DrawHighlight() shows targeting radius ring
  - GetAffectedPawns() finds all valid targets in radius

- **🔧 TECHNICAL IMPROVEMENTS:**
  - Verified all threshold perks working correctly across all abilities
  - StatPart implementations confirmed for Inspiration, Psychic Diffusion, Cognitive Shield, Feast of Mind
  - Optimized for endgame performance - no heavy tick consumption
  - All abilities share consistent duration scaling pattern
  - Null safety checks throughout all verb files

- **📝 DOCUMENTATION:**
  - Updated README with dynamic duration information for all abilities
  - Added radius scaling formulas and examples
  - Documented Mind Spike area effect mechanics
  - Added duration calculation examples for common sensitivity levels

- **Build Status:** Compiles successfully with 0 errors, 25 warnings (unreachable debug code in MapComponent_ISeeYou)

### Version 1.6.4 (Emergency Fix)
- **⚠️ CRITICAL: v1.6.3 FAILED - User reported neither fix worked in-game**
  - Global abilities STILL showed red targeting circle (screenshot evidence)
  - Spatial Anchor showed NO visual effects and NO debuffs applied
  
- **v1.6.4 ROOT CAUSE DISCOVERIES & CORRECT FIXES:**
  - **Global Abilities FIX (Range=-1 Pattern):**
    - REMOVED failed custom `Ability_GlobalSelfCast` class approach (targeting UI controlled by VerbProperties, not Ability class!)
    - CHANGED all 5 global ability XMLs from `<range>0</range>` to `<range>-1</range>` (vanilla RimWorld self-cast pattern)
    - How it works: `range=-1` tells RimWorld "cast on caster immediately" (same as Word of Inspiration in Royalty DLC)
    - No custom C# classes needed - vanilla XML pattern handles everything
  
  - **Spatial Anchor HEAVY DEBUG LOGGING:**
    - Added extensive yellow warning logs to diagnose runtime failure:
      - TryCastShot: ThingDef lookup, ThingMaker.MakeThing, GenSpawn.Spawn success verification
      - Tick(): Per-second status (Thing exists, Spawned, Map, Position)
      - ProcessGraviticEffects(): Total pawn scan, hostile count, affected pawn list with positions
      - ApplyGraviticPull(): Hediff creation success, HediffDef null checks, comp.ticksToDisappear values
    - User MUST enable Dev Mode + F12 console to see diagnostic output
    - Logs will reveal exact failure point (Thing not spawning? Tick not called? Hediff not applying?)
  
  - **Files Modified (v1.6.4):**
    - `Defs/AbilityDefs/Ability_Inspiration.xml` (removed abilityClass, range → -1)
    - `Defs/AbilityDefs/Ability_ISeeYou.xml` (removed abilityClass, range → -1)
    - `Defs/AbilityDefs/Ability_Hallucination.xml` (removed abilityClass, range → -1)
    - `Defs/AbilityDefs/Ability_CognitiveShield.xml` (removed abilityClass, range → -1)
    - `Defs/AbilityDefs/Ability_PsychicDiffusion.xml` (removed abilityClass, range → -1)
    - `Source/ProjectOvermind/Verb_SpatialAnchor.cs` (added heavy debug logging)
  
  - **Build Status:** Code compiles successfully (0 errors)
  - **Testing Required:** User MUST test with Dev Mode enabled:
    1. Global abilities should cast immediately without targeting circle
    2. F12 console shows yellow "[SPATIAL ANCHOR v1.6.4]" warnings with Thing spawn/tick/hediff verification
    3. Report exact console messages if either fix still fails

### Version 1.6.3 (DEPRECATED - Solutions Failed)
- **❌ v1.6.3 FAILED IN-GAME - Do not use**
  - Custom `Ability_GlobalSelfCast` class did NOT prevent targeting UI (wrong approach)
  - Spatial Anchor Thing did NOT spawn or visual effects did NOT appear (root cause unknown)
  - Build succeeded but runtime behavior completely broken
  - User provided screenshot evidence proving neither fix worked
  
- **v1.6.3 Attempted Fixes (Incorrect Theories):**
  - Created custom `Ability_GlobalSelfCast` class to override `QueueCastingJob()` (WRONG - targeting UI controlled by VerbProperties, not Ability)
  - Added `DrawHighlight()` to Spatial Anchor for targeting preview
  - Enhanced Tick() with persistent visual effects
  - Reduced CheckInterval from 60→30 ticks for reliable debuff application
  - All code compiled cleanly but did not execute correctly in-game

### Version 1.6.2
- **Previous attempt at auto-cast fix (SUPERSEDED by v1.6.3)**
  - Used `TryStartCastOn` override approach (didn't work - targeting UI still appeared)
  - Enhanced Spatial Anchor visuals with debug logging
  - Upgraded Psychic Diffusion to map-wide buff/debuff
- **Note:** Version 1.6.2 fixes were incomplete. Version 1.6.3 provides the definitive solution.

### Version 1.6.0
- Added Spatial Anchor psycast (area control with gravity field)
- Added Cognitive Shield psycast (defensive buff scaling with psychic sensitivity)
- Added Psychic Diffusion psycast (aura buff spreading to nearby allies)
- Added 3 new psytrainer items
- Updated trader stocks to include new psytrainers
- Spatial Anchor features:
  - Level 4 ability creating 20-second gravitational anomaly
  - 10-tile radius, applies -40% move speed and -20% dodge to enemies
  - 10% chance per second to pull enemies toward center
  - Perfect for raid defense and chokepoint control
- Cognitive Shield features:
  - Level 5 ability protecting all colonists for 25 seconds
  - Base +25% psychic sensitivity, -30% mental damage
  - Scales with psychic sensitivity (0.1 = +1%)
  - Mental immunity at threshold 5.0+
  - Threshold perks at 3.0/5.0/8.0 for consciousness and healing
- Psychic Diffusion features:
  - Level 6 ability affecting allies within 20 tiles for 20 seconds
  - Base +10% move/work speed, +5 mood
  - Scales with psychic sensitivity (0.1 = +1%)
  - Threshold perks at 3.0/5.0/8.0 for medical quality, damage reduction, heal pulse
- Critical fixes:
  - Fixed psytrainer comp class error (was using non-existent CompProperties_UseEffectGiveAbility)
  - Fixed duplicate StatDef errors (converted Stats_FeastOfMind.xml to patch operations)
  - All new psytrainers now use correct ProjectOvermind.CompProperties_UseEffect_LearnAbility class
- Build verified: 0 errors, 0 warnings

### Version 1.5.0
- Added Feast of Mind psycast (support/sustenance ability)
- Added Feast of Mind Psytrainer item (1,500 silver, level 3 required)
- Updated trader stocks to include Feast of Mind psytrainer
- Feast of Mind features:
  - Reduces target's hunger rate by 50% (scaled by psychic sensitivity)
  - Increases eating speed by 50% (scaled by psychic sensitivity)
  - 90 second duration, 60 second cooldown
  - Can target self or any friendly pawn
  - Perfect for sustained crafting or long operations
  - Cyan-green visual effects with floating text
  - Does not stack - recasting refreshes duration
- Build verified: 0 errors, 0 warnings

### Version 1.4.0
- Added Inspiration psycast (global buff for all player pawns)
- Added Hallucination psycast (global debuff for all hostile pawns/animals)
- Added Inspiration Psytrainer item (2,000 silver, level 4 required)
- Added Hallucination Psytrainer item (2,400 silver, level 5 required)
- Updated trader stocks to include new psytrainers
- Inspiration features:
  - Map-wide buff: +200% work speed, +70% learning, +20% quality, +15% move
  - 60 second duration, 200 second cooldown
  - Effects scale with psychic sensitivity (0.1x to 2.5x multiplier)
  - Blue aura visual effect, prevents stacking
- Hallucination features:
  - Map-wide debuff: -40% accuracy, -15% move speed
  - 40 second duration, 300 second cooldown
  - 25% panic chance per second (attacks empty space or moves erratically)
  - Purple shimmer visual effect, excludes mechanoids
  - No faction goodwill penalty
- Performance optimized: 60-tick interval processing for both abilities
- Build verified: 0 errors, 0 warnings

### Version 1.3.0
- Added Mind Read psycast (intelligence gathering, reveals complete mental profile)
- Added Mind Read Psytrainer item (1,600 silver, level 3 required)
- Updated I See You alert sound behavior: plays ONLY when hostile invisible entities detected
- Updated trader stocks to include Mind Read psytrainer
- Mind Read features:
  - Works on any humanlike pawn (enemies, visitors, guests, prisoners, quest pawns)
  - Reveals all traits, skills, passions, mood, health, and hidden quest intentions
  - No faction goodwill penalty - safe to use on anyone
  - Psychically Dull pawns have 50% resistance chance
- Build verified: 0 errors, all defs load correctly

### Version 1.2.0
- Added I See You psycast (reveal hidden entities map-wide)
- Added MapComponent_ISeeYou with safe overlay detection system
- Added custom MGS-style alert sound for I See You
- Added I See You Psytrainer item (2,200 silver, level 5 required)
- Updated trader stocks to include I See You psytrainer
- Confirmed Translocate Target works with animals
- Safe compatibility: detects modded invisibility without breaking other mods
- Added save/load support for active reveal sessions

### Version 1.1.0
- Added Mind Spike psycast (enemy mind control)
- Added chain effect mechanic (jumps to nearby enemies on death)
- Added Disoriented hediff (post-control debuff)
- Added Mind Spike Psytrainer item
- Updated trader stocks to include new psytrainer

### Version 1.0.0
- Initial release
- Added Translocate Target psycast
- Added Spatial Daze hediff
- Added Psytrainer item
- Trader and quest reward integration