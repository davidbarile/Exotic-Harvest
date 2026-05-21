using static GlobalEnums;

public class MagnifyingGlass : PassiveHarvester
{
    protected override void Start()
    {
        base.Start();
        RefreshQuantityDisplay();
    }
    
    protected override bool CheckGenerationConditions()
    {
        // Only generate when it is not raining and it is night
        return !WeatherManager.IsRaining && !TimeManager.CurrentTimeOfDay.HasFlag(ETimeOfDay.Night);
    }
}
