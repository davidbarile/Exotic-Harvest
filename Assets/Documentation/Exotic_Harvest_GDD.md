# Exotic Harvest — Design Document

## Working Titles
- Jungle Harvest  
- Amazon Harvest  
- Exotic Harvest  
- Enchanted Harvest

---

## Core Concept
A Unity desktop-overlay idle/harvesting game with a transparent background that sits on top of the user's desktop. Combines a cozy, decorative environment (customizable pets, draggable decorations) with an incremental harvesting/foraging loop and upgrades.

---

# 1. Game Design Document (Lean GDD)

## High-Level Overview
Players decorate a desktop space with jungle-themed items and pets while collecting resources that spawn over time or via interaction. Progression is achieved via upgrades, pets, tools, puzzles and timed events.

## Core Gameplay Loop
1. Collect resources manually or passively (time, weather, pets).  
2. Upgrade harvesters, pets, tools, inventory capacity.  
3. Decorate the environment and lock layouts.  
4. Unlock new resources, puzzles, and rare encounters.  
5. Repeat, with daily/weekly/monthly cycles and events.

## Primary Systems

### Decorative Environment
- Transparent overlay; draggable, placeable decorations with lock mode.  
- Jungle motif: plants, vines, tiki elements, small structures.  
- Optional day/night cycle and weather (rain, snow, clouds).  
- Ambient life: birds, snakes, cocoons, hatching eggs, fireflies.  
- Draggable foliage patches to avoid blocking important screen areas.

### Pets & Helpers
- Pet examples: cat, gecko, chameleon, fish, octopus, squirrel, rabbit, spider, caterpillar→butterfly.  
- Behaviors: eye-follow cursor, be petted, get hungry/clean, help harvest (catch flies, gather nuts), make sounds/emotes.  
- Pets live in small windows/terrariums/bowls and can interact with the desktop world.

### Resources & Time Effects
- Morning: dew, nectar, sap, feathers, shells, clovers.  
- Day: insects, butterflies, berries, flowers, mushrooms.  
- Night: moonbeams, shooting stars, auroras.  
- Rare: amethyst, spiderwebs, 4-leaf clovers, rainbow events, pot of gold.  
- Weather affects resource types (rain fills buckets, storms yield lightning items).

### Foraging & Harvesting
- Actions: click, multi-click, hold-to-fill, drag backgrounds, dig, overturn rocks, open shells.  
- Tools: nets, shovels, buckets, magnifying glass, mirrors, telescope, microscope.  
- Auto-collectors: limited-life devices that can be purchased/upgraded.  
- Catan-style spawn likelihoods: upgrades modify spawn frequencies.

### Progression Systems
- Daily tasks, daily challenges, and streak bonuses (with purchasable streak protection).  
- Puzzle fragments awarded from activities — complete puzzles for keys/rewards.  
- Rare encounters (mermaid, unicorn) with long cooldowns.  
- Quests: collect, interact, discover; reward tiers scale with difficulty.

### Economy & Store
- Soft currency: resources.  
- Hard currency: IAP.  
- Ads: optional boosts or direct currency offers.  
- Store tabs: Decorations, Pets, Resources, Harvesters, Currency, Ads.  
- Inventory: limited, upgradeable; combine resources to craft items.

### UI / UX
- Draggable windows: inventory, store, quests, pets.  
- Hotkey/hot-corner show-hide; minimized 'cockpit' mode.  
- Notification toasts.  
- Mailbox for messages and mobile-client sync.

### Technical
- Transparent overlay window; addressables for assets.  
- Global hotkey detection (need platform-specific handling when unfocused).  
- Dynamic ambient audio that mutes when app hidden.  
- Save/load layouts via JSON.

### Mobile Companion
- Lightweight mobile app for remote harvesting; syncs with desktop client.

---

# 2. Missing Systems / Gaps

- **Balance & Economy:** spawn rates, cost curves, caps, diminishing returns.  
- **Pet AI:** state machines, emotion/need timers, productivity influence.  
- **Placement Logic:** z-ordering, collision, snap/grid vs freeform.  
- **Biome Progression:** how/when biomes unlock and transition rules.  
- **Quest Templates:** types, reward structure, difficulty curve.  
- **Cloud Sync / Accounts:** play across devices, conflict resolution.  
- **Anti-frustration:** overflow handling, away compensation.  
- **Failure Modes:** consequences for neglect (pets/resources).  
- **Event Scheduling:** templates for weekly/monthly events.

---

# 3. Minimal MVP — Scalable Prototype Plan

**Goal:** Prove core experience: transparent overlay, draggable decoration, one pet, basic harvesting loop.

## Phase 1 — Foundation (Weekend)
- Transparent overlay window (URP + native shim).  
- Global hotkey to toggle visibility.  
- Drag-and-drop decorative objects with lock.  
- Save/load layout (JSON).  
- One jungle theme sprite set.  
- One pet: eye-follow + click reaction.  
- Basic resource: Dew Drops spawn on a timer; manual click to collect.  
- ResourceDefinition ScriptableObject for extensibility.

## Phase 2 — Time & Weather
- Day/night cycle.  
- Rain event that fills a bucket object.  
- Ambient sound and mute-on-hide.  
- Toggle visibility/opacity.

## Phase 3 — Inventory & Store
- Draggable inventory UI.  
- Grid inventory for collected items.  
- Store UI with one purchasable upgrade (Bigger Bucket).  
- One auto-collector (Bee) with limited charges.

## Phase 4 — Pets & Foraging
- One helper pet (Chameleon) that catches flies.  
- Flies spawn and move across screen; pet catches them.  
- Simple dig interaction: click dirt mound → reward.

## Phase 5 — Quests & Daily
- Daily login reward.  
- 3 starter quests (collect X dew, catch Y flies).  
- Puzzle fragment placeholder.

## Phase 6 — Optional Mobile Sync
- Minimal mobile client showing timers and auto-collect.  
- Sync via PlayFab/Firebase.

---

# Development Notes & Next Steps
- Create visual mockups and a prototype task list (Trello/Jira).  
- Implement core data-driven systems (ResourceDefinition, QuestDefinition SOs).  
- Plan crowdfunding pitch and dev-log schedule.

---

# Contacts / Assets
- Use Addressables for art/music.  
- Consider Unity-native plugins for global hotkey and transparent overlay compatibility (Windows & macOS).

---

# Appendix: Quick Feature Checklist (MVP+)
- [ ] Transparent overlay window  
- [ ] Hotkey toggle  
- [ ] Drag & lock decorations  
- [ ] Save/load layout  
- [ ] One pet with cursor-follow  
- [ ] Dew resource spawn + manual collect  
- [ ] Basic inventory grid  
- [ ] Store with 1 upgrade  
- [ ] Day/night + rain events  
- [ ] One auto-collector (bee)  
- [ ] Daily reward + simple quests

---

_End of document._
