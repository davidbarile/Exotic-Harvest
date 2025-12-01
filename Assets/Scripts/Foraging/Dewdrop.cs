using UnityEngine;

/// <summary>
/// Dewdrop collectable - appears in morning, click to collect water
/// </summary>
public class Dewdrop : Collectable
{
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 0.1f;
    
    private Vector3 startPosition;
    
    protected override void Start()
    {
        resourceType = ResourceType.Water;
        amount = 1;
        collectionMethod = CollectionMethod.Click;
        lifetime = 60f; // Dewdrops last longer
        
        startPosition = transform.position;
        base.Start();
    }
    
    private void Update()
    {
        // Gentle bobbing animation
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = startPosition + Vector3.up * bob;
    }
    
    protected override void OnCollected()
    {
        // TODO: Add particle effects, sound
        base.OnCollected();
    }
    
    private void OnMouseDown()
    {
        OnClick();
    }
}