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
- **Special**: If target dies while controlled, chains to nearest enemy within 6 tiles (once per cast)

### I See You Psycast (NEW!)
- **Description**: Reveal all hidden entities on the map for 60 seconds
- **Target**: Self (map-wide effect)
- **Range**: Entire map
- **Cast Time**: 1.5 seconds
- **Cooldown**: 120 seconds (2 minutes)
- **Duration**: 60 seconds
- **Psyfocus Cost**: 0.25
- **Heat Cost**: 0.12
- **Required Psycast Level**: 5
- **Special**: Detects invisible creatures, stealthed entities, and anomalies
  - Works with vanilla and modded invisibility systems
  - Safe overlay system - doesn't break other mods
  - Effect ends if caster dies or leaves map
  - Alert sound plays ONLY when hostile invisible entities detected

### Mind Read Psycast (NEW!)
- **Description**: Probe a target's mind to reveal all thoughts, traits, skills, and hidden intentions
- **Target**: Any humanlike pawn (enemies, visitors, guests, prisoners, quest pawns)
- **Range**: 10 tiles
- **Cast Time**: 1.0 seconds
- **Cooldown**: 30 seconds
- **Psyfocus Cost**: 0.30
- **Heat Cost**: 0.08
- **Required Psycast Level**: 3
- **Special**: Reveals complete mental profile
  - All traits (including hidden ones)
  - All skills and passions
  - Current mood and thoughts
  - Health summary
  - Quest intentions (betrayal, leaving, joining, etc.)
  - Does NOT affect faction goodwill or relationships
  - Psychically Dull pawns have 50% resistance chance
  - "The mind speaks louder than words."

### Feast of Mind Psycast (NEW!)
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
  - Perfect for sustained operations, long crafting sessions
  - "The mind nourishes the body."

### Gameplay Details
- **Translocate Target**: Non-violent ability (no damage, no relationship penalties)
  - Applies "Spatial Daze" debuff (5 seconds) causing brief disorientation
  - Works on both friendly and hostile pawns (including animals)
  
- **Mind Spike**: Tactical mind control ability
  - Forces enemy into berserk state targeting their allies
  - After effect ends, applies "Disoriented" debuff (-50% move speed, -20% aim for 5s)
  - Chain effect on death allows single-target control to spread chaos
  - "The mind is a weapon — sharpen it well."

- **Mind Read**: Intelligence gathering ability
  - Read complete mental profile without affecting relationships
  - Works on any humanlike: enemies, visitors, guests, prisoners, quest pawns
  - Shows all hidden information including quest betrayal plans
  - Psychically Dull pawns can resist (50% chance)
  - Safe to use: no goodwill penalties or faction reactions
  - "The mind speaks louder than words."

- **Feast of Mind**: Psychic sustenance ability
  - Supports extended operations by reducing hunger needs
  - Perfect for crafters, researchers, or pawns on long hauls
  - Effects scale dynamically with target's psychic sensitivity
  - Can be used on any friendly pawn including yourself
  - Non-combat support ability with zero risk
  - "The mind nourishes the body."

- **I See You**: Anti-ambush and anomaly detection ability
  - Reveals hidden creatures with exclamation mark indicators
  - Detects: invisible pawns, stealthed animals, concealed entities, anomalies
  - Safe detection: checks hediffs, comps, and thing defs for common stealth patterns
  - Player-only reveal: enemies don't gain targeting information
  - Alert sound plays ONLY when hostile entities detected (silent if none found)
  - "Nothing stays hidden from the mind's eye."

### Inspiration Psycast (NEW!)
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
  - "Unlock the mind's full potential."

