using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    public static UiManager IN;

    [SerializeField] private TMP_Text debugText;
    [SerializeField] private UiSettingsPanel settingsPanel;
    [SerializeField] private UiShopPanel shopPanel;
    [SerializeField] private UiInventoryPanel inventoryPanel;

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
        this.shopPanel.SetVisible(false, true);
        this.settingsPanel.SetVisible(true);
        this.inventoryPanel.SetVisible(false);
    }

    private void OnDestroy()
    {
        InputManager.OnSpacePress -= HandleSpacePress;
        InputManager.OnSettingsPress -= ToggleSettingsPanelVisibility;
        InputManager.OnShopPress -= ToggleShopPanelVisibility;
        InputManager.OnInventoryPress -= ToggleInventoryPanelVisibility;
    }

    private void HandleSpacePress()
    {
        SetDebugText($"Frame Count: {Time.frameCount}");
    }
    
    public void ToggleSettingsPanelVisibility()
    {
        if (!this.settingsPanel)
            return;

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
        if (!this.shopPanel)
            return;

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
        if (!this.inventoryPanel)
            return;

        if (this.inventoryPanel.IsShowing)
            this.inventoryPanel.Hide();
        else
            this.inventoryPanel.Show();
    }

    public void SetDebugText(string text)
    {
        if (this.debugText != null)
            this.debugText.text = text;
    }
}