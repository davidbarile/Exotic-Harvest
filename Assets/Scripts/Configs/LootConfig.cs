using System.Collections.Generic;
using UnityEngine;
using static LootData;

[CreateAssetMenu(fileName = "LootConfig", menuName = "Exotic Harvest/LootConfig")]
public class LootConfig : ScriptableObject
{
    public string DisplayName;
    public ELootType LootType;
    public LootData[] LootDatas = new LootData[0];

    public LootData GetSingleRandomLoot(bool isGuaranteedDrop = false, int maxLootQuantity = 1)
    {
        var loots = GetRandomLoot(isGuaranteedDrop, maxLootQuantity, 1);
        return loots.Count > 0 ? loots[0] : null;
    }

    public List<LootData> GetRandomLoot(bool isGuaranteedDrop = false, int maxLootQuantity = 1, int maxLootTypes = 1)
    {
        if (this.LootDatas.Length == 0)
        {
            Debug.LogError($"LootConfig {this.name} has no LootDatas defined.");
            return null;
        }

        var selectedLoots = new List<LootData>();
        var randomLootDatas = new List<LootData>(this.LootDatas);
        randomLootDatas.RandomizeList();

        if (isGuaranteedDrop)
        {
            //add at least one loot based on the ChanceToDrop weights
            float totalChancesToDrop = 0f;
            foreach (var loot in randomLootDatas)
            {
                totalChancesToDrop += loot.ChanceToDrop.MaxQuantity;
            }

            float randomValue = Random.Range(0f, totalChancesToDrop);

            foreach (var loot in randomLootDatas)
            {
                if (randomValue <= loot.ChanceToDrop.MaxQuantity)
                {
                    selectedLoots.Add(loot);//add the first loot we land on based on the random value and break out of the loop
                        break;
                }
                totalChancesToDrop -= loot.ChanceToDrop.MaxQuantity;
                randomValue = Random.Range(0f, totalChancesToDrop);
            }
        }
       
        if(!isGuaranteedDrop || selectedLoots.Count > 0)
        {
            //then roll for additional possible loot based on the ChanceToDrop weights until we hit the maxLootTypes limit
            foreach (var loot in randomLootDatas)
            {
                var w = loot.ChanceToDrop;
                var chance = WeightedRandom.GetWeightedRandomFloat(w.MinQuantity, w.MaxQuantity, w.MinMaxWeightFactor);
                float randomValue = Random.Range(0f, 100f);
                if (randomValue <= chance)
                {
                    if (!selectedLoots.Contains(loot))
                        selectedLoots.Add(loot);
                        
                    if (selectedLoots.Count >= maxLootTypes)
                        break;
                }
            }
        }

        if(selectedLoots.Count > 0)
        {
            // Determine quantity to drop based on the QuantityToDrop weighted random
            foreach (var loot in selectedLoots)             
            {
                var q = loot.QuantityToDrop;
                loot.Quantity = Mathf.Clamp(WeightedRandom.GetWeightedRandomInt(q.MinQuantity, q.MaxQuantity, q.MinMaxWeightFactor), 1, maxLootQuantity);
            }
        }
        
        return selectedLoots;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        this.DisplayName = this.name.Substring(this.name.IndexOf("_") + 1, this.name.Length - this.name.IndexOf("_") - 1).Trim();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}