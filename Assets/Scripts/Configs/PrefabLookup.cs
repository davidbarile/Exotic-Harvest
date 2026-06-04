using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PrefabLookup", menuName = "Exotic Harvest/PrefabLookup")]
public class PrefabLookup : ScriptableObject
{
    [Serializable]
    public class PrefabPair
    {
        public string Name;
        public GameObject Prefab;
    }

    [SerializeField] private PrefabPair[] prefabPairs = new PrefabPair[0];

    public Dictionary<string, GameObject> PrefabDictionary => this.prefabDictionary;
    private Dictionary<string, GameObject> prefabDictionary = new();

#if UNITY_EDITOR
    private void OnValidate()
    {
        var nameSet = new HashSet<string>();
        for (int i = 0; i < this.prefabPairs.Length; i++)
        {
            var pair = this.prefabPairs[i];
            if (string.IsNullOrWhiteSpace(pair.Name))
            {
                if (pair.Prefab != null)
                    pair.Name = pair.Prefab.name; 
            }

            // Ensure prefab names are unique
            if (!nameSet.Add(pair.Name))
            {
                Debug.LogError($"[PrefabLookup] Duplicate prefab name detected: {pair.Name}. Please ensure all prefab names are unique.");
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    private void OnEnable()
    {
        this.prefabDictionary = new Dictionary<string, GameObject>();

        foreach (var pair in this.prefabPairs)
        {
            if (string.IsNullOrWhiteSpace(pair.Name))
                continue;
                
            if (!this.prefabDictionary.ContainsKey(pair.Name))
            {
                this.prefabDictionary.Add(pair.Name, pair.Prefab);
            }
            else
            {
                Debug.LogError($"[PrefabLookup] Duplicate prefab name detected: {pair.Name}. Skipping.");
            }
        }
    }

    public GameObject GetPrefabByName(string name)
    {
        if (this.prefabDictionary.TryGetValue(name, out var prefab))
        {
            return prefab;
        }
        else
        {
            Debug.LogError($"[PrefabLookup] Prefab with name '{name}' not found.");
            return null;
        }
    }
}