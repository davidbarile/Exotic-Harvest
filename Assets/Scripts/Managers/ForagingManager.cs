using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Lean.Pool;
using Sirenix.OdinInspector;
using static GlobalEnums;

/// <summary>
/// Manages active foraging - spawning collectables based on time/weather
/// </summary>
public class ForagingManager : MonoBehaviour, ITickable
{
    public static ForagingManager IN;
    
    private static WaitForSeconds _waitForSeconds0_1 = new(0.1f);
   
    public static bool IsInitialized { get; private set; }

    public static bool IgnoreTimeOfDayAndWeather => IN != null && IN.ignoreTimeOfDayAndWeather;
    [SerializeField] private bool ignoreTimeOfDayAndWeather; // For testing - ignore time/weather conditions and spawn all collectables

    [Header("Raindrop Settings --------------")]
    [SerializeField] private RectTransform rainParent; // UI container for rain collectables
    public RectTransform RainParent => this.rainParent;
    [Range(0,10), SerializeField] private float raindropSpawnRate = 5f;// 
    private int numActiveRaindrops = 0;

    [Header("Lightning Settings --------------")]
    [SerializeField] private UiFadeTween lightningFlash;
    [SerializeField] private LightningBolt lightningBolt;

    [Space, SerializeField] private WeightedRandom minMaxTimeBetweenLightningStrikes;
    private DateTime lastLightningStrikeTime = DateTime.MinValue;
    private float secondsUntilNextLightningStrike = 5f;//gets set by weighted random on start and after each change

    [Header("Dewdrop Settings --------------")]
    [SerializeField] private RectTransform dewDropSpawnParent; // UI container for dewdrop collectables
    [SerializeField] private float dewGridSize = 20f; // Grid size for potential dewdrop spawn positions
    [Range(0,1), SerializeField] private float dewdropSpawnChance = 0.1f; //during morning
    [SerializeField] private bool debugSpawnAllDewdrops; // For testing - force spawn dewdrops on start
    private List<Dewdrop> activeDewdrops = new();
    private List<Vector3> dewSpawnPositions = new();

    [Header("Wind Settings --------------")]
    [Range(0,1), SerializeField] private float windItemSpawnChance = 0.1f;
    [SerializeField] private bool debugSpawnAllWindItems; // For testing - force spawn WindItems on start
    private List<WindItem> activeWindItems = new();

    [Space, SerializeField] private WeightedRandom minMaxTimeBetweenWindItems;
    private DateTime lastWindItemTime = DateTime.MinValue;
    private float secondsUntilNextWindItemSpawn = 5f;//gets set by weighted random on start and after each change

    public static bool CanSpawnDewdrops => (TimeManager.CurrentTimeOfDay.HasFlag(ETimeOfDay.Morning) &&
        (WeatherManager.IsClear || WeatherManager.IsFoggy) || WeatherManager.IsWindy);

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

     [Header("Fireflies Settings --------------")]
    [SerializeField] private RectTransform firefliesSpawnRect; // UI container for fireflies searchable positions
    [SerializeField] private Fireflies[] firefliesPrefabs; // Different fireflies variants to spawn
    [SerializeField] private float firefliesGridSize = 100f; // Grid size for potential fireflies searchable positions
    [SerializeField] private WeightedRandom minMaxTimeBetweenFirefliesSpawns; // Chance to spawn fireflies each hour during clear nights
    private DateTime lastFirefliesSpawnHour = DateTime.MinValue;
    private float secondsUntilNextFirefliesSpawn = 5f;//gets set by weighted random on start and after each change
    private List<Fireflies> activeFireflies = new();

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

