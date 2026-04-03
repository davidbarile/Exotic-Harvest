using UnityEngine;

[RequireComponent(typeof(Jar))]
public class UiWorldItemJar : UiDecorationBase
{
    protected override void Awake()
    {
        this.linkedPassiveHarvester = GetComponent<Jar>();
    }
}