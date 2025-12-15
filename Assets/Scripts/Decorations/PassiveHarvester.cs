using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Decorations that passively generate resources over time
/// </summary>
public abstract class PassiveHarvester : DecorationBase, ITickable
{
    [Header("Harvester Properties")]
    [SerializeField] protected EResourceType generatedResource;
    [SerializeField] protected int maxCapacity = 10;
    [SerializeField] protected float generationInterval = 30f; // Seconds between generation
    [SerializeField] protected bool requiresSpecificConditions = true;
    
    [Header("Current State")]
    [SerializeField] protected int currentAmount = 0;
    [SerializeField] protected float lastGenerationTime = 0f;
    [SerializeField] protected bool isActive = true;

    [SerializeField] protected TextMeshProUGUI quantityText;
    
    // Properties
    public EResourceType GeneratedResource => generatedResource;
    public int CurrentAmount => currentAmount;
    public int MaxCapacity => maxCapacity;
    public bool IsFull => currentAmount >= maxCapacity;
    public bool IsEmpty => currentAmount <= 0;
    public float CapacityPercent => (float)currentAmount / maxCapacity;
    
    // Events
    public static event Action<PassiveHarvester, int> OnResourceGenerated;
    public static event Action<PassiveHarvester, int> OnResourceCollected;
    public static event Action<PassiveHarvester> OnCapacityFull;
    
    protected virtual void OnEnable()
    {
        TickManager.OnSecondTick += SecondTick;
    }
    
    protected virtual void OnDisable()
    {
        TickManager.OnSecondTick -= SecondTick;
    }
    
    public virtual void Tick()
    {
        // Fast tick updates if needed
    }
    
    public virtual void SecondTick()
    {
        if (CanGenerate())
        {
            TryGenerate();
        }
    }
    
    protected virtual bool CanGenerate()
    {
        if (!this.isActive || this.IsFull)
            return false;
            
        if (Time.time - this.lastGenerationTime < this.generationInterval)
            return false;
            
        if (this.requiresSpecificConditions && !CheckGenerationConditions())
            return false;
            
        return true;
    }
    
    protected abstract bool CheckGenerationConditions();

    protected virtual void TryGenerate()
    {
        int amountToGenerate = GetGenerationAmount();

        if (amountToGenerate > 0)
        {
            int actualAmount = Mathf.Min(amountToGenerate, this.maxCapacity - this.currentAmount);
            this.currentAmount += actualAmount;
            this.lastGenerationTime = Time.time;

            if (actualAmount > 0)
            {
                OnResourceGenerated?.Invoke(this, actualAmount);
                OnGenerated(actualAmount);
                RefreshQuantityText();
            }

            if (IsFull)
                OnCapacityFull?.Invoke(this);
        }
    }

    public bool AddAmount(int amount)
    {
        if (amount <= 0)
            return false;

        int actualAmount = Mathf.Min(amount, this.maxCapacity - this.currentAmount);
        this.currentAmount += actualAmount;

        if (actualAmount > 0)
        {
            OnResourceCollected?.Invoke(this, actualAmount);
            OnGenerated(actualAmount);
            RefreshQuantityText();
        }

        if (IsFull)
            OnCapacityFull?.Invoke(this);

        return actualAmount > 0;
    }
    
    protected virtual void RefreshQuantityText()
    {
        if (this.quantityText != null)
        {
            this.quantityText.text = $"{this.currentAmount}/{this.maxCapacity}";
        }
    }
    
    protected virtual int GetGenerationAmount()
    {
        return 1; // Base generation amount
    }
    
    protected virtual void OnGenerated(int amount)
    {
        // Override for generation effects
    }
    
    public virtual bool CollectAll()
    {
        if (this.IsEmpty)
            return false;
            
        if (ResourceManager.IN.AddResource(this.generatedResource, this.currentAmount))
        {
            int collectedAmount = this.currentAmount;
            this.currentAmount = 0;
            OnResourceCollected?.Invoke(this, collectedAmount);
            OnCollected(collectedAmount);
            RefreshQuantityText();
            return true;
        }
        
        return false; // Inventory full
    }
    
    protected virtual void OnCollected(int amount)
    {
        // Override for collection effects
    }
    
    public virtual void SetActive(bool active)
    {
        this.isActive = active;
    }
    
    // Mouse interaction for collection
    protected virtual void OnMouseDown()
    {
        if (!this.IsEmpty && !DragManager.IsDragModeActivated)
        {
            CollectAll();
        }
    }
    
    public override DecorationData GetSaveData()
    {
        var baseData = base.GetSaveData();
        baseData.CurrentAmount = this.currentAmount;
        baseData.LastGenerationTime = this.lastGenerationTime;
        baseData.IsActive = this.isActive;
        return baseData;
    }
    
    public override void LoadSaveData(DecorationData data)
    {
        base.LoadSaveData(data);
        this.currentAmount = data.CurrentAmount;
        this.lastGenerationTime = data.LastGenerationTime;
        this.isActive = data.IsActive;
        RefreshQuantityText();
    }
}