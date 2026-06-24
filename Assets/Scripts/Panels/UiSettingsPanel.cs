using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GlobalEnums;

public class UiSettingsPanel : UIPanelBase
{
    public Toggle[] CategoryTabs;
    [SerializeField] private GameObject[] contentDisplays;

    [SerializeField] private GameObject initContentDisplay;

    private bool isInitialized;

    #region Settings
    private IEnumerator Start()
    {
        if (PlatformManager.IsMobile)
            this.BgAlphaSlider.value = 1f;

        while (SaveManager.Data == null)
            yield return null;
            
        SaveManager.Data.BgAlpha = this.BgAlphaSlider.value;
    }

    public override void Show()
    {
        base.Show();

        if (!this.isInitialized)
        {
            HideAllContentDisplays();
            this.initContentDisplay.SetActive(true);
        }

        this.isInitialized = true;

        this.UseRealTimeToggle.isOn = TimeManager.IN.UseRealTime;
        SetTimeSlidersActive(TimeManager.IN.UseRealTime);
    }

    private void HideAllContentDisplays()
    {
        foreach (var display in this.contentDisplays)
        {
            display.SetActive(false);
        }
    }

    public void HandleToggleChanged_0(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[0].SetActive(isOn);
        this.CategoryTabs[0].SetIsOnWithoutNotify(isOn);
    }

    public void HandleToggleChanged_1(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[1].SetActive(isOn);
        this.CategoryTabs[1].SetIsOnWithoutNotify(isOn);
    }

    public void HandleToggleChanged_2(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[2].SetActive(isOn);
        this.CategoryTabs[2].SetIsOnWithoutNotify(isOn);
    }

    public void HandleToggleChanged_3(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[3].SetActive(isOn);
        this.CategoryTabs[3].SetIsOnWithoutNotify(isOn);
    }

    public void HandleToggleChanged_4(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[4].SetActive(isOn);
        this.CategoryTabs[4].SetIsOnWithoutNotify(isOn);

    }

    public void HandleToggleChanged_5(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[5].SetActive(isOn);
        this.CategoryTabs[5].SetIsOnWithoutNotify(isOn);
    }

    public void HandleToggleChanged_6(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[6].SetActive(isOn);
        this.CategoryTabs[6].SetIsOnWithoutNotify(isOn);
    }

    public void HandleToggleChanged_7(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[7].SetActive(isOn);
        this.CategoryTabs[7].SetIsOnWithoutNotify(isOn);
    }
    #endregion

    #region General
    [Header("General ---------------")]
    public Toggle ShowSplashScreenToggle;

    #endregion

    #region Screen
    [Header("Screen ---------------")]
    [SerializeField] private Slider[] panelColorSliders; // 0-Red, 1-Green, 2-Blue, 3-Alpha

    [Space] public Toggle ShowTimeWeatherPanelToggle;
    public Toggle ShowUiButtonsToggle;
    public Toggle ShowNotificationsToggle;
    public Toggle ShowDecorationsToggle;
    public Toggle ShowSunAndMoonToggle;
    public Toggle ShowCloudsToggle;
    public Toggle ShowMountainsToggle;
    [Space]
    public Slider BgAlphaSlider;

    public void ApplySavedColorsToMenus(Color inPanelColor)
    {
        this.panelColorSliders[0].value = inPanelColor.r;
        this.panelColorSliders[1].value = inPanelColor.g;
        this.panelColorSliders[2].value = inPanelColor.b;
        this.panelColorSliders[3].value = inPanelColor.a;
    }

    public void ApplySaveDataToUI()
    {
        this.ShowSplashScreenToggle.isOn = SaveManager.Data.ShowSplashScreen;
        this.ShowTimeWeatherPanelToggle.isOn = SaveManager.Data.ShowTimeWeatherPanel;
        this.ShowUiButtonsToggle.isOn = SaveManager.Data.ShowPanelsButtons;
        this.ShowNotificationsToggle.isOn = SaveManager.Data.ShowNotifications;
        this.ShowDecorationsToggle.isOn = SaveManager.Data.ShowDecorations;
        this.ShowSunAndMoonToggle.isOn = SaveManager.Data.ShowSunAndMoon;
        this.ShowCloudsToggle.isOn = SaveManager.Data.ShowClouds;
        this.ShowMountainsToggle.isOn = SaveManager.Data.ShowMountains;
        this.BgAlphaSlider.value = SaveManager.Data.BgAlpha;

        Debug.Log($"Applied saved screen settings to UI: TimeWeatherPanel={SaveManager.Data.ShowTimeWeatherPanel}, PanelsButtons={SaveManager.Data.ShowPanelsButtons}, Notifications={SaveManager.Data.ShowNotifications}, Decorations={SaveManager.Data.ShowDecorations}, SunAndMoon={SaveManager.Data.ShowSunAndMoon}, Clouds={SaveManager.Data.ShowClouds}, Mountains={SaveManager.Data.ShowMountains}, BgAlpha={SaveManager.Data.BgAlpha}");

        this.TimeOfDaySlider.value = TimeManager.CurrentHour;
        this.TimeScaleSlider.value = TimeManager.IN.TimeScale;

        this.GrantAllResourcesToggle.isOn = SaveManager.Data.DebugGrantAllResources;
        this.FreezeTimeToggle.isOn = SaveManager.Data.FreezeTime;
    }

