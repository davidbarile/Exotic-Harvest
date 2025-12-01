using UnityEngine;

/// <summary>
/// ScriptableObject definition for resources
/// </summary>
[CreateAssetMenu(fileName = "New Resource", menuName = "Exotic Harvest/Resource Definition")]
public class ResourceDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string displayName;
    [TextArea(2, 4)] public string description;
    public Sprite icon;
    public Color uiColor = Color.white;
    
    [Header("Resource Properties")]
    public ResourceType resourceType;
    public ResourceCategory category;
    public int baseValue = 1; // Base worth for trading/selling
    public int maxStackSize = 999;
    
    [Header("Availability")]
    public bool isAvailableAtStart = true;
    public TimeOfDay[] availableTimes; // Empty = always available
    public WeatherType[] availableWeather; // Empty = all weather
    
    [Header("Generation Settings")]
    public bool canBeActivelyForaged = true;
    public bool canBePassivelyGenerated = false;
    public float baseGenerationRate = 1f; // Resources per minute
    public float rarityMultiplier = 1f; // 1 = common, 10 = very rare
    
    [Header("Audio")]
    public AudioClip collectionSound;
    public AudioClip spawnSound;
    
    // Runtime properties
    public string ID => name; // Use ScriptableObject name as ID
    
    public bool IsAvailableAtTime(TimeOfDay currentTime)
    {
        if (availableTimes == null || availableTimes.Length == 0)
            return true;
            
        foreach (var time in availableTimes)
        {
            if (time == currentTime)
                return true;
        }
        return false;
    }
    
    public bool IsAvailableInWeather(WeatherType currentWeather)
    {
        if (availableWeather == null || availableWeather.Length == 0)
            return true;
            
        foreach (var weather in availableWeather)
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