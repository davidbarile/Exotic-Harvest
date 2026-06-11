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
    private float weatherIntensity = 1f; // 0-1 for effects strength
    private int windDirection;//-1 = left, 1 = right

    [Space, SerializeField] private ParticleHelper fogParticle;
    [SerializeField] private Color[] fogColors;

    [Space, SerializeField] private ParticleHelper[] windParticles;

    [SerializeField] private float windEmissionRatio = 1f;
    [SerializeField] private float windSpeedRatio = 1f;
    
    private float weatherTimer;
    private float nextWeatherChange;
    private bool isForcingWeather;

    // Properties
    public static EWeatherType CurrentWeather => IN.currentWeather;
    public static EWeatherType LastWeather { get; private set; }
    public static float WeatherIntensity => IN.weatherIntensity;
    public static int WindDirection => IN.windDirection;
    public static bool IsRaining => IN.currentWeather.HasFlag(EWeatherType.Rain) || IN.currentWeather.HasFlag(EWeatherType.Storm);
    public static bool IsStorm => IN.currentWeather.HasFlag(EWeatherType.Storm);
    public static bool IsWindy => IN.currentWeather.HasFlag(EWeatherType.Wind);
    public static bool IsSnow => IN.currentWeather.HasFlag(EWeatherType.Snow);
    public static bool IsFoggy => IN.currentWeather.HasFlag(EWeatherType.Foggy);
    public static bool IsClear => IN.currentWeather == EWeatherType.Clear;
    
    private IEnumerator Start()
    {
        this.fogParticle.gameObject.SetActive(true);

        foreach (var windParticle in this.windParticles)
            windParticle.gameObject.SetActive(true);
        
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
    
    public void ChangeWeather()
    {
        // Simple weather transition logic
        if(!this.isForcingWeather)
        {
            EWeatherType[] possibleWeathers = GetPossibleWeathers(this.currentWeather);
            this.currentWeather = possibleWeathers[UnityEngine.Random.Range(0, possibleWeathers.Length)];
        }

        // Set intensity based on weather type
        this.weatherIntensity = GetWeatherIntensity(this.currentWeather);
        
        UpdateWeatherDisplay();

        // Fire events
        if (LastWeather != this.currentWeather || this.isForcingWeather)
        {
            OnWeatherChanged?.Invoke(this.currentWeather);

            // Rain-specific events
            if (!IsWeatherRain(LastWeather) && IsWeatherRain(this.currentWeather))
                OnRainStarted?.Invoke();
            else if (IsWeatherRain(LastWeather) && !IsWeatherRain(this.currentWeather))
                OnRainStopped?.Invoke();

            // Wind-specific events
            if (!IsWeatherWind(LastWeather) && IsWeatherWind(this.currentWeather))
                OnWindStarted?.Invoke();
            else if (IsWeatherWind(LastWeather) && !IsWeatherWind(this.currentWeather))
                OnWindStopped?.Invoke();

            if (IsWeatherWind(this.currentWeather))
            {
                this.windDirection = UnityEngine.Random.value < .5f ? -1 : 1;

                var particleIndex = this.windDirection == -1 ? 1 : 0;

                var inactivePartile = this.windParticles[1 - particleIndex];
                inactivePartile.Stop();

                var windParticle = this.windParticles[particleIndex];
                windParticle.SetEmissionRate(this.weatherIntensity * this.windEmissionRatio);
                var windSpeedMin = this.weatherIntensity * this.windSpeedRatio * -this.windDirection;
                windParticle.SetSpeed(windSpeedMin, windSpeedMin * 2f);
                windParticle.Play();
            }

            //Fog-specific effects
            if(IsWeatherFoggy(this.currentWeather))
            {
                this.fogParticle.SetEmissionRate(20f * this.weatherIntensity);//update fog effect based on intensity

                var color1 = new Color(this.fogColors[0].r, this.fogColors[0].g, this.fogColors[0].b, this.weatherIntensity);
                var color2 = new Color(this.fogColors[1].r, this.fogColors[1].g, this.fogColors[1].b, this.weatherIntensity);
                this.fogParticle.SetStartColors(color1, color2);

                this.fogParticle.Play();
            }
        }

        if (!IsWeatherWind(this.currentWeather))
        {
            foreach (var windParticle in this.windParticles)
                windParticle.Stop();
        }
        
        if(!IsWeatherFoggy(this.currentWeather))
        {
            this.fogParticle.Stop();
        }

        OnWeatherIntensityChanged?.Invoke(this.currentWeather, this.weatherIntensity);
        LastWeather = this.currentWeather;
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
            case EWeatherType.Clear: return 0f;
            case EWeatherType.Rain: return UnityEngine.Random.Range(0.05f, 0.3f);
            case EWeatherType.Storm: return UnityEngine.Random.Range(0.8f, 1f);
            case EWeatherType.Wind: return UnityEngine.Random.Range(0.2f, 0.6f);
            case EWeatherType.Snow: return UnityEngine.Random.Range(0.4f, 0.7f);
            case EWeatherType.Foggy: return UnityEngine.Random.Range(0.3f, 0.5f);
            default: return 0.5f;
        }
    }

    private bool IsWeatherRain(EWeatherType weather)
    {
        return weather.HasFlag(EWeatherType.Rain) || weather.HasFlag(EWeatherType.Storm);
    }

     private bool IsWeatherStorm(EWeatherType weather)
    {
        return weather.HasFlag(EWeatherType.Storm);
    }

    private bool IsWeatherWind(EWeatherType weather)
    {
        return weather.HasFlag(EWeatherType.Wind);
    }
    
    private bool IsWeatherFoggy(EWeatherType weather)
    {
        return weather.HasFlag(EWeatherType.Foggy);
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
        this.isForcingWeather = true;
        EWeatherType oldWeather = this.currentWeather;
        this.currentWeather = weather;
        this.weatherTimer = 0f;

        ChangeWeather();

        this.isForcingWeather = false;
    }
    
    private void UpdateWeatherDisplay()
    {
        if (this.weatherDisplayText != null)
            this.weatherDisplayText.text = $"Weather: {this.currentWeather}\n<size=80%>Intensity: {this.weatherIntensity:P0}</size>";
    }
}