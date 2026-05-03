using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonbeamGenerator : MonoBehaviour
{
    [SerializeField] private Moonbeam[] moonbeams;

    public void SpawnMoonbeams()
    {
        StopAllCoroutines();
        StartCoroutine(SpawnMoonbeamsCo());
    }
    
    private IEnumerator SpawnMoonbeamsCo()
    {
        var numToSpawn = Random.Range(1, 6); // Spawn 1-5 moonbeams
        var moonbeamsToSpawn = new List<Moonbeam>(this.moonbeams);
        moonbeamsToSpawn.RandomizeList();
        moonbeamsToSpawn = moonbeamsToSpawn.GetRange(0, numToSpawn);

        var delayBetweenSpawns = Random.Range(0.2f, 0.5f); // Random delay between spawns

        foreach (var moonbeam in this.moonbeams)
        {
            if (moonbeamsToSpawn.Contains(moonbeam))
            {
                moonbeam.Spawn();
                delayBetweenSpawns = Random.Range(0.3f, 0.6f);
                yield return new WaitForSeconds(delayBetweenSpawns); // Stagger spawn times
            }
            else
                moonbeam.gameObject.SetActive(false);
        }
    }
}