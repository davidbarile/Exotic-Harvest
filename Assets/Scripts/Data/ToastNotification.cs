using UnityEngine;

/// <summary>
/// Data structure for toast notifications
/// </summary>
[System.Serializable]
public class ToastNotification
{
    [Header("Notification Content")]
    public string title;
    public string message;
    public Sprite icon;
    
    [Header("Notification Properties")]
    public NotificationType type;
    public float displayDuration = 3f;
    public bool autoDismiss = true;
    public bool playSound = true;
    
    [Header("Visual Style")]
    public Color backgroundColor = Color.white;
    public Color textColor = Color.black;
    
    public ToastNotification(string title, string message, NotificationType type = NotificationType.Info)
    {
        this.title = title;
        this.message = message;
        this.type = type;
        SetDefaultStyle();
    }
    
    public ToastNotification(string message, NotificationType type = NotificationType.Info) 
        : this("", message, type) { }
    
    private void SetDefaultStyle()
    {
        switch (type)
        {
            case NotificationType.Success:
                backgroundColor = new Color(0.2f, 0.8f, 0.2f, 0.9f); // Green
                textColor = Color.white;
                break;
            case NotificationType.Error:
                backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.9f); // Red
                textColor = Color.white;
                displayDuration = 5f; // Errors stay longer
                break;
            case NotificationType.Warning:
                backgroundColor = new Color(0.8f, 0.6f, 0.2f, 0.9f); // Orange
                textColor = Color.white;
                break;
            case NotificationType.ResourceGained:
                backgroundColor = new Color(0.2f, 0.6f, 0.8f, 0.9f); // Blue
                textColor = Color.white;
                displayDuration = 2f; // Quick for resource gains
                break;
            case NotificationType.Achievement:
                backgroundColor = new Color(0.8f, 0.4f, 0.8f, 0.9f); // Purple
                textColor = Color.white;
                displayDuration = 4f; // Achievements stay longer
                break;
            default:
                backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.9f); // Gray
                textColor = Color.white;
                break;
        }
    }
    
    public static ToastNotification ResourceGained(ResourceType resourceType, int amount)
    {
        return new ToastNotification(
            "Resource Collected!",
            $"+{amount} {resourceType}",
            NotificationType.ResourceGained
        );
    }
    
    public static ToastNotification ItemPurchased(string itemName)
    {
        return new ToastNotification(
            "Purchase Successful!",
            $"{itemName} purchased",
            NotificationType.Success
        );
    }
    
    public static ToastNotification InventoryFull()
    {
        return new ToastNotification(
            "Inventory Full!",
            "Clear some space to collect more items",
            NotificationType.Warning
        );
    }
    
    public static ToastNotification WeatherChanged(WeatherType weather)
    {
        string message = weather switch
        {
            WeatherType.Rain => "It's starting to rain! Place buckets to collect water.",
            WeatherType.Storm => "A storm is approaching! Lightning rods will be active.",
            WeatherType.Clear => "The weather has cleared up. Perfect for foraging!",
            WeatherType.Wind => "The wind is picking up. Watch plants sway!",
            _ => $"Weather changed to {weather}"
        };
        
        return new ToastNotification(
            "Weather Update",
            message,
            NotificationType.Info
        );
    }
    
    public static ToastNotification TimeOfDayChanged(TimeOfDay timeOfDay)
    {
        string message = timeOfDay switch
        {
            TimeOfDay.Morning => "Morning has arrived! Look for dewdrops to collect.",
            TimeOfDay.Evening => "Evening approaches. Fireflies will appear soon.",
            TimeOfDay.Night => "Night falls. Moonbeams and stardust await!",
            TimeOfDay.Afternoon => "It's afternoon. Perfect time for foraging.",
            _ => $"Time changed to {timeOfDay}"
        };
        
        return new ToastNotification(
            "Time Update",
            message,
            NotificationType.Info
        );
    }
}