using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    public static UiManager IN;

    public UiSettingsPanel SettingsPanel => this.settingsPanel;
    public UiShopPanel ShopPanel => this.shopPanel;
    public UiInventoryPanel InventoryPanel => this.inventoryPanel;
    public UiResourcesPanel ResourcesPanel => this.resourcesPanel;
    public UiCompass Compass => this.compass;
    
    [SerializeField] private UiSettingsPanel settingsPanel;
    [SerializeField] private UiShopPanel shopPanel;
    [SerializeField] private UiInventoryPanel inventoryPanel;
    [SerializeField] private UiResourcesPanel resourcesPanel;
    [SerializeField] private UIPanelBase timeWeatherPanel;
    [SerializeField] private UiCompass compass;
    [SerializeField] private TMP_Text debugText;

    private void Awake()
    {
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
        this.settingsPanel.Toggle();  
    }

    public void ToggleShopPanelVisibility()
    {
        this.inventoryPanel.Hide();
        this.shopPanel.Toggle();  
    }

    public void ToggleInventoryPanelVisibility()
    {
        this.shopPanel.Hide();
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
}