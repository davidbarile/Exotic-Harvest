using UnityEngine;

public class Moonbeam : Collectable
{
    [SerializeField] private Animation anim;
    
    public override void Spawn()
    {
        base.Spawn();

        this.gameObject.SetActive(true);

        this.anim.Rewind();
        this.anim.Play();
    }
}
