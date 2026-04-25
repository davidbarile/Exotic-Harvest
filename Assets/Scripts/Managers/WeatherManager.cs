using System;
using System.Collections;
using UnityEngine;
using TMPro;
using static GlobalEnums;

/// <summary>
/// Manages weather effects and weather-based resource generation
/// </summary>
public class WeatherManager : MonoBehaviour, ITickable
{
    public static WeatherManager IN;

    // Events
    public static Action<EWeatherType> OnWeatherChanged;
    public static Action<EWeatherType, float> OnWeatherIntensityChanged;
    public static Action OnRainStarted;
    public static Action OnRainStopped;
    public static Action OnWindStarted;
    public static Action OnWindStopped;

    [SerializeField] private TMP_Text weatherDisplayText;

    [SerializeField] private EWeatherType currentWeather = EWeatherType.Clear;
    [SerializeField] private EWeatherType debugWeather = EWeatherType.Clear;
    [SerializeField] private float weatherChangeInterval = 300f; // 5 minutes in seconds
    [SerializeField] private float weatherIntensity = 1f; // 0-1 for effects strength
    
    private float weatherTimer;
    private float nextWeatherChange;

    // Properties
    public static EWeatherType CurrentWeather => IN.currentWeather;
    public static EWeatherType LastWeather { get; private set; }
    public static float WeatherIntensity => IN.weatherIntensity;
    public static bool IsRaining => IN.currentWeather.HasFlag(EWeatherType.Rain) || IN.currentWeather.HasFlag(EWeatherType.Storm);
    
    private IEnumerator Start()
    {
        this.nextWeatherChange = this.weatherChangeInterval;

        TickManager.OnSecondTick += SecondTick;

        ChangeWeather();

        yield return new WaitForSeconds(1f); // Wait a moment to ensure everything is initialized
        
        if ((int)this.debugWeather > 0)
            ForceWeather(this.debugWeather);
    }
    
    private void OnDestroy()
    {
        TickManager.OnSecondTick -= SecondTick;
    }
    
    public void Tick()
    {
        // Optional: Fast tick updates for weather effects
    }
    
    public void SecondTick()
    {
        this.weatherTimer += 1f * TimeManager.IN.TimeScale;
        
        // Check for weather changes
        if (this.weatherTimer >= this.nextWeatherChange)
        {
            ChangeWeather();
            this.weatherTimer = 0f;
            this.nextWeatherChange = UnityEngine.Random.Range(this.weatherChangeInterval * 0.5f, this.weatherChangeInterval * 1.5f);
        }
    }
    
    private void ChangeWeather()
    {
        EWeatherType oldWeather = this.currentWeather;
        
        // Simple weather transition logic
        EWeatherType[] possibleWeathers = GetPossibleWeathers(this.currentWeather);
        this.currentWeather = possibleWeathers[UnityEngine.Random.Range(0, possibleWeathers.Length)];

        // Set intensity based on weather type
        this.weatherIntensity = GetWeatherIntensity(this.currentWeather);
        
        UpdateWeatherDisplay();
        
        // Fire events
        if (oldWeather != this.currentWeather)
        {
            OnWeatherChanged?.Invoke(this.currentWeather);

            // Rain-specific events for resource generation
            if (!IsWeatherRain(oldWeather) && IsWeatherRain(this.currentWeather))
                OnRainStarted?.Invoke();
            else if (IsWeatherRain(oldWeather) && !IsWeatherRain(this.currentWeather))
                OnRainStopped?.Invoke();
                
            // Wind-specific events
            if (!IsWeatherWind(oldWeather) && IsWeatherWind(this.currentWeather))
                OnWindStarted?.Invoke();
            else if (IsWeatherWind(oldWeather) && !IsWeatherWind(this.currentWeather))
                OnWindStopped?.Invoke();
        }
        
        OnWeatherIntensityChanged?.Invoke(this.currentWeather, this.weatherIntensity);
    }
    
    private EWeatherType[] GetPossibleWeathers(EWeatherType current)
    {
        switch (current)
        {
            case EWeatherType.Clear:
                return new[] { EWeatherType.Clear, EWeatherType.Rain, EWeatherType.Wind, EWeatherType.Foggy };
            case EWeatherType.Rain:
                return new[] { EWeatherType.Rain, EWeatherType.Storm, EWeatherType.Clear, EWeatherType.Foggy };
            case EWeatherType.Storm:
                return new[] { EWeatherType.Rain, EWeatherType.Clear, EWeatherType.Wind };
            case EWeatherType.Wind:
                return new[] { EWeatherType.Clear, EWeatherType.Rain, EWeatherType.Wind };
            case EWeatherType.Foggy:
                return new[] { EWeatherType.Clear, EWeatherType.Rain };
            default:
                return new[] { EWeatherType.Clear };
        }
    }
    
    private float GetWeatherIntensity(EWeatherType weather)
    {
        switch (weather)
        {
            case EWeatherType.Clear: return 0.2f;
            case EWeatherType.Rain: return UnityEngine.Random.Range(0.4f, 0.8f);
            case EWeatherType.Storm: return UnityEngine.Random.Range(0.8f, 1f);
            case EWeatherType.Wind: return UnityEngine.Random.Range(0.3f, 0.6f);
            case EWeatherType.Snow: return UnityEngine.Random.Range(0.4f, 0.7f);
            case EWeatherType.Foggy: return UnityEngine.Random.Range(0.2f, 0.5f);
            default: return 0.5f;
        }
    }

    private bool IsWeatherRain(EWeatherType weather)
    {
        return weather.HasFlag(EWeatherType.Rain) || weather.HasFlag(EWeatherType.Storm);
    }
    
    private bool IsWeatherWind(EWeatherType weather)
    {
        return weather.HasFlag(EWeatherType.Wind);
    }

    public float GetResourceMultiplier(EResourceType resourceType)
    {
        switch (resourceType)
        {
            case EResourceType.Rain:
                return IsRaining ? (2f + this.weatherIntensity) : 1f;
            case EResourceType.Seeds:
                return this.currentWeather.HasFlag(EWeatherType.Rain) ? 1.5f : 1f;
            case EResourceType.Fireflies:
                return this.currentWeather.HasFlag(EWeatherType.Clear) ? 1.3f : 0.8f;
            case EResourceType.Stardust:
                return this.currentWeather.HasFlag(EWeatherType.Clear) ? 1.5f : 0.5f;
            default:
                return 1f;
        }
    }

    public void ForceWeather(EWeatherType weather)
    {
        EWeatherType oldWeather = this.currentWeather;
        this.currentWeather = weather;
        this.weatherIntensity = GetWeatherIntensity(weather);
        this.weatherTimer = 0f;

        UpdateWeatherDisplay();

        OnWeatherChanged?.Invoke(this.currentWeather);
        OnWeatherIntensityChanged?.Invoke(this.currentWeather, this.weatherIntensity);

        if (!IsWeatherRain(oldWeather) && IsWeatherRain(this.currentWeather))
            OnRainStarted?.Invoke();
        else if (IsWeatherRain(oldWeather) && !IsWeatherRain(this.currentWeather))
            OnRainStopped?.Invoke();
    }
    
    private void UpdateWeatherDisplay()
    {
        if (this.weatherDisplayText != null)
            this.weatherDisplayText.text = $"Weather: {this.currentWeather}\n<size=80%>Intensity: {this.weatherIntensity:P0}</size>";
    }
}