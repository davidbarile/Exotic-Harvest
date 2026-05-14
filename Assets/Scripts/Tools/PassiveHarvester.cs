using System;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GlobalEnums;

/// <summary>
/// Decorations that passively generate resources over time
/// </summary>
public abstract class PassiveHarvester : MonoBehaviour, ITickable
{
    [Serializable]
    public class CollectableResourceData
    {
        //maybe this class should be moved into DecorationData so it can be defined in ShopConfig
        //that would allow for different sizes of crystal for example to collect different types, amounts and conversion ratios of resources
        //but I don't want to be saving such large amounts of data for each decoration in the world save data
        public EResourceType ResourceType;
        [Range(0, 1)] public float ConversionRatio = 1f;
        public int MaxAmount = 0;
    }

    public DecorationData DecorationData;// { get; protected set; }

    public EResourceType GeneratedResource => this.DecorationData != null ? this.DecorationData.GeneratedResource : EResourceType.None;

    public int CurrentAmount => this.DecorationData != null ? this.DecorationData.CurrentAmount : 0;
    public int MaxCapacity
    {
        get
        {
            //defined in the prefab CollectableResourceData - this allows different resource types to have different capacities
            if (this.ActiveResourceData != null
                && this.ActiveResourceData.ResourceType != EResourceType.None
                && this.ActiveResourceData.MaxAmount > 0)
                return this.ActiveResourceData.MaxAmount;
            
            //get it from DecorationData - nice if we have one prefab using various ShopConfigs with different capacities
            return this.DecorationData?.MaxAmount ?? 0;
        }
    }
    public float GenerationInterval => this.DecorationData != null ? this.DecorationData.GenerationInterval : 0f;
    public bool RequiresSpecificConditions => this.DecorationData != null ? this.DecorationData.RequiresSpecificConditions : false;
    
    public float LastGenerationTime => this.DecorationData != null ? this.DecorationData.LastGenerationTime : 0f;
    public bool IsActive => this.DecorationData != null ? this.DecorationData.IsActive : true;

    public bool IsFull => this.CurrentAmount >= this.MaxCapacity;
    public bool IsEmpty => this.CurrentAmount + this.leftoverFractionAmount <= 0;
    public float CapacityPercent => ((float)this.CurrentAmount + this.leftoverFractionAmount) / this.MaxCapacity;

    public CollectableResourceData ActiveResourceData { get; protected set; }

    [SerializeField] private CollectableResourceData[] collectableResources;

    [Space, SerializeField] protected Image fillImage;
    [Range(0, 2),SerializeField] protected float fillAnimationDuration = 0.5f;

    [SerializeField] protected TextMeshProUGUI quantityText;
    [SerializeField] protected bool showQuantityTextWhenEmpty;
    [SerializeField] protected bool showQuantityTextAsPercent;

    [Space, SerializeField] protected ActiveResourceDisplay activeResourceDisplay;

    protected float targetFillAmount;
    protected float leftoverFractionAmount;

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
        SetActiveResourceData(GetResourceDataOfType(this.DecorationData.ActiveResourceType));
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

    public bool TryAddAmount(float inAmount, EResourceType inResourceType)
    {
        if (this.DecorationData == null)
            return false;

        if (inAmount <= 0)
            return false;

        this.leftoverFractionAmount += inAmount;
        int amountToAdd = Mathf.FloorToInt(this.leftoverFractionAmount);

        if (amountToAdd < this.MaxCapacity)
            this.leftoverFractionAmount -= amountToAdd;
        else
            this.leftoverFractionAmount = 0;

        Debug.Log($"Trying to add {inAmount} of {inResourceType} to {this.gameObject.name}. Amount to add: {amountToAdd}, Leftover fraction: {this.leftoverFractionAmount}. isEmpty = {this.IsEmpty}");

        if (this.DecorationData.CurrentAmount == 0 && inResourceType != EResourceType.None)
        {
            if (!CanCollectResourceType(inResourceType))
                return false;

            SetActiveResourceData(GetResourceDataOfType(inResourceType));
        }
        else if (inResourceType != EResourceType.None && inResourceType != this.ActiveResourceData?.ResourceType)
        {
            // Can't add different resource type
            return false;
        }

        int actualAmount = Mathf.Min(amountToAdd, this.MaxCapacity - this.CurrentAmount);
        this.DecorationData.CurrentAmount += actualAmount;

        if (actualAmount > 0)
        {
            ResourceManager.OnResourceGained?.Invoke(this.ActiveResourceData.ResourceType, actualAmount);
            OnGenerated(actualAmount);
        }

        RefreshQuantityDisplay();

        return actualAmount > 0;
    }
    
