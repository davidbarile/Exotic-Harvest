/// <summary>
/// Types of notifications for different game events
/// </summary>
public enum NotificationType
{
    ResourceGained,     // Collected resources
    ResourceLost,       // Spent resources
    ItemPurchased,      // Shop purchases
    DecorationPlaced,   // Decoration events
    InventoryFull,      // Inventory warnings
    WeatherChanged,     // Weather events
    TimeChanged,        // Time of day events
    Achievement,        // Achievements unlocked
    Error,              // Error messages
    Info,               // General information
    Success,            // Success confirmations
    Warning             // Warning messages
}