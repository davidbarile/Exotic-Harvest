/// <summary>
/// Types of decorations that can be placed on the desktop
/// </summary>
public enum EDecorationType
{
    None = 0,

    // Passive Harvesters
    Bucket = 1,        // Collects water during rain
    Jar = 2,           // Collects fireflies in the evening
    FlowerPot = 3,     // Grows seeds into plants
    LightningRod = 4,  // Collects lightning energy
    Crystal = 5,   // Charges with moonbeams at night
    SpiderWeb = 6,     // Traps insects
    MagnifyingGlass = 7, // Focuses sunlight to generate heat
    Sponge = 8,         // Absorbs dew

    // Visual Decorations
    Plant = 20,         // Corner leaf clusters
    WindChimes = 21,    // Bamboo wind chimes
    TikiTorch = 22,     // Ambient lighting
    Fountain = 23,      // Water feature
    Mask = 24,          // Tribal decoration

    // Interactive Elements
    BirdPerch = 40,     // Attracts birds
    Terrarium = 41,     // Houses small creatures
    Mailbox = 42,       // Receives gifts/quests
    Hut = 43           // Shelter decoration
}