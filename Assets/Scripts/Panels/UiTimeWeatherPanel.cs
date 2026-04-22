using UnityEngine;

public class UiTimeWeatherPanel : UIPanelBase
{
    public void OpenTimeSettings()
    {
        UiManager.IN.SettingsPanel.Show();
        UiManager.IN.SettingsPanel.HandleToggleChanged_5(true);
    }
}