/// <summary>
/// Categories for organizing shop items
/// </summary>
public enum EShopCategory
{
    Decorations,    // Visual and functional decorations
    Resources,      // Direct resource purchases
    Tools,          // Harvesting tools and upgrades
    Upgrades,       // System improvements
    Premium,        // Hard currency items
    Special         // Limited time or rare items
}

/// <summary>
/// Types of items that can be purchased
/// </summary>
public enum EItemType
{
    Decoration,     // Placeable decoration
    Resource,       // Direct resource grant
    ToolUpgrade,    // Improve harvesting efficiency
    Capacity,       // Increase limits (inventory, etc.)
    Multiplier,     // Boost generation rates
    Unlock,         // Unlock new features
    Consumable,      // One-time use items
    Permanent
}