    //kinda hack function to be called from Attractor before TryAddAmount()
    public bool TrySetActiveResourceType(EResourceType inResourceType)
    {
        if (this.DecorationData.CurrentAmount == 0 && inResourceType == EResourceType.None || CanCollectResourceType(inResourceType))
        {
            SetActiveResourceData(GetResourceDataOfType(inResourceType));
            return true;
        }
        return false;
    }

    protected virtual void RefreshQuantityDisplay()
    {
        // if (this.DecorationData == null)
        //     return;

        if (this.showQuantityTextWhenEmpty || !this.IsEmpty)
        {
            if (this.showQuantityTextAsPercent)
                SetText($"{this.CapacityPercent:P1}");
            else
                SetText($"{this.CurrentAmount}/{this.MaxCapacity}");
        }
        else
            SetText(string.Empty);
            
        if (this.activeResourceDisplay != null)
        {
            this.activeResourceDisplay.SetValue(this.CurrentAmount, this.MaxCapacity);
            this.activeResourceDisplay.gameObject.SetActive(this.showQuantityTextWhenEmpty || !this.IsEmpty);
        }

        UpdateFillMeter(false);
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
         UpdateFillMeter();
    }

    public virtual bool CollectAll()
    {
        if (this.DecorationData == null || this.IsEmpty
            || this.ActiveResourceData == null || this.ActiveResourceData.ResourceType == EResourceType.None)
            return false;

        var resourceType = this.ActiveResourceData.ResourceType;

        ResourceManager.IN.AddResource(resourceType, this.DecorationData.CurrentAmount);

        this.leftoverFractionAmount = 0f;
        this.DecorationData.CurrentAmount = 0;
        
        SetActiveResourceData(null);
        ResourceManager.OnResourceGained?.Invoke(resourceType, this.DecorationData.CurrentAmount);
        OnCollected(this.DecorationData.CurrentAmount);
        RefreshQuantityDisplay();

        return true;
    }
    
    protected virtual void SetActiveResourceData(CollectableResourceData inResourceData)
    {
        this.ActiveResourceData = inResourceData;
        this.DecorationData.ActiveResourceType = inResourceData != null ? inResourceData.ResourceType : EResourceType.None;

        //Debug.Log($"<color=yellow>Set active resource type to {this.DecorationData.ActiveResourceType} for {this.gameObject.name}</color>", this.gameObject);

        if (this.activeResourceDisplay != null)
        {
            if (inResourceData != null)
                this.activeResourceDisplay.SetIcon(ResourceManager.IN.GetResourceSprite(inResourceData.ResourceType));
        }
    }
    
    protected virtual void OnCollected(int amount)
    {
        UpdateFillMeter();
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

    protected virtual void UpdateFillMeter(bool shouldAnimate = true)
    {
        if (this.fillImage == null)
            return;

        // Smooth fill animation
        if (shouldAnimate)
        {
            this.targetFillAmount = this.CapacityPercent;

            this.fillImage.DOFillAmount(this.targetFillAmount, this.fillAnimationDuration)
                .SetEase(Ease.OutQuad);
            return;
        }

        this.fillImage.fillAmount = this.CapacityPercent;
    }

    public bool CanCollectResourceType(EResourceType inResourceType)
    {
        return this.collectableResources.Any(r => r.ResourceType == inResourceType);
    }

    public CollectableResourceData GetResourceDataOfType(EResourceType inResourceType)
    {
        return this.collectableResources.FirstOrDefault(r => r.ResourceType == inResourceType);
    }

    public float GetConversionRatioForResource(EResourceType inResourceType)
    {
        var resourceData = this.collectableResources.FirstOrDefault(r => r.ResourceType == inResourceType);
        return resourceData != null ? resourceData.ConversionRatio : 0f;
    }
    
    public bool ShouldAttract(EResourceType inResourceType)
    {
        return this.ActiveResourceData == null || this.ActiveResourceData.ResourceType == inResourceType;
    }
}