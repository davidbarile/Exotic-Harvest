using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//TODO: add toggle to show all resources vs only owned resources
public class UiResourcesPanel : UIPanelBase
{
    [Header("UI Settings")]
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private ContentSizeFitter contentSizeFitter;
    [SerializeField] private EResourceCategory categoriesToShow;
    [SerializeField] private bool showOnlyOwnedResources = true;

    [SerializeField] private int maxItemsPerRow = 15;
    
    private Dictionary<EResourceType, ResourceDisplayUI> activeDisplaysDict = new();
    
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

        if (resourcesToShow.Length < this.maxItemsPerRow)
        {
            this.grid.constraint = GridLayoutGroup.Constraint.Flexible;
            this.contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }
        else
        {
            this.grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            this.grid.constraintCount = this.maxItemsPerRow;
            this.contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        
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
        ResourceDisplayUI displayUI = PrefabManager.IN.SpawnPrefab<ResourceDisplayUI>($"ResourceDisplayUI", this.grid.transform);
        
        displayUI.Configure(resourceConfig.ResourceType, resourceConfig, true);
        this.activeDisplaysDict[resourceConfig.ResourceType] = displayUI;
        
        OnResourceChanged(resourceConfig.ResourceType, ResourceManager.IN.GetResourceAmount(resourceConfig.ResourceType));
    }
    
    private void OnResourceChanged(EResourceType type, int newAmount)
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