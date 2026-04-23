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

    [Header("Statistics")]
    public GameStatsData StatsData = new();

    [Header("Audio")]
    public float MusicVolume = 1f;
    public float EffectsVolume = 1f;
    public float AmbientVolume = 1f;

    public float MusicVolume_Minimized = 0.5f;
    public float EffectsVolume_Minimized = 0.5f;
    public float AmbientVolume_Minimized = 0.5f;

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
        return newData;
    }
}