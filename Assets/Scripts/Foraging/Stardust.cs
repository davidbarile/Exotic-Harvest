using UnityEngine;

public class Stardust : Collectable
{
    public override void Spawn()
    {
        base.Spawn();

        this.gameObject.SetActive(true);
    }

    public void Reset()
    {
        this.gameObject.SetActive(false);
    }
}