using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages active foraging - spawning collectables based on time/weather
/// </summary>
public class ForagingManager : MonoBehaviour, ITickable
{
    public static ForagingManager IN;
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnParent;
    [SerializeField] private Camera mainCamera;
    
    [Header("Dewdrop Settings")]
    [SerializeField] private GameObject dewdropPrefab;
    [SerializeField] private int maxDewdrops = 5;
    [SerializeField] private float dewdropSpawnChance = 0.1f; // Per second during morning
    
    [Header("Raindrop Settings")]
    [SerializeField] private GameObject raindropPrefab;
    [SerializeField] private float raindropSpawnRate = 2f; // Per second during rain
    
    [Header("Spawn Boundaries")]
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-8, 3);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(8, 5);
    
    private List<Collectable> activeCollectables = new List<Collectable>();
    private float secondTimer = 0f;
    
    // Events
    public static event Action<int> OnCollectableCountChanged;
    
    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        if (spawnParent == null)
            spawnParent = transform;
    }
    
    private void OnEnable()
    {
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
        // Fast tick updates if needed
    }
    
    public void SecondTick()
    {
        secondTimer += 1f;
        
        // Spawn dewdrops during morning
        if (TimeManager.IN != null && TimeManager.IN.CurrentTimeOfDay == TimeOfDay.Morning)
        {
            SpawnDewdrops();
        }
        
        // Spawn raindrops during rain
        if (WeatherManager.IN != null && WeatherManager.IN.IsRaining)
        {
            SpawnRaindrops();
        }
    }
    
    private void SpawnDewdrops()
    {
        if (dewdropPrefab == null || GetCollectableCount(ResourceType.Water, CollectionMethod.Click) >= maxDewdrops)
            return;
            
        if (UnityEngine.Random.value < dewdropSpawnChance)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            Instantiate(dewdropPrefab, spawnPos, Quaternion.identity, spawnParent);
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
            Vector3 spawnPos = new Vector3(
                UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                spawnAreaMax.y,
                0f
            );
            
            Instantiate(raindropPrefab, spawnPos, Quaternion.identity, spawnParent);
        }
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        return new Vector3(
            UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            UnityEngine.Random.Range(spawnAreaMin.y, spawnAreaMax.y),
            0f
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