using UnityEngine;

[RequireComponent(typeof(Crystal))]
public class WorldItemCrystal : DecorationBase
{
    protected override void Awake()
    {
        this.linkedPassiveHarvester = GetComponent<Crystal>();
    }
}