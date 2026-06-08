using UnityEngine;

[RequireComponent(typeof(Bucket))]
public class WorldItemBucket : DecorationBase
{
    protected override void Awake()
    {
        this.linkedForager = GetComponent<Bucket>();
    }
}