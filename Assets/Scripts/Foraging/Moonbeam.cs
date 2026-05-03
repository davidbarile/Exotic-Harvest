using UnityEngine;

public class Moonbeam : Collectable
{
    [SerializeField] private Animation anim;
    
    public override void Spawn()
    {
        base.Spawn();
        
        if (!this.anim.isPlaying)
            this.anim.Play();
    }
}
