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

    public static GameSaveData Data;
    
    [Header("Save Settings")]
    [SerializeField] private string saveFileName = "exotic_harvest_save.json";
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5 minutes
    [SerializeField] private bool saveOnApplicationPause = true;
    
    private string savePath;
    
    private float autoSaveTimer = 0f;
    private float sessionStartTime;
    
    // Events
    public static event Action OnGameSaved;
    public static event Action OnGameLoaded;
    public static event Action<string> OnSaveError;
    public static event Action<string> OnLoadError;
    
    // Properties
    public bool HasSaveFile => File.Exists(savePath);
    
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
        var isNewGame = !this.HasSaveFile;

        if (isNewGame)
        {
            CreateNewSave();
            DecorationManager.IN.InitDecorationsInWorld(isNewGame);
        }
        else
        {
            DecorationManager.IN.InitDecorationsInWorld(isNewGame);
            LoadGame();
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
        Data = new GameSaveData();
        ApplySaveDataToGame();
    }
    
    public bool SaveGame()
    {
        try
        {
            // Update save data from current game state
            CollectSaveDataFromGame();

            JSON json = JSON.Serialize(Data);

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
            if (!this.HasSaveFile)
            {
                Debug.Log("No save file found, creating new save");
                CreateNewSave();
            }

            DeserializeSettings deserializeSettings = new DeserializeSettings()
            {
                RequireAllFieldsArePopulated = false
            };

            JSON savedMapProgressDataJSON = LoadTextFileToJsonObject(this.savePath);
            Data = savedMapProgressDataJSON.Deserialize<GameSaveData>(deserializeSettings);

            if (Data == null)
                throw new Exception("Failed to deserialize save data");

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
        if (Data == null)
            Data = new GameSaveData();
            
        // Update metadata
        Data.SaveTime = DateTime.Now;
        Data.TotalPlayTime += Time.time - this.sessionStartTime;
        this.sessionStartTime = Time.time;

        Data.InventoryDataDict = InventoryManager.IN.GetSaveData();
        Data.ResourcesSaveDatas = ResourceManager.IN.GetSaveData();
        //Data.WorldItems = DecorationManager.IN.GetSaveData();

        Data.CurrentGameHour = TimeManager.IN.CurrentHour;

        Data.CurrentWeather = WeatherManager.IN.CurrentWeather;
        Data.WeatherIntensity = WeatherManager.IN.WeatherIntensity;
        
        // Settings (window position, etc.)
        CollectSettingsData();
        
        // Statistics
        CollectStatsData();
    }
    
    private void ApplySaveDataToGame()
    {
        InventoryManager.IN.CreateDictFromSaveData(Data.InventoryDataDict);
        InventoryManager.IN.LoadAllInventory(Data.AllInventoryItems);
        ResourceManager.IN.LoadFromSaveData(Data.ResourcesSaveDatas);
        DecorationManager.IN.LoadFromSaveData(Data.WorldItems);

        TimeManager.IN.SetTime(Data.CurrentGameHour);
        WeatherManager.IN.ForceWeather(Data.CurrentWeather);
        
        ApplySettingsData();
    }
    
    private void CollectSettingsData()
    {
        // Window settings (using UniWindowController)
        if (Kirurobo.UniWindowController.current != null)
        {
            var controller = Kirurobo.UniWindowController.current;
            Data.SettingsData.WindowTransparency = controller.isTransparent ? 0.8f : 1f;
            Data.SettingsData.AlwaysOnTop = controller.isTopmost;
        }
        
        // Audio settings (placeholder - implement when audio system is added)
        // Time scale
        Data.SettingsData.TimeScale = 1f; // Will be implemented
    }
    
    private void ApplySettingsData()
    {
        // Window settings
        if (Kirurobo.UniWindowController.current != null)
        {
            var controller = Kirurobo.UniWindowController.current;
            controller.isTransparent = Data.SettingsData.WindowTransparency < 1f;
            controller.isTopmost = Data.SettingsData.AlwaysOnTop;
        }
    }

    private void CollectStatsData()
    {
        // This will be expanded as we track more statistics
        if (Data.StatsData == null)
            Data.StatsData = new GameStatsData();

        Data.StatsData.SessionsPlayed++;
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
                Data = null;
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
    public void RecordResourceCollected(EResourceType type, int amount)
    {
        if (Data?.StatsData == null) return;
        
        Data.StatsData.TotalResourcesCollected += amount;
        
        switch (type)
        {
            case EResourceType.Rain:
                Data.StatsData.WaterCollected += amount;
                break;
            case EResourceType.Seeds:
                Data.StatsData.SeedsCollected += amount;
                break;
            case EResourceType.Gems:
                Data.StatsData.GemsCollected += amount;
                break;
        }
    }
    
    public void RecordDecorationPlaced()
    {
        if (Data?.StatsData != null)
            Data.StatsData.DecorationsPlaced++;
    }

    public void RecordRareEvent(EResourceType eventType)
    {
        if (Data?.StatsData == null) return;

        Data.StatsData.RareEventsWitnessed++;

        if (eventType.HasFlag(EResourceType.UnicornBlessing))
            Data.StatsData.UnicornEncounters++;
        else if (eventType.HasFlag(EResourceType.MermaidSong))
            Data.StatsData.MermaidEncounters++;
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