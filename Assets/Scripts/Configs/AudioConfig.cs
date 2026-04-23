using UnityEngine;
using static GlobalEnums;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Exotic Harvest/AudioConfig")]
public class AudioConfig : ScriptableObject
{
    public ETimeOfDay TimeOfDay;
    public EWeatherType WeatherType;
    [Space] public AudioClip[] AudioClips;
}