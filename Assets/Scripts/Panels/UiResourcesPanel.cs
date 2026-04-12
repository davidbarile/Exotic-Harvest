using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static GlobalEnums;

public class UiResourcesPanel : UIPanelBase
{
    [Header("UI Settings")]
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private EResourceCategory categoriesToShow;
    [SerializeField] private bool showOnlyOwnedResources = true;

    [SerializeField] private int maxItemsPerRow = 15;

    private List<GameObject> allResourceObjects = new();
    
    private Dictionary<EResourceType, UiResourceDisplay> activeDisplaysDict = new();
    
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
    
    public void HandleShowAllToggleChanged(bool value)
    {
        this.showOnlyOwnedResources = !value;
        RefreshAllDisplays();
    }
    
    private void CreateResourceDisplays()
    {
        // Get resources to display
        var resourcesToShow = GetResourcesToDisplay();

        foreach (var resourceConfig in resourcesToShow)
        {
            CreateResourceDisplay(resourceConfig);
        }

        var visibleResources = resourcesToShow.Where(r => ResourceManager.IN.GetResourceAmount(r.ResourceType) > 0 || !this.showOnlyOwnedResources).ToArray();

        this.grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

        if (visibleResources.Length < this.maxItemsPerRow + 1)
        {
            this.grid.constraintCount = 1;
        }
        else if (visibleResources.Length < this.maxItemsPerRow * 2 + 1)
        {
            this.grid.constraintCount = 2;
        }
        else
        {
            this.grid.constraintCount = this.maxItemsPerRow;
            this.grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        }

        if(ResourceManager.IN.DebugGrantAllResources)
        {
            foreach (var resource in resourcesToShow)
            {
                ResourceManager.IN.AddResource(resource.ResourceType, ResourceManager.IN.DebugAddAmount);
            }
        }
    }
    
    private ResourceConfig[] GetResourcesToDisplay()
    {
        var allResources = ResourceManager.IN.Database.AllResources;
        
        return allResources.Where(r => r != null && this.categoriesToShow.HasFlag(r.Category)).ToArray();
    }
    
    private void CreateResourceDisplay(ResourceConfig resourceConfig)
    {
        UiResourceDisplay displayUI = PrefabManager.IN.SpawnPrefab<UiResourceDisplay>($"ResourceDisplayUI", this.grid.transform);
        this.allResourceObjects.Add(displayUI.gameObject);
        
        displayUI.Configure(resourceConfig.ResourceType, resourceConfig, true);
        this.activeDisplaysDict[resourceConfig.ResourceType] = displayUI;
        
        OnResourceChanged(resourceConfig.ResourceType, ResourceManager.IN.GetResourceAmount(resourceConfig.ResourceType));
    }
    
    private void OnResourceChanged(EResourceType type, int newAmount)
    {            
        if (this.activeDisplaysDict.TryGetValue(type, out UiResourceDisplay display))
            display.gameObject.SetActive(newAmount > 0 || !this.showOnlyOwnedResources);
      
    }
    
    public void RefreshAllDisplays()
    {
        // Clear existing displays
        foreach (var resourceObject in this.allResourceObjects)
        {
            Destroy(resourceObject);
        }
        
        this.activeDisplaysDict.Clear();
        this.allResourceObjects.Clear();
        
        // Recreate displays
        CreateResourceDisplays();
    }
}