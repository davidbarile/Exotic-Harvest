using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Manages the display of resource UI elements
/// </summary>
public class ResourceDisplayManager : MonoBehaviour
{
    public static ResourceDisplayManager IN;

    [Header("UI Settings")]
    [SerializeField] private ResourceDisplayUI resourceDisplayPrefab; // Assign ResourceDisplayUI prefab here
    [SerializeField] private Transform resourceDisplayParent;
    [SerializeField] private ResourceCategory categoriesToShow; // Which categories to display
    [SerializeField] private bool showOnlyOwnedResources = true;
    
    private Dictionary<ResourceType, ResourceDisplayUI> activeDisplaysDict = new();
    
    public void Init()
    {
        CreateResourceDisplays();
        ResourceManager.OnResourceChanged += OnResourceChanged;
    }
    
    private void OnDestroy()
    {
        if (ResourceManager.IN != null)
        {
            ResourceManager.OnResourceChanged -= OnResourceChanged;
        }
    }
    
    private void CreateResourceDisplays()
    {
        // Get resources to display
        var resourcesToShow = GetResourcesToDisplay();
        
        foreach (var resourceConfig in resourcesToShow)
        {
            CreateResourceDisplay(resourceConfig);
        }
    }
    
    private ResourceConfig[] GetResourcesToDisplay()
    {
        var allResources = ResourceManager.IN.Database.AllResources;
        
        return allResources.Where(r => r != null && this.categoriesToShow.HasFlag(r.Category)).ToArray();
    }
    
    private void CreateResourceDisplay(ResourceConfig resourceConfig)
    {
        ResourceDisplayUI displayUI = Instantiate(this.resourceDisplayPrefab, this.resourceDisplayParent);
        
        displayUI.Initialize(resourceConfig.ResourceType, resourceConfig);
        this.activeDisplaysDict[resourceConfig.ResourceType] = displayUI;
        
        OnResourceChanged(resourceConfig.ResourceType, ResourceManager.IN.GetResourceAmount(resourceConfig.ResourceType));
    }
    
    private void OnResourceChanged(ResourceType type, int newAmount)
    {            
        if (this.showOnlyOwnedResources)
        {
            if (this.activeDisplaysDict.TryGetValue(type, out ResourceDisplayUI display))
            {
                display.gameObject.SetActive(newAmount > 0);
            }
        }
    }
    
    public void RefreshAllDisplays()
    {
        // Clear existing displays
        foreach (var display in this.activeDisplaysDict.Values)
        {
            if (display != null)
                Destroy(display.gameObject);
        }
        this.activeDisplaysDict.Clear();
        
        // Recreate displays
        CreateResourceDisplays();
    }
    
    [ContextMenu("Refresh Displays")]
    private void RefreshDisplaysContextMenu()
    {
        RefreshAllDisplays();
    }
}