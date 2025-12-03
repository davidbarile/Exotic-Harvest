/// <summary>
/// Different ways to collect resources through foraging
/// </summary>
public enum CollectionMethod
{
    Click,        // Click to collect (dewdrops, seeds)
    Hover,       // Hover over to collect (raindrops)
    Drag,         // Drag across screen (bucket for raindrops)
    Swipe,        // Net swiping (butterflies, fireflies)
    Hold,         // Click and hold (mining rocks, digging)
    Interact      // Special interaction (pollination, etc.)
}