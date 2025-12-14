using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the day/night cycle and time-based events
/// </summary>
public class TimeManager : MonoBehaviour, ITickable
{
    public static TimeManager IN;
    
    [SerializeField] private float dayLengthInMinutes = 24f; // Real minutes for a full game day
    [SerializeField] private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;
    [SerializeField, Range(0f, 24f)] private float currentHour = 8f; // Start at 8 AM

    [Header("UI Elements")]
    [SerializeField] private TMP_Text timeDisplayText;
    [Space, SerializeField] private TMP_Text timeSliderText;
    [SerializeField] private TMP_Text timeScaleSliderText;
    
    public float TimeScale { get; private set; } = 1f; // Speed multiplier for time progression
    
    // Events
    public static event Action<TimeOfDay> OnTimeOfDayChanged;
    public static event Action<float> OnHourChanged;
    public static event Action OnNewDay;
    
    // Properties
    public TimeOfDay CurrentTimeOfDay => currentTimeOfDay;
    public float CurrentHour => currentHour;
    public float DayProgress => currentHour / 24f; // 0-1 progress through day
    
    private void OnEnable()
    {
        TickManager.OnSecondTick += SecondTick;
    }
    
    private void OnDisable()
    {
        TickManager.OnSecondTick -= SecondTick;
    }

    public void Tick()
    {
        // Optional: Fast tick updates if needed
    }
    
    //TODO: make this work with real time, using SecondTick for debug only
    public void SecondTick()
    {
        // Advance time
        float hoursPerSecond = 24f / (this.dayLengthInMinutes * 60f);
        this.currentHour += hoursPerSecond * this.TimeScale;
        
        // Handle day rollover
        if (this.currentHour >= 24f)
        {
            this.currentHour -= 24f;
            OnNewDay?.Invoke();
        }
        
        OnHourChanged?.Invoke(this.currentHour);
        
        // Check for time of day changes
        TimeOfDay newTimeOfDay = GetTimeOfDayFromHour(this.currentHour);
        if (newTimeOfDay != this.currentTimeOfDay)
        {
            this.currentTimeOfDay = newTimeOfDay;
            OnTimeOfDayChanged?.Invoke(this.currentTimeOfDay);
        }

         if(this.timeDisplayText != null)
                this.timeDisplayText.text = $"Time: {this.currentHour:00.00} ({this.currentTimeOfDay})";
    }
    
    private TimeOfDay GetTimeOfDayFromHour(float hour)
    {
        if (hour >= 6f && hour < 12f)
            return TimeOfDay.Morning;
        else if (hour >= 12f && hour < 18f)
            return TimeOfDay.Afternoon;
        else if (hour >= 18f && hour < 22f)
            return TimeOfDay.Evening;
        else
            return TimeOfDay.Night;
    }
    
    public bool IsTimeForResource(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Water:
                return true; // Always available, but more during rain
            case ResourceType.Moonbeams:
            case ResourceType.Stardust:
            case ResourceType.FallingStars:
                return this.currentTimeOfDay.HasFlag(TimeOfDay.Night);
            case ResourceType.Fireflies:
                return this.currentTimeOfDay.HasFlag(TimeOfDay.Evening) || this.currentTimeOfDay.HasFlag(TimeOfDay.Night);
            case ResourceType.Nectar:
                return this.currentTimeOfDay.HasFlag(TimeOfDay.Morning);
            case ResourceType.Seeds:
            case ResourceType.Berries:
                return this.currentTimeOfDay.HasFlag(TimeOfDay.Afternoon);
            default:
                return true; // Most resources available anytime
        }
    }
    
    public void SetTimeScale(Single scale)
    {
        this.TimeScale = Mathf.Max(0f, scale);

        if (this.timeScaleSliderText != null)
            this.timeScaleSliderText.text = $"Time Scale: {this.TimeScale:0.0}x";
    }
    
    public void SetTime(Single hour)
    {
        this.currentHour = Mathf.Clamp(hour, 0f, 24f);
        this.currentTimeOfDay = GetTimeOfDayFromHour(this.currentHour);
        OnHourChanged?.Invoke(this.currentHour);
        OnTimeOfDayChanged?.Invoke(this.currentTimeOfDay);

        if (this.timeSliderText != null)
            this.timeSliderText.text = $"Time: {this.currentHour:00.00} ({this.currentTimeOfDay})";
    }
}