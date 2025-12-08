using System;
using UnityEngine;

/// <summary>
/// Player settings and preferences
/// </summary>
[Serializable]
public class GameSettingsData
{
    [Header("Window Settings")]
    public Vector3 WindowPosition = Vector3.zero;
    public float WindowTransparency = 0.8f;
    public bool AlwaysOnTop = true;
    
    [Header("Audio Settings")]
    public float MasterVolume = 1f;
    public float MusicVolume = 1f;
    public float SfxVolume = 1f;
    public bool MuteWhenHidden = true;
    
    [Header("Gameplay Settings")]
    public float TimeScale = 1f;
    public bool ShowNotifications = true;
    public bool AutoCollectEnabled = false;
    
    [Header("UI Settings")]
    public bool ShowDebugInfo = false;
    public bool CompactMode = false;
}