using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class RockPile : MonoBehaviour
{
    [SerializeField] private int rockCount;
    [SerializeField] private RectTransform rockSpawnArea;
    [Range(0f, 100f), SerializeField] private float gridSize;
    [Range(0f, 30f), SerializeField] private float offsetRange;
    [SerializeField] private Vector2 rockMinMaxScale = new Vector2(0.35f, 0.75f);
    [SerializeField] private Gradient rockColorGradient;
    [SerializeField] private LootConfig lootConfig;

    private List<Vector3> rockSpawnPositions = new();
    private List<Rock> activeRocks = new();

    public void InitRockPositions()
    {
        // If no predefined positions, generate a grid of positions within the spawn area
        this.rockSpawnPositions = ForagingManager.GetRandomPositions(this.rockSpawnArea, this.rockCount, this.gridSize, this.offsetRange);

        var layerMask = LayerMask.GetMask("RocksSpawn");

        // Raycast to only show elements in colliders
        for (int i = 0; i < this.rockSpawnPositions.Count; i++)
        {
            var screenPos = this.rockSpawnPositions[i];// + DragManager.ScreenToWorldCameraDelta;
            var worldPos = this.rockSpawnArea.TransformPoint(screenPos);

            Collider2D hitCollider = Physics2D.OverlapPoint(worldPos, layerMask);

            if (hitCollider == null)
            {
                // No collider at this position, remove it from the list
                this.rockSpawnPositions.RemoveAt(i);
                i--;
            }
            else
            {
                //this.rockSpawnPositions[i] = new Vector3(screenPos.x, screenPos.y, hitCollider.transform.position.z);
            }
        }
    }

    public void SpawnRocks()
    {
        ClearRocks(); 

        for (int i = 0; i < this.rockSpawnPositions.Count; i++)
        {
            var newRock = PrefabManager.IN.SpawnPrefab<Rock>($"Rock", this.rockSpawnArea);
            newRock.name = $"Rock_{i}";
            newRock.gameObject.SetActive(true);

            //set rocks to grid with random offset, rotation, scale and color variation
            newRock.transform.localPosition = this.rockSpawnPositions[i];

            newRock.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-30f, 30f));
            newRock.transform.localScale = Vector3.one * Random.Range(this.rockMinMaxScale.x, this.rockMinMaxScale.y);
            newRock.SetColor(this.rockColorGradient.Evaluate(Random.Range(0f, 1f)));
            newRock.Configure(this.lootConfig);
            this.activeRocks.Add(newRock);
        }
    }

    public void ClearRocks()
    {
        foreach (var rock in this.activeRocks)
        {
            if (rock != null)
                Destroy(rock.gameObject);
        }
        this.activeRocks.Clear();
    }
}