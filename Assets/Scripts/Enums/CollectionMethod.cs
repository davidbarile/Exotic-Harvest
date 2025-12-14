using System;

[Flags]
public enum ECollectionMethod
{
    Click = 1 << 0,        // Click to collect (dewdrops, seeds)
    Hover = 1 << 1,       // Hover over to collect (raindrops)
    Drag = 1 << 2,         // Drag across screen (bucket for raindrops)
    Swipe = 1 << 3,        // Net swiping (butterflies, fireflies)
    Hold = 1 << 4,         // Click and hold (mining rocks, digging)
    Interact = 1 << 5,      // Special interaction (pollination, etc.)
    DragCollector = 1 << 6 // Drag a collector item over the resource to collect (e.g., using a jar to collect fireflies)
}