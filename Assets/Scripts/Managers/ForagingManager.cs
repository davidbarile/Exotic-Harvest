using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Sirenix.OdinInspector;
using static GlobalEnums;

/// <summary>
/// Manages active foraging - spawning collectables based on time/weather
/// </summary>
public class ForagingManager : MonoBehaviour, ITickable
{
    public static ForagingManager IN;

    [SerializeField] private bool debugIgnoreTimeOfDayAndWeather; // For testing - ignore time/weather conditions and spawn all collectables

    [Header("Raindrop Settings --------------")]
    [SerializeField] private RectTransform rainParent; // UI container for rain collectables
    public RectTransform RainParent => this.rainParent;
    [SerializeField] private float raindropSpawnRate = 2f; // Per second during rain
    private int numActiveRaindrops = 0;

    [Header("Dewdrop Settings --------------")]
    [SerializeField] private RectTransform dewDropSpawnParent; // UI container for dewdrop collectables
    [SerializeField] private float dewGridSize = 20f; // Grid size for potential dewdrop spawn positions
    [SerializeField] private float dewdropSpawnChance = 0.1f; // Per second during morning
    [SerializeField] private bool debugSpawnAllDewdrops; // For testing - force spawn dewdrops on start
    private List<Dewdrop> activeDewdrops = new();
    [SerializeField] private List<Vector3> dewSpawnPositions = new();

    [Header("Meadow Settings --------------")]
    [SerializeField] private Transform meadowLootField; // Parent transform for meadow collectables (e.g. mushrooms, flowers)
    public Transform MeadowLootField => this.meadowLootField;
    [SerializeField] private RectTransform meadowSearchableParent; // UI container for meadow searchable positions
    [SerializeField] private float meadowGridSize = 20f; // Grid size for potential meadow searchable positions
    [Tooltip("1 = spawn every hour, 0.5 = spawn every 1/2 hour, etc.")]
    [SerializeField] private float meadowRefreshFrequency = 1f; // Rate to spawn meadow collectables
    [SerializeField] private bool debugSpawnAllMeadowSearchables; // For testing - force spawn meadow searchables on start
    private List<Searchable> activeMeadowSearchables = new();
    private List<Vector3> meadowSearchablePositions = new();

    private float nextMeadowRefreshTime = -1;

    [Header("Night Sky Settings --------------")]
    [SerializeField] private Transform nightSkyLootField; // Parent transform for sky collectables (e.g. stars, constellations)
    public Transform NightSkyLootField => this.nightSkyLootField;
    [SerializeField] private RectTransform nightSkySearchableParent; // UI container for sky searchable positions
    [SerializeField] private float nightSkyGridSize = 20f; // Grid size for potential sky searchable positions
    [Tooltip("1 = spawn every hour, 0.5 = spawn every 1/2 hour, etc.")]
    [SerializeField] private float nightSkyRefreshFrequency = 1f; // Rate to spawn night sky collectables
    [SerializeField] private bool debugSpawnAllNightSkySearchables; // For testing - force spawn sky searchables on start
    private List<Searchable> activeNightSkySearchables = new();
    private List<Vector3> nightSkySearchablePositions = new();

    private float nextNightSkyRefreshTime = -1;

    [Header("Moonbeam Settings --------------")]
    [SerializeField] private MoonbeamGenerator moonbeamGenerator;
    [SerializeField] private WeightedRandom minMaxTimeBetweenMoonbeamSpawns; // Chance to spawn moonbeam each hour during clear nights
    private DateTime lastMoonbeamsSpawnHour = DateTime.MinValue;
    private float secondsUntilNextMoonbeamSpawn = 5f;//gets set by weighted random on start and after each change

    [Header("Rock Pile Settings --------------")]
    [SerializeField] private RockPile rockPile; // Reference to rock pile for spawning rocks
    [Tooltip("1 = spawn every hour, 0.5 = spawn every 1/2 hour, etc.")]
    [SerializeField] private float rockRefreshFrequency = 1f; // Rate to spawn rocks
    private float nextRockRefreshTime = -1;

