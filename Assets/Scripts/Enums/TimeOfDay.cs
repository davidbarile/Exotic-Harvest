using System;

[Flags]
public enum TimeOfDay
{
    Morning = 1 << 0,   // 6-12: Dewdrops, early birds, nectar
    Afternoon = 1 << 1, // 12-18: Seeds, berries, bugs, full sunlight
    Evening = 1 << 2,   // 18-22: Fireflies, sunset pollen bloom
    Night = 1 << 3,      // 22-6: Moonbeams, stardust, falling stars
    All = ~0
}