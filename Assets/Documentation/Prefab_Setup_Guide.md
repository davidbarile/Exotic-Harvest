# Exotic Harvest - Prefab Setup Guide

## Overview

This guide explains how to set up the created prefabs and ScriptableObjects in Unity for the Exotic Harvest project. All prefabs have been created with placeholder components and need to be connected to the manager systems.

## Prefabs Created

### UI Prefabs (`Assets/Prefabs/UI/`)

#### ToastNotification.prefab
- **Purpose**: Animated notification display for game events
- **Components**: RectTransform, Image (background), CanvasGroup, ToastNotificationUI script
- **Setup Required**: 
  - Assign child elements for title text, message text, icon, and dismiss button
  - Connect to NotificationManager.toastNotificationPrefab field

#### ShopItemUI.prefab
- **Purpose**: Individual shop item display in store
- **Components**: RectTransform, Image (background), Button, ShopItemUI script
- **Setup Required**:
  - Add child TextMeshPro components for name and price
  - Add child Image for item icon
  - Connect to UiShopPanel.shopItemPrefab field

#### ResourceDisplayUI.prefab
- **Purpose**: Shows individual resource amounts in UI
- **Components**: RectTransform, Image (background), HorizontalLayoutGroup, ResourceDisplayUI script
- **Setup Required**:
  - Add child Image for resource icon
  - Add child TextMeshPro for amount display
  - Use with ResourceDisplayManager

### Decoration Prefabs (`Assets/Prefabs/Decorations/`)

#### Bucket.prefab
- **Purpose**: Water collection decoration (Phase 1 passive harvester)
- **Components**: Transform, SpriteRenderer, CircleCollider2D, Bucket script
- **Setup Required**:
  - Replace placeholder sprite with bucket artwork
  - Add child object for water level visual
  - Connect waterVisual field in Bucket script
  - Assign to DecorationManager.bucketPrefab field

#### PlantDecoration.prefab
- **Purpose**: Visual-only jungle plant decoration
- **Components**: Transform, SpriteRenderer, PlantDecoration script
- **Setup Required**:
  - Replace placeholder sprite with plant artwork
  - Add child objects for swaying parts (leaves, branches)
  - Connect swayingParts array in PlantDecoration script
  - Assign to DecorationManager.plantPrefab field

### Foraging Prefabs (`Assets/Prefabs/Foraging/`)

#### Dewdrop.prefab
- **Purpose**: Click-to-collect water droplets (morning spawn)
- **Components**: Transform, SpriteRenderer, CircleCollider2D, Dewdrop script
- **Setup Required**:
  - Replace placeholder sprite with dewdrop artwork
  - Adjust collider size to match sprite
  - Assign to ForagingManager.dewdropPrefab field

#### Raindrop.prefab
- **Purpose**: Drag-to-collect falling raindrops
- **Components**: Transform, SpriteRenderer, CircleCollider2D, Rigidbody2D, Raindrop script
- **Setup Required**:
  - Replace placeholder sprite with raindrop artwork
  - Adjust physics settings (mass, gravity scale, drag)
  - Assign to ForagingManager.raindropPrefab field

## ScriptableObjects Created

### Resource System (`Assets/ScriptableObjects/Resources/`)

#### ResourceDefinition Assets
- **Water.asset**: Primary resource, always available
- **Seeds.asset**: Nature resource, afternoon availability
- **Gems.asset**: Valuable resource, rare

#### ResourceDatabase.asset
- **Purpose**: Central database of all resource definitions
- **Setup Required**: 
  - Auto-populate using context menu "Auto-Populate Resources"
  - Assign to ResourceManager.resourceDatabase field

### Shop System (`Assets/ScriptableObjects/ShopItems/`)

#### ShopItemDefinition Assets
- **BucketItem.asset**: Bucket decoration purchase
- **PlantItem.asset**: Plant decoration purchase  
- **WaterBottle.asset**: Water resource purchase

