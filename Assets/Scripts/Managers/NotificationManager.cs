using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all game notifications and toast messages
/// </summary>
public class NotificationManager : MonoBehaviour
{
    public static NotificationManager IN;
    
    [Header("Notification Settings")]
    [SerializeField] private GameObject toastNotificationPrefab;
    [SerializeField] private Transform notificationParent;
    [SerializeField] private int maxNotifications = 5;
    [SerializeField] private float notificationSpacing = 10f;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip infoSound;
    
    private Queue<ToastNotificationUI> activeNotifications = new Queue<ToastNotificationUI>();
    private bool notificationsEnabled = true;
    
    // Events
    public static event Action<ToastNotification> OnNotificationShown;
    public static event Action<ToastNotification> OnNotificationDismissed;
    
    private void Awake()
    {
        if (notificationParent == null)
            notificationParent = transform;
            
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
    
    private void OnEnable()
    {
        // Listen to game events and show appropriate notifications
        SetupEventListeners();
    }
    
    private void OnDisable()
    {
        RemoveEventListeners();
    }
    
    private void SetupEventListeners()
    {
        // Resource events
        if (ResourceManager.IN != null)
        {
            ResourceManager.OnResourceGained += OnResourceGained;
            ResourceManager.OnInventoryFull += OnInventoryFull;
        }
        
        // Weather events
        if (WeatherManager.IN != null)
        {
            WeatherManager.OnWeatherChanged += OnWeatherChanged;
        }
        
        // Time events
        if (TimeManager.IN != null)
        {
            TimeManager.OnTimeOfDayChanged += OnTimeOfDayChanged;
        }
        
        // Shop events
        if (ShopManager.IN != null)
        {
            ShopManager.OnItemPurchased += OnItemPurchased;
            ShopManager.OnPurchaseFailed += OnPurchaseFailed;
        }
        
        // Decoration events
        if (DecorationManager.IN != null)
        {
            DecorationManager.OnDecorationAdded += OnDecorationPlaced;
        }
        
        // Save events
        if (SaveManager.IN != null)
        {
            SaveManager.OnGameSaved += OnGameSaved;
            SaveManager.OnSaveError += OnSaveError;
        }
    }
    
    private void RemoveEventListeners()
    {
        // Resource events
        if (ResourceManager.IN != null)
        {
            ResourceManager.OnResourceGained -= OnResourceGained;
            ResourceManager.OnInventoryFull -= OnInventoryFull;
        }
        
        // Weather events
        if (WeatherManager.IN != null)
        {
            WeatherManager.OnWeatherChanged -= OnWeatherChanged;
        }
        
        // Time events
        if (TimeManager.IN != null)
        {
            TimeManager.OnTimeOfDayChanged -= OnTimeOfDayChanged;
        }
        
        // Shop events
        if (ShopManager.IN != null)
        {
            ShopManager.OnItemPurchased -= OnItemPurchased;
            ShopManager.OnPurchaseFailed -= OnPurchaseFailed;
        }
        
        // Decoration events
        if (DecorationManager.IN != null)
        {
            DecorationManager.OnDecorationAdded -= OnDecorationPlaced;
        }
        
        // Save events
        if (SaveManager.IN != null)
        {
            SaveManager.OnGameSaved -= OnGameSaved;
            SaveManager.OnSaveError -= OnSaveError;
        }
    }
    
    public void ShowNotification(ToastNotification notification)
    {
        if (!notificationsEnabled || toastNotificationPrefab == null)
            return;
            
        // Remove oldest notification if at max capacity
        while (activeNotifications.Count >= maxNotifications)
        {
            var oldest = activeNotifications.Dequeue();
            if (oldest != null)
                oldest.Dismiss();
        }
        
        // Create notification UI
        GameObject notificationObj = Instantiate(toastNotificationPrefab, notificationParent);
        ToastNotificationUI notificationUI = notificationObj.GetComponent<ToastNotificationUI>();
        
        if (notificationUI != null)
        {
            notificationUI.Initialize(notification, OnNotificationDismissedCallback);
            activeNotifications.Enqueue(notificationUI);
            
            // Position notification
            PositionNotification(notificationUI);
            
            // Play sound
            if (notification.playSound)
                PlayNotificationSound(notification.type);
                
            OnNotificationShown?.Invoke(notification);
        }
        else
        {
            Debug.LogError("Toast notification prefab missing ToastNotificationUI component");
            Destroy(notificationObj);
        }
    }
    
    private void PositionNotification(ToastNotificationUI notification)
    {
        // Position based on number of active notifications
        RectTransform rectTransform = notification.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            int index = activeNotifications.Count - 1;
            float yOffset = -index * (rectTransform.rect.height + notificationSpacing);
            
            Vector2 anchoredPos = rectTransform.anchoredPosition;
            anchoredPos.y = yOffset;
            rectTransform.anchoredPosition = anchoredPos;
        }
    }
    
