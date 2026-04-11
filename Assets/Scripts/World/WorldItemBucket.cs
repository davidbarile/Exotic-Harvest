using UnityEngine;

[RequireComponent(typeof(Bucket))]
public class WorldItemBucket : DecorationBase
{
    protected override void Awake()
    {
        this.linkedPassiveHarvester = GetComponent<Bucket>();
    }
}