using System;
using UnityEngine;

/// <summary>
/// Base class for all decorations that can be placed on the desktop
/// </summary>
public abstract class DecorationBase : MonoBehaviour
{
    [Header("Decoration Properties")]
    [SerializeField] protected DecorationType decorationType;
    [SerializeField] protected string decorationName = "Decoration";
    [SerializeField] protected bool isDraggable = true;
    [SerializeField] protected bool canBeLocked = true;
    [SerializeField] protected ResourceCost purchaseCost;
    
    [Header("State")]

    [SerializeField] protected Vector3 defaultPosition;
    
    protected bool isInitialized = false;
    
    // Properties
    public DecorationType Type => this.decorationType;
    public string Name => this.decorationName;
    public bool IsDraggable => this.isDraggable;
    public ResourceCost PurchaseCost => this.purchaseCost;
    
    // Events
    public static event Action<DecorationBase> OnDecorationPlaced;
    public static event Action<DecorationBase> OnDecorationMoved;
    public static event Action<DecorationBase> OnDecorationLocked;
    public static event Action<DecorationBase> OnDecorationUnlocked;
    public static event Action<DecorationBase> OnDecorationRemoved;
    
    protected virtual void Start()
    {
        if (!this.isInitialized)
            Initialize();
    }
    
    public virtual void Initialize()
    {
        this.defaultPosition = transform.position;
        this.isInitialized = true;
        OnPlaced();
        OnDecorationPlaced?.Invoke(this);
    }
    
    protected virtual void OnPlaced()
    {
        // Override for placement effects
    }
    
    public virtual void SetPosition(Vector3 position)
    {
        transform.position = position;
        OnMoved();
        OnDecorationMoved?.Invoke(this);
    }
    
    protected virtual void OnMoved()
    {
        // Override for movement effects
    }
    
    public virtual void Remove()
    {
        OnRemoved();
        OnDecorationRemoved?.Invoke(this);
        Destroy(gameObject);
    }
    
    protected virtual void OnRemoved()
    {
        // Override for removal effects
    }
    
    /// <summary>
    /// Called when decoration is interacted with (clicked/tapped)
    /// </summary>
    public virtual void OnInteract()
    {
        // Override in derived classes for specific interaction behavior
        Debug.Log($"Interacted with decoration: {this.decorationName}");
    }
    
    /// <summary>
    /// Called when decoration position changes (for UI updates, save data, etc.)
    /// </summary>
    public virtual void OnPositionChanged(Vector2 newUIPosition)
    {
        // Update any position-dependent logic
        // Save position changes will be handled by the decoration manager
        Debug.Log($"Decoration {this.decorationName} moved to UI position: {newUIPosition}");
    }
    
    // Drag handling (works with existing UiDraggable if needed)
    public virtual bool CanStartDrag()
    {
        return IsDraggable && DragManager.IsDragModeActivated;
    }
    
    public virtual void OnStartDrag()
    {
        // Override for drag start effects
    }
    
    public virtual void OnEndDrag()
    {
        // Override for drag end effects
        OnMoved();
    }
    
    // Save/Load data
    public virtual DecorationData GetSaveData()
    {
        return new DecorationData
        {
            Type = this.decorationType,
            Position = transform.position,
        };
    }
    
    public virtual void LoadSaveData(DecorationData data)
    {
        transform.position = data.Position;
    }
}