using UnityEngine;

public class MoonbeamGenerator : MonoBehaviour
{
    [SerializeField] private Moonbeam[] moonbeams;

    //enabled and disabled by day-night cycle anim
    private void OnEnable()
    {
        ForagingManager.IN.TryActivateMoonbeamGenerator();
    }

    //enabled and disabled by day-night cycle anim
    private void OnDisable()
    {
        ForagingManager.IN.DeactivateMoonbeamGenerator();
    }
    
    public void SpawnMoonbeam()
    {
        foreach (var moonbeam in this.moonbeams)
        {
            moonbeam.Spawn();
        }
    }
}