using System;
using System.Collections.Generic;
using UnityEngine;
using static GlobalEnums;

public class LootManager : MonoBehaviour
{
    public static LootManager IN;

    //[SerializeField] private LootConfig[] lootConfigs;

    [SerializeField] private List<LootBundle> lootBundles;

    public LootConfig GetLootConfig(string lootName)
    {
        foreach (var bundle in this.lootBundles)
        {
            foreach (var config in bundle.LootConfigs)
            {
                if (config.DisplayName == lootName)
                {
                    return config;
                }
            }
        }

        Debug.LogError($"LootManager.GetLootConfig()  No LootConfig found with DisplayName: {lootName}");
        return null;
    }

    public List<LootConfig> GetLootConfigsOfType(ELootType lootType, ETimeOfDay timeOfDay = ETimeOfDay.All, EDayOfWeek dayOfWeek = EDayOfWeek.All)
    {
        var lootConfigsOfType = new List<LootConfig>();

        foreach (var bundle in this.lootBundles)
        {
            if (bundle.LootType == lootType &&
                (timeOfDay == ETimeOfDay.All || bundle.TimeOfDay.HasFlag(timeOfDay)) &&
                (dayOfWeek == EDayOfWeek.All || bundle.DayOfWeek.HasFlag(dayOfWeek)))
            {
                lootConfigsOfType.AddRange(bundle.LootConfigs);
            }
        }
        return lootConfigsOfType;
    }

    public LootConfig GetRandomLootConfigOfType(ELootType lootType, ETimeOfDay timeOfDay = ETimeOfDay.All, EDayOfWeek dayOfWeek = EDayOfWeek.All)
    {
        var configs = GetLootConfigsOfType(lootType, timeOfDay, dayOfWeek);
        if (configs.Count == 0)
        {
            Debug.LogWarning($"No LootConfigs found for type {lootType} at time {timeOfDay} and day {dayOfWeek}");
            return null;
        }
        return configs[UnityEngine.Random.Range(0, configs.Count)];
    }

    public List<LootData> GetLootDatas(EResourceType type, int maxLootTypes = 1)
    {
        var lootDataList = new List<LootData>();
        foreach (var bundle in this.lootBundles)
        {
            foreach (var config in bundle.LootConfigs)
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
        }
        return lootDataList;
    }

    // public string GetLootSpriteName(string inDisplayName)
}

[Serializable]
public class LootBundle
{
    public ELootType LootType;
    public ETimeOfDay TimeOfDay;
    public EDayOfWeek DayOfWeek;
    public List<LootConfig> LootConfigs;
}