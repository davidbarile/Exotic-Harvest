using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages weather effects and weather-based resource generation
/// </summary>
public class WeatherManager : MonoBehaviour, ITickable
{
    public static WeatherManager IN;

    [SerializeField] private TMP_Text weatherDisplayText;
    
    [SerializeField] private WeatherType currentWeather = WeatherType.Clear;
    [SerializeField] private float weatherChangeInterval = 300f; // 5 minutes in seconds
    [SerializeField] private float weatherIntensity = 1f; // 0-1 for effects strength
    
    private float weatherTimer;
    private float nextWeatherChange;
    
    // Events
    public static event Action<WeatherType> OnWeatherChanged;
    public static event Action<WeatherType, float> OnWeatherIntensityChanged;
    public static event Action OnRainStarted;
    public static event Action OnRainStopped;
    
    // Properties
    public WeatherType CurrentWeather => currentWeather;
    public float WeatherIntensity => weatherIntensity;
    public bool IsRaining => currentWeather.HasFlag(WeatherType.Rain) || currentWeather.HasFlag(WeatherType.Storm);
    
    private void Start()
    {
        this.nextWeatherChange = this.weatherChangeInterval;
        ChangeWeather();
    }
    
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
        WeatherType oldWeather = this.currentWeather;
        
        // Simple weather transition logic
        WeatherType[] possibleWeathers = GetPossibleWeathers(this.currentWeather);
        this.currentWeather = possibleWeathers[UnityEngine.Random.Range(0, possibleWeathers.Length)];

        // Set intensity based on weather type
        this.weatherIntensity = GetWeatherIntensity(this.currentWeather);
        
        if(this.weatherDisplayText != null)
            this.weatherDisplayText.text = $"Weather: {this.currentWeather} (Intensity: {this.weatherIntensity:F2})";
        
        // Fire events
        if (oldWeather != this.currentWeather)
        {
            OnWeatherChanged?.Invoke(this.currentWeather);
            
            // Rain-specific events for resource generation
            if (!IsWeatherRain(oldWeather) && IsWeatherRain(this.currentWeather))
                OnRainStarted?.Invoke();
            else if (IsWeatherRain(oldWeather) && !IsWeatherRain(this.currentWeather))
                OnRainStopped?.Invoke();
        }
        
        OnWeatherIntensityChanged?.Invoke(this.currentWeather, this.weatherIntensity);
    }
    
    private WeatherType[] GetPossibleWeathers(WeatherType current)
    {
        switch (current)
        {
            case WeatherType.Clear:
                return new[] { WeatherType.Clear, WeatherType.Rain, WeatherType.Wind, WeatherType.Foggy };
            case WeatherType.Rain:
                return new[] { WeatherType.Rain, WeatherType.Storm, WeatherType.Clear, WeatherType.Foggy };
            case WeatherType.Storm:
                return new[] { WeatherType.Rain, WeatherType.Clear, WeatherType.Wind };
            case WeatherType.Wind:
                return new[] { WeatherType.Clear, WeatherType.Rain, WeatherType.Wind };
            case WeatherType.Foggy:
                return new[] { WeatherType.Clear, WeatherType.Rain };
            default:
                return new[] { WeatherType.Clear };
        }
    }
    
    private float GetWeatherIntensity(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Clear: return 0.2f;
            case WeatherType.Rain: return UnityEngine.Random.Range(0.4f, 0.8f);
            case WeatherType.Storm: return UnityEngine.Random.Range(0.8f, 1f);
            case WeatherType.Wind: return UnityEngine.Random.Range(0.3f, 0.6f);
            case WeatherType.Snow: return UnityEngine.Random.Range(0.4f, 0.7f);
            case WeatherType.Foggy: return UnityEngine.Random.Range(0.2f, 0.5f);
            default: return 0.5f;
        }
    }
    
    private bool IsWeatherRain(WeatherType weather)
    {
        return weather.HasFlag(WeatherType.Rain) || weather.HasFlag(WeatherType.Storm);
    }
    
    public float GetResourceMultiplier(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Water:
                return IsRaining ? (2f + this.weatherIntensity) : 1f;
            case ResourceType.Seeds:
                return this.currentWeather.HasFlag(WeatherType.Rain) ? 1.5f : 1f;
            case ResourceType.Fireflies:
                return this.currentWeather.HasFlag(WeatherType.Clear) ? 1.3f : 0.8f;
            case ResourceType.Stardust:
                return this.currentWeather.HasFlag(WeatherType.Clear) ? 1.5f : 0.5f;
            default:
                return 1f;
        }
    }
    
    public void ForceWeather(WeatherType weather)
    {
        WeatherType oldWeather = this.currentWeather;
        this.currentWeather = weather;
        this.weatherIntensity = GetWeatherIntensity(weather);
        this.weatherTimer = 0f;
        
        OnWeatherChanged?.Invoke(this.currentWeather);
        OnWeatherIntensityChanged?.Invoke(this.currentWeather, this.weatherIntensity);
        
        if (!IsWeatherRain(oldWeather) && IsWeatherRain(this.currentWeather))
            OnRainStarted?.Invoke();
        else if (IsWeatherRain(oldWeather) && !IsWeatherRain(this.currentWeather))
            OnRainStopped?.Invoke();
    }
}