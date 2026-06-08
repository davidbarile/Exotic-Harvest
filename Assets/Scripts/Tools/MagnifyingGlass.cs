using static GlobalEnums;

public class MagnifyingGlass : ForagerBase
{
    public override void OnSpawn()
    {
        base.OnSpawn();
        RefreshQuantityDisplay();
    }
    
    protected override bool CheckGenerationConditions()
    {
        // Only generate when it is not raining and it is night
        return !WeatherManager.IsRaining && !TimeManager.CurrentTimeOfDay.HasFlag(ETimeOfDay.Night);
    }
}
