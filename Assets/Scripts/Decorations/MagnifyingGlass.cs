using System;
using UnityEngine;

public class MagnifyingGlass : PassiveHarvester
{
    [SerializeField] private RectTransform innerWorld;
    [SerializeField] private Transform container;
    [SerializeField] private float scrollSpeed = 1f;

    protected override void Start()
    {
        base.Start();
        RefreshQuantityDisplay();
    }

    protected override void RefreshQuantityDisplay()
    {
        base.RefreshQuantityDisplay();
    }

    public void ScrollInnerWorld()
    {
        this.innerWorld.localPosition = this.transform.localPosition * -1 * this.scrollSpeed;
    }

    private void OnMouseDown()
    {
        // For testing, collect all resources on click
        CollectAll();
    }

    protected override bool CheckGenerationConditions()
    {
        // Only generate when it is not raining and it is morning
        return !WeatherManager.IN.IsRaining && TimeManager.IN.CurrentTimeOfDay.HasFlag(ETimeOfDay.Morning);
    }
}
