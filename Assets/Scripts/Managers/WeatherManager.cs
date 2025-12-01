using System;
using UnityEngine;

/// <summary>
/// Manages weather effects and weather-based resource generation
/// </summary>
public class WeatherManager : MonoBehaviour, ITickable
{
    public static WeatherManager IN;
    
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
    public bool IsRaining => currentWeather == WeatherType.Rain || currentWeather == WeatherType.Storm;
    
    private void Start()
    {
        nextWeatherChange = weatherChangeInterval;
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
        weatherTimer += 1f;
        
        // Check for weather changes
        if (weatherTimer >= nextWeatherChange)
        {
            ChangeWeather();
            weatherTimer = 0f;
            nextWeatherChange = UnityEngine.Random.Range(weatherChangeInterval * 0.5f, weatherChangeInterval * 1.5f);
        }
    }
    
    private void ChangeWeather()
    {
        WeatherType oldWeather = currentWeather;
        
        // Simple weather transition logic
        WeatherType[] possibleWeathers = GetPossibleWeathers(currentWeather);
        currentWeather = possibleWeathers[UnityEngine.Random.Range(0, possibleWeathers.Length)];
        
        // Set intensity based on weather type
        weatherIntensity = GetWeatherIntensity(currentWeather);
        
        // Fire events
        if (oldWeather != currentWeather)
        {
            OnWeatherChanged?.Invoke(currentWeather);
            
            // Rain-specific events for resource generation
            if (!IsWeatherRain(oldWeather) && IsWeatherRain(currentWeather))
                OnRainStarted?.Invoke();
            else if (IsWeatherRain(oldWeather) && !IsWeatherRain(currentWeather))
                OnRainStopped?.Invoke();
        }
        
        OnWeatherIntensityChanged?.Invoke(currentWeather, weatherIntensity);
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
        return weather == WeatherType.Rain || weather == WeatherType.Storm;
    }
    
    public float GetResourceMultiplier(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Water:
                return IsRaining ? (2f + weatherIntensity) : 1f;
            case ResourceType.Seeds:
                return currentWeather == WeatherType.Rain ? 1.5f : 1f;
            case ResourceType.Fireflies:
                return currentWeather == WeatherType.Clear ? 1.3f : 0.8f;
            case ResourceType.Stardust:
                return currentWeather == WeatherType.Clear ? 1.5f : 0.5f;
            default:
                return 1f;
        }
    }
    
    public void ForceWeather(WeatherType weather)
    {
        WeatherType oldWeather = currentWeather;
        currentWeather = weather;
        weatherIntensity = GetWeatherIntensity(weather);
        weatherTimer = 0f;
        
        OnWeatherChanged?.Invoke(currentWeather);
        OnWeatherIntensityChanged?.Invoke(currentWeather, weatherIntensity);
        
        if (!IsWeatherRain(oldWeather) && IsWeatherRain(currentWeather))
            OnRainStarted?.Invoke();
        else if (IsWeatherRain(oldWeather) && !IsWeatherRain(currentWeather))
            OnRainStopped?.Invoke();
    }
}