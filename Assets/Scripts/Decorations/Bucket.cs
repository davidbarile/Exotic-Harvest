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
    [SerializeField] private Image bucketImage;
    [SerializeField] private Image waterFillImage; // Shows water level
    [SerializeField] private float fillAnimationDuration = 0.5f;
    
    private float targetFillAmount;
    
    protected override void Start()
    {
        // Setup bucket properties
        decorationType = DecorationType.Bucket;
        decorationName = "Water Bucket";
        generatedResource = ResourceType.Water;
        maxCapacity = 5;
        generationInterval = 5f; // Collect water every 5 seconds during rain
        requiresSpecificConditions = true;
        
        // Initialize UI components
        if (bucketImage == null)
            bucketImage = GetComponent<Image>();
        if (waterFillImage == null)
            waterFillImage = transform.Find("WaterFill")?.GetComponent<Image>();
            
        base.Start();
        UpdateWaterVisual();
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
    
    private void UpdateWaterVisual()
    {
        if (waterFillImage == null)
            return;
            
        targetFillAmount = CapacityPercent;
        
        // Smooth fill animation
        waterFillImage.DOFillAmount(targetFillAmount, fillAnimationDuration)
            .SetEase(Ease.OutQuad);
    }
    
    // Update method removed - DOTween handles all animations automatically
}