    [Header("Stardust Settings --------------")]
    [SerializeField] private RectTransform stardustSpawnRect; // UI container for stardust searchable positions
    [SerializeField] private RectTransform stardustSpawnParent; // UI container for stardust searchable positions
    [SerializeField] private Stardust[] stardustPrefabs; // Different stardust variants to spawn
    [SerializeField] private float stardustGridSize = 100f; // Grid size for potential stardust searchable positions
    [SerializeField] private WeightedRandom minMaxTimeBetweenStardustSpawns; // Chance to spawn stardust each hour during clear nights
    private DateTime lastStardustSpawnHour = DateTime.MinValue;
    private float secondsUntilNextStardustSpawn = 5f;//gets set by weighted random on start and after each change
    private List<Stardust> activeStardusts = new();

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
    public Transform LootContainersParent => this.lootContainersParent.transform;
    [Space, SerializeField] private RectTransform[] windItemsContainers;

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

    public static string GetHarvestRejectMessage(EHarvestLocation inLocation, out string outTitle)
    {
        var message = string.Empty;
        outTitle = "Invalid Location";

        switch(inLocation)
        {
            case EHarvestLocation.MeadowDew:
                if(!CanSpawnDewdrops)
                {
                    outTitle = "No Dewdrops";
                    message = $"Come back in the Morning when\nweather is Clear, Foggy or Windy";
                }
                break;
            case EHarvestLocation.MeadowSearch:
                outTitle = "Invalid Location";
                message = $"You can't harvest here.";
                break;
            case EHarvestLocation.NightSky:
                outTitle = "Invalid Location";
                message = $"Requires: Clear, Foggy\nTime: Night";
                break;
            case EHarvestLocation.Beach:
                outTitle = "No Beach Loot";
                message = $"Come back in the Afternoon or Evening";
                break;
            default:
                outTitle = "Invalid Location";
                message = "Cannot harvest here.";
                break;
        }

        return message;
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

        this.rockPile.InitRockPositions();
        this.rockPile.ResetRocks();

        InitDewDropPositions();
        InitMeadowSearchablePositions();
        InitNightSkySearchablePositions();

        this.lootContainersParent.SetActive(false);//hide AFTER initializing

        if (this.debugSpawnAllDewdrops)
            DebugDewSpawn();

        if (this.debugSpawnRocks)
            DebugSpawnRocks();

        if (this.debugSpawnAllMeadowSearchables)
            DebugSpawnMeadowSearchables();
        else
            SpawnMeadowSearchables();//TODO: sync this with time of day/weather instead of spawning on start

        if (this.debugSpawnAllNightSkySearchables)
            DebugSpawnNightSkySearchables();
        else
            SpawnNightSkySearchables();//TODO: sync this with time of day/weather instead of spawning on start

        IsInitialized = true;
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
        if (WeatherManager.IsRaining)
        {
            SpawnRaindrops();
        }
        else
        {
            this.numActiveRaindrops = 0;
        }

        if (WeatherManager.IsStorm)
        {
            var secondsElapsed = (DateTime.Now - this.lastLightningStrikeTime).TotalSeconds;

            if (secondsElapsed > this.secondsUntilNextLightningStrike)
            {
                this.lastLightningStrikeTime = DateTime.Now;
                this.secondsUntilNextLightningStrike = this.minMaxTimeBetweenLightningStrikes.GetWeightedRandomQuantity();

                SpawnLightning();
            }
        }
        
        if(WeatherManager.IsWindy)
        {
            var secondsElapsed = (DateTime.Now - this.lastWindItemTime).TotalSeconds;

            if (secondsElapsed > this.secondsUntilNextWindItemSpawn)
            {
                this.lastWindItemTime = DateTime.Now;
                this.secondsUntilNextWindItemSpawn = this.minMaxTimeBetweenWindItems.GetWeightedRandomQuantity();

                SpawnWindItems();
            }
        }

        // Spawn dewdrops during morning
        if (CanSpawnDewdrops || this.ignoreTimeOfDayAndWeather)
            SpawnDewdrops();
    }
    
    public void SecondTick()
    {
        // Do not use - we use OnHourChanged instead
    }

    public void OnHourChanged(float inCurrentHour)
    {
        StopAllCoroutines();
        StartCoroutine(DistributedHourRefresh());
    }

