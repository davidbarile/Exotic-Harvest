using System;
using UnityEngine;
using TMPro;
using static GlobalEnums;

/// <summary>
/// Manages the day/night cycle and time-based events
/// </summary>
public class TimeManager : MonoBehaviour, ITickable
{
    public static TimeManager IN;

    // Events
    public static Action<ETimeOfDay> OnTimeOfDayChanged;
    public static Action<float> OnHourChanged;
    public static Action OnNewDay;

    public bool UseRealTime => this.useRealTime;
    public float HoursToSecondsRatio => 24f / this.dayLengthInMinutes;
    
    [SerializeField] private bool useRealTime; // If true, time advances based on real seconds, otherwise uses SecondTick for testing
    public bool FreezeTime;
    [SerializeField] private EDayOfWeek currentDayOfWeek = EDayOfWeek.Sunday;
    [SerializeField] private float dayLengthInMinutes = 24f; // Real minutes for a full game day
    [SerializeField] private ETimeOfDay currentTimeOfDay = ETimeOfDay.Morning;
    [SerializeField, Range(0f, 24f)] private float currentHour = 8f; // Start at 8 AM

    [Header("UI Elements")]
    [SerializeField] private TMP_Text timeDisplayText;
    [Space, SerializeField] private TMP_Text timeSliderText;
    [SerializeField] private TMP_Text timeScaleSliderText;
    
    public float TimeScale { get; private set; } = 1f; // Speed multiplier for time progression

    // Properties
    public static ETimeOfDay CurrentTimeOfDay => IN.currentTimeOfDay;
    public static ETimeOfDay LastTimeOfDay { get; private set; }
    public static float CurrentHour => IN.currentHour;
    public static float DayProgress => IN.currentHour / 24f; // 0-1 progress through day
    
    private void Start() 
    {
        TickManager.OnSecondTick += SecondTick;
        //TickManager.OnTick += Tick;
    }
        
    private void OnDestroy()
    {
        TickManager.OnSecondTick -= SecondTick;
        //TickManager.OnTick -= Tick;

    }

    public void Tick()
    {
        // Optional: Fast tick updates if needed
    }
    
    public void SecondTick()
    {
        if (this.FreezeTime && Time.frameCount > 1) // Allow initial time setup but prevent progression
            return;
                
        if (this.useRealTime)
        {   
            var realTime = DateTime.Now;
            this.currentHour = realTime.Hour + (realTime.Minute / 60f) + (realTime.Second / 3600f);
            this.currentDayOfWeek = GetDayOfWeekFromInt((int)realTime.DayOfWeek);
            //OnNewDay?.Invoke();
        }
        else
        {
            // Advance time
            float hoursPerSecond = 24f / (this.dayLengthInMinutes * 60f);
            this.currentHour += hoursPerSecond * this.TimeScale;

            // Handle day rollover
            if (this.currentHour >= 24f)
            {
                this.currentHour -= 24f;
                OnNewDay?.Invoke();
                this.currentDayOfWeek = GetDayOfWeekFromInt((int)this.currentDayOfWeek + 1);
            }
        }
        
        OnHourChanged?.Invoke(this.currentHour);
        
        // Check for time of day changes
        ETimeOfDay newTimeOfDay = GetTimeOfDayFromHour(this.currentHour);
        if (newTimeOfDay != this.currentTimeOfDay)
        {
            LastTimeOfDay = this.currentTimeOfDay;
            this.currentTimeOfDay = newTimeOfDay;
            OnTimeOfDayChanged?.Invoke(this.currentTimeOfDay);
        }

        if(this.timeDisplayText)
            this.timeDisplayText.text = $"{FormatFloatAsTime(this.currentHour)}\n<size=80%>{this.currentDayOfWeek} {this.currentTimeOfDay}</size>";
    }
    
    private ETimeOfDay GetTimeOfDayFromHour(float hour)
    {
        if (hour >= 6f && hour < 12f)
            return ETimeOfDay.Morning;
        else if (hour >= 12f && hour < 16f)
            return ETimeOfDay.Afternoon;
        else if (hour >= 16f && hour < 20f)
            return ETimeOfDay.Evening;
        else
            return ETimeOfDay.Night;
    }
    
    public bool IsTimeForResource(EResourceType resourceType)
    {
        switch (resourceType)
        {
            case EResourceType.Rain:
                return true; // Always available, but more during rain
            case EResourceType.Moonbeams:
            case EResourceType.Stardust:
            case EResourceType.FallingStars:
                return this.currentTimeOfDay.HasFlag(ETimeOfDay.Night);
            case EResourceType.Fireflies:
                return this.currentTimeOfDay.HasFlag(ETimeOfDay.Evening) || this.currentTimeOfDay.HasFlag(ETimeOfDay.Night);
            case EResourceType.Nectar:
                return this.currentTimeOfDay.HasFlag(ETimeOfDay.Morning);
            case EResourceType.Seeds:
            case EResourceType.Berries:
                return this.currentTimeOfDay.HasFlag(ETimeOfDay.Afternoon);
            default:
                return true; // Most resources available anytime
        }
    }
    
    public void SetTimeScale(Single scale)
    {
        this.TimeScale = Mathf.Max(0f, scale);

        if (this.timeScaleSliderText)
            this.timeScaleSliderText.text = $"Time Scale: {this.TimeScale:0.0}x";
    }

    public void SetTime(Single hour)
    {
        this.currentHour = Mathf.Clamp(hour, 0f, 24f);
        this.currentTimeOfDay = GetTimeOfDayFromHour(this.currentHour);
        OnHourChanged?.Invoke(this.currentHour);
        OnTimeOfDayChanged?.Invoke(this.currentTimeOfDay);

        if (this.timeSliderText)
            this.timeSliderText.text = $"Time: {FormatFloatAsTime(this.currentHour)} <i>({this.currentTimeOfDay})</i>";

        if(this.timeDisplayText)
            this.timeDisplayText.text = $"{FormatFloatAsTime(this.currentHour)}\n<size=80%>{this.currentDayOfWeek} {this.currentTimeOfDay}</size>";
    }

    public void ToggleRealTime(bool useReal)
    {
        this.useRealTime = useReal;
    }
    
    public static string FormatFloatAsTime(float hour)
    {
        var amPmHour = hour % 24f;
        int h = Mathf.FloorToInt(amPmHour);
        int m = Mathf.FloorToInt((amPmHour - h) * 60f);
        var amPm = h >= 12 ? "PM" : "AM";
        h %= 12;
        if (h == 0) h = 12; // Convert 0 to 12 for 12-hour format
        return $"{h:00}:{m:00} {amPm}";
    }
}