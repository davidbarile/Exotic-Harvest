# Exotic Harvest – Development Phases

## Table of Contents

1. [Overview](#overview)
2. [Phase 1: MVP (Minimum Viable Product)](#phase-1-mvp-minimum-viable-product)
3. [Phase 2: Core Features](#phase-2-core-features)
4. [Phase 3: Full Release](#phase-3-full-release)
5. [Phase 4: Post-Launch Content](#phase-4-post-launch-content)
6. [Development Timeline](#development-timeline)
7. [Success Metrics](#success-metrics)

---

## Overview

This document outlines the development phases for **Exotic Harvest**, organizing features by implementation priority and release milestones. Each phase builds upon the previous one, ensuring a solid foundation while progressively adding complexity and polish.

**Target Audience**: Desktop users seeking a relaxing companion app

**Platform Strategy**: PC first, mobile companion later

**Monetization**: Premium ($8+ Steam price) with optional IAPs

---

## Phase 1: MVP (Minimum Viable Product)

### Core Goal
Validate the transparent desktop companion concept with basic functionality.

### Essential Features

#### Desktop Integration
- ✅ **Transparent background window** (Unity with UniWindowController)
- ✅ **Basic minimize/maximize functionality** (hotkey toggle)
- ✅ **Draggable UI elements** (moveable panels)
- ✅ **Click-through detection** when transparent

#### Basic Visual Elements
- **Draggable jungle decorations** (2-3 corner leaf clusters, wind chimes, potted plant, tiki torches)
- **Simple day/night cycle** (background color transition)
- **Basic weather effect** (rain animation only)

#### Core Gameplay Loop
- **One passive harvester**: Bucket that fills during rain
- **One active foraging**: Click dewdrops to collect water
- **Basic inventory system** (water resource only)
- **Simple shop interface** (buy more buckets)

#### Minimal UI
- **Cockpit UI**: minimize/maximize, transparency slider
- **Drag mode toggle** with visual indicator
- **Toast notifications** for resource collection
- **Basic inventory display** (water count)

#### Technical Foundation
- **Save/load system** for player progress
- **Settings persistence** (window position, transparency)
- **Basic error handling** and crash prevention

### MVP Success Criteria
- App runs stably for 1+ hours without crashes
- Players can collect and spend resources in basic loop
- Transparent window integration works smoothly
- Basic gameplay is engaging for 15+ minutes

### Estimated Timeline: 4-6 weeks

---

## Phase 2: Core Features

### Goal
Expand the core loop with multiple systems and engaging progression.

### Gameplay Systems

#### Enhanced Resource Economy
- **5 resource types**: Water, Bugs, Seeds, Moonbeams, Gems
- **3 active foraging methods**: 
  - Click collection (dewdrops, seeds)
  - Drag collection (bucket for raindrops)
  - Net swiping (butterflies, fireflies)
- **5 passive harvesters**: Buckets, Flower pots, Lightning rods, Moon crystals, Spider webs

#### Pet System (Basic)
- **2 pet types**: Chameleon (catches bugs), Squirrel (collects nuts)
- **Pet interactions**: Eye tracking, petting responses
- **Pet maintenance**: Simple feeding system
- **Pet moods**: Happy/hungry states affecting collection rate

#### Enhanced UI & Visual Polish
- **Complete side menu tabs**: Inventory, Shop, Pets, Settings
- **Draggable/dockable windows**
- **Hot corner activation**
- **5 draggable decorations**: Plants, torches, chimes, masks, fountains
- **Improved weather**: Rain, storm, clear sky with particles

#### Time & Daily Systems
- **Enhanced day/night cycle** (adjustable realtime-to-gametime day/night cycle)
- **Resource availability by time**: Morning dewdrops, night fireflies
- **Daily task system**: Simple collection goals
- **Daily rewards**: Bonus resources

#### Crafting & Upgrades
- **Basic crafting recipes**: Combine 2-3 resource types
- **Tool upgrades**: Bigger buckets, faster nets, better crystals
- **Pet upgrades**: Improved collection rates
- **Decoration unlocks**: New visual items through progression

### Phase 2 Success Criteria
- 30+ minute average session length
- Clear progression visible to players
- Multiple viable strategies for resource optimization
- Pets feel alive and engaging

### Estimated Timeline: 8-10 weeks

---

## Phase 3: Full Release

### Goal
Polish to commercial quality with rich content and advanced features.

### Advanced Systems

#### Complete Resource Economy
- **All 20+ resource types** from GDD
- **10+ foraging interactions**: Mining, pollination, treasure hunting
- **15+ passive harvesters** with unique mechanics
- **Multi-resource crafting recipes** (Catan-style complexity)

#### Full Pet Ecosystem
- **8 pet types** with unique abilities and animations
- **Pet lifecycles**: Caterpillar → Cocoon → Butterfly
- **Advanced pet interactions**: Playful behaviors, habitat customization
- **Pet synergies**: Combinations that boost each other

#### Maps & Exploration
- **3 major maps**: Meadows, Sky Realm, Beach
- **Hidden map discovery** system
- **Map-specific resources and events**
- **Portal decorations** for map access

#### Advanced Visual Systems
- **Dynamic wildlife**: Birds, snakes, seasonal creatures
- **Weather variety**: Snow, wind, storms with unique effects
- **Moon phases** affecting night resources
- **Seasonal changes** and special events

#### Progression & Maintenance
- **Complete upgrade trees** for all systems
- **Space management strategies** (finite desktop area)
- **Weekly/monthly challenges**
- **Streak systems** with meaningful rewards

#### Audio & Polish
- **Dynamic ambient soundtrack** (layers based on time/weather)
- **Complete SFX library**: Environmental sounds, UI feedback
- **Full Localization**: 8-10 languages
- **Accessibility options**: Colorblind support, audio cues
- **Performance optimization** for older systems

### Social & Monetization Features

#### Store & Economy
- **Complete IAP integration** (optional hard currency)
- **Ad integration** for boosts (optional, PC can disable)
- **Inventory management** with expansion options
- **Seasonal content** and limited-time offers

#### Cross-Platform Integration
- **Mobile companion app** (basic version)
- **Cloud save synchronization**
- **Cross-device progress sharing**

### Special Content

#### Rare Events System
- **Monthly rare creatures**: Unicorn, Mermaid, Phoenix
- **Special event calendars**: Halloween, Winter, Spring themes
- **Achievement system** with rare rewards
- **Hidden secrets** and easter eggs

### Phase 3 Success Criteria
- Professional visual and audio quality
- 2+ hours average session engagement
- Positive Steam reviews (80%+ positive)
- Sustainable monetization metrics
- Ready for marketing campaign

### Estimated Timeline: 12-16 weeks

---

## Phase 4: Post-Launch Content

### Goal
Expand and retain playerbase through ongoing content updates.

### Content Expansion
- **New maps and biomes**: Desert, Arctic, Underwater
- **Seasonal events**: Limited-time maps, rare resources
- **New pet types**: Dragons, magical creatures, mythical animals
- **Advanced decorations**: Animated items, interactive furniture
- **Prestige system**: New game+ with enhanced rewards

### Community Features
- **Photo mode**: Capture and share desktop setups
- **Custom themes**: Player-created decoration sets
- **Leaderboards**: Collection competitions, rare event tracking
- **Social sharing**: Achievement sharing, pet photos

### Platform Expansion
- **Full mobile app**: Standalone companion with all features

### Advanced Systems
- **AI pet behaviors**: Machine learning for more realistic interactions
- **Procedural events**: Dynamic rare event generation
- **Weather API integration**: Real-world weather affects game
- **Calendar integration**: Real holidays trigger special events

### Estimated Timeline: Ongoing (3-6 month content cycles)

---

## Development Timeline

### Overall Schedule
- **Phase 1 (MVP)**: Weeks 1-6
- **Phase 2 (Core)**: Weeks 7-16
- **Phase 3 (Full Release)**: Weeks 17-32
- **Phase 4 (Post-Launch)**: Ongoing

### Key Milestones
1. **Week 6**: MVP playable build
2. **Week 12**: Core systems integrated
3. **Week 20**: Alpha build with all major features
4. **Week 28**: Beta build ready for testing
5. **Week 32**: Gold master for Steam release
6. **Week 36**: Launch + Day 1 patch

### Parallel Development Tracks
- **Art pipeline**: Decorations, pets, environments
- **Audio production**: Music, SFX, integration
- **Platform testing**: Windows/Mac compatibility
- **Marketing preparation**: Steam page, trailer, press kit

---

## Success Metrics

### MVP Metrics
- **Technical stability**: <1% crash rate
- **User engagement**: 15+ minute average session
- **Core loop validation**: Players complete 3+ collection cycles

### Core Feature Metrics
- **Session length**: 30+ minute average
- **Retention**: 60% day-1, 30% day-7
- **Progression**: 80% players unlock 2nd pet

### Full Release Metrics
- **Revenue**: $8+ average selling price sustainable
- **Reviews**: 80%+ positive on Steam
- **Retention**: 70% day-1, 40% day-7, 20% day-30
- **Monetization**: 10% IAP conversion among active players

### Post-Launch Metrics
- **Content engagement**: 60% players try new maps/pets
- **Community growth**: Active Discord/Reddit communities
- **Platform expansion**: Successful mobile companion adoption
- **Longevity**: 12+ month active player retention

---

*Last updated: November 30, 2025*