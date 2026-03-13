using UnityEngine;

public class RockPile : MonoBehaviour
{
    [SerializeField] private Rock rockPrefab;
    [SerializeField] private int rockCount;
    [SerializeField] private RectTransform rockSpawnArea;
    [Range(0f, 100f), SerializeField] private float gridSize;
    [Range(0f, 30f), SerializeField] private float offsetRange;
    [SerializeField] private Vector2 rockMinMaxScale = new Vector2(0.35f, 0.75f);
    [SerializeField] private Gradient rockColorGradient;

    private void Start()
    {
        SpawnRocks();
    }

    private void SpawnRocks()
    {
        var xPos = 0f;
        var yPos = 0f;

        for (int i = 0; i < this.rockCount; i++)
        {
            Rock newRock = Instantiate(this.rockPrefab, this.rockSpawnArea);
            newRock.name = $"Rock_{i}";
            if (xPos > this.rockSpawnArea.rect.width)
            {
                xPos = 0;
                yPos += this.gridSize;

                if (yPos > this.rockSpawnArea.rect.height)
                    yPos = 0;
            }
            else if (yPos > this.rockSpawnArea.rect.height)
            {
                yPos = 0;
                xPos = 0;
            }
            else
            {
                xPos += this.gridSize;
            }
            //newRock.transform.localPosition = GetRandomPositionWithinSpawnArea();

            newRock.transform.localPosition = new Vector3(xPos, yPos, 0f);
            newRock.transform.localPosition += new Vector3(
                Random.Range(-this.offsetRange, this.offsetRange),
                Random.Range(-this.offsetRange, this.offsetRange),
                0f
            );
            newRock.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-30f, 30f));
            newRock.transform.localScale = Vector3.one * Random.Range(this.rockMinMaxScale.x, this.rockMinMaxScale.y);
            newRock.SetColor(this.rockColorGradient.Evaluate(Random.Range(0f, 1f)));
        }
    }

    private Vector3 GetRandomPositionWithinSpawnArea()
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(0f, this.rockSpawnArea.rect.width),
            Random.Range(0f, this.rockSpawnArea.rect.height),
            0f
        );
        return spawnPosition;
    }
}