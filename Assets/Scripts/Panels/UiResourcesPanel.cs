using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UiResourcesPanel : UIPanelBase
{
    [Header("UI Settings")]
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private ContentSizeFitter contentSizeFitter;
    [SerializeField] private EResourceCategory categoriesToShow;
    [SerializeField] private bool showOnlyOwnedResources = true;

    [SerializeField] private int maxItemsPerRow = 15;

    private List<GameObject> allResourceObjects = new();
    
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
    
    public void HandleShowAllToggleChanged(bool value)
    {
        this.showOnlyOwnedResources = !value;
        RefreshAllDisplays();
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
        this.allResourceObjects.Add(displayUI.gameObject);
        
        displayUI.Configure(resourceConfig.ResourceType, resourceConfig, true);
        this.activeDisplaysDict[resourceConfig.ResourceType] = displayUI;
        
        OnResourceChanged(resourceConfig.ResourceType, ResourceManager.IN.GetResourceAmount(resourceConfig.ResourceType));
    }
    
    private void OnResourceChanged(EResourceType type, int newAmount)
    {            
        if (this.activeDisplaysDict.TryGetValue(type, out ResourceDisplayUI display))
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