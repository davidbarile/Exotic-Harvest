using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Lean.Pool;
using static GlobalEnums;

public class UiResourcesPanel : UIPanelBase
{
    [Header("UI Settings")]
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private EResourceCategory categoriesToShow;
    [SerializeField] private bool showAllResources;

    [SerializeField] private int maxItemsPerRow = 15;

    private List<GameObject> allResourceObjects = new();

    private Dictionary<EResourceType, UiResourceDisplay> activeDisplaysDict = new();
    
    private ResourceConfig[] visibleResources = new ResourceConfig[0];
    
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

    public override void Show()
    {
        base.Show();
        RefreshGridLayout();
    }

    public void HandleShowAllToggleChanged(bool value)
    {
        this.showAllResources = value;
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

        this.visibleResources = resourcesToShow.Where(r => ResourceManager.IN.GetResourceAmount(r.ResourceType) > 0 || this.showAllResources).ToArray();

        if (ResourceManager.IN.DebugGrantAllResources)
            GrantAllResources();
        else
            RefreshGridLayout();
    }

    public void RefreshGridLayout()
    {
        this.grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
         
        if (this.visibleResources.Length < this.maxItemsPerRow + 1)
        {
            this.grid.constraintCount = 1;
        }
        else if (this.visibleResources.Length < this.maxItemsPerRow * 2 + 1)
        {
            this.grid.constraintCount = 2;
        }
        else
        {
            this.grid.constraintCount = this.maxItemsPerRow;
            this.grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        }

        // this.grid.CalculateLayoutInputHorizontal();
        // this.grid.CalculateLayoutInputVertical();
        // this.grid.SetLayoutHorizontal();
        // this.grid.SetLayoutVertical();
    }
    
    public void GrantAllResources()
    {
        var resourcesToShow = GetResourcesToDisplay();
        foreach (var resource in resourcesToShow)
        {
            ResourceManager.IN.AddResource(resource.ResourceType, ResourceManager.IN.DebugAddAmount);
        }
    }
    
    private ResourceConfig[] GetResourcesToDisplay()
    {
        var allResources = ResourceManager.IN.Database.AllResources;
        
        return allResources.Where(r => r != null && this.categoriesToShow.HasFlag(r.Category)).ToArray();
    }
    
    private void CreateResourceDisplay(ResourceConfig resourceConfig)
    {
        UiResourceDisplay displayUI = Pool.Spawn<UiResourceDisplay>($"ResourceDisplayUI", this.grid.transform);
        this.allResourceObjects.Add(displayUI.gameObject);
        
        displayUI.Configure(resourceConfig.ResourceType, resourceConfig, true);
        this.activeDisplaysDict[resourceConfig.ResourceType] = displayUI;
        
        OnResourceChanged(resourceConfig.ResourceType, ResourceManager.IN.GetResourceAmount(resourceConfig.ResourceType));
    }
    
    private void OnResourceChanged(EResourceType type, int newAmount)
    {            
        if (this.activeDisplaysDict.TryGetValue(type, out UiResourceDisplay display))
        {
            display.gameObject.SetActive(newAmount > 0 || this.showAllResources);
            RefreshGridLayout();
        }
    }
    
    public void RefreshAllDisplays()
    {
        // Clear existing displays
        foreach (var resourceObject in this.allResourceObjects)
        {
            LeanPool.Despawn(resourceObject);
        }
        
        this.activeDisplaysDict.Clear();
        this.allResourceObjects.Clear();
        
        // Recreate displays
        CreateResourceDisplays();
    }
}