using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Bucket decoration - collects water during rain (Phase 1 MVP)
/// UI-based for desktop overlay gameplay
/// </summary>
public class Bucket : PassiveHarvester
{
    [Header("Bucket UI Components")]
    [SerializeField] private Image fillImage;
    [Range(0, 2),SerializeField] private float fillAnimationDuration = 0.5f;
    
    private float targetFillAmount;

    protected override void Start()
    {
        base.Start();
        RefreshQuantityDisplay();
    }

    protected override void RefreshQuantityDisplay()
    {
        base.RefreshQuantityDisplay();
        UpdateWaterMeter(false);
    }
    
    protected override bool CheckGenerationConditions()
    {
        // Only generate during rain
        return WeatherManager.IsRaining;
    }
    
    protected override int GetGenerationAmount()
    {
        if (WeatherManager.IN != null)
        {
            // More water during heavier rain
            float intensity = WeatherManager.WeatherIntensity;
            return Mathf.RoundToInt(1 + intensity); // 1-2 water per generation
        }
        return 1;
    }
    
    protected override void OnGenerated(int amount)
    {
        UpdateWaterMeter();
        // TODO: Add water drop effects
    }
    
    protected override void OnCollected(int amount)
    {
        UpdateWaterMeter();
        // TODO: Add collection effects
    }
    
    private void UpdateWaterMeter(bool shouldAnimate = true)
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
}