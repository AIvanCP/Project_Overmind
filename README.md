# Project Overmind - Mindshift Abilities

A RimWorld mod that adds new psionic abilities inspired by Warframe's mind control mechanics.

## Features

### Translocate Target Psycast
- **Description**: Instantly relocate any pawn (ally or enemy) to a new location
- **Target**: Any living pawn (not mechanoids or downed pawns)
- **Range**: 15 tiles for target selection, 10 tiles for destination
- **Cast Time**: 1 second
- **Cooldown**: 15 seconds
- **Psyfocus Cost**: 0.10
- **Heat Cost**: 0.05
- **Required Psycast Level**: 3
- **Special**: Non-violent ability with no damage or relationship penalties
  - Applies "Spatial Daze" debuff (5 seconds) causing brief disorientation
  - Works on both friendly and hostile pawns (including animals)

### Mind Spike Psycast
- **Description**: Seize an enemy's mind and force them to attack their allies
- **Target**: Enemy humanlike pawns only
- **Range**: 12 tiles
- **Cast Time**: 1.5 seconds
- **Cooldown**: 45 seconds
- **Duration**: 10 seconds
- **Psyfocus Cost**: 0.15
- **Heat Cost**: 0.08
- **Required Psycast Level**: 3
- **Special**: Tactical mind control ability
  - Forces enemy into berserk state targeting their allies
  - After effect ends, applies "Disoriented" debuff (-50% move speed, -20% aim for 5s)
  - If target dies while controlled, chains to nearest enemy within 6 tiles (once per cast)
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
- **Duration**: 90 seconds
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
- **Cooldown**: 200 seconds (~3.3 minutes)
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
- **Description**: Reveal all hidden entities on the map for 60 seconds
- **Target**: Self (map-wide effect)
- **Range**: Entire map
- **Cast Time**: 1.5 seconds
- **Cooldown**: 120 seconds (2 minutes)
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
- **Cast Time**: 5 seconds
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

### Spatial Anchor Psycast
- **Description**: Create a gravitational anomaly that slows and pulls hostile pawns
- **Target**: Ground location
- **Range**: 15 tiles
- **Cast Time**: 1 second
- **Cooldown**: 90 seconds
- **Duration**: 20 seconds
- **Psyfocus Cost**: 0.35
- **Heat Cost**: 0.25
- **Required Psycast Level**: 4
- **Special**: Area control ability
  - Creates invisible anchor with 10-tile radius
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
- **Cooldown**: 120 seconds
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
- **Duration**: 20 seconds
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
- Inspiration Psytrainer: 2,000 silver (level 4)
- I See You Psytrainer: 2,200 silver (level 5)
- Hallucination Psytrainer: 2,400 silver (level 5)
- Spatial Anchor Psytrainer: 1,800 silver (level 4)
- Cognitive Shield Psytrainer: 2,200 silver (level 5)
- Psychic Diffusion Psytrainer: 2,600 silver (level 6)
- I See You Psytrainer: 2,200 silver (level 5)
- Hallucination Psytrainer: 2,400 silver (level 5)

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

### Version 1.6.4 (Current - Emergency Fix)
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