    // the application of these values are all handled in ScreenManager.ToggleElementsVisibility
    // (it doesn't need to change now, because it only applies on minimize)
    public void HandleShowSplashScreenPanelToggle(bool active)
    {
        SaveManager.Data.ShowSplashScreen = active;
    }
    
    public void HandleShowTimeWeatherPanelToggle(bool active)
    {            
        SaveManager.Data.ShowTimeWeatherPanel = active;
    }

    public void HandleShowPanelsButtonsToggle(bool active)
    {
        SaveManager.Data.ShowPanelsButtons = active;
    }

    public void HandleShowNotificationsToggle(bool active)
    {
        SaveManager.Data.ShowNotifications = active;
    }

    public void HandleShowDecorationsToggle(bool active)
    {
        SaveManager.Data.ShowDecorations = active;
    }

    public void HandleShowSunAndMoonToggle(bool active)
    {
        SaveManager.Data.ShowSunAndMoon = active;
    }

    public void HandleShowCloudsToggle(bool active)
    {
        SaveManager.Data.ShowClouds = active;
    }

    public void HandleShowMountainsToggle(bool active)
    {
        SaveManager.Data.ShowMountains = active;
    }
    
    public void HandleBgAlphaValueChanged(float value)
    {
        ScreenManager.IN.SetWorldBgAlpha(value);
        
        if(SaveManager.Data != null)
            SaveManager.Data.BgAlpha = value;
    }

    public void HandlePanelColorSliderChanged_Red(float value)
    {
        ColorManager.IN.PanelColor = new Color(value, ColorManager.IN.PanelColor.g, ColorManager.IN.PanelColor.b, ColorManager.IN.PanelColor.a);
        ColorManager.OnPanelColorChanged?.Invoke(ColorManager.IN.PanelColor);
    }

    public void HandlePanelColorSliderChanged_Green(float value)
    {
        ColorManager.IN.PanelColor = new Color(ColorManager.IN.PanelColor.r, value, ColorManager.IN.PanelColor.b, ColorManager.IN.PanelColor.a);
        ColorManager.OnPanelColorChanged?.Invoke(ColorManager.IN.PanelColor);
    }

    public void HandlePanelColorSliderChanged_Blue(float value)
    {
        ColorManager.IN.PanelColor = new Color(ColorManager.IN.PanelColor.r, ColorManager.IN.PanelColor.g, value, ColorManager.IN.PanelColor.a);
        ColorManager.OnPanelColorChanged?.Invoke(ColorManager.IN.PanelColor);
    }

    public void HandlePanelColorSliderChanged_Alpha(float value)
    {
        ColorManager.IN.PanelColor = new Color(ColorManager.IN.PanelColor.r, ColorManager.IN.PanelColor.g, ColorManager.IN.PanelColor.b, value);
        ColorManager.OnPanelColorChanged?.Invoke(ColorManager.IN.PanelColor);
    }
    #endregion

    #region Audio
    [Header("Audio ---------------")]
    public Slider MusicVolumeSlider;
    public Slider AmbientVolumeSlider;
    public Slider EffectsVolumeSlider;
    [Space]
    public Slider MusicVolumeSlider_Minimized;
    public Slider AmbientVolumeSlider_Minimized;
    public Slider EffectsVolumeSlider_Minimized;

    #endregion

    #region Time & Weather
    [Header("Time & Weather ---------------")]
    public Toggle UseRealTimeToggle;
    [Space] public Slider TimeOfDaySlider;
    public Slider TimeScaleSlider;
    [Space] public TMP_Dropdown ForceWeatherDropdown;

    public void SetTimeSlidersActive(bool active)
    {
        //reversed to work with Toggle
        this.TimeOfDaySlider.gameObject.SetActive(!active);
        this.TimeScaleSlider.gameObject.SetActive(!active);

        TimeManager.IN.ToggleRealTime(active);
        SaveManager.Data.UseRealTime = active;
    }

    public void HandleForceWeatherDropdownChanged(int index)
    {
        switch (index)
        {
            case 1:
                WeatherManager.IN.ForceWeather(EWeatherType.Clear);
                break;
            case 2:
                WeatherManager.IN.ForceWeather(EWeatherType.Rain);
                break;
            case 3:
                WeatherManager.IN.ForceWeather(EWeatherType.Storm);
                break;
            case 4:
                WeatherManager.IN.ForceWeather(EWeatherType.Wind);
                break;
            case 5:
                WeatherManager.IN.ForceWeather(EWeatherType.Foggy);
                break;
            default:
                WeatherManager.IN.ChangeWeather();
                break;
        }
    }
    #endregion

    #region Debug
    [Header("Debug ---------------")]
    public Toggle GrantAllResourcesToggle;
    public Toggle FreezeTimeToggle;

    public void HandleGrantAllResourcesToggle(bool active)
    {
        ResourceManager.IN.DebugGrantAllResources = active;

        if (active)
            UiManager.IN.ResourcesPanel.GrantAllResources();
            
        SaveManager.Data.DebugGrantAllResources = active;
    }

    public void HandleFreezeTimeToggle(bool active)
    {
        TimeManager.IN.FreezeTime = active;
        SaveManager.Data.FreezeTime = active;
    }

    public void HandleEnableDebugTextToggle(bool active)
    {
        if(UiManager.IN.DebugText)
            UiManager.IN.DebugText.gameObject.SetActive(active);
    }

    #endregion
}