/// <summary>
/// Player statistics and achievements tracking
/// </summary>
[System.Serializable]
public class GameStatsData
{
    [UnityEngine.Header("Collection Stats")]
    public int totalResourcesCollected = 0;
    public int totalActivelyForaged = 0;
    public int totalPassivelyHarvested = 0;
    
    [UnityEngine.Header("Resource Specific")]
    public int waterCollected = 0;
    public int bugsCollected = 0;
    public int seedsCollected = 0;
    public int gemsCollected = 0;
    
    [UnityEngine.Header("Decoration Stats")]
    public int decorationsPlaced = 0;
    public int decorationsMoved = 0;
    public int harvestersBuilt = 0;
    
    [UnityEngine.Header("Time Stats")]
    public int daysPlayed = 0;
    public float longestSession = 0f;
    public int sessionsPlayed = 0;
    
    [UnityEngine.Header("Special Events")]
    public int rareEventsWitnessed = 0;
    public int unicornEncounters = 0;
    public int mermaidEncounters = 0;
}