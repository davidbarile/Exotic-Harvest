using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    public static UiManager IN;

    [SerializeField] private TMP_Text debugText;
    public UiSettingsPanel SettingsPanel => this.settingsPanel;
    public UiShopPanel ShopPanel => this.shopPanel;
    public UiInventoryPanel InventoryPanel => this.inventoryPanel;
    public UiResourcesPanel ResourcesPanel => this.resourcesPanel;
    public UiCompass Compass => this.compass;
    
    [SerializeField] private UiSettingsPanel settingsPanel;
    [SerializeField] private UiShopPanel shopPanel;
    [SerializeField] private UiInventoryPanel inventoryPanel;
    [SerializeField] private UiResourcesPanel resourcesPanel;
    [SerializeField] private UiCompass compass;

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
    }

    private void HandleSpacePress()
    {
        SetDebugText($"Frame Count: {Time.frameCount}");
    }
    
    public void ToggleSettingsPanelVisibility()
    {
        if (this.settingsPanel.IsShowing)
            this.settingsPanel.Hide();
        else
        {
            this.shopPanel.SetVisible(false);
            this.settingsPanel.Show();
        }
    }

    public void ToggleShopPanelVisibility()
    {
        if (this.shopPanel.IsShowing)
            this.shopPanel.Hide();
        else
        {
            this.settingsPanel.SetVisible(false);
            this.shopPanel.Show();
        }
    }

    public void ToggleInventoryPanelVisibility()
    {
        if (this.inventoryPanel.IsShowing)
            this.inventoryPanel.Hide();
        else
            this.inventoryPanel.Show();
    }

    public void ToggleResourcesPanelVisibility()
    {
        if (this.resourcesPanel.IsShowing)
            this.resourcesPanel.Hide();
        else
            this.resourcesPanel.Show();
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