using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager IN;

    [Header("Prefabs")]
    [SerializeField] private PrefabLookup prefabLookup;

    public GameObject GetPrefabByName(string name)
    {
        return this.prefabLookup.GetPrefabByName(name);
    }

    public T SpawnPrefab<T>(string prefabName, Transform parent, Vector3 position = default, Quaternion rotation = default) where T : Component
    {
        GameObject prefab = GetPrefabByName(prefabName);
        if (prefab != null)
        {
            GameObject instance = Instantiate(prefab, position, rotation, parent);
            return instance.GetComponent<T>();
        }
        else
        {
            Debug.LogError($"[PrefabManager] Prefab not found: {prefabName}");
            return null;
        }
    }
}