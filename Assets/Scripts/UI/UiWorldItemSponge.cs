using UnityEngine;

[RequireComponent(typeof(Sponge))]
public class UiWorldItemSponge : UiDecorationBase
{
    protected override void Awake()
    {
        this.linkedPassiveHarvester = GetComponent<Sponge>();
    }
}