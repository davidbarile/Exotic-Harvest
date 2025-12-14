using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Bucket decoration - collects water during rain (Phase 1 MVP)
/// UI-based for desktop overlay gameplay
/// </summary>
public class Bucket : PassiveHarvester
{
    public EResourceType CollectableResourceTypes;

    [Header("Bucket UI Components")]
    [SerializeField] private Image waterFillImage; // Shows water level
    [SerializeField] private float fillAnimationDuration = 0.5f;
    
    private float targetFillAmount;

    protected override void Start()
    {
        base.Start();
        UpdateWaterVisual(false);
        RefreshQuantityText();
    }
    
    protected override bool CheckGenerationConditions()
    {
        // Only generate during rain
        return WeatherManager.IN != null && WeatherManager.IN.IsRaining;
    }
    
    protected override int GetGenerationAmount()
    {
        if (WeatherManager.IN != null)
        {
            // More water during heavier rain
            float intensity = WeatherManager.IN.WeatherIntensity;
            return Mathf.RoundToInt(1 + intensity); // 1-2 water per generation
        }
        return 1;
    }
    
    protected override void OnGenerated(int amount)
    {
        UpdateWaterVisual();
        // TODO: Add water drop effects
    }
    
    protected override void OnCollected(int amount)
    {
        UpdateWaterVisual();
        // TODO: Add collection effects
    }
    
    private void UpdateWaterVisual(bool shouldAnimate = true)
    {
        if (this.waterFillImage == null)
            return;

        // Smooth fill animation
        if (shouldAnimate)
        {
            this.targetFillAmount = this.CapacityPercent;

            this.waterFillImage.DOFillAmount(this.targetFillAmount, this.fillAnimationDuration)
                .SetEase(Ease.OutQuad);
            return;
        }
        
        this.waterFillImage.fillAmount = this.targetFillAmount;
    }
}