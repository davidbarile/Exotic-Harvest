using UnityEngine;

[RequireComponent(typeof(GameManager))]
[RequireComponent(typeof(ScreenManager))]
[RequireComponent(typeof(DragManager))]
[RequireComponent(typeof(UiManager))]
[RequireComponent(typeof(TickManager))]
[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(ResourceManager))]
[RequireComponent(typeof(TimeManager))]
[RequireComponent(typeof(WeatherManager))]
[RequireComponent(typeof(ForagingManager))]
[RequireComponent(typeof(DecorationManager))]
[RequireComponent(typeof(SaveManager))]
[RequireComponent(typeof(ShopManager))]
[RequireComponent(typeof(InventoryManager))]
[RequireComponent(typeof(NotificationManager))]
public class SingletonManager : MonoBehaviour
{
    [SerializeField] private UIConfirmPanel confirmPanel;
    public void Init()
    {
        GameManager.IN = this.GetComponent<GameManager>();
        ScreenManager.IN = this.GetComponent<ScreenManager>();
        DragManager.IN = this.GetComponent<DragManager>();
        UiManager.IN = this.GetComponent<UiManager>();
        TickManager.IN = this.GetComponent<TickManager>();
        InputManager.IN = this.GetComponent<InputManager>();
        ResourceManager.IN = this.GetComponent<ResourceManager>();
        TimeManager.IN = this.GetComponent<TimeManager>();
        WeatherManager.IN = this.GetComponent<WeatherManager>();
        ForagingManager.IN = this.GetComponent<ForagingManager>();
        DecorationManager.IN = this.GetComponent<DecorationManager>();
        SaveManager.IN = this.GetComponent<SaveManager>();
        ShopManager.IN = this.GetComponent<ShopManager>();
        InventoryManager.IN = this.GetComponent<InventoryManager>();
        NotificationManager.IN = this.GetComponent<NotificationManager>();

        UIConfirmPanel.IN = this.confirmPanel;
    }
}