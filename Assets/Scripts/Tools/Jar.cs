using UnityEngine;

public class Jar : PassiveHarvester
{
    protected override bool CheckGenerationConditions()
    {
        // Only generate during rain
        return WeatherManager.IsRaining;
    }

    protected override int GetGenerationAmount()
    {
        if (WeatherManager.IN != null)
        {
            // More water during heavier rain
            float intensity = WeatherManager.WeatherIntensity;
            return Mathf.RoundToInt(1 + intensity); // 1-2 water per generation
        }
        return 1;
    }
}