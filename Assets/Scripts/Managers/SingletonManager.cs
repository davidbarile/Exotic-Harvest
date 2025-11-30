using UnityEngine;

[RequireComponent(typeof(GameManager))]
[RequireComponent(typeof(ScreenManager))]
[RequireComponent(typeof(UiManager))]
[RequireComponent(typeof(TickManager))]
[RequireComponent(typeof(InputManager))]
public class SingletonManager : MonoBehaviour
{
    public void Init()
    {
        GameManager.IN = this.GetComponent<GameManager>();
        ScreenManager.IN = this.GetComponent<ScreenManager>();
        UiManager.IN = this.GetComponent<UiManager>();
        TickManager.IN = this.GetComponent<TickManager>();
        InputManager.IN = this.GetComponent<InputManager>();
    }
}