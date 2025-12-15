using System;
using System.Collections.Generic;
using UnityEngine;

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
    public Dictionary<string, InventoryItemData[]> InventoryDataDict = new();
    public InventoryItemData[] AllInventoryItems = new InventoryItemData[InventoryManager.NumInventorySlots];

    [Header("World Items")]
    public List<InventoryItemData> WorldItems = new();
    
    [Header("Resources")]
    public ResourceSaveData ResourcesSaveDatas = new();
    
    [Header("Decorations")]
    public List<DecorationData> DecorationDatas = new();
    
    [Header("Settings")]
    public GameSettingsData SettingsData = new();
    
    [Header("Time & Weather")]
    public float CurrentGameHour = 8f;
    public EWeatherType CurrentWeather = EWeatherType.Clear;
    public float WeatherIntensity = 0.5f;
    
    [Header("Statistics")]
    public GameStatsData StatsData = new();
    
    public GameSaveData()
    {
        SaveTime = DateTime.Now;
    }
}