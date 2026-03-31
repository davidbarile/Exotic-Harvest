using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Base class for collectable objects that can be harvested by the player
/// UI-based for desktop overlay gameplay
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public abstract class Collectable : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
{
    // Events
    public static Action<Collectable> OnCollectableSpawned;
    public static Action<Collectable> OnCollectableCollected;
    public static Action<Collectable> OnCollectableExpired;
    
    public EResourceType ResourceType => resourceType;
    [SerializeField] protected EResourceType resourceType;

    public int Amount => amount;
    [SerializeField] protected int amount = 1;

    public ECollectionMethod CollectionMethod => this.collectionType;
    [SerializeField] protected ECollectionMethod collectionType = ECollectionMethod.Click;

    [Tooltip("-1 for infinite, otherwise seconds to destroy")]
    [SerializeField] protected float lifetime = -1; // Seconds before disappearing

    [Header("UI Components")]
    [SerializeField] protected Image collectableImage;
    protected CanvasGroup canvasGroup;
    
    protected float spawnTime;
    protected bool isCollected = false;
    protected bool isDragging = false;

    protected virtual void Awake()
    {
        if (this.collectableImage == null)
            this.collectableImage = GetComponent<Image>();

        this.canvasGroup = GetComponent<CanvasGroup>();
    }
    
    public virtual void Spawn()
    {            
        this.spawnTime = Time.time;
        OnCollectableSpawned?.Invoke(this);
        
        if (this.lifetime > 0)
            Destroy(gameObject, this.lifetime);
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
        if (this.collectionType.HasFlag(ECollectionMethod.Click))
        {
            OnClick();
        }
    }
    
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (this.collectionType.HasFlag(ECollectionMethod.Hover))
        {
            Collect();
        }
    }
    
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (this.collectionType == ECollectionMethod.Drag)
        {
            this.isDragging = true;
            OnDragStart();
        }
    }
    
    public virtual void OnDrag(PointerEventData eventData)
    {
        if (this.collectionType.HasFlag(ECollectionMethod.Drag) && this.isDragging)
        {
            OnDragOver();
        }
    }
    
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (this.collectionType.HasFlag(ECollectionMethod.Drag) && this.isDragging)
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
    
    public virtual void Collect(bool inShouldAddResourceImmediately = true)
    {
        if (!CanBeCollected())
            return;

        if (inShouldAddResourceImmediately)
        {
            // Try to add to inventory
            if (ResourceManager.IN.AddResource(this.ResourceType, this.Amount))
            {
                DoCollect();
            }
            else
            {
                // Inventory full - don't collect
            }
        }
        else
        {
            // Just mark as collected, actual resource addition will be handled by collector (e.g. bucket)
            DoCollect();
        }

        void DoCollect()
        {
            this.isCollected = true;
            OnCollected();
            OnCollectableCollected?.Invoke(this);
        }
    }
    
    protected virtual void OnCollected()
    {
        // Override for collection effects (particles, sound, animation)
        Destroy(this.gameObject);
    }
    
    // Additional collection methods for future use
    public virtual void OnSwipe()
    {
        if (this.collectionType.HasFlag(ECollectionMethod.Swipe))
            Collect();
    }
    
    public virtual void OnHold()
    {
        if (this.collectionType.HasFlag(ECollectionMethod.Hold))
            Collect();
    }
}