### Hallucination Psycast (NEW!)
- **Description**: Shatter enemy minds with terrifying psychic illusions
- **Target**: Self (map-wide debuff to all hostiles)
- **Range**: Entire map
- **Cast Time**: 5 seconds
- **Cooldown**: 300 seconds (~5 minutes)
- **Duration**: 40 seconds
- **Psyfocus Cost**: 0.50
- **Heat Cost**: 0.30
- **Required Psycast Level**: 5
- **Special**: Afflicts all hostile pawns and animals (excluding mechanoids)
  - -40% shooting accuracy
  - -15% movement speed
  - 25% chance per second to trigger panic attack
  - Panic behavior: attacks empty space or moves erratically for 1-2 seconds
  - Purple shimmer visual effect
  - Does NOT stack with itself - refreshes duration if recast
  - No faction goodwill penalty
  - "Let them fear what isn't there."

### Gameplay Details

- **Translocate Target**: Non-violent ability (no damage, no relationship penalties)
  - Applies "Spatial Daze" debuff (5 seconds) causing brief disorientation
  - Works on both friendly and hostile pawns (including animals)
  
- **Mind Spike**: Tactical mind control ability
  - Forces enemy into berserk state targeting their allies
  - After effect ends, applies "Disoriented" debuff (-50% move speed, -20% aim for 5s)
  - Chain effect on death allows single-target control to spread chaos
  - "The mind is a weapon — sharpen it well."

- **I See You**: Anti-ambush and anomaly detection ability
  - Reveals hidden creatures with exclamation mark indicators
  - Detects: invisible pawns, stealthed animals, concealed entities, anomalies
  - Safe detection: checks hediffs, comps, and thing defs for common stealth patterns
  - Player-only reveal: enemies don't gain targeting information
  - Alert sound plays ONLY when hostile entities detected (silent if none found)
  - "Nothing stays hidden from the mind's eye."

- **Mind Read**: Intelligence gathering ability
  - Read complete mental profile without affecting relationships
  - Works on any humanlike: enemies, visitors, guests, prisoners, quest pawns
  - Shows all hidden information including quest betrayal plans
  - Psychically Dull pawns can resist (50% chance)
  - Safe to use: no goodwill penalties or faction reactions
  - "The mind speaks louder than words."

- **Inspiration**: Mass psychic empowerment
  - Global buff for all colonists and player-faction humanlike pawns
  - Massive productivity boost for 60 seconds
  - Effects scale with psychic sensitivity (higher sensitivity = stronger buff)
  - Perfect for critical construction/crafting/research deadlines
  - Does not affect animals or non-humanlike pawns
  - "Unlock the mind's full potential."

- **Hallucination**: Psychic terror weapon
  - Global debuff targeting all hostile humanlike pawns and animals
  - Does NOT affect mechanoids (immune to psychic effects)
  - Causes severe combat penalties and panic attacks
  - No faction goodwill penalty - pure combat ability
  - Enemies attack empty space or move erratically during panic
  - Perfect for defending against large raids
  - "Let them fear what isn't there."

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

- **Traders**: Orbital Bulk Goods and Exotic Goods traders (rare)
- **Quest Rewards**: Can appear as a reward in some quests
- **Market Value**:
  - Translocate Target Psytrainer: 1200 silver (level 3)
  - Mind Spike Psytrainer: 1800 silver (level 3)
  - Mind Read Psytrainer: 1600 silver (level 3)
  - Feast of Mind Psytrainer: 1500 silver (level 3)
  - Inspiration Psytrainer: 2000 silver (level 4)
  - I See You Psytrainer: 2200 silver (level 5)
  - Hallucination Psytrainer: 2400 silver (level 5)

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

### Version 1.5.0 (Current)
- Added Feast of Mind psycast (support/sustenance ability)
- Added Feast of Mind Psytrainer item (1500 silver, level 3 required)
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
- Added Inspiration Psytrainer item (2000 silver, level 4 required)
- Added Hallucination Psytrainer item (2400 silver, level 5 required)
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
- Added Mind Read Psytrainer item (1600 silver, level 3 required)
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
- Added I See You Psytrainer item (2200 silver, level 5 required)
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
