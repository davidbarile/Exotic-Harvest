using UnityEngine;

/// <summary>
/// Raindrop collectable - appears during rain, collect by dragging bucket
/// </summary>
public class Raindrop : Collectable
{
    [SerializeField] private float fallSpeed = 5f;
    private bool isFalling = true;
    
    protected override void Start()
    {
        resourceType = ResourceType.Water;
        amount = 1;
        collectionMethod = CollectionMethod.Drag;
        lifetime = 10f; // Raindrops fall quickly
        autoDestroy = false; // Will destroy when hitting ground
        
        base.Start();
    }
    
    private void Update()
    {
        if (isFalling)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
            
            // Check if hit ground (assuming y = -5 is ground level)
            if (transform.position.y <= -5f)
            {
                HitGround();
            }
        }
    }
    
    private void HitGround()
    {
        isFalling = false;
        // TODO: Add splash effect
        Destroy(gameObject);
    }
    
    public override void OnDragOver()
    {
        if (isFalling) // Can only collect while falling
            base.OnDragOver();
    }
    
    protected override void OnCollected()
    {
        isFalling = false;
        // TODO: Add collection effect
        base.OnCollected();
    }
}