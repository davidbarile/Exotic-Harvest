using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Decorations that passively generate resources over time
/// </summary>
public abstract class PassiveHarvester : MonoBehaviour, ITickable
{
    // Events
    public static Action<PassiveHarvester, int> OnResourceGenerated;
    public static Action<PassiveHarvester, int> OnResourceCollected;
    public static Action<PassiveHarvester> OnCapacityFull;

    public DecorationData DecorationData { get; private set; }

    public EResourceType GeneratedResource => generatedResource;
    [SerializeField] protected EResourceType generatedResource;

    public int CurrentAmount => this.DecorationData != null ? this.DecorationData.CurrentAmount : 0;
    public int MaxCapacity => this.DecorationData != null ? this.DecorationData.MaxAmount : 0;
    public float GenerationInterval => this.DecorationData != null ? this.DecorationData.GenerationInterval : 0f;
    public bool RequiresSpecificConditions => this.DecorationData != null ? this.DecorationData.RequiresSpecificConditions : false;
    
    public float LastGenerationTime => this.DecorationData != null ? this.DecorationData.LastGenerationTime : 0f;
    public bool IsActive => this.DecorationData != null ? this.DecorationData.IsActive : true;

    public bool IsFull => this.CurrentAmount >= this.MaxCapacity;
    public bool IsEmpty => this.CurrentAmount <= 0;
    public float CapacityPercent => (float)this.CurrentAmount / this.MaxCapacity;

    [SerializeField] protected TextMeshProUGUI quantityText;

    protected virtual void Start()
    {
       TickManager.OnSecondTick += SecondTick;
    }

    protected virtual void OnDestroy()
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
    
    public virtual void SetDecorationData(DecorationData inDecorationData)
    {
        this.DecorationData = DecorationData.Copy(inDecorationData);
    }
    
    protected virtual bool CanGenerate()
    {    
        if (!this.DecorationData.IsActive)
            return false;
        if (!this.IsActive || this.IsFull)
            return false;
            
        if (Time.time - this.LastGenerationTime < this.GenerationInterval)
            return false;
            
        if (this.RequiresSpecificConditions && !CheckGenerationConditions())
            return false;
            
        return true;
    }
    
    protected abstract bool CheckGenerationConditions();

    protected virtual void TryGenerate()
    {
        int amountToGenerate = GetGenerationAmount();

        if (amountToGenerate > 0)
        {
            int actualAmount = Mathf.Min(amountToGenerate, this.MaxCapacity - this.CurrentAmount);
            this.DecorationData.CurrentAmount += actualAmount;
            this.DecorationData.LastGenerationTime = Time.time;

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

    public bool AddAmount(int inAmount)
    {
        if (inAmount <= 0)
            return false;

        int actualAmount = Mathf.Min(inAmount, this.MaxCapacity - this.CurrentAmount);
        this.DecorationData.CurrentAmount += actualAmount;

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
        if (this.quantityText)
            this.quantityText.text = $"{this.CurrentAmount}/{this.MaxCapacity}";
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

        int amountToCollect = Mathf.FloorToInt(this.DecorationData.CurrentAmount * this.DecorationData.ConversionRatio);
            
        if (ResourceManager.IN.AddResource(this.generatedResource, amountToCollect))
        {
            int collectedAmount = amountToCollect;
            this.DecorationData.CurrentAmount = 0;
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
        this.DecorationData.IsActive = active;
    }

    protected virtual void OnMouseOver()
    {
        // Check for right mouse button press (button 1)
        if (Input.GetMouseButtonDown(1))
        {
            if (!this.IsEmpty && !DragManager.IsDragModeActivated)
            {
                //not sure if I really want this here, but good for testing
                CollectAll();
            }
        }
    }
}