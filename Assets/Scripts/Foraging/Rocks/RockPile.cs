using System.Collections.Generic;
using UnityEngine;

public class RockPile : MonoBehaviour
{
    [SerializeField] private int rockCount;
    [SerializeField] private RectTransform rockSpawnArea;
    [Range(0f, 100f), SerializeField] private float gridSize;
    [Range(0f, 30f), SerializeField] private float offsetRange;
    [SerializeField] private Vector2 rockMinMaxScale = new Vector2(0.35f, 0.75f);
    [SerializeField] private Gradient rockColorGradient;

    [SerializeField] private LootConfig lootConfig;

    private List<Rock> allRocks = new();

    public void SpawnRocks()
    {
        ClearRocks();
        
        var rockPositions = ForagingManager.GetRandomPositions(this.rockSpawnArea, this.rockCount, this.gridSize, this.offsetRange);

        for (int i = 0; i < rockPositions.Count; i++)
        {
            var newRock = PrefabManager.IN.SpawnPrefab<Rock>($"Rock", this.rockSpawnArea);
            newRock.name = $"Rock_{i}";
            newRock.gameObject.SetActive(true);

            //set rocks to grid with random offset, rotation, scale and color variation
            newRock.transform.localPosition = rockPositions[i];

            newRock.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-30f, 30f));
            newRock.transform.localScale = Vector3.one * Random.Range(this.rockMinMaxScale.x, this.rockMinMaxScale.y);
            newRock.SetColor(this.rockColorGradient.Evaluate(Random.Range(0f, 1f)));
            newRock.Configure(this.lootConfig);
            this.allRocks.Add(newRock);
        }
    }

    public void ClearRocks()
    {
        foreach (var rock in this.allRocks)
        {
            if (rock != null)
                Destroy(rock.gameObject);
        }
        this.allRocks.Clear();
    }
}