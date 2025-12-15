using System;
using UnityEngine;

[Serializable]
public class DecorationData
{
    public EDecorationType Type;
    public Vector3 WorldPosition;
    public bool IsInInventory;
    
    // For passive harvesters
    public int CurrentAmount;
    public float LastGenerationTime;
    public bool IsActive = true;
}