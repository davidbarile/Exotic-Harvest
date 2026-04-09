using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UIElements;

/// <summary>
/// Manages active foraging - spawning collectables based on time/weather
/// </summary>
public class ForagingManager : MonoBehaviour, ITickable
{
    public static ForagingManager IN;

    // Events
    public static Action<int> OnCollectableCountChanged;

    [Header("Raindrop Settings --------------")]
    [SerializeField] private RectTransform rainParent; // UI container for rain collectables
    public RectTransform RainParent => this.rainParent;
    [SerializeField] private float raindropSpawnRate = 2f; // Per second during rain

    [Header("Dewdrop Settings --------------")]
    [SerializeField] private RectTransform dewDropSpawnParent; // UI container for dewdrop collectables
    [SerializeField] private int maxDewdrops = 5;
    [SerializeField] private float dewdropSpawnChance = 0.1f; // Per second during morning
    [SerializeField] private bool debugSpawnAllDewdrops; // For testing - force spawn dewdrops on start
    private List<Dewdrop> activeDewdrops = new();
    private List<Vector3> dewSpawnPositions = new();

    [Header("Rock Pile Settings --------------")]
    [SerializeField] private RockPile rockPile; // Reference to rock pile for spawning rocks
    [SerializeField] private float rockSpawnFrequency = 1f; // Rate to spawn rocks each hour in the Afternoon

    [Space, SerializeField] private bool debugSpawnRocks;
    
    private List<Collectable> activeCollectables = new();
    private float secondTimer = 0f;
    private float nextRockSpawnTime = -1;
    private ETimeOfDay lastTimeOfDay = ETimeOfDay.Morning; // Track last time of day to detect changes

    #region Static Methods
    public static List<Vector3> GetRandomPositions(RectTransform inSpawnArea, int inCount, float inGridSize,
        float inOffsetRange, float inChanceToSpawn = 1f, bool inForceGridToSpawnAreaSize = true, int inIterations = 1)
    {
        var positions = new List<Vector3>();

        for (int i = 0; i < inIterations; i++)
        {
            GeneratePositions();
        }

        if (inChanceToSpawn < 1f)
        {
            positions.RemoveAll(p => UnityEngine.Random.value > inChanceToSpawn);
        }

        return positions;

        void GeneratePositions()
        {
            var xPos = 0f;
            var yPos = 0f;

            if (inForceGridToSpawnAreaSize)
                inCount = 99999;

            for (int i = 0; i < inCount; i++)
            {
                var position = new Vector3(xPos, yPos, 0f);
                position += new Vector3(
                    UnityEngine.Random.Range(-inOffsetRange, inOffsetRange),
                    UnityEngine.Random.Range(-inOffsetRange, inOffsetRange),
                    0f
                );
                positions.Add(position);

                if (xPos < inSpawnArea.rect.width)
                {
                    xPos += inGridSize;
                }
                else
                {
                    xPos = 0;
                    yPos += inGridSize;
                }

                if (yPos > inSpawnArea.rect.height)
                {
                    if (inForceGridToSpawnAreaSize)
                        break; // Stop if we've filled the spawn area

                    yPos = 0;
                    xPos = 0;
                }
            }
        }
    }

    #endregion

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

        InitDewDropPositions();

        if (this.debugSpawnAllDewdrops)
            DebugDewSpawn();

         if (this.debugSpawnRocks)
            DebugSpawnRocks();
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
        if (TimeManager.IN.CurrentTimeOfDay.HasFlag(ETimeOfDay.Morning) &&
            (WeatherManager.IN.CurrentWeather == EWeatherType.Clear || WeatherManager.IN.CurrentWeather.HasFlag(EWeatherType.Foggy)))
        {
            //TODO: clear dew if it rains
            SpawnDewdrops();
        }

        if (TimeManager.IN.CurrentTimeOfDay.HasFlag(ETimeOfDay.Afternoon))
        {
            if (this.lastTimeOfDay != ETimeOfDay.Afternoon)
                this.rockPile.InitRockPositions();//do once when it turns afternoon
                
            //spawn rocks every hour in the Afternoon
            if (TimeManager.IN.CurrentHour > this.nextRockSpawnTime)
            {
                if (this.rockSpawnFrequency == 1f)
                    this.nextRockSpawnTime = Mathf.Floor(TimeManager.IN.CurrentHour) + 1f;//lock it to the hour
                else
                    this.nextRockSpawnTime = TimeManager.IN.CurrentHour + this.rockSpawnFrequency;

                this.rockPile.SpawnRocks();
            }
        }
        else
        {
            this.nextRockSpawnTime = -1; // Reset to allow spawning when we enter the time window again
        }

        this.lastTimeOfDay = TimeManager.IN.CurrentTimeOfDay;
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

    // private void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         Vector3 mouseWorldPosition = Input.mousePosition + DragManager.ScreenToWorldCameraDelta;
    //         Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPosition, LayerMask.GetMask("DewSpawn")); // Assuming collectables are on a layer named "Collectable"

