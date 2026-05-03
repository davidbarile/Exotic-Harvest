using System;
using System.Collections.Generic;
using UnityEngine;
using static GlobalEnums;

/// <summary>
/// Complete save data structure for the game
/// </summary>
[Serializable]
public class GameSaveData
{
    [Header("Save Metadata")]
    public string SaveVersion = "1.0";
    public DateTime SaveTime;
    public float TotalPlayTime = 0f;
    
    [Header("Player Progress")]
    public int PlayerLevel = 1;
    public float Experience = 0f;

    [Header("Inventory")]
    public InventoryItemData[] InventoryItems = new InventoryItemData[InventoryManager.NumInventorySlots];

    [Header("World Items")]
    public List<InventoryItemData> WorldItems = new();
    
    [Header("Resources")]
    public ResourceSaveData ResourcesSaveDatas = new();

    [Header("Settings")]
    public GameSettingsData SettingsData = new();
    public Color PanelColor = Color.black;

    [Header("Time & Weather")]
    public float CurrentGameHour = 8f;
    public EWeatherType CurrentWeather = EWeatherType.Clear;
    public float WeatherIntensity = 0.5f;

    public bool UseRealTime;// If true, time advances based on real seconds, otherwise uses SecondTick for testing
    public float TimeScale = 1f;

    [Header("Statistics")]
    public GameStatsData StatsData = new();

    [Header("Audio")]
    public float MusicVolume = 1f;
    public float EffectsVolume = 1f;
    public float AmbientVolume = 1f;

    [Space]
    public float MusicVolume_Minimized = 0.1f;
    public float AmbientVolume_Minimized = 0.1f;
    public float EffectsVolume_Minimized = 0.1f;

    [Header("Screen")]
    public bool ShowTimeWeatherPanel = false;
    public bool ShowPanelsButtons = false;
    public bool ShowNotifications = false;
    public bool ShowDecorations = false;
    public bool ShowSunAndMoon = false;
    public bool ShowClouds = false;
    public bool ShowMountains = false;
    public float BgAlpha = .5f;

    [Header("Debug")]
    public bool DebugGrantAllResources;
    public bool FreezeTime;

    public GameSaveData()
    {
        SaveTime = DateTime.Now;
    }
    
    public static GameSaveData ConvertFrom_v1(GameSaveData_v1 oldData, GameSaveData newData)
    {
        newData.SaveVersion = oldData.SaveVersion;
        newData.SaveTime = oldData.SaveTime;
        newData.TotalPlayTime = oldData.TotalPlayTime;
        newData.PlayerLevel = oldData.PlayerLevel;
        newData.Experience = oldData.Experience;
        newData.InventoryItems = oldData.InventoryItems;
        newData.WorldItems = oldData.WorldItems;
        newData.ResourcesSaveDatas = oldData.ResourcesSaveDatas;
        newData.SettingsData = oldData.SettingsData;
        newData.PanelColor = oldData.PanelColor;
        newData.CurrentGameHour = oldData.CurrentGameHour;
        newData.CurrentWeather = oldData.CurrentWeather;
        newData.WeatherIntensity = oldData.WeatherIntensity;
        newData.StatsData = oldData.StatsData;
        newData.MusicVolume = oldData.MusicVolume;
        newData.EffectsVolume = oldData.EffectsVolume;
        newData.AmbientVolume = oldData.AmbientVolume;
        newData.MusicVolume_Minimized = oldData.MusicVolume_Minimized;
        newData.AmbientVolume_Minimized = oldData.AmbientVolume_Minimized;
        newData.EffectsVolume_Minimized = oldData.EffectsVolume_Minimized;
        newData.ShowTimeWeatherPanel = oldData.ShowTimeWeatherPanel;
        newData.ShowPanelsButtons = oldData.ShowPanelsButtons;
        newData.ShowNotifications = oldData.ShowNotifications;
        newData.ShowDecorations = oldData.ShowDecorations;
        newData.ShowSunAndMoon = oldData.ShowSunAndMoon;
        newData.ShowClouds = oldData.ShowClouds;
        newData.ShowMountains = oldData.ShowMountains;
        newData.BgAlpha = oldData.BgAlpha;
        newData.DebugGrantAllResources = oldData.DebugGrantAllResources;
        newData.FreezeTime = oldData.FreezeTime;
        return newData;
    }
}