using UnityEngine;

public class Bucket : ForagerBase
{     
    protected override bool CheckGenerationConditions()
    {
        return WeatherManager.IsRaining;
    }
    
    protected override int GetGenerationAmount()
    {
        return Mathf.RoundToInt(1 + WeatherManager.WeatherIntensity);
    }
}