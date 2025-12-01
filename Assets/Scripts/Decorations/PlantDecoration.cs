using UnityEngine;

/// <summary>
/// Plant decoration - purely visual, no resource generation (Phase 1)
/// </summary>
public class PlantDecoration : DecorationBase
{
    [Header("Plant Animation")]
    [SerializeField] private float swayAmount = 0.5f;
    [SerializeField] private float swaySpeed = 1f;
    [SerializeField] private Transform[] swayingParts;
    
    private Vector3[] originalRotations;
    
    protected override void Start()
    {
        decorationType = DecorationType.Plant;
        decorationName = "Jungle Plant";
        
        // Store original rotations
        if (swayingParts != null && swayingParts.Length > 0)
        {
            originalRotations = new Vector3[swayingParts.Length];
            for (int i = 0; i < swayingParts.Length; i++)
            {
                if (swayingParts[i] != null)
                    originalRotations[i] = swayingParts[i].localEulerAngles;
            }
        }
        
        base.Start();
    }
    
    private void Update()
    {
        // Gentle swaying animation
        if (swayingParts != null && originalRotations != null)
        {
            for (int i = 0; i < swayingParts.Length; i++)
            {
                if (swayingParts[i] != null)
                {
                    float sway = Mathf.Sin(Time.time * swaySpeed + i) * swayAmount;
                    Vector3 rotation = originalRotations[i];
                    rotation.z += sway;
                    swayingParts[i].localEulerAngles = rotation;
                }
            }
        }
    }
    
    protected override void OnPlaced()
    {
        // TODO: Add placement effects
    }
}