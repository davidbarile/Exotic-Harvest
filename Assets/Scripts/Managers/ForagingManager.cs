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

    public RectTransform RainParent => this.rainParent;
    [SerializeField] private RectTransform rainParent; // UI container for rain collectables
    [SerializeField] private RectTransform dewDropSpawnParent; // UI container for dewdrop collectables
    [SerializeField] private Vector2 spawnAreaPadding = new Vector2(50f, 50f); // Padding from canvas edges
    
    [Header("Collectable Prefabs")]
    [SerializeField] private GameObject[] allCollectablePrefabs; // Array for all collectable types
    
    [Header("Dewdrop Settings")]
    [SerializeField] private int maxDewdrops = 5;
    [SerializeField] private float dewdropSpawnChance = 0.1f; // Per second during morning
    
    [Header("Raindrop Settings")]
    [SerializeField] private float raindropSpawnRate = 2f; // Per second during rain
    
    private List<Collectable> activeCollectables = new();
    private float secondTimer = 0f;
    
    // Events
    public static Action<int> OnCollectableCountChanged;
    
    
    private void Start()
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
    
    private void OnDestroy()
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
        if (WeatherManager.IN.IsRaining)
        {
            SpawnRaindrops();
        }
    }
    
    public void SecondTick()
    {
        secondTimer += 1f;
        
        // Spawn dewdrops during morning
        if (TimeManager.IN.CurrentTimeOfDay.HasFlag(ETimeOfDay.Morning))
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
        if (GetCollectableCount(EResourceType.Dew, ECollectionMethod.Click) >= maxDewdrops)
            return;
            
        if (UnityEngine.Random.value < dewdropSpawnChance)
        {
            Vector2 spawnPos = GetRandomDewPosition();
            var dewdrop = PrefabManager.IN.SpawnPrefab<Dewdrop>("Dewdrop", this.dewDropSpawnParent);
            dewdrop.transform.localPosition = spawnPos;
            dewdrop.Spawn();
        }
    }
    
    private void SpawnRaindrops()
    {            
        // Spawn based on rain intensity
        float spawnChance = this.raindropSpawnRate * (WeatherManager.IN?.WeatherIntensity ?? 0.5f);
        
        if (UnityEngine.Random.value < spawnChance)
        {
            Vector2 spawnPos = GetRaindropSpawnPosition();
            var raindrop = PrefabManager.IN.SpawnPrefab<Raindrop>("Raindrop", this.rainParent);
            raindrop.transform.localPosition = spawnPos;
            raindrop.Spawn();
        }
    }
    
    private Vector2 GetRandomDewPosition()
    {   
        var canvasRect = this.dewDropSpawnParent.rect;
        
        return new Vector2(
            UnityEngine.Random.Range(canvasRect.xMin + spawnAreaPadding.x, canvasRect.xMax - spawnAreaPadding.x),
            UnityEngine.Random.Range(canvasRect.yMin + spawnAreaPadding.y, canvasRect.yMax - spawnAreaPadding.y)
        );
    }
    
    private Vector2 GetRaindropSpawnPosition()
    {
        var canvasRect = this.rainParent.rect;
        
        // Raindrops spawn from the top
        return new Vector2(
            UnityEngine.Random.Range(canvasRect.xMin + spawnAreaPadding.x, canvasRect.xMax - spawnAreaPadding.x),
            canvasRect.yMax + 100f // Slightly above canvas
        );
    }
    
    private int GetCollectableCount(EResourceType type, ECollectionMethod method)
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
    
    private void OnTimeOfDayChanged(ETimeOfDay newTime)
    {
        // Adjust spawning based on time
        if (!newTime.HasFlag(ETimeOfDay.Morning))
        {
            // Clear existing dewdrops when morning ends
            ClearCollectables(EResourceType.Dew, ECollectionMethod.Click);
        }
    }
    
    private void ClearCollectables(EResourceType type, ECollectionMethod method)
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
        return new(this.activeCollectables);
    }
}