using System;
using UnityEngine;

/// <summary>
/// Data structure for toast notifications
/// </summary>
[Serializable]
public class ToastNotification
{
    [Header("Notification Content")]
    public string Title;
    public string Message;
    public Sprite Icon;
    
    [Header("Notification Properties")]
    public NotificationType Type;
    public float DisplayDuration = 3f;
    public bool AutoDismiss = true;
    public bool PlaySound = true;
    
    [Header("Visual Style")]
    public Color BackgroundColor = Color.white;
    public Color TextColor = Color.black;
    
    public ToastNotification(string title, string message, NotificationType type = NotificationType.Info)
    {
        this.Title = title;
        this.Message = message;
        this.Type = type;
        SetDefaultStyle();
    }
    
    public ToastNotification(string message, NotificationType type = NotificationType.Info) 
        : this("", message, type) { }
    
    private void SetDefaultStyle()
    {
        switch (this.Type)
        {
            case NotificationType.Success:
                this.BackgroundColor = new Color(0.2f, 0.8f, 0.2f, 0.9f); // Green
                this.TextColor = Color.white;
                break;
            case NotificationType.Error:
                this.BackgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.9f); // Red
                this.TextColor = Color.white;
                this.DisplayDuration = 5f; // Errors stay longer
                break;
            case NotificationType.Warning:
                this.BackgroundColor = new Color(0.8f, 0.6f, 0.2f, 0.9f); // Orange
                this.TextColor = Color.white;
                break;
            case NotificationType.ResourceGained:
                this.BackgroundColor = new Color(0.2f, 0.6f, 0.8f, 0.9f); // Blue
                this.TextColor = Color.white;
                this.DisplayDuration = 2f; // Quick for resource gains
                break;
            case NotificationType.Achievement:
                this.BackgroundColor = new Color(0.8f, 0.4f, 0.8f, 0.9f); // Purple
                this.TextColor = Color.white;
                this.DisplayDuration = 4f; // Achievements stay longer
                break;
            default:
                this.BackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.9f); // Gray
                this.TextColor = Color.white;
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