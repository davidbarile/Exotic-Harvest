using UnityEngine;
using UnityEngine.UI;

public class UiSettingsPanel : UIPanelBase
{
    [SerializeField] private GameObject timeOfDaySlider;
    [SerializeField] private GameObject timeScaleSlider;

    [SerializeField] private Toggle useRealTimeToggle;

    [SerializeField] private GameObject[] contentDisplays;

    public override void Show()
    {
        base.Show();

        this.useRealTimeToggle.isOn = TimeManager.IN.UseRealTime;
        SetTimeSlidersActive(TimeManager.IN.UseRealTime);
    }

    public void SetTimeSlidersActive(bool active)
    {
        //reversed to work with Toggle
        this.timeOfDaySlider.SetActive(!active);
        this.timeScaleSlider.SetActive(!active);

        TimeManager.IN.ToggleRealTime(active);
    }

    private void HideAllContentDisplays()
    {
        foreach (var display in this.contentDisplays)
        {
            display.SetActive(false);
        }
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

    public void HandleToggleChanged_0(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[0].SetActive(isOn);
    }

    public void HandleToggleChanged_1(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[1].SetActive(isOn);
    }

    public void HandleToggleChanged_2(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[2].SetActive(isOn);
    }

    public void HandleToggleChanged_3(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[3].SetActive(isOn);
    }

    public void HandleToggleChanged_4(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[4].SetActive(isOn);
    }

    public void HandleToggleChanged_5(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[5].SetActive(isOn);
    }

    public void HandleToggleChanged_6(bool isOn)
    {
        HideAllContentDisplays();
        this.contentDisplays[6].SetActive(isOn);
    }
}