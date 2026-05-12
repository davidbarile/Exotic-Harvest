using System;
using UnityEngine;

public class Attractor : MonoBehaviour
{
    public bool IsActive = true;
    [Range(0f, 100f),SerializeField] private float strength = 1f;
    [Range(0f, 1000f), SerializeField] private float maxDistance = 100f;
    [Range(0f, 100f), SerializeField] private float collisionRadius= 5f;
    [SerializeField] private LayerMask attractableLayer;
    [SerializeField] private string tagToAttract = "Attractable";

    [SerializeField] private PassiveHarvester linkedPassiveHarvester;

    private void FixedUpdate()
    {
        if (!IsActive) return;

        if(this.linkedPassiveHarvester != null && this.linkedPassiveHarvester.IsFull)
            return;
        
        Collider2D[] attractables = new Collider2D[10];
        int count = Physics2D.OverlapCircleNonAlloc(this.transform.position, this.maxDistance, attractables, this.attractableLayer);
        for (int i = 0; i < count; i++)
        {
            Collider2D attractable = attractables[i];
            if (!attractable.CompareTag(this.tagToAttract)) continue;

            Vector2 direction = (Vector2)this.transform.position - (Vector2)attractable.transform.position;
            float distance = direction.magnitude;

            //Debug.Log($"Attracting {attractable.gameObject.name} at distance {distance}");

            if (distance > this.collisionRadius)
            {
                Rigidbody2D rb = attractable.attachedRigidbody;
                if (rb != null)
                {
                    Vector2 force = direction.normalized * this.strength * 100f / distance;
                    rb.AddForce(force);
                }
            }
            else
            {
                var collectable = attractable.GetComponent<Collectable>();
                if(collectable != null)
                {
                    if(this.linkedPassiveHarvester != null)
                    {
                        this.linkedPassiveHarvester.AddAmount(collectable.Amount);
                        collectable.OnAttracted();
                    }
                    else
                    {
                        collectable.Collect();
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, this.maxDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, this.collisionRadius);
    }
}
