using System;

[Flags]
public enum EResourceCategory
{
    Primary = 1 << 0,     // Water and basic resources
    Bugs = 1 << 1,        // All insects and creatures
    Nature = 1 << 2,      // Plants, seeds, natural items
    NightSky = 1 << 3,    // Celestial and night-time resources
    Valuables = 1 << 4,   // Gems, gold, precious items
    Abstract = 1 << 5,    // Secrets, memories, intangible resources
    Special = 1 << 6,     // Rare event resources
    Premium = 1 << 7,      // Hard currency
    Beach = 1 << 8,      // Beach resources
    All = ~0
}