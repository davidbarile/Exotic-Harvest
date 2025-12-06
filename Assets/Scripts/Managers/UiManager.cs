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
        shopPanel.SetVisible(false, true);
        settingsPanel.SetVisible(true);
        inventoryPanel.SetVisible(false);
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
        if (!settingsPanel)
            return;

        if (settingsPanel.IsShowing)
            settingsPanel.Hide();
        else
        {
            shopPanel.SetVisible(false);
            settingsPanel.Show();
        }
    }

    public void ToggleShopPanelVisibility()
    {
        if (!shopPanel)
            return;

        if (shopPanel.IsShowing)
            shopPanel.Hide();
        else
        {
            settingsPanel.SetVisible(false);
            shopPanel.Show();
        }
    }

    public void ToggleInventoryPanelVisibility()
    {
        if (!inventoryPanel)
            return;

        if (inventoryPanel.IsShowing)
            inventoryPanel.Hide();
        else
            inventoryPanel.Show();
    }

    public void SetDebugText(string text)
    {
        if (debugText != null)
            debugText.text = text;
    }
}