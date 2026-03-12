using System;
using UnityEngine;


/// <summary>
/// Player statistics and achievements tracking
/// </summary>
[Serializable]
public class GameStatsData
{
    [Header("Collection Stats")]
    public int TotalResourcesCollected = 0;
    public int TotalActivelyForaged = 0;
    public int TotalPassivelyHarvested = 0;
    
    [Header("ResourceData Specific")]
    public int WaterCollected = 0;
    public int BugsCollected = 0;
    public int SeedsCollected = 0;
    public int GemsCollected = 0;
    
    [Header("Decoration Stats")]
    public int DecorationsPlaced = 0;
    public int DecorationsMoved = 0;
    public int HarvestersBuilt = 0;
    
    [Header("Time Stats")]
    public int DaysPlayed = 0;
    public float LongestSession = 0f;
    public int SessionsPlayed = 0;
    
    [Header("Special Events")]
    public int RareEventsWitnessed = 0;
    public int UnicornEncounters = 0;
    public int MermaidEncounters = 0;
}