using System.Collections.Generic;
using UnityEngine;
using static GlobalEnums;

public class RockPile : MonoBehaviour
{
    [SerializeField] private ETimeOfDay spawnTimeOfDay;
    [SerializeField] private RectTransform rockSpawnArea;
    [Range(0f, 100f), SerializeField] private float gridSize;
    [Range(0f, 30f), SerializeField] private float offsetRange;
    [Range(0f, 100f), SerializeField] private int spawnIterations = 1;
    [SerializeField] private Vector2 rockMinMaxScale = new Vector2(0.25f, 0.65f);
    [SerializeField] private Gradient rockColorGradient;

    private List<Vector3> rockSpawnPositions = new();
    private List<Rock> activeRocks = new();

    public void InitRockPositions()
    {
        // If no predefined positions, generate a grid of positions within the spawn area
        this.rockSpawnPositions = ForagingManager.GetRandomPositions(this.rockSpawnArea, inCount: -1, inGridSize: this.gridSize, inOffsetRange: this.offsetRange, inChanceToSpawn: 1f, inForceGridToSpawnAreaSize: true, inIterations: this.spawnIterations);

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

    public void ResetRocks()
    {
        if (this.activeRocks.Count == 0)
        {
            SpawnRocks();
            return;
        }
        
        foreach (var rock in this.activeRocks)
        {
            if (rock != null)
                rock.Reset();
        }
    }

    private void SpawnRocks()
    {
        ClearRocks(); 

        for (int i = 0; i < this.rockSpawnPositions.Count; i++)
        {
            var newRock = PrefabManager.IN.SpawnPrefab<Rock>($"Rock", this.rockSpawnArea);
            newRock.name = $"Rock_{i}";
            newRock.ParentRockPile = this;

            //set rocks to grid with random offset, rotation, scale and color variation
            newRock.SetPosition(this.rockSpawnPositions[i]);
            newRock.transform.localScale = Vector3.one * Random.Range(this.rockMinMaxScale.x, this.rockMinMaxScale.y);

            newRock.SetColor(this.rockColorGradient.Evaluate(Random.Range(0f, 1f)));

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

    public bool CanSpawnLoot()
    {
        var canSpawn = this.spawnTimeOfDay.HasFlag(TimeManager.CurrentTimeOfDay) || ForagingManager.IgnoreTimeOfDayAndWeather;
        // var color = canSpawn ? "green" : "red";
        // Debug.Log($"<color={color}>RockPile.CanSpawnLoot() Checking if can spawn loot. Time ofDay: {TimeManager.CurrentTimeOfDay}, SpawnTimeOfDay: {this.spawnTimeOfDay}, IgnoreTimeOfDayAndWeather: {ForagingManager.IgnoreTimeOfDayAndWeather}  CanSpawn: {canSpawn}</color>");
        return canSpawn;
    }
}