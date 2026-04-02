using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Bucket))]
public class UiWorldItemBucket : UiDecorationBase
{
    [SerializeField] private Bucket linkedBucket;

    private void Awake()
    {
        this.linkedBucket = GetComponent<Bucket>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.TryGetComponent<Collectable>(out var collectible))
            {
                if (!this.linkedBucket.CollectableResourceTypes.Contains(collectible.ResourceType))
                    return;

                if(!collectible.CollectionMethod.HasFlag(ECollectionMethod.DragCollector))
                    return;

                var success = this.linkedBucket.AddAmount(collectible.Amount);

                if (success)
                    collectible.Collect(false);//do not add to inventory immediately, bucket will handle it on collection
            }
        }
    }
}