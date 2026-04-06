using System;

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
    Planter = 1 << 14,
    Stool = 1 << 15,
    Sign = 1 << 16,
    PetHome = 1 << 17,
    Plant = 1 << 20,
    Pet = 1 << 21,
    All = ~0
}