## Manager Setup Instructions

### 1. GameManager GameObject Setup
Add all manager components to your GameManager GameObject:
- ResourceManager
- TimeManager
- WeatherManager
- ForagingManager
- DecorationManager
- SaveManager
- ShopManager
- NotificationManager

### 2. ResourceManager Configuration
```
✓ Assign ResourceDatabase.asset to resourceDatabase field
✓ Set maxInventorySize (default: 100)
```

### 3. DecorationManager Configuration
```
✓ Assign Bucket.prefab to bucketPrefab field
✓ Assign PlantDecoration.prefab to plantPrefab field
✓ Set decorationParent (container for spawned decorations)
✓ Configure placementBounds for spawn area
```

### 4. ForagingManager Configuration
```
✓ Assign Dewdrop.prefab to dewdropPrefab field
✓ Assign Raindrop.prefab to raindropPrefab field
✓ Set spawnParent (container for spawned collectables)
✓ Configure spawn boundaries and rates
✓ Assign mainCamera reference
```

### 5. ShopManager Configuration
```
✓ Assign ShopItemDefinition assets to allShopItemDefinitions array:
  - BucketItem.asset
  - PlantItem.asset
  - WaterBottle.asset
✓ Enable debugMode for testing
```

### 6. NotificationManager Configuration
```
✓ Assign ToastNotification.prefab to toastNotificationPrefab field
✓ Set notificationParent (UI container for notifications)
✓ Configure maxNotifications and spacing
✓ Assign AudioSource and notification sounds (optional)
```

### 7. UI Panel Setup

#### UiShopPanel
```
✓ Assign ShopItemUI.prefab to shopItemPrefab field
✓ Set up category tabs and item grid
✓ Configure item detail panel components
✓ Connect cost display elements
```

#### Resource Display
```
✓ Create ResourceDisplayManager component in UI
✓ Assign ResourceDisplayUI.prefab to resourceDisplayPrefab field
✓ Set resource display parent container
✓ Configure categories to show
```

## Testing Phase 1 MVP

Once setup is complete, you can test:

1. **Resource Collection**: 
   - Wait for morning (dewdrops spawn)
   - Click dewdrops to collect water
   - Wait for rain (buckets fill automatically)

2. **Shop System**:
   - Open shop panel
   - Purchase buckets with collected water
   - Purchase plants for decoration

3. **Decoration System**:
   - Place purchased decorations
   - Drag decorations around (when drag mode enabled)
   - Lock/unlock decorations

4. **Save System**:
   - Resources and decorations persist between sessions
   - Auto-save every 5 minutes

5. **Notification System**:
   - Notifications appear for all game events
   - Animated slide-in/out effects
   - Automatic dismissal after duration

## Art Asset Integration

When you create sprites for the game:

1. **Replace Placeholder Sprites**:
   - Update SpriteRenderer components in prefabs
   - Assign icons to ResourceDefinition and ShopItemDefinition assets

2. **Sprite Requirements**:
   - Dewdrop: Small, translucent water droplet
   - Raindrop: Elongated water drop with motion blur
   - Bucket: Rustic wooden or metal bucket
   - Plant: Lush tropical plant with multiple leaves
   - Resource Icons: 32x32 or 64x64 pixel art style

3. **UI Sprites**:
   - Notification backgrounds: Rounded rectangles
   - Shop item frames: Decorative borders
   - Button states: Normal, hover, pressed, disabled

The architecture is designed to be art-ready - simply replace the placeholder sprites and the entire visual system will update automatically!

## Next Steps

1. **Create Art Assets**: Replace all placeholder sprites
2. **Test in Build**: Transparency features only work in standalone builds
3. **Polish Phase 1**: Add particle effects, sounds, and animations
4. **Expand to Phase 2**: Add pets, advanced resources, and more decorations

The foundation is solid and ready for your creative vision!