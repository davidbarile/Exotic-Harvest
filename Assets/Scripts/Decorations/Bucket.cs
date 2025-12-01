using UnityEngine;

/// <summary>
/// Bucket decoration - collects water during rain (Phase 1 MVP)
/// </summary>
public class Bucket : PassiveHarvester
{
    [Header("Bucket Specific")]
    [SerializeField] private Transform waterVisual;
    [SerializeField] private float fillAnimationSpeed = 2f;
    
    private Vector3 originalWaterScale;
    
    protected override void Start()
    {
        // Setup bucket properties
        decorationType = DecorationType.Bucket;
        decorationName = "Water Bucket";
        generatedResource = ResourceType.Water;
        maxCapacity = 5;
        generationInterval = 5f; // Collect water every 5 seconds during rain
        requiresSpecificConditions = true;
        
        if (waterVisual != null)
            originalWaterScale = waterVisual.localScale;
            
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
        if (waterVisual == null)
            return;
            
        float fillPercent = CapacityPercent;
        Vector3 targetScale = new Vector3(
            originalWaterScale.x,
            originalWaterScale.y * fillPercent,
            originalWaterScale.z
        );
        
        waterVisual.localScale = targetScale;
    }
    
    private void Update()
    {
        // Smooth water level animation
        if (waterVisual != null)
        {
            float targetY = originalWaterScale.y * CapacityPercent;
            float currentY = waterVisual.localScale.y;
            float newY = Mathf.MoveTowards(currentY, targetY, fillAnimationSpeed * Time.deltaTime);
            
            waterVisual.localScale = new Vector3(
                originalWaterScale.x,
                newY,
                originalWaterScale.z
            );
        }
    }
}