using UnityEngine;
using static GlobalEnums;

public class Telescope : PassiveHarvester
{
    protected override void Start()
    {
        base.Start();
        RefreshQuantityDisplay();
    }
    
    protected override bool CheckGenerationConditions()
    {
        // Only generate when it is not raining and it is night
        return !WeatherManager.IN.IsRaining && TimeManager.IN.CurrentTimeOfDay.HasFlag(ETimeOfDay.Night);
    }
}