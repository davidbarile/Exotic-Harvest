using System;
using UnityEngine;

public class Attractor : MonoBehaviour, ITickable
{
    public bool IsActive = true;
    [Range(0f, 100f),SerializeField] private float strength = 1f;
    [Range(0f, 1000f), SerializeField] private float maxDistance = 100f;
    [Range(0f, 100f), SerializeField] private float collisionRadius= 5f;
    [SerializeField] private LayerMask attractableLayer;
    [SerializeField] private string tagToAttract = "Attractable";

    [SerializeField] private PassiveHarvester linkedPassiveHarvester;

    private Collider2D[] attractables = new Collider2D[10];
    private Collectable currentCollectable;
    private Collider2D currentAttractable;

    private bool tickHasHappened;

    public void Configure(AttractorData inData)
    {
        this.strength = inData.Strength;
        this.maxDistance = inData.MaxDistance;
        this.collisionRadius = inData.CollisionRadius;
        this.attractableLayer = inData.AttractableLayer;
        this.tagToAttract = inData.TagToAttract;
    }

    private void Start()
    {
        TickManager.OnTick += Tick;
    }

    private void OnDestroy()
    {
        TickManager.OnTick -= Tick;
    }

    public void Tick()
    {
        // Attractor logic is handled in FixedUpdate for consistent physics interactions
        this.tickHasHappened = true;
    }

    public void SecondTick()
    {
        // No second tick logic needed for this class
    }

    private void FixedUpdate()
    {
        if (!IsActive) return;

        if(this.linkedPassiveHarvester == null || this.linkedPassiveHarvester.IsFull)
            return;

        var count = Physics2D.OverlapCircleNonAlloc(this.transform.position, this.maxDistance, this.attractables, this.attractableLayer);

        for (int i = 0; i < count; i++)
        {
            this.currentAttractable = this.attractables[i];
            if (!this.currentAttractable.CompareTag(this.tagToAttract)) continue;

            if (this.currentAttractable.TryGetComponent(out this.currentCollectable))
            {
                if (!this.linkedPassiveHarvester.ShouldAttract(this.currentCollectable.ResourceType))
                    continue;
            }
            else
                continue;

            var direction = (Vector2)this.transform.position - (Vector2)this.currentAttractable.transform.position;
            var distance = direction.magnitude;

            //Debug.Log($"Attracting {this.currentAttractable.gameObject.name} at distance {distance}");

            if (distance > this.collisionRadius)
            {
                if (this.currentAttractable.attachedRigidbody != null)
                {
                    Vector2 force = 100f * this.strength * direction.normalized / distance;
                    this.currentAttractable.attachedRigidbody.AddForce(force);
                }
            }
            else
            {
                if (!this.tickHasHappened)
                    return;

                this.linkedPassiveHarvester.TrySetActiveResourceType(this.currentCollectable.ResourceType);

                float amountToAdd = this.currentCollectable.Amount;
                if (this.linkedPassiveHarvester.ActiveResourceData != null)
                    amountToAdd *= this.linkedPassiveHarvester.ActiveResourceData.ConversionRatio;

                var success = this.linkedPassiveHarvester.TryAddAmount(amountToAdd, this.currentCollectable.ResourceType);

                if (success)
                    this.currentCollectable.OnAttracted();
                    
                this.currentCollectable = null; // Clear reference after attraction
            }
        }

        this.tickHasHappened = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, this.maxDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, this.collisionRadius);
    }
}
