using UnityEngine;

public class Net : ForagerBase
{
    protected override bool CheckGenerationConditions()
    {
        return true;// not using this
    }
    
    protected override int GetGenerationAmount()
    {
        return 1; //Mathf.RoundToInt(1 + WeatherManager.WeatherIntensity);
    }
}