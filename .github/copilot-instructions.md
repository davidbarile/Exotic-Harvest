# Copilot Instructions for Exotic Harvest

## Project Overview
"Exotic Harvest" is a Unity 6000.2.7f2 standalone application featuring transparent windows on macOS and Windows. The project combines custom transparent UI elements with native window manipulation capabilities for creating desktop overlay applications.

## Core Architecture

### UniWindowController Integration
This project heavily integrates **UniWindowController** (Kirurobo namespace) for native window manipulation:
- **Main Component**: `Assets/Kirurobo/UniWindowController/Runtime/Scripts/UniWindowController.cs` - singleton pattern controlling window transparency, positioning, and interaction
- **Native Bridge**: `Assets/Kirurobo/UniWindowController/Runtime/Scripts/LowLevel/UniWinCore.cs` - P/Invoke wrapper for platform-specific native libraries
- **Key Features**: 
  - Window transparency with hit-testing (opacity-based or raycast-based)
  - Click-through functionality with automatic detection
  - Drag window support via `DragMoveCanvas` prefab
  - File drop handling and monitor management

### Key Systems
- **ScreenManager**: Singleton managing global state with drag mode toggling (Tab key for background fade, D key for drag mode, Escape to quit)
- **UI Framework**: Combination of Unity UI with ProceduralUIImage for advanced shapes and DOTween for animations
- **Project Structure**: Multi-assembly setup with separate UniWindowController assemblies for runtime and editor functionality

## Development Patterns
For public, private and protected class variables, add "this." prefix when accessing them inside methods.  Only do this to class variables, not methods.
When generating new code and prefabs and creating such classes as menu items, inventory items, etc. create classes for these prefabs with public Initialize() and/or Configure() methods to pass them the information they need to populate their fields, images, etc.  When needed, cache these variables in private fields for use in OnPress methods or other event handlers.
Name scriptable objects for Data containers with "Config" suffix, e.g. ItemConfig, EnemyConfig, etc.
Serialized data objects (that need to be JSON serialized) should have "Data" suffix, e.g. PlayerData, GameData, etc.

### Window Management
```csharp
// Access UniWindowController singleton
UniWindowController.current.isTransparent = true;
UniWindowController.current.SetTransparentType(TransparentType.Alpha);
```

### UI Interaction Modes
- **Normal Mode**: Standard UI interactions
- **Drag Mode**: Activated via `ScreenManager.IsDragModeActivated` - enables `UiDraggablePanel` components to move UI elements
- **Click-Through**: Automatic detection based on pixel opacity or raycast hits when window is transparent

### Custom UI Components
- **ProceduralUIImage**: Custom UI system for procedural shapes with modifiers (RoundModifier, FreeModifier, etc.)
- **UiDraggablePanel**: Draggable UI panels that respect the global drag mode state

## Build Configuration

### Platform-Specific Builds
- **macOS**: Builds to `Builds/macOS/` with `.app` bundle
- **Windows**: Builds to `Builds/Win64/` with executable
- **Editor Build Script**: `Assets/Kirurobo/UniWindowController/Editor/Scripts/UniWindowControllerBatch.cs`

### Critical Build Requirements
- **Standalone Only**: Transparency features only work in standalone builds, not in Unity Editor
- **Player Settings**: Must configure specific settings for transparent window support (automated via UniWindowController Editor UI)
- **Assembly References**: Project uses 5 separate assemblies including UniWindowController runtime and editor assemblies

## Dependencies & External Libraries
- **DOTween** (`Assets/Plugins/Demigiant/DOTween/`) - Animation framework used by ScreenManager for UI transitions
- **TextMeshPro** - Text rendering (referenced in ScreenManager debug display)
- **ProceduralUIImage** - Custom procedural UI shapes system with extensive editor integration

## Key Files for AI Context
- `Assets/Scripts/Managers/ScreenManager.cs` - Central game state and input handling
- `Assets/Kirurobo/UniWindowController/Runtime/Scripts/UniWindowController.cs` - Window control core
- `Assets/Scripts/UiDraggablePanel.cs` - UI dragging behavior
- `Exotic Harvest.slnx` - Solution structure with 5 assemblies
- `Assets/Scenes/Main.unity` - Primary scene

## Development Workflow
1. **Scene Setup**: Main scene contains UniWindowController prefab for window management
2. **Transparency Testing**: Must build to standalone to test transparency features
3. **Input System**: Supports both Legacy Input Manager and New Input System
4. **Editor Integration**: UniWindowController provides custom inspector with Player Settings automation

## Platform-Specific Notes
- **macOS**: Uses native .bundle for window manipulation, supports Retina displays
- **Windows**: Supports both Alpha and ColorKey transparency modes with different performance characteristics
- **Hit Testing**: Two methods available - Opacity (pixel-based, slower but accurate) and Raycast (collider-based, faster)

## Common Patterns
- Singleton access via `UniWindowController.current` and `ScreenManager.IN`
- Event-driven UI updates via `ScreenManager.OnDragModeChanged`
- Assembly-based organization with clear separation between runtime and editor code
- Prefab-based component system for reusable window management features