    private IEnumerator DistributedHourRefresh()
    {
        // Could be used to spread out spawning/despawning of collectables over several frames to reduce stutter
        RefreshRockPile();
        yield return _waitForSeconds0_1;
        RefreshMeadowSearchables();//causes stutter
        yield return _waitForSeconds0_1;
        RefreshNightSkySearchables();
        yield return _waitForSeconds0_1;
        RefreshMoonbeamGenerator();
        yield return _waitForSeconds0_1;
        RefreshStardustGenerator();
        yield return _waitForSeconds0_1;
        RefreshFirefliesGenerator();
    }

    private void OnTimeOfDayChanged(ETimeOfDay inNewTime)
    {
        // Could add special events or spawn/despawn certain collectables based on time of day
    }

    private void OnWeatherChanged(EWeatherType inNewWeather)
    {
        if (this.debugSpawnAllDewdrops || this.ignoreTimeOfDayAndWeather)
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
        float numSpawns = this.raindropSpawnRate * WeatherManager.WeatherIntensity;

        for(int i = 0; i < Mathf.Ceil(numSpawns); i++)
        {
            if (UnityEngine.Random.value < numSpawns)
            {
                Vector2 spawnPos = GetRaindropSpawnPosition();
                var raindrop = Pool.Spawn<Raindrop>("Raindrop", this.rainParent);
                raindrop.transform.localPosition = spawnPos;
                raindrop.Spawn();
                this.numActiveRaindrops++;
                raindrop.name = $"Raindrop_{this.numActiveRaindrops}";
            }
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
    #endregion

    #region Lightning
    private void SpawnLightning()
    {
        this.lightningBolt.Spawn();

        this.lightningFlash.gameObject.SetActive(true);
        this.lightningFlash.CanvasGroup.alpha = .6f;
        this.lightningFlash.FadeOut();
    }
    #endregion

    #region Wind
    public void SpawnWindItems()
    {
        var lootConfig = LootManager.IN.GetRandomLootConfigOfType(ELootType.Wind, TimeManager.CurrentTimeOfDay);
        var lootDatas = lootConfig.GetRandomLoot(true);

        var windItemParentIndex = WeatherManager.WindDirection == -1 ? 1 : 0;
        var windItemParent = this.windItemsContainers[windItemParentIndex];

        foreach(var data in lootDatas)
        {
            var shouldSpawn = data.ChanceToDrop.GetWeightedRandomQuantity();
            var rnd = UnityEngine.Random.Range(0, 100);
            if (rnd > shouldSpawn) 
                continue;

            for(int i = 0; i < data.Quantity; ++i)
            {
                var yPos = UnityEngine.Random.Range(500, Screen.height - 250);
                Debug.Log($"{name}. yPos = {yPos}. WeatherManager.WindDirection = {WeatherManager.WindDirection}. parent = {windItemParent.name}");

                var windItem = Pool.Spawn<WindItem>("WindItem", windItemParent);
                windItem.transform.localPosition = new Vector3(0, yPos, 0);
                windItem.transform.rotation = Quaternion.identity;
                windItem.transform.localScale = Vector3.one;
                windItem.Configure(data);
                var rndSpeed = UnityEngine.Random.Range(1f, 1.3f);
                rndSpeed = 1 + WeatherManager.WeatherIntensity;//TODO: randomize
                windItem.PlayRandomAnim(rndSpeed);
            }
        }
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

        this.dewSpawnPositions.RandomizeList();

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

            var dewdrop = Pool.Spawn<Dewdrop>("Dewdrop", this.dewDropSpawnParent);
            dewdrop.name = $"Dewdrop_{this.activeDewdrops.Count}";
            dewdrop.transform.localPosition = new Vector3(spawnPos.x, spawnPos.y, 0f);
            var rndDepth = UnityEngine.Random.Range(0f, 50f);
            dewdrop.transform.position = new Vector3(dewdrop.transform.position.x, dewdrop.transform.position.y, spawnPos.z + rndDepth); // Set Z based on collider
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
                LeanPool.Despawn(dewdrop.gameObject);
        }
        this.activeDewdrops.Clear();
    }
    #endregion

    #region Rocks
    [Button(ButtonSizes.Large)]
    private void DebugSpawnRocks()
    {
        this.rockPile.ResetRocks();
    }

    private void RefreshRockPile()
    {
        if (TimeManager.CurrentTimeOfDay.HasFlag(ETimeOfDay.Afternoon) || this.ignoreTimeOfDayAndWeather)
        {
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
        this.meadowSearchablePositions = GetRandomPositions(this.meadowSearchableParent, inCount: -1, inGridSize: this.meadowGridSize,
            inOffsetRange: 0, inChanceToSpawn: 1f, inForceGridToSpawnAreaSize: true, inIterations: 1);

        var layerMask = LayerMask.GetMask("MeadowSearchables");
        var totalPositions = this.meadowSearchablePositions.Count;

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

        var textColor = this.meadowSearchablePositions.Count == 0 ? "red" : "white";
        Debug.Log($"<color={textColor}>InitMeadowSearchablePositions()  meadowSearchablePositions = {this.meadowSearchablePositions.Count}/{totalPositions}</color>");
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
            this.ignoreTimeOfDayAndWeather)
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
    
    //TODO: create a function that loads various loot configs and stores in an array, so they can be quickly chosen and not cause a stutter.
    private void SpawnMeadowSearchables()
    {
        var meadowLootConfig = LootManager.IN.GetRandomLootConfigOfType(ELootType.Meadow);
        if (meadowLootConfig == null)
        {
            Debug.Log("<color=red>No Meadow LootConfigs found. Cannot debug spawn meadow searchables.</color>");
            return;
        }

        DeleteAllMeadowSearchables();

        if(this.meadowSearchablePositions.Count == 0)
        {
            Debug.Log("<color=red>No meadow searchable positions available. Cannot spawn meadow searchables.</color>");
            return;
        }

        this.meadowSearchablePositions.RandomizeList();

        float rnd = 0;

        foreach (var lootData in meadowLootConfig.LootDatas)
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

                var meadowSearchable = Pool.Spawn<Searchable>("MeadowSearchable", this.meadowSearchableParent);
                meadowSearchable.name = $"MeadowSearchable_{this.activeMeadowSearchables.Count}";

                var spawnPosIndex = 0;

                if (this.activeNightSkySearchables.Count > 0)
                    spawnPosIndex = this.activeMeadowSearchables.Count % this.meadowSearchablePositions.Count;

                meadowSearchable.transform.localPosition = this.meadowSearchablePositions[spawnPosIndex];
                meadowSearchable.transform.position = new Vector3(meadowSearchable.transform.position.x, meadowSearchable.transform.position.y, 0f); // Set Z based on collider
                meadowSearchable.Configure(lootData);
                meadowSearchable.Spawn();
                this.activeMeadowSearchables.Add(meadowSearchable);
            }
        }

        Debug.Log($"<color=white>Lootconfig chosen for meadow searchables: {meadowLootConfig.DisplayName} with {meadowLootConfig.LootDatas.Length} loot datas.  activeMeadowSearchables.Count = {this.activeMeadowSearchables.Count}</color>");
    }

