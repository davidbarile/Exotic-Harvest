using System;
using UnityEngine;

/// <summary>
/// Decorations that passively generate resources over time
/// </summary>
public abstract class PassiveHarvester : DecorationBase, ITickable
{
    [Header("Harvester Properties")]
    [SerializeField] protected ResourceType generatedResource;
    [SerializeField] protected int maxCapacity = 10;
    [SerializeField] protected float generationInterval = 30f; // Seconds between generation
    [SerializeField] protected bool requiresSpecificConditions = true;
    
    [Header("Current State")]
    [SerializeField] protected int currentAmount = 0;
    [SerializeField] protected float lastGenerationTime = 0f;
    [SerializeField] protected bool isActive = true;
    
    // Properties
    public ResourceType GeneratedResource => generatedResource;
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
        if (!isActive || IsFull)
            return false;
            
        if (Time.time - lastGenerationTime < generationInterval)
            return false;
            
        if (requiresSpecificConditions && !CheckGenerationConditions())
            return false;
            
        return true;
    }
    
    protected abstract bool CheckGenerationConditions();
    
    protected virtual void TryGenerate()
    {
        int amountToGenerate = GetGenerationAmount();
        
        if (amountToGenerate > 0)
        {
            int actualAmount = Mathf.Min(amountToGenerate, maxCapacity - currentAmount);
            currentAmount += actualAmount;
            lastGenerationTime = Time.time;
            
            OnResourceGenerated?.Invoke(this, actualAmount);
            OnGenerated(actualAmount);
            
            if (IsFull)
                OnCapacityFull?.Invoke(this);
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
        if (IsEmpty)
            return false;
            
        if (ResourceManager.IN.AddResource(generatedResource, currentAmount))
        {
            int collectedAmount = currentAmount;
            currentAmount = 0;
            OnResourceCollected?.Invoke(this, collectedAmount);
            OnCollected(collectedAmount);
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
        isActive = active;
    }
    
    // Mouse interaction for collection
    protected virtual void OnMouseDown()
    {
        if (!IsEmpty && !ScreenManager.IsDragModeActivated)
        {
            CollectAll();
        }
    }
    
    public override DecorationData GetSaveData()
    {
        var baseData = base.GetSaveData();
        baseData.currentAmount = currentAmount;
        baseData.lastGenerationTime = lastGenerationTime;
        baseData.isActive = isActive;
        return baseData;
    }
    
    public override void LoadSaveData(DecorationData data)
    {
        base.LoadSaveData(data);
        currentAmount = data.currentAmount;
        lastGenerationTime = data.lastGenerationTime;
        isActive = data.isActive;
    }
}