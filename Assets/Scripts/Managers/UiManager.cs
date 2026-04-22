using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    public static UiManager IN;

    public Camera WorldCamera => this.worldCamera;
    public Camera DragCamera => this.dragCamera;

    public RectTransform DragCanvas => this.dragCanvas;
    public Canvas WorldCanvas => this.worldCanvas;
    public RectTransform WorldRectTrans => this.worldRectTrans;

    public UiSettingsPanel SettingsPanel => this.settingsPanel;
    public UiShopPanel ShopPanel => this.shopPanel;
    public UiInventoryPanel InventoryPanel => this.inventoryPanel;
    public UiResourcesPanel ResourcesPanel => this.resourcesPanel;
    public UiTimeWeatherPanel TimeWeatherPanel => this.timeWeatherPanel;

    public UiCompass Compass => this.compass;
    public Transform Moon => this.moon;

    [Header("Cameras")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Camera dragCamera;

    [Header("Canvas RectTransforms")]
    [SerializeField] private RectTransform dragCanvas;
    [SerializeField] private RectTransform worldRectTrans;
    [SerializeField] private Canvas worldCanvas;
    
    [Header("Panels")]
    [SerializeField] private UiSettingsPanel settingsPanel;
    [SerializeField] private UiShopPanel shopPanel;
    [SerializeField] private UiInventoryPanel inventoryPanel;
    [SerializeField] private UiResourcesPanel resourcesPanel;
    [SerializeField] private UiTimeWeatherPanel timeWeatherPanel;

    [Header("Misc")]
    [SerializeField] private UiCompass compass;
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private Transform moon;

    private void OnDrawGizmos()
    {
        var wc = this.worldCamera.transform;
        this.worldCamera.transform.position = new Vector3(wc.position.x, this.worldRectTrans.position.y, wc.position.z);
    }

    private void Awake()
    {
        OnDrawGizmos();
        SetDebugText(string.Empty);
    }

    private void Start()
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

    public void SetDebugText(string text, bool append = false)
    {
        if (this.debugText)
        {
            if (append)
                this.debugText.text += $"\n{text}";
            else
                this.debugText.text = text;
        }
    }

    public void ClearDebugText()
    {
        if (this.debugText)
        {
            this.debugText.text = string.Empty;
        }
    }
}