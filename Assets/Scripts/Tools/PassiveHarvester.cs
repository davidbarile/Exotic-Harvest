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

    public DecorationData DecorationData { get; protected set; }

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
    public bool IsEmpty => this.CurrentAmount <= 0;
    public float CapacityPercent => (float)this.CurrentAmount / this.MaxCapacity;

    public CollectableResourceData ActiveResourceData { get; protected set; }

    [SerializeField] private CollectableResourceData[] collectableResources;

    [Space, SerializeField] protected Image fillImage;
    [Range(0, 2),SerializeField] protected float fillAnimationDuration = 0.5f;

    [SerializeField] protected TextMeshProUGUI quantityText;
    [SerializeField] protected bool showQuantityTextWhenEmpty;

    [Space, SerializeField] protected ActiveResourceDisplay activeResourceDisplay;
  
    protected float targetFillAmount;

    protected virtual void Start()
    {
        TickManager.OnSecondTick += SecondTick;
        SetText(string.Empty);

        RefreshQuantityDisplay();

        if(this.activeResourceDisplay != null)
            this.activeResourceDisplay.gameObject.SetActive(false);
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

    public bool TryAddAmount(int inAmount, EResourceType inResourceType)
    {
        if (this.DecorationData == null)
            return false;

        if (inAmount <= 0)
            return false;

        if(this.DecorationData.CurrentAmount == 0 && inResourceType != EResourceType.None)
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

        int actualAmount = Mathf.Min(inAmount, this.MaxCapacity - this.CurrentAmount);
        this.DecorationData.CurrentAmount += actualAmount;

        if (actualAmount > 0)
        {
            ResourceManager.OnResourceGained?.Invoke(this.ActiveResourceData.ResourceType, actualAmount);
            OnGenerated(actualAmount);
            RefreshQuantityDisplay();
        }

        return actualAmount > 0;
    }

    protected virtual void RefreshQuantityDisplay()
    {
        if (this.DecorationData == null || this.ActiveResourceData == null)
            return;

        if (this.quantityText && (this.showQuantityTextWhenEmpty || !this.IsEmpty))
        {
            if (this.activeResourceDisplay != null)
            {
                int amountCollected = Mathf.FloorToInt((float)this.CurrentAmount * this.ActiveResourceData.ConversionRatio);
                int total = Mathf.FloorToInt((float)this.MaxCapacity * this.ActiveResourceData.ConversionRatio);

                this.activeResourceDisplay.SetValue(amountCollected, total);
            }
            
            this.quantityText.text = $"{this.CurrentAmount}/{this.MaxCapacity}";    
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

        int amountToCollect = Mathf.FloorToInt((float)this.DecorationData.CurrentAmount * this.ActiveResourceData.ConversionRatio);

        var resourceType = this.ActiveResourceData.ResourceType;

        ResourceManager.IN.AddResource(resourceType, amountToCollect);

        int collectedAmount = amountToCollect;
        this.DecorationData.CurrentAmount = 0;
        
        SetActiveResourceData(null);
        ResourceManager.OnResourceGained?.Invoke(resourceType, collectedAmount);
        OnCollected(collectedAmount);
        RefreshQuantityDisplay();

        return true;
    }
    
    protected virtual void SetActiveResourceData(CollectableResourceData inResourceData)
    {
        this.ActiveResourceData = inResourceData;

        if (this.activeResourceDisplay != null)
        {
            this.activeResourceDisplay.gameObject.SetActive(inResourceData != null && inResourceData.ResourceType != EResourceType.None);

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