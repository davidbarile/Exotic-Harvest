using System;
using UnityEngine;

/// <summary>
/// Serializable data for saving/loading decorations
/// </summary>
[Serializable]
public class DecorationData
{
    public DecorationType type;
    public Vector3 position;
    public bool isLocked;
    
    // For passive harvesters
    public int currentAmount;
    public float lastGenerationTime;
    public bool isActive = true;
}