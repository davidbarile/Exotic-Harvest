using UnityEngine;

//maybe change to abstract class and just inherit from it
public interface ITickable
{
    public void Tick();
    public void SecondTick();

    // private void Start()
    // {
    //     TickManager.OnTick += Tick;
    //     TickManager.OnSecondTick += SecondTick;
    // }

    // private void OnDestroy()
    // {
    //     TickManager.OnTick -= Tick;
    //     TickManager.OnSecondTick -= SecondTick;
    // }
}