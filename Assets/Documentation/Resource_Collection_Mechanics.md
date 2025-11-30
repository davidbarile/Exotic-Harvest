# Resource Collection Mechanics System
*Exotic Harvest - Advanced Collection Gameplay Design*

## Overview
A comprehensive collection mechanics framework leveraging the existing transparent overlay, drag systems, and animation pipeline to create engaging active and passive collection gameplay with tool progression and environmental interaction.

---

## Core Collection Philosophy

### Active vs Passive Collection
- **Active Collection**: Player-driven interactions requiring skill, timing, and attention
- **Passive Collection**: Set-and-forget systems that reward strategic placement and tool investment
- **Hybrid Systems**: Tools that can be used both actively and passively with different efficiency rates

### Progression Philosophy
- **Tool Evolution**: Simple tools upgrade into complex systems with multiple interaction modes
- **Skill Development**: Player expertise directly impacts collection efficiency and rare resource discovery
- **Strategic Depth**: Environmental factors, timing, and tool synergies create optimization gameplay

---

## Collection Mechanics Breakdown

### 1. Bucket Rain Collection System

#### Active Collection Mode
**Core Mechanic**: Drag bucket around screen to intercept falling raindrops using physics collision detection

**Implementation Details**:
- Raindrops spawn from top of transparent overlay during weather events
- Bucket collision radius expands with upgrades (visual feedback via highlight ring)
- Real-time physics: raindrop trajectory affected by wind patterns
- Bucket capacity meter fills progressively with satisfying audio/visual feedback

**Skill Elements**:
- Prediction of raindrop patterns and optimal positioning
- Quick repositioning when weather patterns shift
- Risk/reward timing: wait for heavy downpour vs steady collection

#### Passive Collection Mode
**Core Mechanic**: Place bucket on desktop with collection radius indicator

**Implementation Details**:
- Static collection zone with visual boundary (subtle transparent circle)
- Collection rate based on weather intensity and bucket specifications
- Overflow mechanics: bucket must be emptied or water is lost
- Multiple bucket coordination for advanced players

#### Upgrade Progression
1. **Basic Wooden Bucket**: Small capacity, manual emptying required
2. **Metal Pail**: Increased capacity, shows fill percentage
3. **Collector Basin**: Auto-empties to storage, wider collection radius
4. **Funnel Network**: Connects multiple collectors with visible pipe system
5. **Storm Harvester**: Magnetic attraction pulls raindrops from distance
6. **Weather Station**: Predicts rain patterns, shows optimal placement zones

#### Advanced Mechanics
- **Funnel System**: Visual pipe connections between buckets for distributed collection
- **Vacuum Collector**: Active tool with energy meter and cooldown mechanics
- **Weather Prediction**: Forecast overlay showing incoming precipitation patterns
- **Contamination System**: Different water types (clean rain, storm water, morning dew) with varying values

---

### 2. Firefly Jar Collection System

#### Active Swiping Collection
**Core Mechanic**: Click-drag net cursor over fireflies with realistic capture physics

**Implementation Details**:
- Fireflies follow believable flight AI: attracted to light sources, flee from sudden movements
- Net cursor with trailing effect shows swipe path and capture zone
- Capture success based on swipe smoothness, speed, and firefly behavior state
- Different firefly species require adapted capture techniques

**Skill Elements**:
- Learning individual firefly movement patterns and preferences
- Smooth, controlled swiping motions for higher success rates
- Timing captures during firefly "calm" behavioral states
- Managing multiple fireflies simultaneously during swarm events

#### Passive Nectar Trap
**Core Mechanic**: Place jars with nectar to attract fireflies over time

**Implementation Details**:
- Nectar quality and type determines attraction radius and species selectivity
- Visual nectar level decreases over time, requiring periodic refreshing
- Different jar sizes and materials affect collection efficiency
- Firefly behavior changes based on jar placement and environmental factors

#### Upgrade Progression
1. **Mason Jar**: Basic capacity, requires regular nectar refills
2. **Enchanted Vessel**: Slower nectar consumption, glows to attract fireflies
3. **Breeding Habitat**: Captured fireflies reproduce, creating sustainable population
4. **Bioluminescent Network**: Connected jars share firefly populations
5. **Fairy Light Generator**: Converts captured fireflies to magical energy
6. **Ecosystem Manager**: Balances firefly populations across desktop environment

#### Advanced Mechanics
- **Bioluminescent Upgrades**: Released fireflies create temporary light sources attracting more insects
- **Breeding System**: Combine firefly types for new species with unique collection bonuses
- **Light Pollution**: Desktop brightness and monitor settings affect spawn rates
- **Circadian Rhythms**: Firefly activity peaks at specific real-world times

---

### 3. Shell & Treasure Hunting System

#### Beach Combing Mechanics
**Core Mechanic**: Shells spawn along dynamic "shore lines" that shift with virtual tides

**Implementation Details**:
- Invisible beach zones that reveal when hovered (subtle sand texture overlay)
- Click-drag digging with limited searches per area to prevent endless grinding
- Metal detector tool provides audio/haptic feedback showing treasure proximity
- Beach "refreshes" with new shell deposits based on real-world tidal data

