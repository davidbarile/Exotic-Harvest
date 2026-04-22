using UnityEngine;
using UnityEngine.UI;

public class UiSettingsPanel : UIPanelBase
{
    public Toggle[] CategoryTabs;
    [SerializeField] private GameObject[] contentDisplays;

    [SerializeField] private GameObject initContentDisplay;

    #region Settings
    private void Start()
    {
        this.bgAlphaSlider.value = PlatformManager.IsMobile ? 1f : .5f;
    }

    public override void Show()
    {
        base.Show();

        HideAllContentDisplays();
        this.initContentDisplay.SetActive(true);

        this.useRealTimeToggle.isOn = TimeManager.IN.UseRealTime;
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
    //[Header("General ---------------")]


    #endregion

    #region Screen
    [Header("Screen ---------------")]
    [SerializeField] private Slider[] panelColorSliders; // 0-Red, 1-Green, 2-Blue, 3-Alpha

    [Space] public Toggle ShowTimeWeatherPanelToggle;
    public Toggle ShowPanelsButtonsToggle;
    public Toggle ShowNotificationsToggle;
    public Toggle ShowDecorationsToggle;
    public Toggle ShowSunAndMoonToggle;
    public Toggle ShowCloudsToggle;
    public Toggle ShowMountainsToggle;

    [SerializeField] private Slider bgAlphaSlider;

    public void ApplySettingsDataToUI(Color inPanelColor)
    {
        this.panelColorSliders[0].value = inPanelColor.r;
        this.panelColorSliders[1].value = inPanelColor.g;
        this.panelColorSliders[2].value = inPanelColor.b;
        this.panelColorSliders[3].value = inPanelColor.a;
    }

    public void HandleShowTimeWeatherPanelToggle(bool active)
    {
        //UiManager.IN.panel
    }

    public void HandleShowPanelsButtonsToggle(bool active)
    {
        //UiManager.IN.ToggleAllPanelButtons(active);
    }

    public void HandleShowNotificationsToggle(bool active)
    {

    }

    public void HandleShowDecorationsToggle(bool active)
    {

    }

    public void HandleShowSunAndMoonToggle(bool active)
    {

    }

    public void HandleShowCloudsToggle(bool active)
    {

    }
    
    public void HandleShowMountainsToggle(bool active)
    {
        
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
    [SerializeField] private GameObject timeOfDaySlider;
    [SerializeField] private GameObject timeScaleSlider;
    [Space, SerializeField] private Toggle useRealTimeToggle;

    public void SetTimeSlidersActive(bool active)
    {
        //reversed to work with Toggle
        this.timeOfDaySlider.SetActive(!active);
        this.timeScaleSlider.SetActive(!active);

        TimeManager.IN.ToggleRealTime(active);
    }
    #endregion

    #region Debug
    [Header("Debug ---------------")]
    [SerializeField] private Toggle grantAllResourcesToggle;
    [SerializeField] private Toggle freezeTimeToggle;

    public void HandleGrantAllResourcesToggle(bool active)
    {
        ResourceManager.IN.DebugGrantAllResources = active;

        if(active)
            UiManager.IN.ResourcesPanel.GrantAllResources();
    }

    public void HandleFreezeTimeToggle(bool active)
    {
        TimeManager.IN.FreezeTime = active;
    }

    #endregion
}