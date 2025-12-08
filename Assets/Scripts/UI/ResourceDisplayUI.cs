using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for displaying a single resource amount
/// </summary>
public class ResourceDisplayUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image backgroundImage;
    
    private ResourceType resourceType;
    private ResourceConfig resourceConfig;
    
    public void Initialize(ResourceType type, ResourceConfig config)
    {
        resourceType = type;
        resourceConfig = config;
        
        UpdateDisplay();
        
        // Subscribe to resource changes
        if (ResourceManager.IN != null)
        {
            ResourceManager.OnResourceChanged += OnResourceChanged;
        }
    }
    
    private void OnDestroy()
    {
        if (ResourceManager.IN != null)
        {
            ResourceManager.OnResourceChanged -= OnResourceChanged;
        }
    }
    
    private void OnResourceChanged(ResourceType type, int newAmount)
    {
        if (type == resourceType)
        {
            UpdateDisplay();
        }
    }
    
    private void UpdateDisplay()
    {
        if (ResourceManager.IN == null) return;
        
        int currentAmount = ResourceManager.IN.GetResourceAmount(resourceType);
        
        // Update amount text
        if (amountText != null)
        {
            amountText.text = currentAmount.ToString();
            amountText.color = resourceConfig?.UiColor ?? Color.white;
        }
        
        // Update icon
        if (iconImage != null && resourceConfig != null)
        {
            iconImage.sprite = resourceConfig.Icon;
            iconImage.color = resourceConfig.UiColor;
        }
        
        // Update background color based on resource category
        if (backgroundImage != null && resourceConfig != null)
        {
            Color bgColor = GetCategoryColor(resourceConfig.Category);
            bgColor.a = 0.3f;
            backgroundImage.color = bgColor;
        }
    }
    
    private Color GetCategoryColor(ResourceCategory category)
    {
        return category switch
        {
            ResourceCategory.Primary => new Color(0.2f, 0.6f, 1f), // Blue
            ResourceCategory.Bugs => new Color(0.8f, 0.4f, 0.2f), // Orange
            ResourceCategory.Nature => new Color(0.2f, 0.8f, 0.2f), // Green
            ResourceCategory.NightSky => new Color(0.4f, 0.2f, 0.8f), // Purple
            ResourceCategory.Valuables => new Color(1f, 0.8f, 0.2f), // Gold
            ResourceCategory.Abstract => new Color(0.6f, 0.6f, 0.6f), // Gray
            ResourceCategory.Special => new Color(1f, 0.2f, 0.8f), // Pink
            ResourceCategory.Premium => new Color(0.8f, 0.2f, 0.2f), // Red
            _ => Color.white
        };
    }
}