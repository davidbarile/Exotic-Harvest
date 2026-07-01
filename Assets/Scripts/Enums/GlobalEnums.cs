using System;

public class GlobalEnums
{
    public enum EListernerType
    {
        OnGameFocusChanged,
        OnMinimizeMaximizeToggled,
        Both
    }

    public enum EColorType
    {
        Main,
        Dark,
        Light,
        Disabled
    }
    
    [Flags]
    public enum ETimeOfDay
    {
        Morning = 1 << 0,   // 6-12: Dewdrops, early birds, nectar
        Afternoon = 1 << 1, // 12-18: Seeds, berries, bugs, full sunlight
        Evening = 1 << 2,   // 18-22: Fireflies, sunset pollen bloom
        Night = 1 << 3,      // 22-6: Moonbeams, stardust, falling stars
        All = ~0
    }

    [Flags]
    public enum EDayOfWeek
    {
        None = 0,
        Sunday = 1 << 0,
        Monday = 1 << 1,
        Tuesday = 1 << 2,
        Wednesday = 1 << 3,
        Thursday = 1 << 4,
        Friday = 1 << 5,
        Saturday = 1 << 6,
        Weekday = Monday | Tuesday | Wednesday | Thursday | Friday,
        Weekend = Saturday | Sunday,
        All = ~0
    }

    public static EDayOfWeek GetDayOfWeekFromInt(int inDayNum)
    {
        var dayNum = inDayNum % 7; // Ensure it's within 0-6
        
        switch (dayNum)
        {
            case 0: return EDayOfWeek.Sunday;
            case 1: return EDayOfWeek.Monday;
            case 2: return EDayOfWeek.Tuesday;
            case 3: return EDayOfWeek.Wednesday;
            case 4: return EDayOfWeek.Thursday;
            case 5: return EDayOfWeek.Friday;
            case 6: return EDayOfWeek.Saturday;
            default:
                return EDayOfWeek.None; // Default fallback
        }
    }

    [Flags]
    public enum EWeatherType
    {
        Clear = 1 << 0,
        Rain = 1 << 1,
        Storm = 1 << 2,
        Snow = 1 << 3,
        Wind = 1 << 4,
        Foggy = 1 << 5,
        All = ~0
    }

    public enum EItemCategory
    {
        Decorations,
        Resources,
        Tools,
        Special
    }

    public enum EShopCategory
    {
        Tools,
        Decorations,
        Resources,
        Pets,
        Upgrades,
        Special,
        All
    }

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

    public enum ELootType
    {
        None,
        RockPile,
        NightSky,
        Meadow,
        Wind
    }

    [Flags]
    public enum ECollectionMethod
    {
        Click = 1 << 0,        // Click to collect (dewdrops, seeds)
        Hover = 1 << 1,       // Hover over to collect (raindrops)
        Drag = 1 << 2,         // Drag across screen (bucket for raindrops)
        Swipe = 1 << 3,        // Net swiping (butterflies, fireflies)
        Hold = 1 << 4,         // Click and hold (mining rocks, digging)
        Interact = 1 << 5,      // Special interaction (pollination, etc.)
        DragCollector = 1 << 6 // Drag a collector item over the resource to collect (e.g., using a jar to collect fireflies)
    }

    public enum EHarvestLocation
    {
        None,
        MeadowDew,
        MeadowSearch,
        Beach,
        Marsh,
        Jungle,
        NightSky,
        NightSkySearch,
        Windy,
        Rainy,
        Stormy
    }

    [Flags]
    public enum EDecorationType
    {
        None = 0,
        Tool = 1 << 0,
        PassiveHarvester = 1 << 1,
        Decoration = 1 << 2,
        Furniture = 1 << 3,
        Bucket = 1 << 10,
        Jar = 1 << 11,
        Crystal = 1 << 12,
        Sponge = 1 << 13,
        Telescope = 1 << 14,
        Planter = 1 << 15,
        Stool = 1 << 16,
        Sign = 1 << 17,
        PetHome = 1 << 18,
        Plantable = 1 << 19,
        Pet = 1 << 20,
        HangingTop = 1 << 21,
        FloorMount = 1 << 22,
        SideMountLeft = 1 << 23,
        SideMountRight = 1 << 24,
        ScreenSpace = 1 << 25,
        WorldSpace = 1 << 26,
        Net = 1 << 27,
        All = ~0
    }

    public enum ENotificationType
    {
        ResourceGained,     // Collected resources
        ResourceLost,       // Spent resources
        ItemPurchased,      // Shop purchases
        DecorationPlaced,   // Decoration events
        InventoryFull,      // Inventory warnings
        WeatherChanged,     // Weather events
        TimeChanged,        // Time of day events
        Achievement,        // Achievements unlocked
        Error,              // Error messages
        Info,               // General information
        Success,            // Success confirmations
        Warning             // Warning messages
    }

    public enum EResourceType
    {
        None = 0,
        Dew = 1,
        Rain = 2,

        // Bugs
        Caterpillars = 20,
        Butterflies = 21,
        Dragonflies = 22,
        Bees = 23,
        Crickets = 24,
        Fireflies = 25,
        Ladybugs = 26,
        Snails = 27,

        // Nature
        Seeds = 40,
        Clovers = 41,
        FourLeafClovers = 42,
        Nuts = 43,
        Berries = 44,
        Feathers = 45,
        Shells = 46,
        Oysters = 47,
        Nectar = 48,
        Pollen = 49,
        Herbs = 50,
        Toadstools = 51,
        Dandelions = 52,

        // Night Sky
        Moonbeams = 60,
        Stardust = 61,
        Comets = 62,
        FallingStars = 63,
        Planets = 64,
        Asteroids = 65,
        Meteors = 66,
        Lightning = 70,

        // Valuables
        Pearls = 80,
        Gems = 81,
        Gold = 82,
        Jewelry = 83,
        Diamonds = 84,
        Keys = 85,

        // Abstract
        Secrets = 100,
        Memories = 101,
        Dreams = 102,

        GoldenApples = 200, // For premium currency if needed
    }
}