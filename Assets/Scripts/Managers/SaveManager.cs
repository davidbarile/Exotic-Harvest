using System;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;
using Leguar.TotalJSON;

/// <summary>
/// Manages saving and loading game data
/// </summary>
public class SaveManager : MonoBehaviour, ITickable
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
        var saveFolder = Application.persistentDataPath;

#if UNITY_EDITOR
        saveFolder = "Assets/PlayerData";
#endif

        this.savePath = Path.Combine(saveFolder, this.saveFileName);
        this.sessionStartTime = Time.time;
    }

    public void Init()
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
    
    public void Tick()
    {
        // Placeholder for per-frame updates if needed
    }
    
    public void SecondTick()
    {
        if (this.autoSaveEnabled)
        {
            ++this.autoSaveTimer;
            if (this.autoSaveTimer >= this.autoSaveInterval)
            {
                this.autoSaveTimer = 0f;
                SaveGame();
            }
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && this.saveOnApplicationPause)
        {
            SaveGame();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && this.saveOnApplicationPause)
        {
            SaveGame();
        }
    }
    
    public void CreateNewSave()
    {
        this.currentSaveData = new GameSaveData();
        ApplySaveDataToGame();
    }
    
    public bool SaveGame()
    {
        try
        {
            // Update save data from current game state
            CollectSaveDataFromGame();



            JSON json = JSON.Serialize(this.currentSaveData);

#if UNITY_EDITOR
            string jsonAsString = json.CreatePrettyString();
#else
            string jsonAsString = json.CreateString();
#endif

            var writer = new StreamWriter(this.savePath);
            writer.WriteLine(jsonAsString);
            writer.Close();

            OnGameSaved?.Invoke();
            Debug.Log($"Game saved successfully to {this.savePath}");

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
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

            DeserializeSettings deserializeSettings = new DeserializeSettings()
            {
                RequireAllFieldsArePopulated = false
            };

            JSON savedMapProgressDataJSON = LoadTextFileToJsonObject(this.savePath);
            this.currentSaveData = savedMapProgressDataJSON.Deserialize<GameSaveData>(deserializeSettings);

            if (this.currentSaveData == null)
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
    
    private static JSON LoadTextFileToJsonObject(string inFilePath)
    {
        var reader = new StreamReader(inFilePath);
        string jsonAsString = reader.ReadToEnd();
        reader.Close();
        JSON jsonObject = JSON.ParseString(jsonAsString);
        return jsonObject;
    }
    
    private void CollectSaveDataFromGame()
    {
        if (this.currentSaveData == null)
            this.currentSaveData = new GameSaveData();
            
        // Update metadata
        this.currentSaveData.SaveTime = DateTime.Now;
        this.currentSaveData.TotalPlayTime += Time.time - this.sessionStartTime;
        this.sessionStartTime = Time.time;

        this.currentSaveData.InventoryDataDict = InventoryManager.IN.GetSaveData();
        this.currentSaveData.ResourcesSaveDatas = ResourceManager.IN.GetSaveData();
        this.currentSaveData.DecorationDatas = DecorationManager.IN.GetSaveData();

        this.currentSaveData.CurrentGameHour = TimeManager.IN.CurrentHour;

        this.currentSaveData.CurrentWeather = WeatherManager.IN.CurrentWeather;
        this.currentSaveData.WeatherIntensity = WeatherManager.IN.WeatherIntensity;
        
        // Settings (window position, etc.)
        CollectSettingsData();
        
        // Statistics
        CollectStatsData();
    }
    
    private void ApplySaveDataToGame()
    {
        if (currentSaveData == null)
            return;
        
        InventoryManager.IN.LoadSaveData(currentSaveData.InventoryDataDict);
        ResourceManager.IN.LoadSaveData(currentSaveData.ResourcesSaveDatas);
        DecorationManager.IN.LoadSaveData(currentSaveData.DecorationDatas);
        TimeManager.IN.SetTime(currentSaveData.CurrentGameHour);
        WeatherManager.IN.ForceWeather(currentSaveData.CurrentWeather);
        
        ApplySettingsData();
    }
    
    private void CollectSettingsData()
    {
        // Window settings (using UniWindowController)
        if (Kirurobo.UniWindowController.current != null)
        {
            var controller = Kirurobo.UniWindowController.current;
            currentSaveData.SettingsData.WindowTransparency = controller.isTransparent ? 0.8f : 1f;
            currentSaveData.SettingsData.AlwaysOnTop = controller.isTopmost;
        }
        
        // Audio settings (placeholder - implement when audio system is added)
        // Time scale
        if (TimeManager.IN != null)
            currentSaveData.SettingsData.TimeScale = 1f; // Will be implemented
    }
    
    private void ApplySettingsData()
    {
        // Window settings
        if (Kirurobo.UniWindowController.current != null)
        {
            var controller = Kirurobo.UniWindowController.current;
            controller.isTransparent = currentSaveData.SettingsData.WindowTransparency < 1f;
            controller.isTopmost = currentSaveData.SettingsData.AlwaysOnTop;
        }
    }

    private void CollectStatsData()
    {
        // This will be expanded as we track more statistics
        if (currentSaveData.StatsData == null)
            currentSaveData.StatsData = new GameStatsData();

        currentSaveData.StatsData.SessionsPlayed++;
    }

    [Button(ButtonSizes.Large)]
    private void NukeSaveFile()
    {
        Awake();
        DeleteSave();
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

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif          
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
        if (currentSaveData?.StatsData == null) return;
        
        currentSaveData.StatsData.TotalResourcesCollected += amount;
        
        switch (type)
        {
            case ResourceType.Water:
                currentSaveData.StatsData.WaterCollected += amount;
                break;
            case ResourceType.Seeds:
                currentSaveData.StatsData.SeedsCollected += amount;
                break;
            case ResourceType.Gems:
                currentSaveData.StatsData.GemsCollected += amount;
                break;
        }
    }
    
    public void RecordDecorationPlaced()
    {
        if (currentSaveData?.StatsData != null)
            currentSaveData.StatsData.DecorationsPlaced++;
    }

    public void RecordRareEvent(ResourceType eventType)
    {
        if (currentSaveData?.StatsData == null) return;

        currentSaveData.StatsData.RareEventsWitnessed++;

        if (eventType == ResourceType.UnicornBlessing)
            currentSaveData.StatsData.UnicornEncounters++;
        else if (eventType == ResourceType.MermaidSong)
            currentSaveData.StatsData.MermaidEncounters++;
    }
    
    public void HandeDeleteDataButtonPress()
    {
        UIConfirmPanel.IN.Show("Delete Save Data", "Are you sure you want to delete all save data?\nThis action cannot be undone.\nThis will also quit the game.", () =>
        {
            DeleteSave();

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif  
        });
    }
}