**Search Patterns**:
- **Surface Collection**: Obvious shells scattered on visible beach areas
- **Shallow Digging**: Sand mounds that require single clicks to excavate
- **Deep Archaeology**: Multi-stage digging requiring tool upgrades and patience
- **Tide Pool Investigation**: Interactive pools with hidden creatures and treasures

#### Tide Pool Exploration
**Core Mechanic**: Interactive tide pools reveal contents when investigated

**Implementation Details**:
- Pools change contents based on time of day and weather conditions
- Some shells guarded by virtual crabs requiring food bribes or careful timing
- Water temperature and clarity affect what species and treasures are present
- Ecosystem balance: overfishing reduces future spawn rates

#### Upgrade Progression
1. **Beach Comb**: Reveals nearby surface shells with subtle highlighting
2. **Sand Shovel**: Allows shallow digging in marked areas
3. **Metal Detector**: Audio/visual feedback for buried treasures
4. **Archaeological Kit**: Deep digging with preservation tools for rare artifacts
5. **Marine Biologist Gear**: Identifies species and their preferred habitats
6. **Treasure Hunter's Arsenal**: Advanced detection, underwater exploration capabilities

#### Advanced Features
- **Specimen Collection**: Detailed catalog system with identification mini-games
- **Trading Network**: Exchange rare shells with NPC collectors or community marketplace
- **Archaeological Expeditions**: Special timed events with ancient artifact discoveries
- **Conservation Mechanics**: Sustainable collection practices unlock rare species access

---

### 4. Moonbeam Collection System

#### Focusing Mechanics
**Core Mechanic**: Use draggable mirrors and crystals to redirect moonbeams into collection containers

**Implementation Details**:
- Real-time ray-tracing system showing beam paths and reflection angles
- Mirror placement requires geometric thinking and spatial reasoning
- Different moon phases provide varying beam intensities and spectral properties
- Cloud coverage affects beam availability (integrated with weather system)

**Puzzle Elements**:
- **Single Reflection**: Simple mirror positioning challenges
- **Multi-Bounce Systems**: Complex arrangements requiring multiple reflections
- **Prismatic Splitting**: Crystal tools that separate moonbeams into component colors
- **Beam Combination**: Merge different lunar energies for rare essence creation

#### Passive Collection
**Core Mechanic**: Place moonstone collectors that automatically gather ambient lunar energy

**Implementation Details**:
- Collection efficiency tied to real-world moon phase data
- Atmospheric clarity simulation affects beam intensity and collection rates
- Collector positioning matters: elevation, obstacles, and interference patterns
- Energy can be concentrated for potent effects or distributed for sustained benefits

#### Upgrade Progression
1. **Polished Stone**: Basic lunar energy absorption with minimal efficiency
2. **Silver Mirror**: Directed collection with manual beam focusing
3. **Crystal Array**: Automated collection with spectral analysis capabilities
4. **Lunar Observatory**: Advanced positioning system with phase prediction
5. **Celestial Resonator**: Amplifies rare astronomical events
6. **Astral Conduit**: Connects to cosmic events beyond Earth's moon

#### Celestial Events
- **Lunar Eclipses**: Blood moon beams with unique alchemical properties
- **Meteor Showers**: Shooting star fragments enhance collection equipment
- **Planetary Alignments**: Rare cosmic events creating prismatic moonbeam effects
- **Aurora Integration**: Northern lights create spectacular visual combinations with moonbeams

---

### 5. Four-Leaf Clover Searching System

#### Pattern Recognition Gameplay
**Core Mechanic**: Examine clover patches using observational skills and visual analysis

**Implementation Details**:
- Realistic clover generation with statistically accurate 4-leaf probability (1 in 5,000-10,000)
- Subtle visual hints: slight color variations, leaf spacing irregularities, growth patterns
- Magnifying glass tool reveals fine details but has limited daily uses
- Weather effects: rain makes clovers more vibrant, sun creates searching shadows

**Search Strategies**:
- **Systematic Scanning**: Grid-based examination of large clover fields
- **Intuitive Hunting**: Following visual hunches and pattern recognition
- **Scientific Approach**: Using tools and knowledge to increase success probability
- **Meditative Search**: Relaxed observation that sometimes yields unexpected discoveries

#### Cultivation System
**Core Mechanic**: Plant and nurture clover gardens to increase 4-leaf probability over time

**Implementation Details**:
- Garden plots require water, fertilizer, and pest protection over real-world days
- Genetic breeding mechanics: crossbreed rare specimens for new varieties
- Environmental factors: soil quality, sunlight, temperature affect growth rates
- Patience rewards: well-tended gardens develop higher luck probability over time