    private void DeleteAllMeadowSearchables()
    {
        foreach (var meadowSearchable in this.activeMeadowSearchables)
        {
            if (meadowSearchable != null)
                LeanPool.Despawn(meadowSearchable.gameObject);
        }
        this.activeMeadowSearchables.Clear();
    }
    #endregion

    #region Fireflies Searchables
    private void RefreshFirefliesGenerator()
    {
        //Debug.Log($"<color=grey>RefreshFirefliesGenerator()  lastFirefliesSpawnHour = {this.lastFirefliesSpawnHour}   secondsUntilNextFirefliesSpawn = {this.secondsUntilNextFirefliesSpawn}</color>");
        //the world animation shows and hides the parent containter, so we don't need to check for time of day here
        if (!this.firefliesSpawnRect.gameObject.activeInHierarchy || !(WeatherManager.IsClear || WeatherManager.IsFoggy || this.ignoreTimeOfDayAndWeather))
            return;

        var secondsElapsed = (DateTime.Now - this.lastFirefliesSpawnHour).TotalSeconds;
        //secondsElapsed *= TimeManager.IN.TimeScale;

        //Debug.Log($"<color=white>RefreshFirefliesGenerator()  lastFirefliesSpawnHour = {this.lastFirefliesSpawnHour}   secondsUntilNextFirefliesSpawn = {this.secondsUntilNextFirefliesSpawn}</color>");

        if (secondsElapsed > this.secondsUntilNextFirefliesSpawn)
        {
            this.lastFirefliesSpawnHour = DateTime.Now;
            this.secondsUntilNextFirefliesSpawn = this.minMaxTimeBetweenFirefliesSpawns.GetWeightedRandomQuantity();

            SpawnFireflies();
        }
    }
    private void SpawnFireflies()
    {
        //ResetFireflies();

        var firefliesPrefab = this.firefliesPrefabs[UnityEngine.Random.Range(0, this.firefliesPrefabs.Length)];//random fireflies variant
        var fireflies = Instantiate(firefliesPrefab, this.firefliesSpawnRect);
        fireflies.transform.localPosition = GetRandomFirefliesSpawnPosition();
        fireflies.transform.SetAsLastSibling(); // Ensure fireflies appears above other elements
        fireflies.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.35f, 0.5f); // Randomize size for visual variety
        fireflies.Spawn();
        this.activeFireflies.Add(fireflies);

