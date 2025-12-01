using System;
using UnityEngine;

/// <summary>
/// Base class for collectable objects that can be harvested by the player
/// </summary>
public abstract class Collectable : MonoBehaviour
{
    [SerializeField] protected ResourceType resourceType;
    [SerializeField] protected int amount = 1;
    [SerializeField] protected CollectionMethod collectionMethod = CollectionMethod.Click;
    [SerializeField] protected float lifetime = 30f; // Seconds before disappearing
    [SerializeField] protected bool autoDestroy = true;
    
    protected float spawnTime;
    protected bool isCollected = false;
    
    public ResourceType ResourceType => resourceType;
    public int Amount => amount;
    public CollectionMethod CollectionMethod => collectionMethod;
    
    // Events
    public static event Action<Collectable> OnCollectableSpawned;
    public static event Action<Collectable> OnCollectableCollected;
    public static event Action<Collectable> OnCollectableExpired;
    
    protected virtual void Start()
    {
        spawnTime = Time.time;
        OnCollectableSpawned?.Invoke(this);
        
        if (autoDestroy)
        {
            Destroy(gameObject, lifetime);
        }
    }
    
    protected virtual void OnDestroy()
    {
        if (!isCollected)
            OnCollectableExpired?.Invoke(this);
    }
    
    public virtual bool CanBeCollected()
    {
        return !isCollected && gameObject.activeInHierarchy;
    }
    
    public virtual void Collect()
    {
        if (!CanBeCollected())
            return;
            
        isCollected = true;
        
        // Try to add to inventory
        if (ResourceManager.IN.AddResource(resourceType, amount))
        {
            OnCollected();
            OnCollectableCollected?.Invoke(this);
        }
        else
        {
            // Inventory full - don't collect
            isCollected = false;
            return;
        }
    }
    
    protected virtual void OnCollected()
    {
        // Override for collection effects (particles, sound, animation)
        Destroy(gameObject);
    }
    
    // For different collection methods
    public virtual void OnClick()
    {
        if (collectionMethod == CollectionMethod.Click)
            Collect();
    }
    
    public virtual void OnDragOver()
    {
        if (collectionMethod == CollectionMethod.Drag)
            Collect();
    }
    
    public virtual void OnSwipe()
    {
        if (collectionMethod == CollectionMethod.Swipe)
            Collect();
    }
    
    public virtual void OnHold()
    {
        if (collectionMethod == CollectionMethod.Hold)
            Collect();
    }
}