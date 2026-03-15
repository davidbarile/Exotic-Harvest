using System;

[Flags]
public enum EResourceType
{
    None = 0,
    Dew = 1 << 0,
    Rain = 1 << 1,
    // Bugs
    Caterpillars = 1 << 2,
    Butterflies = 1 << 3,
    Dragonflies = 1 << 4,
    Bees = 1 << 5,
    Crickets = 1 << 6,
    Fireflies = 1 << 7,
    Ladybugs = 1 << 8,
    // Nature
    Seeds = 1 << 9,
    Clovers = 1 << 10,
    FourLeafClovers = 1 << 11,
    Nuts = 1 << 12,
    Berries = 1 << 13,
    Feathers = 1 << 14,
    Shells = 1 << 15,
    TreeSap = 1 << 16,
    Nectar = 1 << 17,
    Pollen = 1 << 18,
    // Night Sky
    Moonbeams = 1 << 19,
    Stardust = 1 << 20,
    Comets = 1 << 21,
    FallingStars = 1 << 22,
    Planets = 1 << 23,
    // Valuables
    Gems = 1 << 24,
    Gold = 1 << 25,
    Jewelry = 1 << 26,
    RareRelics = 1 << 27,

    // Abstract
    Secrets = 1 << 28,
    Shadows = 1 << 29,
    Memories = 1 << 30,
    Lullabies = 1 << 31,

    // Special Events
    UnicornBlessing = 1 << 32,
    MermaidSong = 1 << 33,

    // Hard Currency (Premium)
    PremiumCurrency = 1 << 34
}