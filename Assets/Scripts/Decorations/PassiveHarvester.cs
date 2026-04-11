using TMPro;
using UnityEngine;

/// <summary>
/// Decorations that passively generate resources over time
/// </summary>
public abstract class PassiveHarvester : MonoBehaviour, ITickable
{
    public DecorationData DecorationData { get; protected set; }

    public EResourceType GeneratedResource => this.DecorationData != null ? this.DecorationData.GeneratedResource : EResourceType.None;

    public int CurrentAmount => this.DecorationData != null ? this.DecorationData.CurrentAmount : 0;
    public int MaxCapacity => this.DecorationData != null ? this.DecorationData.MaxAmount : 0;
    public float GenerationInterval => this.DecorationData != null ? this.DecorationData.GenerationInterval : 0f;
    public bool RequiresSpecificConditions => this.DecorationData != null ? this.DecorationData.RequiresSpecificConditions : false;
    
    public float LastGenerationTime => this.DecorationData != null ? this.DecorationData.LastGenerationTime : 0f;
    public bool IsActive => this.DecorationData != null ? this.DecorationData.IsActive : true;

    public bool IsFull => this.CurrentAmount >= this.MaxCapacity;
    public bool IsEmpty => this.CurrentAmount <= 0;
    public float CapacityPercent => (float)this.CurrentAmount / this.MaxCapacity;

    public EResourceType[] CollectableResourceTypes;

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
            TryGenerate();
    }
    
    public virtual void SetDecorationData(DecorationData inDecorationData)
    {
        this.DecorationData = inDecorationData;
        RefreshQuantityDisplay();
    }
    
    protected virtual bool CanGenerate()
    {
        if (this.DecorationData == null || !this.DecorationData.IsActive)
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
        if (this.DecorationData == null)
            return;
            
        int amountToGenerate = GetGenerationAmount();

        if (amountToGenerate > 0)
        {
            int actualAmount = Mathf.Min(amountToGenerate, this.MaxCapacity - this.CurrentAmount);
            this.DecorationData.CurrentAmount += actualAmount;
            this.DecorationData.LastGenerationTime = Time.time;

            if (actualAmount > 0)
            {
                ResourceManager.OnResourceGained?.Invoke(this.GeneratedResource, actualAmount);
                OnGenerated(actualAmount);
                RefreshQuantityDisplay();
            }
        }
    }

    public bool AddAmount(int inAmount)
    {
        if (this.DecorationData == null)
            return false;
            
        if (inAmount <= 0)
            return false;

        int actualAmount = Mathf.Min(inAmount, this.MaxCapacity - this.CurrentAmount);
        this.DecorationData.CurrentAmount += actualAmount;

        if (actualAmount > 0)
        {
            ResourceManager.OnResourceGained?.Invoke(this.GeneratedResource, actualAmount);
            OnGenerated(actualAmount);
            RefreshQuantityDisplay();
        }

        return actualAmount > 0;
    }

    protected virtual void RefreshQuantityDisplay()
    {
        if (this.DecorationData == null)
            return;

        if (this.quantityText)
        {
            if (this.DecorationData.ConversionRatio != 1f)
            {
                int amountCollected = Mathf.FloorToInt((float)this.DecorationData.CurrentAmount * this.DecorationData.ConversionRatio);
                int total = Mathf.FloorToInt((float)this.DecorationData.MaxAmount * this.DecorationData.ConversionRatio);
                this.quantityText.text = $"{amountCollected}/{total}\n<size=80%><i>({this.CurrentAmount}/{this.MaxCapacity})</i></size>";
            }
            else
                this.quantityText.text = $"{this.CurrentAmount}/{this.MaxCapacity}";
        }
    }
    
    public void SetText(string inText)
    {
        if (this.quantityText)
            this.quantityText.text = inText;
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
        if (this.DecorationData == null || this.IsEmpty)
            return false;

        int amountToCollect = Mathf.FloorToInt((float)this.DecorationData.CurrentAmount * this.DecorationData.ConversionRatio);

        ResourceManager.IN.AddResource(this.GeneratedResource, amountToCollect);

        int collectedAmount = amountToCollect;
        this.DecorationData.CurrentAmount = 0;
        ResourceManager.OnResourceGained?.Invoke(this.GeneratedResource, collectedAmount);
        OnCollected(collectedAmount);
        RefreshQuantityDisplay();
        
        return true;
    }
    
    protected virtual void OnCollected(int amount)
    {
        // Override for collection effects
    }
    
    public virtual void SetActive(bool active)
    {
        if (this.DecorationData == null)
            return;
            
        this.DecorationData.IsActive = active;
    }

    protected virtual void OnMouseOver()
    {
        // Check for right mouse button press (button 1)
        if (Input.GetMouseButtonDown(1))
        {
            if (!DragManager.IsDragModeActivated)
            {
                //not sure if I really want this here, but good for testing
                CollectAll();
            }
        }
    }
}