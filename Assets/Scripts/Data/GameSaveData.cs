using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
    public List<DecorationData> Inventory = new();
    
    [FormerlySerializedAs("ResourceDatas")] [Header("resourcesSave")]
    public ResourceSaveData resourcesSave = new();
    
    [Header("Decorations")]
    public List<DecorationData> Decorations = new();
    
    [Header("Settings")]
    public GameSettingsData Settings = new();
    
    [Header("Time & Weather")]
    public float CurrentGameHour = 8f;
    public WeatherType CurrentWeather = WeatherType.Clear;
    public float WeatherIntensity = 0.5f;
    
    [Header("Statistics")]
    public GameStatsData Stats = new();
    
    public GameSaveData()
    {
        SaveTime = DateTime.Now;
    }
}