using System;
using System.IO;
using UnityEngine;
using Sirenix.OdinInspector;
using Leguar.TotalJSON;
using static GlobalEnums;

/// <summary>
/// Manages saving and loading game data
/// </summary>
public class SaveManager : MonoBehaviour, ITickable
{
    public static SaveManager IN;

    public static GameSaveData Data;

    // Events
    public static Action OnGameSaved;
    public static Action OnGameLoaded;
    public static Action<string> OnSaveError;
    public static Action<string> OnLoadError;
    
    [Header("Save Settings")]
    [SerializeField] private string saveFileName = "exotic_harvest_save.json";
    [SerializeField] private bool autoSaveEnabled = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5 minutes
    [SerializeField] private bool saveOnApplicationPause = true;
    [SerializeField] private bool nukeDataOnStart;

    [SerializeField] private int currentPlayerDataVersion = 1; // Increment this when making breaking changes to save data structure to trigger new save creation
    
    private string savePath;
    
    private float autoSaveTimer = 0f;
    private float sessionStartTime;
    private bool isDeletingSave;
    
    // Properties
    public bool HasSaveFile => File.Exists(savePath);

    public void Init()
    {
        var saveFolder = Application.persistentDataPath;

        Debug.Log($"Save folder path: {saveFolder}");

#if UNITY_EDITOR
        saveFolder = "Assets/PlayerData";
#endif

        this.savePath = Path.Combine(saveFolder, this.saveFileName);
        this.sessionStartTime = Time.time;

        if(this.nukeDataOnStart)
            PlayerPrefs.DeleteAll();

        var isNewGame = !this.HasSaveFile || this.nukeDataOnStart;

        var gamePlayerDataVersion = PlayerPrefs.GetInt("PlayerDataVersion", 0);

        if (gamePlayerDataVersion == 0)
        {
            CreateNewSave();
            DecorationManager.IN.InitDecorationsInWorld(isNewGame);
        }
        else
        {
            DecorationManager.IN.InitDecorationsInWorld(isNewGame);
            LoadGame();
        }

        PlayerPrefs.SetInt("PlayerDataVersion", this.currentPlayerDataVersion);

        TickManager.OnSecondTick += SecondTick;
    }

    private void OnDestroy()
    {
        TickManager.OnSecondTick -= SecondTick;
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
        if (this.isDeletingSave) return false; // Don't save if we're in the process of deleting the save file
         
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
        bool isGameDataOutOfDate = false;
        try
        {
            if (!this.HasSaveFile)
            {
                Debug.Log("No save file found, creating new save");
                CreateNewSave();
            }
            else
            {
                var gamePlayerDataVersion = PlayerPrefs.GetInt("PlayerDataVersion", 0);
                isGameDataOutOfDate = gamePlayerDataVersion < this.currentPlayerDataVersion;
            }

            var deserializeSettings = new DeserializeSettings()
            {
                RequireAllFieldsArePopulated = false
            };

            JSON savedMapProgressDataJSON = LoadTextFileToJsonObject(this.savePath);

            if (isGameDataOutOfDate)
            {
                Debug.Log("Save data version is outdated. Creating new save data with default values and applying any compatible data from old save.");

                // Try to populate compatible fields from old save
                var oldData = savedMapProgressDataJSON.Deserialize<GameSaveData_v1>(deserializeSettings);

                if (oldData == null)
                    throw new Exception("Failed to deserialize GameSaveData_v1 data");

                if (oldData != null)
                {
                    Data = GameSaveData.ConvertFrom_v1(oldData, new GameSaveData());
                    //add new variables here with default values

                    // NOTE: if changes are made to any serialized classes or enums, it will still break
                    // need to do a similar conversion process for any serialized classes or enums that have breaking changes as well 
                    // (ResourceSaveData, InventoryItemData, GameSettingsData, GameStatsData, EWeatherType)
                }
            }
            else
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

        Data.ResourcesSaveDatas = ResourceManager.IN.GetSaveData();
        //Data.WorldItems = DecorationManager.IN.GetSaveData();

        Data.CurrentGameHour = TimeManager.CurrentHour;

        Data.CurrentWeather = WeatherManager.CurrentWeather;
        Data.WeatherIntensity = WeatherManager.WeatherIntensity;

        Data.PanelColor = ColorManager.IN.PanelColor;
        
        // Settings (window position, etc.)
        CollectSettingsData();
        
        // Statistics
        CollectStatsData();
    }
    
    private void ApplySaveDataToGame()
    {
        InventoryManager.IN.ApplyAllInventory(Data.InventoryItems);
        ResourceManager.IN.ApplyFromSaveData(Data.ResourcesSaveDatas);
        DecorationManager.IN.ApplyFromSaveData(Data.WorldItems);

        TimeManager.IN.SetTime(Data.CurrentGameHour);
        TimeManager.IN.ToggleRealTime(Data.UseRealTime);
        TimeManager.IN.SetTimeScale(Data.TimeScale);

        WeatherManager.IN.ForceWeather(Data.CurrentWeather);

        ColorManager.IN.PanelColor = Data.PanelColor;
        ColorManager.OnPanelColorChanged?.Invoke(Data.PanelColor);

        UiManager.IN.SettingsPanel.ApplySavedColorsToMenus(Data.PanelColor);
        UiManager.IN.SettingsPanel.ApplySaveDataToUI();

        AudioManager.IN.ApplyAudioSettingsFromSaveData();
        
        ApplySettingsData();
    }
    
    private void CollectSettingsData()
    {
#if UNITY_STANDALONE
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
#endif
    }
    
    private void ApplySettingsData()
    {
#if UNITY_STANDALONE
        // Window settings
        if (Kirurobo.UniWindowController.current != null)
        {
            var controller = Kirurobo.UniWindowController.current;
            controller.isTransparent = Data.SettingsData.WindowTransparency < 1f;
            controller.isTopmost = Data.SettingsData.AlwaysOnTop;
        }
#endif
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
        this.isDeletingSave = true; // Set flag to prevent saving during deletion process

        var saveFolder = Application.persistentDataPath;

#if UNITY_EDITOR
        saveFolder = "Assets/PlayerData";
#endif

        this.savePath = Path.Combine(saveFolder, this.saveFileName);
        DeleteSave();
    }

    public bool DeleteSave()
    {
        PlayerPrefs.DeleteAll();

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
    
    public void HandleDeleteDataButtonPress()
    {
        UIConfirmPanel.IN.Show("Delete Save Data", "Are you sure you want to delete all save data?\nThis action cannot be undone.\nThis will also quit the game.", () =>
        {
            this.isDeletingSave = true; // Set flag to prevent saving during deletion process
            
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