    private void OnNotificationDismissedCallback(ToastNotificationUI notification)
    {
        // Remove from active notifications (it might not be the first one if manually dismissed)
        var notificationsList = new List<ToastNotificationUI>(activeNotifications);
        notificationsList.Remove(notification);
        
        activeNotifications.Clear();
        foreach (var n in notificationsList)
        {
            if (n != null)
                activeNotifications.Enqueue(n);
        }
        
        // Reposition remaining notifications
        RepositionNotifications();
    }
    
    private void RepositionNotifications()
    {
        int index = 0;
        foreach (var notification in activeNotifications)
        {
            if (notification != null)
            {
                RectTransform rectTransform = notification.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float yOffset = -index * (rectTransform.rect.height + notificationSpacing);
                    Vector2 anchoredPos = rectTransform.anchoredPosition;
                    anchoredPos.y = yOffset;
                    rectTransform.anchoredPosition = anchoredPos;
                }
                index++;
            }
        }
    }
    
    private void PlayNotificationSound(NotificationType type)
    {
        if (audioSource == null)
            return;
            
        AudioClip clipToPlay = type switch
        {
            NotificationType.Success or NotificationType.ResourceGained or NotificationType.Achievement => successSound,
            NotificationType.Error or NotificationType.Warning => errorSound,
            _ => infoSound
        };
        
        if (clipToPlay != null)
            audioSource.PlayOneShot(clipToPlay);
    }
    
    // Event handlers
    private void OnResourceGained(ResourceType resourceType, int amount)
    {
        ShowNotification(ToastNotification.ResourceGained(resourceType, amount));
    }
    
    private void OnInventoryFull()
    {
        ShowNotification(ToastNotification.InventoryFull());
    }
    
    private void OnWeatherChanged(WeatherType weatherType)
    {
        ShowNotification(ToastNotification.WeatherChanged(weatherType));
    }
    
    private void OnTimeOfDayChanged(TimeOfDay timeOfDay)
    {
        ShowNotification(ToastNotification.TimeOfDayChanged(timeOfDay));
    }
    
    private void OnItemPurchased(ShopItem item)
    {
        ShowNotification(ToastNotification.ItemPurchased(item.displayName));
    }
    
    private void OnPurchaseFailed(ShopItem item, string reason)
    {
        var notification = new ToastNotification(
            "Purchase Failed",
            reason,
            NotificationType.Error
        );
        ShowNotification(notification);
    }
    
    private void OnDecorationPlaced(DecorationBase decoration)
    {
        var notification = new ToastNotification(
            "Decoration Placed!",
            $"{decoration.Name} has been placed",
            NotificationType.Success
        );
        ShowNotification(notification);
    }
    
    private void OnGameSaved()
    {
        var notification = new ToastNotification(
            "Game Saved",
            "Your progress has been saved",
            NotificationType.Info
        );
        notification.displayDuration = 1.5f; // Brief confirmation
        ShowNotification(notification);
    }
    
    private void OnSaveError(string error)
    {
        var notification = new ToastNotification(
            "Save Failed",
            $"Could not save: {error}",
            NotificationType.Error
        );
        ShowNotification(notification);
    }
    
    // Public utility methods
    public void ShowCustomNotification(string message, NotificationType type = NotificationType.Info)
    {
        ShowNotification(new ToastNotification(message, type));
    }
    
    public void ShowCustomNotification(string title, string message, NotificationType type = NotificationType.Info)
    {
        ShowNotification(new ToastNotification(title, message, type));
    }
    
    public void SetNotificationsEnabled(bool enabled)
    {
        notificationsEnabled = enabled;
        
        if (!enabled)
        {
            // Clear all active notifications
            DismissAllNotifications();
        }
    }
    
    public void DismissAllNotifications()
    {
        while (activeNotifications.Count > 0)
        {
            var notification = activeNotifications.Dequeue();
            if (notification != null)
                notification.Dismiss();
        }
    }
}