    [Space, SerializeField] private bool debugSpawnRocks;

    [Header("Misc --------------")]
    [SerializeField] private GameObject lootContainersParent;

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
        TimeManager.OnHourChanged += OnHourChanged;

        // Listen to weather/time events
        WeatherManager.OnWeatherChanged += OnWeatherChanged;
        WeatherManager.OnRainStarted += OnRainStarted;
        WeatherManager.OnRainStopped += OnRainStopped;
        WeatherManager.OnWindStarted += OnWindStarted;
        WeatherManager.OnWindStopped += OnWindStopped;

        TimeManager.OnTimeOfDayChanged += OnTimeOfDayChanged;

        this.lootContainersParent.SetActive(false);

        InitDewDropPositions();
        InitMeadowSearchablePositions();
        InitNightSkySearchablePositions();

        if (this.debugSpawnAllDewdrops)
            DebugDewSpawn();

        if (this.debugSpawnRocks)
            DebugSpawnRocks();
        else
            this.rockPile.InitRockPositions();//do once when it turns afternoon

        if (this.debugSpawnAllMeadowSearchables)
            DebugSpawnMeadowSearchables();
        else
            SpawnMeadowSearchables();//TODO: sync this with time of day/weather instead of spawning on start

        if (this.debugSpawnAllNightSkySearchables)
            DebugSpawnNightSkySearchables();
        else
            SpawnNightSkySearchables();//TODO: sync this with time of day/weather instead of spawning on start
    }

    private void OnDestroy()
    {
        TickManager.OnTick -= Tick;
        TimeManager.OnHourChanged -= OnHourChanged;

        WeatherManager.OnWeatherChanged -= OnWeatherChanged;
        WeatherManager.OnRainStarted -= OnRainStarted;
        WeatherManager.OnRainStopped -= OnRainStopped;
        WeatherManager.OnWindStarted -= OnWindStarted;
        WeatherManager.OnWindStopped -= OnWindStopped;

        TimeManager.OnTimeOfDayChanged -= OnTimeOfDayChanged;
    }

    #region Timed Events
    public void Tick()
    {
        // Spawn raindrops during rain
        if (WeatherManager.IsRaining || this.debugIgnoreTimeOfDayAndWeather)
        {
            SpawnRaindrops();
        }
        else
        {
            this.numActiveRaindrops = 0;
        }

        // Spawn dewdrops during morning
        if (TimeManager.CurrentTimeOfDay.HasFlag(ETimeOfDay.Morning) &&
            (WeatherManager.CurrentWeather == EWeatherType.Clear ||
            WeatherManager.CurrentWeather.HasFlag(EWeatherType.Foggy)) ||
            this.debugIgnoreTimeOfDayAndWeather)
        {
            SpawnDewdrops();
        }
    }
    
    public void SecondTick()
    {
        // Do not use - we use OnHourChanged instead
    }

    public void OnHourChanged(float inCurrentHour)
    {
        RefreshRockPile();
        RefreshMeadowSearchables();
        RefreshNightSkySearchables();
        RefreshMoonbeamGenerator();
    }

    private void OnTimeOfDayChanged(ETimeOfDay inNewTime)
    {
        // Could add special events or spawn/despawn certain collectables based on time of day
    }

    private void OnWeatherChanged(EWeatherType inNewWeather)
    {
        if (this.debugSpawnAllDewdrops || this.debugIgnoreTimeOfDayAndWeather)
            return;

        if(WeatherManager.IsClear || WeatherManager.IsFoggy || WeatherManager.IsWindy)
            return;
            
        DeleteAllDewdrops();
    }
    #endregion


    #region Raindrops
    private void SpawnRaindrops()
    {
        // Spawn based on rain intensity
        float spawnChance = this.raindropSpawnRate * WeatherManager.WeatherIntensity;

        if (UnityEngine.Random.value < spawnChance)
        {
            Vector2 spawnPos = GetRaindropSpawnPosition();
            var raindrop = PrefabManager.IN.SpawnPrefab<Raindrop>("Raindrop", this.rainParent);
            raindrop.transform.localPosition = spawnPos;
            raindrop.Spawn();
            this.numActiveRaindrops++;
            raindrop.name = $"Raindrop_{this.numActiveRaindrops}";
        }
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
    
    private void OnRainStarted()
    {
        // Could add special effects or increase spawn rates
    }

    private void OnRainStopped()
    {
        // Stop rain effects
    }

    private void OnWindStarted()
    {
        // Could add special effects or increase spawn rates for certain collectables
    }

    private void OnWindStopped()
    {
        // Stop wind effects
    }
    #endregion


    #region Dewdrops
    private void InitDewDropPositions()
    {
        // If no predefined positions, generate a grid of positions within the spawn area
        this.dewSpawnPositions = GetRandomPositions(this.dewDropSpawnParent, inCount: -1, inGridSize: this.dewGridSize, inOffsetRange: 0, inChanceToSpawn: 1f, inForceGridToSpawnAreaSize: true, inIterations: 1);

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

        //Debug.Log($"<color=yellow>InitDewDropPositions(). this.dewSpawnPositions = {this.dewSpawnPositions.Count}</color>");
    }

    private void SpawnDewdrops()
    {
        if (this.dewSpawnPositions.Count == 0)
        {
            Debug.Log("<color=red>No dewdrop spawn positions available. Cannot spawn dewdrops.</color>");
            return;
        }

        if(this.activeDewdrops.Count >= this.dewSpawnPositions.Count)
            return;

        if (UnityEngine.Random.value < this.dewdropSpawnChance)
        {
            var index = this.activeDewdrops.Count % this.dewSpawnPositions.Count; // Cycle through positions based on current active count
            Vector3 spawnPos = this.dewSpawnPositions[index]; // Cycle through predefined positions

            var dewdrop = PrefabManager.IN.SpawnPrefab<Dewdrop>("Dewdrop", this.dewDropSpawnParent);
            dewdrop.name = $"Dewdrop_{this.activeDewdrops.Count}";
            dewdrop.transform.localPosition = new Vector3(spawnPos.x, spawnPos.y, 0f);
            dewdrop.transform.position = new Vector3(dewdrop.transform.position.x, dewdrop.transform.position.y, spawnPos.z); // Set Z based on collider
            dewdrop.Spawn();
            this.activeDewdrops.Add(dewdrop);
            //Debug.Log($"Spawn {dewdrop.name} [{index}] at position {spawnPos}. localPos = {dewdrop.transform.localPosition}   count: {this.activeDewdrops.Count} / {this.dewSpawnPositions.Count}.  frame = {Time.frameCount}", dewdrop.gameObject);
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
        //Debug.Log($"<color=red>DeleteAllDewdrops().  this.activeDewdrops = {this.activeDewdrops.Count}  frame = {Time.frameCount}</color>");
        foreach (var dewdrop in this.activeDewdrops)
        {
            if (dewdrop != null)
                Destroy(dewdrop.gameObject);
        }
        this.activeDewdrops.Clear();
    }
    #endregion

    #region Rocks
    [Button(ButtonSizes.Large)]
    private void DebugSpawnRocks()
    {
        this.rockPile.InitRockPositions();
        this.rockPile.SpawnRocks();
    }

    private void RefreshRockPile()
    {
        if (TimeManager.CurrentTimeOfDay.HasFlag(ETimeOfDay.Afternoon) || this.debugIgnoreTimeOfDayAndWeather)
        {
            if (TimeManager.LastTimeOfDay != ETimeOfDay.Afternoon || this.debugIgnoreTimeOfDayAndWeather)
                this.rockPile.InitRockPositions();//do once when it turns afternoon

            //refresh rocks every hour in the Afternoon
            if (TimeManager.CurrentHour > this.nextRockRefreshTime)
            {
                if (this.rockRefreshFrequency == 1f)
                    this.nextRockRefreshTime = Mathf.Floor(TimeManager.CurrentHour) + 1f;//lock it to the hour
                else
                    this.nextRockRefreshTime = TimeManager.CurrentHour + this.rockRefreshFrequency;

                this.rockPile.ResetRocks();
            }
        }
        else
        {
            this.nextRockRefreshTime = -1; // Reset to allow spawning when we enter the time window again
        }
    }
    #endregion

    #region Meadow Searchables
    private void InitMeadowSearchablePositions()
    {
        // If no predefined positions, generate a grid of positions within the spawn area
        this.meadowSearchablePositions = GetRandomPositions(this.meadowSearchableParent, inCount: -1, inGridSize: this.meadowGridSize, inOffsetRange: 0, inChanceToSpawn: 1f, inForceGridToSpawnAreaSize: true, inIterations: 1);

        var layerMask = LayerMask.GetMask("MeadowSearchables");

        // Raycast to only show elements in colliders
        for (int i = 0; i < this.meadowSearchablePositions.Count; i++)
        {
            var screenPos = this.meadowSearchablePositions[i];
            var worldPos = this.meadowSearchableParent.TransformPoint(screenPos);

            Collider2D hitCollider = Physics2D.OverlapPoint(worldPos, layerMask);

            if (hitCollider == null)
            {
                // No collider at this position, remove it from the list
                this.meadowSearchablePositions.RemoveAt(i);
                i--;
            }
            else
            {
                this.meadowSearchablePositions[i] = new Vector3(screenPos.x, screenPos.y, hitCollider.transform.position.z);
            }
        }
    }

    [Button(ButtonSizes.Large)]
    private void DebugSpawnMeadowSearchables()
    {
        this.debugSpawnAllMeadowSearchables = true;
        SpawnMeadowSearchables();
        this.debugSpawnAllMeadowSearchables = false;
    }

    private void RefreshMeadowSearchables()
    {
        if (TimeManager.CurrentTimeOfDay.HasFlag(ETimeOfDay.Morning) ||
            TimeManager.CurrentTimeOfDay.HasFlag(ETimeOfDay.Afternoon) ||
            this.debugIgnoreTimeOfDayAndWeather)
        {
            //spawn meadow searchables every hour in the Morning/Afternoon
            if (TimeManager.CurrentHour > this.nextMeadowRefreshTime)
            {
                if (this.meadowRefreshFrequency == 1f)
                    this.nextMeadowRefreshTime = Mathf.Floor(TimeManager.CurrentHour) + 1f;//lock it to the hour
                else
                    this.nextMeadowRefreshTime = TimeManager.CurrentHour + this.meadowRefreshFrequency;

                SpawnMeadowSearchables();
            }
        }
        else
        {
            this.nextMeadowRefreshTime = -1; // Reset to allow spawning when we enter the time window again
            DeleteAllMeadowSearchables();
        }
    }
    
    private void SpawnMeadowSearchables()
    {
        var meadowLootConfig = LootManager.IN.GetRandomLootConfigOfType(ELootType.Meadow);
        if (meadowLootConfig == null)
        {
            Debug.Log("<color=red>No Meadow LootConfigs found. Cannot debug spawn meadow searchables.</color>");
            return;
        }


        DeleteAllMeadowSearchables();

        Debug.Log($"Lootconfig chosen for meadow searchables: {meadowLootConfig.DisplayName} with {meadowLootConfig.LootDatas.Length} loot datas");

        if(this.meadowSearchablePositions.Count == 0)
        {
            Debug.Log("<color=red>No meadow searchable positions available. Cannot spawn meadow searchables.</color>");
            return;
        }

        this.meadowSearchablePositions.RandomizeList();

        float rnd = 0;

        foreach(var lootData in meadowLootConfig.LootDatas)
        {
            //possible number of this loot to drop based on the config settings
            var quantity = lootData.QuantityToDrop.GetWeightedRandomQuantity();

            if (this.debugSpawnAllMeadowSearchables)
                quantity = lootData.QuantityToDrop.MaxQuantity;//force spawn max quantity for testing

            for (int i = 0; i < quantity; i++)
            {
                //for each potential loot drop, roll to see if it actually drops based on the ChanceToDrop weight
                var chance = lootData.ChanceToDrop.GetWeightedRandomQuantity();
                rnd = UnityEngine.Random.value * 100f;
                
                if (rnd > chance && !this.debugSpawnAllMeadowSearchables)
                    continue;

                var meadowSearchable = PrefabManager.IN.SpawnPrefab<Searchable>("MeadowSearchable", this.meadowSearchableParent);
                meadowSearchable.name = $"MeadowSearchable_{this.activeMeadowSearchables.Count}";

                var spawnPosIndex = 0;

                if(this.activeNightSkySearchables.Count > 0)
                    spawnPosIndex =this.activeMeadowSearchables.Count % this.meadowSearchablePositions.Count;

                meadowSearchable.transform.localPosition = this.meadowSearchablePositions[spawnPosIndex];
                meadowSearchable.transform.position = new Vector3(meadowSearchable.transform.position.x, meadowSearchable.transform.position.y, 0f); // Set Z based on collider
                meadowSearchable.Configure(lootData);
                meadowSearchable.Spawn();
                this.activeMeadowSearchables.Add(meadowSearchable);
            }
        }
    }

    private void DeleteAllMeadowSearchables()
    {
        foreach (var meadowSearchable in this.activeMeadowSearchables)
        {
            if (meadowSearchable != null)
                Destroy(meadowSearchable.gameObject);
        }
        this.activeMeadowSearchables.Clear();
    }
    #endregion

    #region Night Sky Searchables
    private void InitNightSkySearchablePositions()
    {
        // If no predefined positions, generate a grid of positions within the spawn area
        this.nightSkySearchablePositions = GetRandomPositions(this.nightSkySearchableParent, inCount: -1, inGridSize: this.nightSkyGridSize, inOffsetRange: 0, inChanceToSpawn: 1f, inForceGridToSpawnAreaSize: true, inIterations: 1);

        var layerMask = LayerMask.GetMask("NightSkySearchables");

        // Raycast to only show elements in colliders
        for (int i = 0; i < this.nightSkySearchablePositions.Count; i++)
        {
            var screenPos = this.nightSkySearchablePositions[i];
            var worldPos = this.nightSkySearchableParent.TransformPoint(screenPos);

            Collider2D hitCollider = Physics2D.OverlapPoint(worldPos, layerMask);

            if (hitCollider == null)
            {
                // No collider at this position, remove it from the list
                this.nightSkySearchablePositions.RemoveAt(i);
                i--;
            }
            else
            {
                this.nightSkySearchablePositions[i] = new Vector3(screenPos.x, screenPos.y, hitCollider.transform.position.z);
            }
        }
    }

    [Button(ButtonSizes.Large)]
    private void DebugSpawnNightSkySearchables()
    {
        this.debugSpawnAllNightSkySearchables = true;
        SpawnNightSkySearchables();
        this.debugSpawnAllNightSkySearchables = false;
    }

    private void RefreshNightSkySearchables()
    {
        if (TimeManager.CurrentTimeOfDay.HasFlag(ETimeOfDay.Night) || this.debugIgnoreTimeOfDayAndWeather)
        {
            //spawn night sky searchables every hour at night
            if (TimeManager.CurrentHour > this.nextNightSkyRefreshTime)
            {
                if (this.nightSkyRefreshFrequency == 1f)
                    this.nextNightSkyRefreshTime = Mathf.Floor(TimeManager.CurrentHour) + 1f;//lock it to the hour
                else
                    this.nextNightSkyRefreshTime = TimeManager.CurrentHour + this.nightSkyRefreshFrequency;

                SpawnNightSkySearchables();
            }
        }
        else
        {
            this.nextNightSkyRefreshTime = -1; // Reset to allow spawning when we enter the time window again
            DeleteAllNightSkySearchables();
        }
    }
    
    private void SpawnNightSkySearchables()
    {
        var nightSkyLootConfig = LootManager.IN.GetRandomLootConfigOfType(ELootType.NightSky);
        if (nightSkyLootConfig == null)
        {
            Debug.Log("<color=red>No Night Sky LootConfigs found. Cannot debug spawn night sky searchables.</color>");
            return;
        }

        Debug.Log($"Lootconfig chosen for night sky searchables: {nightSkyLootConfig.DisplayName} with {nightSkyLootConfig.LootDatas.Length} loot datas");

        DeleteAllNightSkySearchables();

        if(  this.nightSkySearchablePositions.Count == 0)
        {
            Debug.Log("<color=red>No night sky searchable positions available. Cannot spawn night sky searchables.</color>");
            return;
        }

        this.nightSkySearchablePositions.RandomizeList();

        float rnd = 0;

        foreach(var lootData in nightSkyLootConfig.LootDatas)
        {
            //possible number of this loot to drop based on the config settings
            var quantity = lootData.QuantityToDrop.GetWeightedRandomQuantity();

            if (this.debugSpawnAllNightSkySearchables)
                quantity = lootData.QuantityToDrop.MaxQuantity;//force spawn max quantity for testing

            for (int i = 0; i < quantity; i++)
            {
                //for each potential loot drop, roll to see if it actually drops based on the ChanceToDrop weight
                var chance = lootData.ChanceToDrop.GetWeightedRandomQuantity();
                rnd = UnityEngine.Random.value * 100f;
                
                if (rnd > chance && !this.debugSpawnAllNightSkySearchables)
                    continue;

                var nightSkySearchable = PrefabManager.IN.SpawnPrefab<Searchable>("NightSkySearchable", this.nightSkySearchableParent);
                nightSkySearchable.name = $"NightSkySearchable_{this.activeNightSkySearchables.Count}";

                var spawnPosIndex = 0;

                if(this.activeNightSkySearchables.Count > 0)
                    spawnPosIndex =this.activeNightSkySearchables.Count % this.nightSkySearchablePositions.Count;

                nightSkySearchable.transform.localPosition = this.nightSkySearchablePositions[spawnPosIndex];
                nightSkySearchable.transform.position = new Vector3(nightSkySearchable.transform.position.x, nightSkySearchable.transform.position.y, 0f); // Set Z based on collider
                nightSkySearchable.Configure(lootData);
                nightSkySearchable.Spawn();
                this.activeNightSkySearchables.Add(nightSkySearchable);
            }
        }
    }

    private void DeleteAllNightSkySearchables()
    {
        foreach (var nightSkySearchable in this.activeNightSkySearchables)
        {
            if (nightSkySearchable != null)
                Destroy(nightSkySearchable.gameObject);
        }
        this.activeNightSkySearchables.Clear();
    }

    #endregion

    #region Moonbeams
    private void RefreshMoonbeamGenerator()
    {
        if (!this.moonbeamGenerator.gameObject.activeInHierarchy || !(WeatherManager.IsClear || this.debugIgnoreTimeOfDayAndWeather))
            return;

        var secondsElapsed = (DateTime.Now - this.lastMoonbeamsSpawnHour).TotalSeconds;
        secondsElapsed *= TimeManager.IN.TimeScale;

        if (secondsElapsed > this.secondsUntilNextMoonbeamSpawn)
        {
            this.lastMoonbeamsSpawnHour = DateTime.Now;

            this.secondsUntilNextMoonbeamSpawn = this.minMaxTimeBetweenMoonbeamSpawns.GetWeightedRandomQuantity();

            this.moonbeamGenerator.SpawnMoonbeams();
        }
    }
    #endregion
}