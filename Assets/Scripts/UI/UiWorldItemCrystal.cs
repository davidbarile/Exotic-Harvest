using UnityEngine;

[RequireComponent(typeof(Crystal))]
public class UiWorldItemCrystal : UiDecorationBase
{
    protected override void Awake()
    {
        this.linkedPassiveHarvester = GetComponent<Crystal>();
    }
}