using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages active foraging - spawning collectables based on time/weather
/// </summary>
public class ForagingManager : MonoBehaviour, ITickable
{
    public static ForagingManager IN;

    [Header("UI Spawn Configuration")]
    [SerializeField] private RectTransform gameplayCanvas; // Main gameplay area
    [SerializeField] private RectTransform rainSpawnParent; // UI container for rain collectables
    [SerializeField] private RectTransform dewDropSpawnParent; // UI container for dewdrop collectables
    [SerializeField] private Vector2 spawnAreaPadding = new Vector2(50f, 50f); // Padding from canvas edges
    
    [Header("Collectable Prefabs")]
    [SerializeField] private GameObject dewdropPrefab; // Assign Dewdrop prefab here
    [SerializeField] private GameObject raindropPrefab; // Assign Raindrop prefab here
    [SerializeField] private GameObject[] allCollectablePrefabs; // Array for all collectable types
    
    [Header("Dewdrop Settings")]
    [SerializeField] private int maxDewdrops = 5;
    [SerializeField] private float dewdropSpawnChance = 0.1f; // Per second during morning
    
    [Header("Raindrop Settings")]
    [SerializeField] private float raindropSpawnRate = 2f; // Per second during rain
    
    private List<Collectable> activeCollectables = new List<Collectable>();
    private float secondTimer = 0f;
    
    // Events
    public static event Action<int> OnCollectableCountChanged;
    
    private void Awake()
    {            
        if (gameplayCanvas == null)
            gameplayCanvas = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
    }
    
    private void OnEnable()
    {
        TickManager.OnTick += Tick;
        TickManager.OnSecondTick += SecondTick;
        Collectable.OnCollectableSpawned += OnCollectableSpawned;
        Collectable.OnCollectableCollected += OnCollectableCollected;
        Collectable.OnCollectableExpired += OnCollectableExpired;
        
        // Listen to weather/time events
        WeatherManager.OnRainStarted += OnRainStarted;
        WeatherManager.OnRainStopped += OnRainStopped;
        TimeManager.OnTimeOfDayChanged += OnTimeOfDayChanged;
    }
    
    private void OnDisable()
    {
        TickManager.OnTick -= Tick;
        TickManager.OnSecondTick -= SecondTick;
        Collectable.OnCollectableSpawned -= OnCollectableSpawned;
        Collectable.OnCollectableCollected -= OnCollectableCollected;
        Collectable.OnCollectableExpired -= OnCollectableExpired;
        
        WeatherManager.OnRainStarted -= OnRainStarted;
        WeatherManager.OnRainStopped -= OnRainStopped;
        TimeManager.OnTimeOfDayChanged -= OnTimeOfDayChanged;
    }
    
    public void Tick()
    {
        // Spawn raindrops during rain
        // if (WeatherManager.IN.IsRaining)
        // {
        //     SpawnRaindrops();
        // }

        SpawnRaindrops();
    }
    
    public void SecondTick()
    {
        secondTimer += 1f;
        
        // Spawn dewdrops during morning
        if (TimeManager.IN.CurrentTimeOfDay == TimeOfDay.Morning)
        {
            SpawnDewdrops();
        }
        
        // // Spawn raindrops during rain
        // if (WeatherManager.IN != null && WeatherManager.IN.IsRaining)
        // {
        //     SpawnRaindrops();
        // }
    }
    
    private void SpawnDewdrops()
    {
        if (dewdropPrefab == null || GetCollectableCount(ResourceType.Water, CollectionMethod.Click) >= maxDewdrops)
            return;
            
        if (UnityEngine.Random.value < dewdropSpawnChance)
        {
            Vector2 spawnPos = GetRandomUIPosition();
            GameObject dewdrop = Instantiate(dewdropPrefab, dewDropSpawnParent);
            RectTransform dewdropRect = dewdrop.GetComponent<RectTransform>();
            if (dewdropRect != null)
            {
                dewdropRect.anchoredPosition = spawnPos;
            }
        }
    }
    
    private void SpawnRaindrops()
    {
        if (raindropPrefab == null)
            return;
            
        // Spawn based on rain intensity
        float spawnChance = raindropSpawnRate * (WeatherManager.IN?.WeatherIntensity ?? 0.5f);
        
        if (UnityEngine.Random.value < spawnChance)
        {
            Vector2 spawnPos = GetRaindropSpawnPosition();
            GameObject raindrop = Instantiate(raindropPrefab, rainSpawnParent);
            RectTransform raindropRect = raindrop.GetComponent<RectTransform>();
            if (raindropRect != null)
            {
                raindropRect.anchoredPosition = spawnPos;
            }
        }
    }
    
    private Vector2 GetRandomUIPosition()
    {
        if (gameplayCanvas == null)
            return Vector2.zero;
            
        Rect canvasRect = gameplayCanvas.rect;
        
        return new Vector2(
            UnityEngine.Random.Range(canvasRect.xMin + spawnAreaPadding.x, canvasRect.xMax - spawnAreaPadding.x),
            UnityEngine.Random.Range(canvasRect.yMin + spawnAreaPadding.y, canvasRect.yMax - spawnAreaPadding.y)
        );
    }
    
    private Vector2 GetRaindropSpawnPosition()
    {
        if (gameplayCanvas == null)
            return Vector2.zero;
            
        Rect canvasRect = gameplayCanvas.rect;
        
        // Raindrops spawn from the top
        return new Vector2(
            UnityEngine.Random.Range(canvasRect.xMin + spawnAreaPadding.x, canvasRect.xMax - spawnAreaPadding.x),
            canvasRect.yMax + 100f // Slightly above canvas
        );
    }
    
    private int GetCollectableCount(ResourceType type, CollectionMethod method)
    {
        int count = 0;
        foreach (var collectable in activeCollectables)
        {
            if (collectable != null && collectable.ResourceType == type && collectable.CollectionMethod == method)
                count++;
        }
        return count;
    }
    
    private void OnCollectableSpawned(Collectable collectable)
    {
        activeCollectables.Add(collectable);
        OnCollectableCountChanged?.Invoke(activeCollectables.Count);
    }
    
    private void OnCollectableCollected(Collectable collectable)
    {
        activeCollectables.Remove(collectable);
        OnCollectableCountChanged?.Invoke(activeCollectables.Count);
    }
    
    private void OnCollectableExpired(Collectable collectable)
    {
        activeCollectables.Remove(collectable);
        OnCollectableCountChanged?.Invoke(activeCollectables.Count);
    }
    
    private void OnRainStarted()
    {
        // Could add special effects or increase spawn rates
    }
    
    private void OnRainStopped()
    {
        // Stop rain effects
    }
    
    private void OnTimeOfDayChanged(TimeOfDay newTime)
    {
        // Adjust spawning based on time
        if (newTime != TimeOfDay.Morning)
        {
            // Clear existing dewdrops when morning ends
            ClearCollectables(ResourceType.Water, CollectionMethod.Click);
        }
    }
    
    private void ClearCollectables(ResourceType type, CollectionMethod method)
    {
        for (int i = activeCollectables.Count - 1; i >= 0; i--)
        {
            var collectable = activeCollectables[i];
            if (collectable != null && collectable.ResourceType == type && collectable.CollectionMethod == method)
            {
                Destroy(collectable.gameObject);
            }
        }
    }
    
    public List<Collectable> GetActiveCollectables()
    {
        return new List<Collectable>(activeCollectables);
    }
}