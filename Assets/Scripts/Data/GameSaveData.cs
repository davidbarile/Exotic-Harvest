using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Complete save data structure for the game
/// </summary>
[System.Serializable]
public class GameSaveData
{
    [Header("Save Metadata")]
    public string saveVersion = "1.0";
    public DateTime saveTime;
    public float totalPlayTime = 0f;
    
    [Header("Player Progress")]
    public int playerLevel = 1;
    public float experience = 0f;
    
    [Header("Resources")]
    public ResourceData resources = new ResourceData();
    
    [Header("Decorations")]
    public List<DecorationData> decorations = new List<DecorationData>();
    
    [Header("Settings")]
    public GameSettingsData settings = new GameSettingsData();
    
    [Header("Time & Weather")]
    public float currentGameHour = 8f;
    public WeatherType currentWeather = WeatherType.Clear;
    public float weatherIntensity = 0.5f;
    
    [Header("Statistics")]
    public GameStatsData stats = new GameStatsData();
    
    public GameSaveData()
    {
        saveTime = DateTime.Now;
    }
}