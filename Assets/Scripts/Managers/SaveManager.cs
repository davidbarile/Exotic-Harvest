using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Manages saving and loading game data
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager IN;
    
    [Header("Save Settings")]
    [SerializeField] private string saveFileName = "exotic_harvest_save.json";
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5 minutes
    [SerializeField] private bool saveOnApplicationPause = true;
    
    private string savePath;
    private GameSaveData currentSaveData;
    private float autoSaveTimer = 0f;
    private float sessionStartTime;
    
    // Events
    public static event Action OnGameSaved;
    public static event Action OnGameLoaded;
    public static event Action<string> OnSaveError;
    public static event Action<string> OnLoadError;
    
    // Properties
    public bool HasSaveFile => File.Exists(savePath);
    public GameSaveData CurrentSaveData => currentSaveData;
    
    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        sessionStartTime = Time.time;
    }
    
    private void Start()
    {
        // Auto-load on start
        if (HasSaveFile)
        {
            LoadGame();
        }
        else
        {
            CreateNewSave();
        }
    }
    
    private void Update()
    {
        if (autoSaveEnabled)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                autoSaveTimer = 0f;
                SaveGame();
            }
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && saveOnApplicationPause)
        {
            SaveGame();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && saveOnApplicationPause)
        {
            SaveGame();
        }
    }
    
    public void CreateNewSave()
    {
        currentSaveData = new GameSaveData();
        ApplySaveDataToGame();
    }
    
    public bool SaveGame()
    {
        try
        {
            // Update save data from current game state
            CollectSaveDataFromGame();
            
            // Serialize to JSON
            string json = JsonUtility.ToJson(currentSaveData, true);
            
            // Write to file
            File.WriteAllText(savePath, json);
            
            OnGameSaved?.Invoke();
            Debug.Log($"Game saved successfully to {savePath}");
            return true;
        }
        catch (Exception e)
        {
            string error = $"Failed to save game: {e.Message}";
            Debug.LogError(error);
            OnSaveError?.Invoke(error);
            return false;
        }
    }
    
    public bool LoadGame()
    {
        try
        {
            if (!HasSaveFile)
            {
                Debug.LogWarning("No save file found, creating new save");
                CreateNewSave();
                return true;
            }
            
            // Read file
            string json = File.ReadAllText(savePath);
            
            // Deserialize
            currentSaveData = JsonUtility.FromJson<GameSaveData>(json);
            
            if (currentSaveData == null)
            {
                throw new Exception("Failed to deserialize save data");
            }
            
            // Apply to game
            ApplySaveDataToGame();
            
            OnGameLoaded?.Invoke();
            Debug.Log("Game loaded successfully");
            return true;
        }
        catch (Exception e)
        {
            string error = $"Failed to load game: {e.Message}";
            Debug.LogError(error);
            OnLoadError?.Invoke(error);
            
            // Fallback to new save
            CreateNewSave();
            return false;
        }
    }
    
    private void CollectSaveDataFromGame()
    {
        if (currentSaveData == null)
            currentSaveData = new GameSaveData();
            
        // Update metadata
        currentSaveData.saveTime = DateTime.Now;
        currentSaveData.totalPlayTime += Time.time - sessionStartTime;
        sessionStartTime = Time.time;
        
        // Resources
        if (ResourceManager.IN != null)
            currentSaveData.resources = ResourceManager.IN.GetSaveData();
        
        // Decorations
        if (DecorationManager.IN != null)
            currentSaveData.decorations = DecorationManager.IN.GetSaveData();
        
        // Time & Weather
        if (TimeManager.IN != null)
            currentSaveData.currentGameHour = TimeManager.IN.CurrentHour;
        if (WeatherManager.IN != null)
        {
            currentSaveData.currentWeather = WeatherManager.IN.CurrentWeather;
            currentSaveData.weatherIntensity = WeatherManager.IN.WeatherIntensity;
        }
        
        // Settings (window position, etc.)
        CollectSettingsData();
        
        // Statistics
        CollectStatsData();
    }
    
    private void ApplySaveDataToGame()
    {
        if (currentSaveData == null)
            return;
            
        // Resources
        if (ResourceManager.IN != null)
            ResourceManager.IN.LoadSaveData(currentSaveData.resources);
        
        // Decorations
        if (DecorationManager.IN != null)
            DecorationManager.IN.LoadSaveData(currentSaveData.decorations);
        
        // Time & Weather
        if (TimeManager.IN != null)
            TimeManager.IN.SetTime(currentSaveData.currentGameHour);
        if (WeatherManager.IN != null)
            WeatherManager.IN.ForceWeather(currentSaveData.currentWeather);
        
        // Settings
        ApplySettingsData();
    }
    
    private void CollectSettingsData()
    {
        // Window settings (using UniWindowController)
        if (Kirurobo.UniWindowController.current != null)
        {
            var controller = Kirurobo.UniWindowController.current;
            currentSaveData.settings.windowTransparency = controller.isTransparent ? 0.8f : 1f;
            currentSaveData.settings.alwaysOnTop = controller.isTopmost;
        }
        
        // Audio settings (placeholder - implement when audio system is added)
        // Time scale
        if (TimeManager.IN != null)
            currentSaveData.settings.timeScale = 1f; // Will be implemented
    }
    
    private void ApplySettingsData()
    {
        // Window settings
        if (Kirurobo.UniWindowController.current != null)
        {
            var controller = Kirurobo.UniWindowController.current;
            controller.isTransparent = currentSaveData.settings.windowTransparency < 1f;
            controller.isTopmost = currentSaveData.settings.alwaysOnTop;
        }
    }
    
    private void CollectStatsData()
    {
        // This will be expanded as we track more statistics
        if (currentSaveData.stats == null)
            currentSaveData.stats = new GameStatsData();
        
        currentSaveData.stats.sessionsPlayed++;
    }
    
    public bool DeleteSave()
    {
        try
        {
            if (HasSaveFile)
            {
                File.Delete(savePath);
                currentSaveData = null;
                Debug.Log("Save file deleted");
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save: {e.Message}");
            return false;
        }
    }
    
    public void SetAutoSave(bool enabled)
    {
        autoSaveEnabled = enabled;
    }
    
    public void SetAutoSaveInterval(float intervalSeconds)
    {
        autoSaveInterval = Mathf.Max(30f, intervalSeconds); // Minimum 30 seconds
    }
    
    // Statistics helpers
    public void RecordResourceCollected(ResourceType type, int amount)
    {
        if (currentSaveData?.stats == null) return;
        
        currentSaveData.stats.totalResourcesCollected += amount;
        
        switch (type)
        {
            case ResourceType.Water:
                currentSaveData.stats.waterCollected += amount;
                break;
            case ResourceType.Seeds:
                currentSaveData.stats.seedsCollected += amount;
                break;
            case ResourceType.Gems:
                currentSaveData.stats.gemsCollected += amount;
                break;
        }
    }
    
    public void RecordDecorationPlaced()
    {
        if (currentSaveData?.stats != null)
            currentSaveData.stats.decorationsPlaced++;
    }
    
    public void RecordRareEvent(ResourceType eventType)
    {
        if (currentSaveData?.stats == null) return;
        
        currentSaveData.stats.rareEventsWitnessed++;
        
        if (eventType == ResourceType.UnicornBlessing)
            currentSaveData.stats.unicornEncounters++;
        else if (eventType == ResourceType.MermaidSong)
            currentSaveData.stats.mermaidEncounters++;
    }
}