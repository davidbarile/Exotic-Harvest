using UnityEngine;

[RequireComponent(typeof(Sponge))]
public class WorldItemSponge : DecorationBase
{
    protected override void Awake()
    {
        this.linkedPassiveHarvester = GetComponent<Sponge>();
    }
}