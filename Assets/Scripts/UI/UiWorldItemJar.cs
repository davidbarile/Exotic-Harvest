using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Jar))]
public class UiWorldItemJar : UiDecorationBase
{
    [SerializeField] private Jar linkedJar;

    private void Awake()
    {
        this.linkedJar = GetComponent<Jar>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if(collision.TryGetComponent<Collectable>(out var collectible))
            {
                if (!this.linkedJar.CollectableResourceTypes.Contains(collectible.ResourceType))
                    return;

                var success = this.linkedJar.AddAmount(collectible.Amount);

                if (success)
                    collectible.Collect(false);
            }
        }
    }
}