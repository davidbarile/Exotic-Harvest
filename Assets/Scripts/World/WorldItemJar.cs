using UnityEngine;

[RequireComponent(typeof(Jar))]
public class WorldItemJar : DecorationBase
{
    protected override void Awake()
    {
        this.linkedPassiveHarvester = GetComponent<Jar>();
    }
}