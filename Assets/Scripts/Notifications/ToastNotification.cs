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
    public ENotificationType Type;
    public float DisplayDuration = 3f;
    public bool AutoDismiss = true;
    public bool PlaySound = true;
    
    [Header("Visual Style")]
    public Color BackgroundColor = Color.white;
    public Color TextColor = Color.black;
    
    public ToastNotification(string title, string message, ENotificationType type = ENotificationType.Info)
    {
        this.Title = title;
        this.Message = message;
        this.Type = type;
        SetDefaultStyle();
    }
    
    public ToastNotification(string message, ENotificationType type = ENotificationType.Info) 
        : this("", message, type) { }
    
    private void SetDefaultStyle()
    {
        switch (this.Type)
        {
            case ENotificationType.Success:
                this.BackgroundColor = new Color(0.2f, 0.8f, 0.2f, 0.9f); // Green
                this.TextColor = Color.white;
                break;
            case ENotificationType.Error:
                this.BackgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.9f); // Red
                this.TextColor = Color.white;
                this.DisplayDuration = 5f; // Errors stay longer
                break;
            case ENotificationType.Warning:
                this.BackgroundColor = new Color(0.8f, 0.6f, 0.2f, 0.9f); // Orange
                this.TextColor = Color.white;
                break;
            case ENotificationType.ResourceGained:
                this.BackgroundColor = new Color(0.2f, 0.6f, 0.8f, 0.9f); // Blue
                this.TextColor = Color.white;
                this.DisplayDuration = 2f; // Quick for resource gains
                break;
            case ENotificationType.Achievement:
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
    
    public static ToastNotification ResourceGained(EResourceType resourceType, int amount)
    {
        return new ToastNotification(
            "ResourceData Collected!",
            $"+{amount} {resourceType}",
            ENotificationType.ResourceGained
        );
    }
    
    public static ToastNotification ItemPurchased(string itemName)
    {
        return new ToastNotification(
            "Purchase Successful!",
            $"{itemName} purchased",
            ENotificationType.Success
        );
    }
    
    public static ToastNotification InventoryFull()
    {
        return new ToastNotification(
            "Inventory Full!",
            "Clear some space to collect more items",
            ENotificationType.Warning
        );
    }
    
    public static ToastNotification WeatherChanged(EWeatherType weather)
    {
        string message = weather switch
        {
            EWeatherType.Rain => "It's starting to rain! Place buckets to collect water.",
            EWeatherType.Storm => "A storm is approaching! Lightning rods will be active.",
            EWeatherType.Clear => "The weather has cleared up. Perfect for foraging!",
            EWeatherType.Wind => "The wind is picking up. Watch plants sway!",
            _ => $"Weather changed to {weather}"
        };
        
        return new ToastNotification(
            "Weather Update",
            message,
            ENotificationType.Info
        );
    }
    
    public static ToastNotification TimeOfDayChanged(ETimeOfDay timeOfDay)
    {
        string message = timeOfDay switch
        {
            ETimeOfDay.Morning => "Morning has arrived! Look for dewdrops to collect.",
            ETimeOfDay.Afternoon => "It's afternoon. Perfect time for foraging.",
            ETimeOfDay.Evening => "Evening approaches. Fireflies will appear soon.",
            ETimeOfDay.Night => "Night falls. Moonbeams and stardust await!",
            _ => $"Time changed to {timeOfDay}"
        };
        
        return new ToastNotification(
            "Time Update",
            message,
            ENotificationType.Info
        );
    }
}