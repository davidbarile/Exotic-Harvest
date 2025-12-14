using System;
using UnityEngine;

/// <summary>
/// Serializable data for saving/loading decorations
/// </summary>
[Serializable]
public class DecorationData
{
    public DecorationType Type;
    public Vector3 Position;
    public bool IsInInventory;
    
    // For passive harvesters
    public int CurrentAmount;
    public float LastGenerationTime;
    public bool IsActive = true;
}