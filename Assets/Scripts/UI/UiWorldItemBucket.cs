using UnityEngine;

[RequireComponent(typeof(Bucket))]
public class UiWorldItemBucket : UiWorldItemBase
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
            if(collision.TryGetComponent<Collectable>(out var collectible))
            {
                if (!this.linkedBucket.CollectableResourceTypes.HasFlag(collectible.ResourceType))
                    return;

                var success = this.linkedBucket.AddAmount(collectible.Amount);

                print($"success: {success}");

                if (success)
                    collectible.Collect();
            }
        }
    }
}