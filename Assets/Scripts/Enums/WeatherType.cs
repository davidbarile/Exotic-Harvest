using System;

[Flags]
public enum WeatherType
{
    Clear = 1 << 0,
    Rain = 1 << 1,
    Storm = 1 << 2,
    Snow = 1 << 3,
    Wind = 1 << 4,
    Foggy = 1 << 5,
    All = ~0
}