using static GlobalEnums;

public class Telescope : ForagerBase
{
    protected override bool CheckGenerationConditions()
    {
        // Only generate when it is not raining and it is night
        return !WeatherManager.IsRaining && TimeManager.CurrentTimeOfDay.HasFlag(ETimeOfDay.Night);
    }
}