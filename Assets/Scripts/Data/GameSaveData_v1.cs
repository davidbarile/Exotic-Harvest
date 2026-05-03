using System;
using System.Collections.Generic;
using UnityEngine;
using static GlobalEnums;

/// <summary>
/// Archive version of GameSaveData for loading older save files. Do not add new fields to this class. 
/// If you need to add new fields, create a new version of GameSaveData and implement conversion logic in SaveManager.
/// </summary>
[Serializable]
public class GameSaveData_v1
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

    [Space]
    public float MusicVolume_Minimized = 0.1f;
    public float AmbientVolume_Minimized = 0.1f;
    public float EffectsVolume_Minimized = 0.1f;

    [Header("Screen")]
    public bool ShowTimeWeatherPanel;
    public bool ShowPanelsButtons;
    public bool ShowNotifications;
    public bool ShowDecorations;
    public bool ShowSunAndMoon;
    public bool ShowClouds;
    public bool ShowMountains;
    public float BgAlpha;

    [Header("Debug")]
    public bool DebugGrantAllResources;
    public bool FreezeTime;

    public GameSaveData_v1()
    {

    }
}