#### Upgrade Progression
1. **Keen Eye**: Slight highlighting of promising clover patches
2. **Magnifying Glass**: Detailed examination tool with limited daily uses
3. **Botanist's Knowledge**: Information about optimal growing conditions
4. **Genetic Analysis Kit**: Understanding clover DNA for breeding programs
5. **Luck Amplifier**: Technology that slightly increases 4-leaf probability in treated areas
6. **Fortune Field Generator**: Advanced cultivation creating sustainable lucky clover populations

#### Luck Mechanics Integration
- **Fortune Effects**: Found 4-leaf clovers provide temporary luck boosts affecting all collection activities
- **Rarity Scaling**: 5, 6, 7+ leaf clovers become exponentially rarer with proportional rewards
- **Preservation Techniques**: Special methods to maintain clover potency and prevent deterioration
- **Luck Banking**: Store and strategically deploy fortune effects for maximum impact

---

## Cross-System Integration

### Tool Synergies
**Interconnected Systems**: Collection tools enhance each other's effectiveness through realistic interactions

- **Firefly Illumination**: Captured fireflies provide light sources for nighttime shell hunting
- **Moonbeam Enhancement**: Lunar energy charges other collection tools for improved efficiency
- **Rain Blessing**: Collected rainwater accelerates clover garden growth rates
- **Lucky Resonance**: 4-leaf clovers increase spawn rates for rare resources across all systems

### Pet Assistance Integration
**Helper Behaviors**: Pets provide specialized collection support based on natural behaviors

- **Feline Hunters**: Cats instinctively track fireflies but require training to avoid damage
- **Avian Scouts**: Birds provide aerial reconnaissance for shell hunting and clover spotting
- **Nocturnal Specialists**: Night-active pets work optimally with moonbeam collection systems
- **Lucky Companions**: Certain pets possess inherent fortune traits boosting clover discovery

### Environmental Storytelling
**Atmospheric Integration**: Collection mechanics reflect and enhance the desktop overlay experience

- **Weather Synchronization**: Real-world weather data influences virtual resource availability
- **Seasonal Variations**: Different times of year emphasize different collection opportunities
- **Location Awareness**: Desktop placement strategies create territorial collection bonuses
- **Circadian Rhythms**: Real-world time affects which collection methods are most effective

### Performance Considerations
**Technical Optimization**: Balancing rich gameplay with transparent overlay performance requirements

- **Object Pooling**: Reuse spawned resources and effect objects to minimize memory allocation
- **Level-of-Detail**: Reduce particle complexity and animation fidelity when overlay is inactive
- **Adaptive Quality**: Scale visual effects based on system performance and user preferences
- **Batch Processing**: Group similar collection operations to minimize frame rate impact

---

## Implementation Architecture

### Resource Definition System
Extend existing ScriptableObject architecture for comprehensive resource management:

```csharp
[CreateAssetMenu(fileName = "New Resource", menuName = "Exotic Harvest/Resource Definition")]
public class ResourceDefinition : ScriptableObject
{
    [Header("Basic Properties")]
    public string resourceName;
    public Sprite icon;
    public ResourceRarity rarity;
    public float baseValue;
    
    [Header("Collection Properties")]
    public CollectionMethod[] validCollectionMethods;
    public SpawnConditions spawnRequirements;
    public CollectionDifficulty difficulty;
    
    [Header("Integration")]
    public ToolSynergy[] toolBonuses;
    public PetInteraction[] petBehaviors;
    public WeatherInfluence[] weatherEffects;
}
```

### Collection Tool Framework
Build upon existing `UiDraggablePanel` system for interactive collection tools:

```csharp
public abstract class CollectionTool : UiDraggablePanel
{
    [Header("Collection Properties")]
    public ToolType toolType;
    public float collectionRadius = 2.0f;
    public int capacity = 10;
    public CollectionMode currentMode = CollectionMode.Active;
    
    // Integration with existing drag system
    protected override void OnDragModeChanged(bool dragMode)
    {
        base.OnDragModeChanged(dragMode);
        UpdateCollectionVisualization(dragMode);
    }
    
    // DOTween integration for collection effects
    public virtual void PlayCollectionEffect(ResourceType resourceType)
    {
        transform.DOPunchScale(Vector3.one * 0.1f, 0.5f);
        // Additional juice effects using existing animation pipeline
    }
}
```

### Integration Points
- **UniWindowController**: Leverage existing file drop system for resource import mechanics
- **DOTween Pipeline**: Extend current animation framework for collection visual feedback
- **Event System**: Build upon `ScreenManager.OnDragModeChanged` for collection mode transitions
- **Transparent Overlay**: Utilize hit-testing capabilities for precise collection interaction zones

---

## Conclusion

This resource collection system creates a rich, skill-based gameplay loop that leverages Exotic Harvest's unique transparent overlay capabilities. By combining active engagement with passive progression, players develop expertise across multiple collection methods while tools and pets provide meaningful advancement paths.

The interconnected nature of the systems encourages experimentation and strategic thinking, while the integration with real-world data (weather, moon phases, time) creates a living, breathing collection experience that evolves with both the player's desktop environment and the natural world.

*This document serves as the comprehensive design specification for implementing advanced resource collection mechanics within the Exotic Harvest desktop overlay application.*