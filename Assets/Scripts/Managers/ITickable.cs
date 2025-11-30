using UnityEngine;

//maybe change to abstract class and just inherit from it
public interface ITickable
{
    public void Tick();
    public void SecondTick();

    // private void OnEnable()
    // {
    //     TickManager.OnTick += Tick;
    //     TickManager.OnSecondTick += SecondTick;
    // }

    // private void OnDisable()
    // {
    //     TickManager.OnTick -= Tick;
    //     TickManager.OnSecondTick -= SecondTick;
    // }
}