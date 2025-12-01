using System;
using UnityEngine;

/// <summary>
/// Manages the day/night cycle and time-based events
/// </summary>
public class TimeManager : MonoBehaviour, ITickable
{
    public static TimeManager IN;
    
    [SerializeField] private float dayLengthInMinutes = 24f; // Real minutes for a full game day
    [SerializeField] private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;
    [SerializeField, Range(0f, 24f)] private float currentHour = 8f; // Start at 8 AM
    
    private float timeScale = 1f;
    
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
    
    public void SecondTick()
    {
        // Advance time
        float hoursPerSecond = 24f / (dayLengthInMinutes * 60f);
        currentHour += hoursPerSecond * timeScale;
        
        // Handle day rollover
        if (currentHour >= 24f)
        {
            currentHour -= 24f;
            OnNewDay?.Invoke();
        }
        
        OnHourChanged?.Invoke(currentHour);
        
        // Check for time of day changes
        TimeOfDay newTimeOfDay = GetTimeOfDayFromHour(currentHour);
        if (newTimeOfDay != currentTimeOfDay)
        {
            currentTimeOfDay = newTimeOfDay;
            OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
        }
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
                return currentTimeOfDay == TimeOfDay.Night;
            case ResourceType.Fireflies:
                return currentTimeOfDay == TimeOfDay.Evening || currentTimeOfDay == TimeOfDay.Night;
            case ResourceType.Nectar:
                return currentTimeOfDay == TimeOfDay.Morning;
            case ResourceType.Seeds:
            case ResourceType.Berries:
                return currentTimeOfDay == TimeOfDay.Afternoon;
            default:
                return true; // Most resources available anytime
        }
    }
    
    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Max(0f, scale);
    }
    
    public void SetTime(float hour)
    {
        currentHour = Mathf.Clamp(hour, 0f, 24f);
        currentTimeOfDay = GetTimeOfDayFromHour(currentHour);
        OnHourChanged?.Invoke(currentHour);
        OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
    }
}