using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MagnifyingGlass))]
public class UiWorldItemMagnifyingGlass : UiDecorationBase
{
    [SerializeField] private MagnifyingGlass linkedMagnifyingGlass;

    private void Awake()
    {
        this.linkedMagnifyingGlass = GetComponent<MagnifyingGlass>();
    }

    protected override bool DoOnDrag()
    {
        this.linkedMagnifyingGlass.ScrollInnerWorld();//dunno maybe this should all be in here

        if (!base.DoOnDrag())
        {
            return false;
        }

        return true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.TryGetComponent<Collectable>(out var collectible))
            {
                if (!this.linkedMagnifyingGlass.CollectableResourceTypes.Contains(collectible.ResourceType))
                    return;

                if (!collectible.CollectionMethod.HasFlag(ECollectionMethod.DragCollector))
                    return;

                // var success = this.linkedMagnifyingGlass.AddAmount(collectible.Amount);

                // if (success)
                //     collectible.Collect(false);
            }
        }
    }
}
