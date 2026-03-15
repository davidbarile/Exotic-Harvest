using System.Collections.Generic;
using UnityEngine;
using static LootData;

public class LootManager : MonoBehaviour
{
    public static LootManager IN;

    [SerializeField] private LootConfig[] lootConfigs;

    public LootConfig GetLootConfig(string lootName)
    {
        foreach (var config in this.lootConfigs)
        {
            if (config.DisplayName == lootName)
            {
                return config;
            }
        }

        Debug.LogError($"LootManager.GetLootConfig()  No LootConfig found with DisplayName: {lootName}");
        return null;
    }

    public List<LootConfig> GetLootConfigsOfType(ELootType lootType)
    {
        var lootConfigsOfType = new List<LootConfig>();
        
        foreach(var config in this.lootConfigs)
        {
            if(config.LootType == lootType)
            {
                lootConfigsOfType.Add(config);
            }
        }
        return lootConfigsOfType;
    }

    public List<LootData> GetLootDatas(EResourceType type, int maxLootTypes = 1)
    {
        var lootDataList = new List<LootData>();
        foreach (var config in this.lootConfigs)
        {
            foreach (var lootData in config.LootDatas)
            {
                if (lootData.ResourceType == type)
                {
                    lootDataList.Add(lootData);
                    if (lootDataList.Count >= maxLootTypes)
                        break;
                }
            }
        }
        return lootDataList;
    }
}