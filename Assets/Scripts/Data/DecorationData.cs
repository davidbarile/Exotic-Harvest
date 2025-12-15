using System;
using UnityEngine;

[Serializable]
public class DecorationData
{
    public EDecorationType Type;
    public Vector3 WorldPosition;
    public int ParentGuid; // For decorations that are children of others;
    
    // For passive harvesters
    public int CurrentAmount;
    public float LastGenerationTime;
    public bool IsActive = true;
}