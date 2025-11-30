# Exotic Harvest – Game Design Document (GDD)

*(a.k.a. Enchanted Harvest / Jungle Harvest / Amazon Harvest)*

## Table of Contents

1. [Overview](#overview)
2. [Visual Experience & Desktop Integration](#visual-experience--desktop-integration)
3. [User Interface Systems](#user-interface-systems)
4. [Economy & Resource Systems](#economy--resource-systems)
5. [Pet Systems](#pet-systems)
6. [Time & Weather Systems](#time--weather-systems)
7. [Inventory & Store Systems](#inventory--store-systems)
8. [Progression & Maintenance Systems](#progression--maintenance-systems)
9. [Map & Exploration Systems](#map--exploration-systems)
10. [Audio Systems](#audio-systems)
11. [Cross-Platform Systems](#cross-platform-systems)
12. [Special Event Systems](#special-event-systems)

---

## Overview

### Core Concept

A peaceful desktop companion game built in Unity, running with a transparent background so the player's Windows/Mac desktop remains visible. The game blends two major components:

**Cozy Visual Environment**
Decorative jungle-themed items, ambient wildlife, day/night/weather effects, pets, and a draggable/arrangeable environment that becomes part of the user's workspace.

**Resource Collection Gameplay**
A combination of active foraging and passive idle harvesting, fueling an economy used to unlock new decorations, tools, pets, and upgrades.

The fantasy-cozy aesthetic includes jungle flora, tiki objects, wildlife, magical night-sky resources, and whimsical rare events (mermaid, unicorn, etc.).

### Monetization Strategy
- **Steam price target**: $8 minimum
- **Optional IAPs** (hard currency)
- **Optional ads for boosts** (PC version may disable ads)

---

## Visual Experience & Desktop Integration

### Desktop Integration
- **Transparent background** lets the player see their actual computer desktop
- **Draggable decorations** decorate screen corners, yet avoid blocking important screen areas
- **Moving wildlife**: snakes slither occasionally, birds fly in to nest, fireflies twinkle, caterpillars become cocoons → butterflies
- **Sky background** has adjustable alpha (fully transparent → fully visible)
- **Weather cycles**: rain, storms, snow, wind, fireflies at night
- **Slow day–night cycle** with sunrise, sunset, moon phases, visible clouds


### Decorations (Draggable World Objects)
Jungle plants, houseplants, bamboo wind chimes, masks, tiki torches, lamps, vines, fountains, bird perches, mailboxes, huts, terrariums, etc.

**Decoration Features:**
- Drag placement
- Lock/unlock position
- Some may animate or have passive effects (branches grow fruit, etc.)

---

## User Interface Systems

### Core UI Components
- **Side Menu Tabs**: Settings, Inventory, Shop, Pets, Maps, Crafting, Upgrades
- **Draggable UI Windows**: moveable and dockable
- **Notification Toasts**: resource gained, pet event, inventory full, etc.

### Interaction Systems
- **Minimize/Maximize Button**: with hotkey press or button click/rollover
- **Hot corners**: optional minimize/maximize triggers
- **Toggle for Drag Mode**: enables rearranging decorations; clear UI indicator

### Visual Feedback
- Clear visual indicators for drag mode activation
- Toast notifications for all major events
- Responsive UI elements that adapt to transparency settings

---

## Economy & Resource Systems

### Economic Flow
**Resources → Crafting → Upgrades → More Resources**

### Resource Categories

#### Primary Resources
- **Water**: raindrops, dew
- **Bugs**: caterpillars, butterflies, dragonflies, bees, crickets, fireflies, ladybugs
- **Nature**: seeds, clovers (incl. 4-leaf), nuts, berries, feathers, shells, tree sap, nectar, pollen
- **Night Sky**: moonbeams, stardust, comets, falling stars, planets

#### Valuable Resources
- **Valuables**: gems, gold, jewelry, rare relics
- **Abstract**: secrets, shadows, memories, lullabies

#### Special Event Resources
- **Special Events**: unicorn blessing, mermaid song (rare; once/month resets)

### Acquisition Systems

#### Foraging (Active)
Player interacts directly with screen elements:
- Click leaves/rocks/dewdrops to collect them
- Swipe butterflies, fireflies, dragonflies using a net
- Drag bushes aside to reveal hidden objects
- Click-and-hold to mine rocks/dig soil
- Drag bucket across the screen to catch raindrops
- Click flowers to send bees to pollinate
- Use tools (jar/telescope/moon crystal) to capture stardust, moonbeams, lightning

*Foraging slightly speeds up some passive systems ("Cookie Clicker effect").*

#### Harvesting (Passive/Idle)
Placed structures and pets generate resources over time:
- **Buckets** generate water when it rains
- **Flower pots** grow seeds → plants → fruits
- **Beehives** send bees to collect pollen/honey
- **Pets** collect their specialized resources
- **Spider webs** catch fireflies and flies
- **Lightning rods** collect lightning energy
- **Moon crystals** charge at night/moon phases

**Tool Properties:**
- Limited lifespan
- Max capacity (must be emptied manually)

---

## Pet Systems

### Pet Types & Behaviors
Visible creatures that live on the screen and interact with both the environment and the player:

- **Chameleon/Frog**: catches bugs with long tongue; reactive animations
- **Squirrel**: collects nuts
- **Bee colony**: produces pollen/honey
- **Spider**: creates web traps
- **Caterpillar → Cocoon → Butterfly**: natural lifecycle
- **Fish/Octopus**: in desktop terrarium/bowl

### Pet Interactions
- **Follow the mouse** with eyes
- **Respond to petting**
- **Have moods** (hungry, playful, sleepy)
- **Require light maintenance** (feeding, cleaning habitats)

### Pet Progression
- Pets can be upgraded for better resource collection
- Different pets specialize in different resource types
- Pet happiness affects collection efficiency

---

## Time & Weather Systems

### Time-Based Gameplay
Time influences appearance, resource availability, and passive generation speed:

- **Morning**: dewdrops, early birds, nectar bursts
- **Day**: seeds, berries, bugs, oysters, shells, starfish
- **Evening**: fireflies, sunset pollen bloom
- **Night**: moonbeams, stardust, falling stars
- **Full moon**: special rare events (charging crystals)
- **Storms**: increased water, lightning, rare items
- **Seasonal/Monthly**: special creatures, rare shells, unique events

### Daily/Weekly Systems
- **Daily Task, Challenge, Reward**
- **Daily/Weekly streak bonuses**
- **Day-specific puzzles, goals, offers/bonuses**

---

## Inventory & Store Systems

### Store Structure
Full-screen multipage store with tabs:
- **Decorations**
- **Pets**
- **Resources** (buy missing types)
- **Tools/Harvesters**
- **Boosts** (speed, timers)
- **Inventory expansions**
- **Streak protection**
- **Hard Currency**
- **Ads & Offers**

### Inventory Management
- **Limited space**, upgradeable
- **Stores resources**, crafted items, tools, and decorations
- **Crafting** uses Catan-like cost patterns (combinations of multiple resource types)

---

## Progression & Maintenance Systems

### Maintenance Loop
Players check in regularly to:
- Water plants
- Feed pets
- Empty full tools (buckets, honeycombs)
- Collect from trees
- Manage inventory space
- Check mailbox for gifts/quests

### Upgrades
Upgradable items include:
- **Beehives**
- **Flowers/gardens**
- **Harvesting tools**
- **Tool efficiency** (buckets, nets, telescopes, shovels)
- **Decorations**
- **Pets**

*Costs use multi-resource combinations and scale gradually.*

### Ground/Screen Space Management
Finite area means players must decide:
- Which harvesters to place
- How many buckets vs. potted plants vs. crystals
- Whether the screen becomes visually busy or resource-optimized

---

## Map & Exploration Systems

### Map Types
Players unlock or find maps to new "zones," each with unique resources and visuals:

- **Meadows**: clovers, ladybugs, honeybees, flowers
- **Sky Realm**: stars, planets, comets, moonbeams
- **Beach**: shells, oysters, pearls, sand dollars, treasure coins, messages in bottles

### Map Implementation
Maps are sometimes hidden inside other maps and can be:
- **Mini overlays**
- **Full-screen zones**
- **Desktop-mounted "portals"**

---

## Audio Systems

### Audio Design
- **Relaxing ambient soundtrack**
- **Dynamic layers** that change based on time of day
- **Option to mute when app is hidden**
- **Background SFX**: crickets, rain, wind chimes, birds, frogs, bubbling water

---

## Cross-Platform Systems

### Mobile Companion App
A mobile app allows:
- Harvesting
- Buying items
- Upgrading tools
- Managing inventory
- Completing daily tasks

*Syncs with PC so progress carries over.*

---

## Special Event Systems

### Rare Events
Occasional magical encounters grant rare one-time items:
- **Mermaid** appears on a beach map
- **Unicorn** in meadow during full moon
- **Phoenix feather** from rare meteor
- **Ancient jungle spirit** drops enchanted items

*After discovering one, a month-long cooldown before another encounter.*

---

## Technical Notes

### Prototype Requirements
A simple first prototype should include:
- Transparent background window
- Draggable decoration
- One passive harvester (e.g., bucket that fills when rain activates)
- One active foraging mechanic (click to pick up dew)
- Basic day/night & weather cycle
- Simple inventory and store placeholder
- Minimal UI (toggle show/hide, drag mode, toast notifications)

### Future Development Areas
- Visual mockups and UI flow
- Dev log creation
- Kickstarter pitch development
- Monetization tuning
- Steam page & trailer

---

*Last updated: November 30, 2025*