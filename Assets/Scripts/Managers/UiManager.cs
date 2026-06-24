using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiManager : MonoBehaviour
{
    public static UiManager IN;

    public UiSplashScreenPanel SplashScreenPanel => this.splashScreenPanel;

    public Camera WorldCamera => this.worldCamera;
    public Camera DragCamera => this.dragCamera;

    public RectTransform DragCanvas => this.dragCanvas;
    public Canvas WorldCanvas => this.worldCanvas;
    public Canvas UICanvas => this.uiCanvas;
    public RectTransform WorldRectTrans => this.worldRectTrans;

    public UiSettingsPanel SettingsPanel => this.settingsPanel;
    public UiShopPanel ShopPanel => this.shopPanel;
    public UiInventoryPanel InventoryPanel => this.inventoryPanel;
    public UiResourcesPanel ResourcesPanel => this.resourcesPanel;
    public UiTimeWeatherPanel TimeWeatherPanel => this.timeWeatherPanel;
    public Canvas NotificationsCanvas => this.notificationsCanvas;
    public GameObject DecorationsContainer => this.decorationsContainer;
    public GameObject UiButtonsPanel => this.uiButtonsPanel;
    public UiMinimizePanel MinimizePanel => this.minimizePanel;
    public Button MaximizeButton => this.maximizeButton;
    public UiCompass Compass => this.compass;
    public Transform Moon => this.moon;
    public Transform ParticlesContainer => this.particlesContainer;

    public DayNightCycleController DayNightCycleController => this.dayNightCycleController;

    [SerializeField] private UiSplashScreenPanel splashScreenPanel;

    [Header("Cameras")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Camera dragCamera;

    [Header("Canvas RectTransforms")]
    [SerializeField] private RectTransform dragCanvas;
    [SerializeField] private RectTransform worldRectTrans;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Canvas uiCanvas;
    
    [Header("Panels")]
    [SerializeField] private UiSettingsPanel settingsPanel;
    [SerializeField] private UiShopPanel shopPanel;
    [SerializeField] private UiInventoryPanel inventoryPanel;
    [SerializeField] private UiResourcesPanel resourcesPanel;
    [SerializeField] private UiTimeWeatherPanel timeWeatherPanel;
    [Space, SerializeField] private Canvas notificationsCanvas;
    [SerializeField] private GameObject decorationsContainer;
    [SerializeField] private GameObject uiButtonsPanel;
    [SerializeField] private UiMinimizePanel minimizePanel;
    [SerializeField] private Button maximizeButton;

    [Header("Misc")]
    [SerializeField] private UiCompass compass;
    public TMP_Text DebugText => this.debugText;
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private Transform moon;
    [SerializeField] private Transform particlesContainer;
    [SerializeField] private DayNightCycleController dayNightCycleController;

    private void Awake()
    {
        SetDebugText(string.Empty);
    }

    public void Init()
    {
        InputManager.OnSpacePress += HandleSpacePress;
        InputManager.OnSettingsPress += ToggleSettingsPanelVisibility;
        InputManager.OnShopPress += ToggleShopPanelVisibility;
        InputManager.OnInventoryPress += ToggleInventoryPanelVisibility;
        InputManager.OnResourcesPress += ToggleResourcesPanelVisibility;
        InputManager.OnTimeWeatherPress += ToggleTimeWeatherPanelVisibility;
        
        this.shopPanel.SetVisible(false, true);
        this.settingsPanel.SetVisible(false, true);
        this.inventoryPanel.SetVisible(false, true);
        this.resourcesPanel.SetVisible(false, true);
    }

    private void OnDestroy()
    {
        InputManager.OnSpacePress -= HandleSpacePress;
        InputManager.OnSettingsPress -= ToggleSettingsPanelVisibility;
        InputManager.OnShopPress -= ToggleShopPanelVisibility;
        InputManager.OnInventoryPress -= ToggleInventoryPanelVisibility;
        InputManager.OnResourcesPress -= ToggleResourcesPanelVisibility;
        InputManager.OnTimeWeatherPress -= ToggleTimeWeatherPanelVisibility;
    }

    private void HandleSpacePress()
    {
        SetDebugText($"Frame Count: {Time.frameCount}");
    }
    
    public void ToggleSettingsPanelVisibility()
    {
        this.inventoryPanel.Hide();
        this.shopPanel.Hide();
        
        this.settingsPanel.Toggle();  
    }

    public void ToggleShopPanelVisibility()
    {
        this.inventoryPanel.Hide();
        this.settingsPanel.Hide();

        this.shopPanel.Toggle();  
    }

    public void ToggleInventoryPanelVisibility()
    {
        this.shopPanel.Hide();
        this.settingsPanel.Hide();

        this.inventoryPanel.Toggle();
    }

    public void ToggleResourcesPanelVisibility()
    {
        this.resourcesPanel.Toggle();
    }

    public void ToggleTimeWeatherPanelVisibility()
    {
        this.timeWeatherPanel.Toggle();
    }

    [HideInCallstack]
    public void SetDebugText(string text, bool append = false)
    {
        if (this.debugText)
        {
            if (append)
                this.debugText.text += $"\n{text}";
            else
                this.debugText.text = text;
        }

        Debug.Log(text);
    }

    public void ClearDebugText()
    {
        if (this.debugText)
        {
            this.debugText.text = string.Empty;
        }
    }
}