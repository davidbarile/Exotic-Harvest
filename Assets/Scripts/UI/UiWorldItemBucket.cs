using UnityEngine;

[RequireComponent(typeof(Bucket))]
public class UiWorldItemBucket : UiDecorationBase
{
    protected override void Awake()
    {
        this.linkedPassiveHarvester = GetComponent<Bucket>();
    }
}