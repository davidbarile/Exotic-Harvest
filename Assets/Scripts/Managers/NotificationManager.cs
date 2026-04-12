using System;
using System.Collections.Generic;
using UnityEngine;
using static GlobalEnums;

[RequireComponent(typeof(AudioSource))]

/// <summary>
/// Manages all game notifications and toast messages
/// </summary>
public class NotificationManager : MonoBehaviour
{
    public static NotificationManager IN;

    // Events
    public static Action<ToastNotification> OnNotificationShown;
    //public static Action<ToastNotification> OnNotificationDismissed;
    
    [Header("Notification Settings")]
    [SerializeField] private Transform notificationParent;
    [SerializeField] private int maxNotifications = 5;
    [SerializeField] private float notificationSpacing = 10f;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip infoSound;
    
    private Queue<UiToastNotification> activeNotifications = new();
    private bool notificationsEnabled = true;
    
    private void Awake()
    {
        if (this.notificationParent == null)
            this.notificationParent = transform;

        if (this.audioSource == null)
            this.audioSource = GetComponent<AudioSource>();
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
        // ResourceData events
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
        
        // Save events
        if (SaveManager.IN != null)
        {
            SaveManager.OnGameSaved += OnGameSaved;
            SaveManager.OnSaveError += OnSaveError;
        }
    }
    
    private void RemoveEventListeners()
    {
        // ResourceData events
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
        
        // Save events
        if (SaveManager.IN != null)
        {
            SaveManager.OnGameSaved -= OnGameSaved;
            SaveManager.OnSaveError -= OnSaveError;
        }
    }
    
    public void ShowNotification(ToastNotification notification)
    {
        if (!this.notificationsEnabled)
            return;
            
        // Remove oldest notification if at max capacity
        while (this.activeNotifications.Count >= this.maxNotifications)
        {
            var oldest = this.activeNotifications.Dequeue();
            if (oldest != null)
                oldest.Dismiss(true);
        }
        
        // Create notification UI
        var notificationUI = PrefabManager.IN.SpawnPrefab<UiToastNotification>("ToastNotification", this.notificationParent);
        
        notificationUI.Initialize(notification, OnNotificationDismissedCallback);
        this.activeNotifications.Enqueue(notificationUI);
        
        // Position notification
        PositionNotification(notificationUI);
        
        // Play sound
        if (notification.PlaySound)
            PlayNotificationSound(notification.Type);
            
        OnNotificationShown?.Invoke(notification);
    }
    
    private void PositionNotification(UiToastNotification notification)
    {
        // Position based on number of active notifications
        RectTransform rectTransform = notification.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            int index = this.activeNotifications.Count - 1;
            float yOffset = -index * (rectTransform.rect.height + this.notificationSpacing);
            
            Vector2 anchoredPos = rectTransform.anchoredPosition;
            anchoredPos.y = yOffset;
            rectTransform.anchoredPosition = anchoredPos;
        }
    }
    
    private void OnNotificationDismissedCallback(UiToastNotification notification)
    {
        // Remove from active notifications (it might not be the first one if manually dismissed)
        var notificationsList = new List<UiToastNotification>(this.activeNotifications);
        notificationsList.Remove(notification);
        
        this.activeNotifications.Clear();
        foreach (var n in notificationsList)
        {
            if (n != null)
                this.activeNotifications.Enqueue(n);
        }
        
        // Reposition remaining notifications
        RepositionNotifications();
    }
    
    private void RepositionNotifications()
    {
        int index = 0;
        foreach (var notification in this.activeNotifications)
        {
            if (notification != null)
            {
                RectTransform rectTransform = notification.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float yOffset = -index * (rectTransform.rect.height + this.notificationSpacing);
                    Vector2 anchoredPos = rectTransform.anchoredPosition;
                    anchoredPos.y = yOffset;
                    rectTransform.anchoredPosition = anchoredPos;
                }
                index++;
            }
        }
    }
    
    private void PlayNotificationSound(ENotificationType type)
    {
        if (this.audioSource == null)
            return;
            
        AudioClip clipToPlay = type switch
        {
            ENotificationType.Success or ENotificationType.ResourceGained or ENotificationType.Achievement => this.successSound,
            ENotificationType.Error or ENotificationType.Warning => this.errorSound,
            _ => this.infoSound
        };
        
        if (clipToPlay != null)
            this.audioSource.PlayOneShot(clipToPlay);
    }
    
    // Event handlers
    private void OnResourceGained(EResourceType resourceType, int amount)
    {
        //ShowNotification(ToastNotification.ResourceGained(resourceType, amount));
    }
    
    private void OnInventoryFull()
    {
        //ShowNotification(ToastNotification.InventoryFull());
    }
    
    private void OnWeatherChanged(EWeatherType weatherType)
    {
        ShowNotification(ToastNotification.WeatherChanged(weatherType));
    }
    
    private void OnTimeOfDayChanged(ETimeOfDay timeOfDay)
    {
        ShowNotification(ToastNotification.TimeOfDayChanged(timeOfDay));
    }
    
    private void OnItemPurchased(ShopItemData itemData)
    {
        ShowNotification(ToastNotification.ItemPurchased(itemData.DisplayName));
    }
    
    private void OnPurchaseFailed(ShopItemData itemData, string reason)
    {
        var notification = new ToastNotification(
            "Purchase Failed",
            reason,
            ENotificationType.Error
        );
        ShowNotification(notification);
    }
    
    private void OnGameSaved()
    {
        var notification = new ToastNotification(
            "Game Saved",
            "Your progress has been saved",
            ENotificationType.Info
        );
        notification.DisplayDuration = 1.5f; // Brief confirmation
        ShowNotification(notification);
    }
    
    private void OnSaveError(string error)
    {
        var notification = new ToastNotification(
            "Save Failed",
            $"Could not save: {error}",
            ENotificationType.Error
        );
        ShowNotification(notification);
    }
    
    // Public utility methods
    public void ShowCustomNotification(string message, ENotificationType type = ENotificationType.Info)
    {
        ShowNotification(new ToastNotification(message, type));
    }
    
    public void ShowCustomNotification(string title, string message, ENotificationType type = ENotificationType.Info)
    {
        ShowNotification(new ToastNotification(title, message, type));
    }
    
    public void SetNotificationsEnabled(bool enabled)
    {
        this.notificationsEnabled = enabled;
        
        if (!enabled)
        {
            // Clear all active notifications
            DismissAllNotifications();
        }
    }
    
    public void DismissAllNotifications()
    {
        while (this.activeNotifications.Count > 0)
        {
            var notification = this.activeNotifications.Dequeue();
            if (notification != null)
                notification.Dismiss();
        }
    }
}