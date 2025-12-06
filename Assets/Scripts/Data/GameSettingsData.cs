using System;
using UnityEngine;

/// <summary>
/// Player settings and preferences
/// </summary>
[Serializable]
public class GameSettingsData
{
    [Header("Window Settings")]
    public Vector3 windowPosition = Vector3.zero;
    public float windowTransparency = 0.8f;
    public bool alwaysOnTop = true;
    
    [Header("Audio Settings")]
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public bool muteWhenHidden = true;
    
    [Header("Gameplay Settings")]
    public float timeScale = 1f;
    public bool showNotifications = true;
    public bool autoCollectEnabled = false;
    
    [Header("UI Settings")]
    public bool showDebugInfo = false;
    public bool compactMode = false;
}