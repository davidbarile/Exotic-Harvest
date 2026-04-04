using UnityEngine;

/// <summary>
/// ScriptableObject definition for resources
/// </summary>
[CreateAssetMenu(fileName = "ResourceConfig", menuName = "Exotic Harvest/ResourceConfig")]
public class ResourceConfig : ScriptableObject
{
    public string ID; // Unique identifier, can be auto-generated from name or set manually

    [Header("Basic Info")]
    public string DisplayName;
    [TextArea(2, 10)] public string Description;
    public Sprite Icon;
    public Color IconColor = Color.white;
    
    [Header("ResourceData Properties")]
    public EResourceType ResourceType;
    public EResourceCategory Category;
    public int BaseValue = 1; // Base worth for trading/selling
    public int MaxStack = 1000000;
    
    [Header("Availability")]
    public bool IsAvailableAtStart = true;
    public ETimeOfDay[] AvailableTimes; // Empty = always available
    public EWeatherType[] AvailableWeather; // Empty = all weather
    
    [Header("Generation Settings")]
    public bool CanBeActivelyForaged = true;
    public bool CanBePassivelyGenerated = false;
    public float BaseGenerationRate = 1f; // resourcesSave per minute
    public float RarityMultiplier = 1f; // 1 = common, 10 = very rare
    
    [Header("Audio")]
    public AudioClip CollectionSound;
    public AudioClip SpawnSound;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        // if (!string.IsNullOrEmpty(this.ID))
        //     return;

        this.ID = this.name;
        //this.ID = this.name.Substring(this.name.IndexOf("_") + 1, this.name.Length - this.name.IndexOf("_") - 1).Trim();
        //this.ID = $"Resource_{this.ID}";
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
    
    public bool IsAvailableAtTime(ETimeOfDay currentTime)
    {
        if (this.AvailableTimes == null || this.AvailableTimes.Length == 0)
            return true;
            
        foreach (var time in this.AvailableTimes)
        {
            if (time == currentTime)
                return true;
        }
        return false;
    }
    
    public bool IsAvailableInWeather(EWeatherType currentWeather)
    {
        if (this.AvailableWeather == null || this.AvailableWeather.Length == 0)
            return true;
            
        foreach (var weather in this.AvailableWeather)
        {
            if (weather == currentWeather)
                return true;
        }
        return false;
    }

    public bool IsCurrentlyAvailable()
    {
        if (TimeManager.IN != null && !IsAvailableAtTime(TimeManager.IN.CurrentTimeOfDay))
            return false;

        if (WeatherManager.IN != null && !IsAvailableInWeather(WeatherManager.IN.CurrentWeather))
            return false;

        return true;
    }
}