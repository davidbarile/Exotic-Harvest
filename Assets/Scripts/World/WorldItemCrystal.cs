using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Crystal))]
public class WorldItemCrystal : DecorationBase
{
    protected override void Awake()
    {
        this.linkedPassiveHarvester = GetComponent<Crystal>();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // if (collision != null)
        // {
        //     if (collision.TryGetComponent<Collectable>(out var collectible))
        //     {
        //         if (!this.linkedPassiveHarvester.CollectableResourceTypes.Contains(collectible.ResourceType))
        //             return;

        //this.startActiveObjectHoverTime = DateTime.Now;

        //         var success = this.linkedPassiveHarvester.AddAmount(collectible.Amount);

        //         if (success)
        //             collectible.Collect(false);
        //     }
        // }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision != null)
        {
            if(collision.TryGetComponent<Collectable>(out var collectible))
            {
                if (!this.linkedPassiveHarvester.CollectableResourceTypes.Contains(collectible.ResourceType))
                    return;

                Debug.Log($"WorldItemCrystal.OnTriggerStay2D()   Collecting {collectible.ResourceType}   Amount: {collectible.Amount}.  frame = {Time.frameCount}");

                // var success = this.linkedPassiveHarvester.AddAmount(collectible.Amount);

                // if (success)
                //     collectible.Collect(false);
            }
        }
    }
}