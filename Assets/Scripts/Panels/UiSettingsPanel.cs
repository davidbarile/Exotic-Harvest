using UnityEngine;
using UnityEngine.UI;

public class UiSettingsPanel : UIPanelBase
{
    [SerializeField] private GameObject timeOfDaySlider;
    [SerializeField] private GameObject timeScaleSlider;

    [SerializeField] private Toggle useRealTimeToggle;

    private void OnEnable()
    {
        this.useRealTimeToggle.isOn = TimeManager.IN.UseRealTime;
        SetTimeSlidersActive(TimeManager.IN.UseRealTime);
    }

    public void SetTimeSlidersActive(bool active)
    {
        //reversed to work with Toggle
        this.timeOfDaySlider.SetActive(!active);
        this.timeScaleSlider.SetActive(!active);
    }
}