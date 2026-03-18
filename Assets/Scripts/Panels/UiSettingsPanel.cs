using UnityEngine;

public class UiSettingsPanel : UIPanelBase
{
    [SerializeField] private GameObject timeOfDaySlider;
    [SerializeField] private GameObject timeScaleSlider;

    public void SetTimeSlidersActive(bool active)
    {
        //reversed to work with Toggle
        this.timeOfDaySlider.SetActive(!active);
        this.timeScaleSlider.SetActive(!active);
    }
}