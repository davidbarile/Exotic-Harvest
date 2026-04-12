using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GlobalEnums;

/// <summary>
/// Database of all resource definitions
/// </summary>
[CreateAssetMenu(fileName = "ResourceDatabase", menuName = "Exotic Harvest/ResourceData Database")]
public class ResourceDatabase : ScriptableObject
{
    [Header("All Resources")]
    [SerializeField] private ResourceConfig[] allResources;
    
    private Dictionary<EResourceType, ResourceConfig> resourceLookup = new();
    private Dictionary<string, ResourceConfig> resourceByIdLookup = new();
    
    public ResourceConfig[] AllResources => allResources;
    
    private void OnEnable()
    {
        BuildLookupTables();
    }
    
    private void OnValidate()
    {
        BuildLookupTables();
    }
    
    private void BuildLookupTables()
    {
        if (this.allResources == null) return;
        
        this.resourceLookup = new();
        this.resourceByIdLookup = new();
        
        foreach (var resource in this.allResources)
        {
            if (resource != null)
            {
                this.resourceLookup[resource.ResourceType] = resource;
                this.resourceByIdLookup[resource.ID] = resource;
            }
        }
    }
    
    public ResourceConfig GetResource(EResourceType type)
    {
        if (this.resourceLookup == null) BuildLookupTables();
        this.resourceLookup.TryGetValue(type, out ResourceConfig resource);
        return resource;
    }
    
    public ResourceConfig GetResource(string id)
    {
        if (this.resourceByIdLookup == null) BuildLookupTables();
        this.resourceByIdLookup.TryGetValue(id, out ResourceConfig resource);
        return resource;
    }
    
    public ResourceConfig[] GetResourcesByCategory(EResourceCategory category)
    {
        if (this.allResources == null) return new ResourceConfig[0];
        return this.allResources.Where(r => r != null && r.Category.HasFlag(category)).ToArray();
    }
    
    public ResourceConfig[] GetAvailableResources()
    {
        if (this.allResources == null) return new ResourceConfig[0];
        return this.allResources.Where(r => r != null && r.IsCurrentlyAvailable()).ToArray();
    }
    
    public ResourceConfig[] GetForageableResources()
    {
        if (this.allResources == null) return new ResourceConfig[0];
        return this.allResources.Where(r => r != null && r.CanBeActivelyForaged && r.IsCurrentlyAvailable()).ToArray();
    }

    public ResourceConfig[] GetPassiveResources()
    {
        if (this.allResources == null) return new ResourceConfig[0];
        return this.allResources.Where(r => r != null && r.CanBePassivelyGenerated).ToArray();
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-Populate resourcesSave")]
    private void AutoPopulateResources()
    {
        // This would be called in editor to automatically find all ResourceConfig assets
        var resourceGuids = UnityEditor.AssetDatabase.FindAssets("t:ResourceConfig");
        var foundResources = new List<ResourceConfig>();

        foreach (var guid in resourceGuids)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var resource = UnityEditor.AssetDatabase.LoadAssetAtPath<ResourceConfig>(path);
            if (resource != null)
                foundResources.Add(resource);
        }

        this.allResources = foundResources.ToArray();
        BuildLookupTables();

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Auto-populated {this.allResources.Length} resources");
    }
#endif
}