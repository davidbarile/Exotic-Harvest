using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Lean.Pool;
using static GlobalEnums;

/// <summary>
/// Base class for collectable objects that can be harvested by the player
/// UI-based for desktop overlay gameplay
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public abstract class Collectable : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPoolable
{
    public EResourceType ResourceType => resourceType;
    [SerializeField] protected EResourceType resourceType;

    public virtual int Amount => amount;
    [SerializeField] protected int amount = 1;

    public ECollectionMethod CollectionMethod => this.collectionType;
    [SerializeField] protected ECollectionMethod collectionType = ECollectionMethod.Click;

    [Tooltip("0 for infinite, otherwise seconds to destroy")]
    [SerializeField] private WeightedRandom lifetimeMinMax;
    protected float lifetime = 0; // Seconds before disappearing

    [Header("UI Components")]
    [SerializeField] protected Image collectableImage;
    protected CanvasGroup canvasGroup;

    protected DateTime spawnTime;
    protected float initScale = 1f;
    protected bool isCollected = false;
    protected bool isDragging = false;

    protected virtual void Awake()
    {
        if (!this.collectableImage)
            this.collectableImage = GetComponent<Image>();

        this.canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void Spawn()
    {
        if (!this.canvasGroup)
            this.canvasGroup = GetComponent<CanvasGroup>();

        this.spawnTime = DateTime.Now;

        this.lifetime = this.lifetimeMinMax.GetWeightedRandomQuantity();

        if (this.lifetime > 0)
        {
            CancelInvoke();
            Invoke(nameof(Expire), this.lifetime);
        } 
    }
    
    public virtual void Expire()
    {
        // Optional: Add lifetime-based fading or other time-based effects here
        LeanPool.Despawn(this.gameObject, this.lifetime);
    }
    
    public virtual bool CanBeCollected()
    {
        return !this.isCollected && gameObject.activeInHierarchy;
    }

    // UI Event System handlers
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Clicked on {gameObject.name} with collection method {this.collectionType}");
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
            ResourceManager.IN.AddResource(this.ResourceType, this.Amount);

        this.isCollected = true;
        OnCollected();
    }
    
    protected virtual void OnCollected()
    {
        // Override for collection effects (particles, sound, animation)
        LeanPool.Despawn(this.gameObject);
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

    public virtual void OnAttracted()
    {
        //Debug.Log("Collectable attracted!");
        LeanPool.Despawn(this.gameObject);
    }
    
    protected virtual void OnDestroy()
    {
        // Cleanup if needed
    }

    public void OnSpawn()
    {
        
    }

    public void OnDespawn()
    {
        OnDestroy();
    }
}