        //Debug.Log($"<color=yellow>SpawnFireflies() pos = {fireflies.transform.localPosition}</color>");

        Vector3 GetRandomFirefliesSpawnPosition()
        {
            var xPos = UnityEngine.Random.Range(this.firefliesSpawnRect.rect.xMin, this.firefliesSpawnRect.rect.xMax);
            var yPos = UnityEngine.Random.Range(this.firefliesSpawnRect.rect.yMin, this.firefliesSpawnRect.rect.yMax);

            xPos = Mathf.Round(xPos / this.firefliesGridSize) * this.firefliesGridSize;
            yPos = Mathf.Round(yPos / this.firefliesGridSize) * this.firefliesGridSize;
            return new Vector3(xPos, yPos, 0f);
        }
    }

    private void ResetFireflies()
    {
        foreach (var firefly in this.activeFireflies)
        {
            if (firefly != null)
                LeanPool.Despawn(firefly.gameObject);
        }
        this.activeFireflies.Clear();
    }
    #endregion

    #region Night Sky Searchables
    private void InitNightSkySearchablePositions()
    {
        // If no predefined positions, generate a grid of positions within the spawn area
        this.nightSkySearchablePositions = GetRandomPositions(this.nightSkySearchableParent, inCount: -1, inGridSize: this.nightSkyGridSize, inOffsetRange: 0, inChanceToSpawn: 1f, inForceGridToSpawnAreaSize: true, inIterations: 1);

        var layerMask = LayerMask.GetMask("NightSkySearchables");

        var totalPositions = this.nightSkySearchablePositions.Count;

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

        var textColor = this.nightSkySearchablePositions.Count == 0 ? "red" : "white";
        Debug.Log($"<color={textColor}>InitNightSkySearchablePositions()  nightSkySearchablePositions = {this.nightSkySearchablePositions.Count}/{totalPositions}</color>");
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
        if (TimeManager.CurrentTimeOfDay.HasFlag(ETimeOfDay.Night) || this.ignoreTimeOfDayAndWeather)
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

        foreach (var lootData in nightSkyLootConfig.LootDatas)
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

                var nightSkySearchable = Pool.Spawn<Searchable>("NightSkySearchable", this.nightSkySearchableParent);
                nightSkySearchable.name = $"NightSkySearchable_{this.activeNightSkySearchables.Count}";

                var spawnPosIndex = 0;

                if (this.activeNightSkySearchables.Count > 0)
                    spawnPosIndex = this.activeNightSkySearchables.Count % this.nightSkySearchablePositions.Count;

                nightSkySearchable.transform.localPosition = this.nightSkySearchablePositions[spawnPosIndex];
                nightSkySearchable.transform.position = new Vector3(nightSkySearchable.transform.position.x, nightSkySearchable.transform.position.y, 0f); // Set Z based on collider
                nightSkySearchable.Configure(lootData);
                nightSkySearchable.Spawn();
                this.activeNightSkySearchables.Add(nightSkySearchable);
            }
        }
        
        Debug.Log($"<color=white>Spawned {this.activeNightSkySearchables.Count} night sky searchables based on loot config: {nightSkyLootConfig.DisplayName}</color>");
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

    #region Stardust Searchables
    private void RefreshStardustGenerator()
    {
        //the world animation shows and hides the parent containter, so we don't need to check for time of day here
        if (!this.stardustSpawnRect.gameObject.activeInHierarchy || !(WeatherManager.IsClear || this.ignoreTimeOfDayAndWeather))
            return;

        var secondsElapsed = (DateTime.Now - this.lastStardustSpawnHour).TotalSeconds;
        //secondsElapsed *= TimeManager.IN.TimeScale;

        if (secondsElapsed > this.secondsUntilNextStardustSpawn)
        {
            this.lastStardustSpawnHour = DateTime.Now;
            this.secondsUntilNextStardustSpawn = this.minMaxTimeBetweenStardustSpawns.GetWeightedRandomQuantity();

            SpawnStardust();
        }
    }

    private void SpawnStardust()
    {
        ResetStardusts();

        var stardustPrefab = this.stardustPrefabs[UnityEngine.Random.Range(0, this.stardustPrefabs.Length)];//random stardust variant
        var stardust = Instantiate(stardustPrefab, this.stardustSpawnRect);
        stardust.transform.localPosition = GetRandomStardustSpawnPosition();
        stardust.transform.SetParent(this.stardustSpawnParent, true);// Move to spawn parent while keeping local position
        stardust.transform.SetAsLastSibling(); // Ensure stardust appears above other elements
        stardust.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.8f, 1.2f); // Randomize size for visual variety
        stardust.Spawn();
        this.activeStardusts.Add(stardust);

        Vector3 GetRandomStardustSpawnPosition()
        {
            var xPos = UnityEngine.Random.Range(this.stardustSpawnRect.rect.xMin, this.stardustSpawnRect.rect.xMax);
            var yPos = UnityEngine.Random.Range(this.stardustSpawnRect.rect.yMin, this.stardustSpawnRect.rect.yMax);

            xPos = Mathf.Round(xPos / this.stardustGridSize) * this.stardustGridSize;
            yPos = Mathf.Round(yPos / this.stardustGridSize) * this.stardustGridSize;
            return new Vector3(xPos, yPos, 0f);
        }
    }

    private void ResetStardusts()
    {
        foreach (var sd in this.activeStardusts)
        {
            if (sd != null)
                sd.Expire();
        }
        this.activeStardusts.Clear();
    }
    #endregion

    #region Moonbeams
    private void RefreshMoonbeamGenerator()
    {
        //the world animation shows and hides the parent containter, so we don't need to check for time of day here
        if (!this.moonbeamGenerator.gameObject.activeInHierarchy || !(WeatherManager.IsClear || this.ignoreTimeOfDayAndWeather))
            return;

        var secondsElapsed = (DateTime.Now - this.lastMoonbeamsSpawnHour).TotalSeconds;
        //secondsElapsed *= TimeManager.IN.TimeScale;

        if (secondsElapsed > this.secondsUntilNextMoonbeamSpawn)
        {
            this.lastMoonbeamsSpawnHour = DateTime.Now;

            this.secondsUntilNextMoonbeamSpawn = this.minMaxTimeBetweenMoonbeamSpawns.GetWeightedRandomQuantity();

            this.moonbeamGenerator.SpawnMoonbeams();
        }
    }
    #endregion
}