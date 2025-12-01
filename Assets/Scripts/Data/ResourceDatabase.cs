using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Database of all resource definitions
/// </summary>
[CreateAssetMenu(fileName = "ResourceDatabase", menuName = "Exotic Harvest/Resource Database")]
public class ResourceDatabase : ScriptableObject
{
    [Header("All Resources")]
    [SerializeField] private ResourceDefinition[] allResources;
    
    private Dictionary<ResourceType, ResourceDefinition> resourceLookup;
    private Dictionary<string, ResourceDefinition> resourceByIdLookup;
    
    public ResourceDefinition[] AllResources => allResources;
    
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
        if (allResources == null) return;
        
        resourceLookup = new Dictionary<ResourceType, ResourceDefinition>();
        resourceByIdLookup = new Dictionary<string, ResourceDefinition>();
        
        foreach (var resource in allResources)
        {
            if (resource != null)
            {
                resourceLookup[resource.resourceType] = resource;
                resourceByIdLookup[resource.ID] = resource;
            }
        }
    }
    
    public ResourceDefinition GetResource(ResourceType type)
    {
        if (resourceLookup == null) BuildLookupTables();
        resourceLookup.TryGetValue(type, out ResourceDefinition resource);
        return resource;
    }
    
    public ResourceDefinition GetResource(string id)
    {
        if (resourceByIdLookup == null) BuildLookupTables();
        resourceByIdLookup.TryGetValue(id, out ResourceDefinition resource);
        return resource;
    }
    
    public ResourceDefinition[] GetResourcesByCategory(ResourceCategory category)
    {
        if (allResources == null) return new ResourceDefinition[0];
        return allResources.Where(r => r != null && r.category == category).ToArray();
    }
    
    public ResourceDefinition[] GetAvailableResources()
    {
        if (allResources == null) return new ResourceDefinition[0];
        return allResources.Where(r => r != null && r.IsCurrentlyAvailable()).ToArray();
    }
    
    public ResourceDefinition[] GetForageableResources()
    {
        if (allResources == null) return new ResourceDefinition[0];
        return allResources.Where(r => r != null && r.canBeActivelyForaged && r.IsCurrentlyAvailable()).ToArray();
    }
    
    public ResourceDefinition[] GetPassiveResources()
    {
        if (allResources == null) return new ResourceDefinition[0];
        return allResources.Where(r => r != null && r.canBePassivelyGenerated).ToArray();
    }
    
    [ContextMenu("Auto-Populate Resources")]
    private void AutoPopulateResources()
    {
        // This would be called in editor to automatically find all ResourceDefinition assets
        var resourceGuids = UnityEditor.AssetDatabase.FindAssets("t:ResourceDefinition");
        var foundResources = new List<ResourceDefinition>();
        
        foreach (var guid in resourceGuids)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var resource = UnityEditor.AssetDatabase.LoadAssetAtPath<ResourceDefinition>(path);
            if (resource != null)
                foundResources.Add(resource);
        }
        
        allResources = foundResources.ToArray();
        BuildLookupTables();
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Auto-populated {allResources.Length} resources");
    }
}