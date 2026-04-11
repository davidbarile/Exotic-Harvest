public class MagnifyingGlass : PassiveHarvester
{
    protected override void Start()
    {
        base.Start();
        RefreshQuantityDisplay();
    }
    
    protected override void RefreshQuantityDisplay()
    {
        base.RefreshQuantityDisplay();
    }
    
    protected override bool CheckGenerationConditions()
    {
        // Only generate when it is not raining and it is morning
        return !WeatherManager.IN.IsRaining && TimeManager.IN.CurrentTimeOfDay.HasFlag(ETimeOfDay.Morning);
    }
}
