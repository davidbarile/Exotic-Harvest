using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Base class for collectable objects that can be harvested by the player
/// UI-based for desktop overlay gameplay
/// </summary>
public abstract class Collectable : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
{
    [SerializeField] protected ResourceType resourceType;
    [SerializeField] protected int amount = 1;
    [SerializeField] protected CollectionMethod collectionMethod = CollectionMethod.Click;
    [SerializeField] protected float lifetime = 30f; // Seconds before disappearing
    [SerializeField] protected bool autoDestroy = true;
    
    [Header("UI Components")]
    [SerializeField] protected Image collectableImage;
    
    protected RectTransform rectTransform;
    protected Canvas parentCanvas;
    protected float spawnTime;
    protected bool isCollected = false;
    protected bool isDragging = false;
    
    public ResourceType ResourceType => resourceType;
    public int Amount => amount;
    public CollectionMethod CollectionMethod => collectionMethod;
    
    // Events
    public static event Action<Collectable> OnCollectableSpawned;
    public static event Action<Collectable> OnCollectableCollected;
    public static event Action<Collectable> OnCollectableExpired;
    
    protected virtual void Start()
    {
        this.rectTransform = GetComponent<RectTransform>();
        this.parentCanvas = GetComponentInParent<Canvas>();
        
        if (this.collectableImage == null)
            this.collectableImage = GetComponent<Image>();
            
        this.spawnTime = Time.time;
        OnCollectableSpawned?.Invoke(this);
        
        if (this.autoDestroy)
        {
            Destroy(gameObject, this.lifetime);
        }
    }
    
    protected virtual void OnDestroy()
    {
        if (!this.isCollected)
            OnCollectableExpired?.Invoke(this);
    }
    
    public virtual bool CanBeCollected()
    {
        return !this.isCollected && gameObject.activeInHierarchy;
    }

    // UI Event System handlers
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (this.collectionMethod == CollectionMethod.Click)
        {
            OnClick();
        }
    }
    
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (this.collectionMethod == CollectionMethod.Hover)
        {
            Collect();
        }
    }
    
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (this.collectionMethod == CollectionMethod.Drag)
        {
            this.isDragging = true;
            OnDragStart();
        }
    }
    
    public virtual void OnDrag(PointerEventData eventData)
    {
        if (this.collectionMethod == CollectionMethod.Drag && this.isDragging)
        {
            OnDragOver();
        }
    }
    
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (this.collectionMethod == CollectionMethod.Drag && this.isDragging)
        {
            this.isDragging = false;
            OnDragEnd();
        }
    }
    
    // Collection methods (can be overridden)
    protected virtual void OnClick()
    {
        if (CanBeCollected())
            Collect();
    }
    
    protected virtual void OnDragStart() { }
    
    public virtual void OnDragOver()
    {
        if (CanBeCollected())
            Collect();
    }
    
    protected virtual void OnDragEnd() { }
    
    public virtual void Collect()
    {
        if (!CanBeCollected())
            return;
            
        this.isCollected = true;
        
        // Try to add to inventory
        if (ResourceManager.IN.AddResource(this.resourceType, this.amount))
        {
            OnCollected();
            OnCollectableCollected?.Invoke(this);
        }
        else
        {
            // Inventory full - don't collect
            this.isCollected = false;
            return;
        }
    }
    
    protected virtual void OnCollected()
    {
        // Override for collection effects (particles, sound, animation)
        Destroy(gameObject);
    }
    
    // Additional collection methods for future use
    public virtual void OnSwipe()
    {
        if (this.collectionMethod == CollectionMethod.Swipe)
            Collect();
    }
    
    public virtual void OnHold()
    {
        if (this.collectionMethod == CollectionMethod.Hold)
            Collect();
    }
}