    //         if (hitCollider != null)
    //         {
    //             var dewdrop = PrefabManager.IN.SpawnPrefab<Dewdrop>("Dewdrop", this.dewDropSpawnParent);
    //             dewdrop.transform.position = mouseWorldPosition;
    //             dewdrop.transform.localScale = Vector3.one * 2f;
    //             dewdrop.name = $"Derp_{UnityEngine.Random.Range(0, 999)}";
    //         }
    //     }
    // }
    
    private void InitDewDropPositions()
    {
        // If no predefined positions, generate a grid of positions within the spawn area
        this.dewSpawnPositions = GetRandomPositions(this.dewDropSpawnParent, inCount: -1, inGridSize: 20, inOffsetRange: 0, inChanceToSpawn: 1f, inForceGridToSpawnAreaSize: true, inIterations: 1);

        var layerMask = LayerMask.GetMask("DewSpawn");

        // Raycast to only show elements in colliders
        for (int i = 0; i < this.dewSpawnPositions.Count; i++)
        {
            var screenPos = this.dewSpawnPositions[i];
            var worldPos = this.dewDropSpawnParent.TransformPoint(screenPos);

            Collider2D hitCollider = Physics2D.OverlapPoint(worldPos, layerMask);

            if (hitCollider == null)
            {
                // No collider at this position, remove it from the list
                this.dewSpawnPositions.RemoveAt(i);
                i--;
            }
            else
            {
                this.dewSpawnPositions[i] = new Vector3(screenPos.x, screenPos.y, hitCollider.transform.position.z);
            }
        }
    }

    private void SpawnDewdrops()
    {
        // if (GetCollectableCount(EResourceType.Dew, ECollectionMethod.Click) >= this.maxDewdrops)
        //     return;

        if (UnityEngine.Random.value < this.dewdropSpawnChance)
        {
            Vector3 spawnPos = this.dewSpawnPositions[this.activeDewdrops.Count % this.dewSpawnPositions.Count]; // Cycle through predefined positions
            var dewdrop = PrefabManager.IN.SpawnPrefab<Dewdrop>("Dewdrop", this.dewDropSpawnParent);
            dewdrop.name = $"Dewdrop_{this.activeDewdrops.Count}";
            dewdrop.transform.localPosition = new Vector3(spawnPos.x, spawnPos.y, 0f);
            dewdrop.Spawn();
            this.activeDewdrops.Add(dewdrop);
        }
    }

    [Button(ButtonSizes.Large)]
    private void DebugDewSpawn()
    {
        DeleteAllDewdrops();

        var originalSpawnChance = this.dewdropSpawnChance;

        this.dewdropSpawnChance = 1f;
        for (int i = 0; i < this.dewSpawnPositions.Count; i++)
        {
            SpawnDewdrops();
        }

        this.dewdropSpawnChance = originalSpawnChance;
    }

    private void DeleteAllDewdrops()
    {
        foreach (var dewdrop in this.activeDewdrops)
        {
            if (dewdrop != null)
                Destroy(dewdrop.gameObject);
        }
        this.activeDewdrops.Clear();
    }
    
    private Vector2 GetRaindropSpawnPosition()
    {
        var canvasRect = this.rainParent.rect;
        
        // Raindrops spawn from the top
        return new Vector2(
            UnityEngine.Random.Range(canvasRect.xMin, canvasRect.xMax),
            canvasRect.yMax + 100f // Slightly above canvas
        );
    }
    
    private int GetCollectableCount(EResourceType type, ECollectionMethod method)
    {
        int count = 0;
        foreach (var collectable in this.activeCollectables)
        {
            if (collectable != null && collectable.ResourceType == type && collectable.CollectionMethod == method)
                count++;
        }
        return count;
    }
    
    private void OnCollectableSpawned(Collectable collectable)
    {
        this.activeCollectables.Add(collectable);
        OnCollectableCountChanged?.Invoke(this.activeCollectables.Count);
    }
    
    private void OnCollectableCollected(Collectable collectable)
    {
        this.activeCollectables.Remove(collectable);
        OnCollectableCountChanged?.Invoke(this.activeCollectables.Count);
    }
    
    private void OnCollectableExpired(Collectable collectable)
    {
        this.activeCollectables.Remove(collectable);
        OnCollectableCountChanged?.Invoke(this.activeCollectables.Count);
    }
    
    private void OnRainStarted()
    {
        // Could add special effects or increase spawn rates
    }

    private void OnRainStopped()
    {
        // Stop rain effects
    }
    
    [Button(ButtonSizes.Large)]
    private void DebugSpawnRocks()
    {
        this.rockPile.InitRockPositions();
        this.rockPile.SpawnRocks();
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
        for (int i = this.activeCollectables.Count - 1; i >= 0; i--)
        {
            var collectable = this.activeCollectables[i];
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