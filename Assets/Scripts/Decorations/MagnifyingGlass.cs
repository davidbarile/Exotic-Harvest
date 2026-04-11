using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MagnifyingGlass : PassiveHarvester
{
    public bool IsInSearchMode { get; private set; }
    [SerializeField] private RectTransform innerWorld;
    [SerializeField] private Transform container;
    [SerializeField] private Mask lensMask;
    [SerializeField] private CanvasGroup lensCanvasGroup;
    [SerializeField] private float scrollSpeed = 1f;

    [SerializeField] private bool isMaskActive = true;

    private Tween lensTween;

    private void OnValidate()
    {
        this.lensMask.showMaskGraphic = this.isMaskActive;
    }

    protected override void Start()
    {
        base.Start();
        RefreshQuantityDisplay();
        SetSearchMode(false);
    }

    public void SetSearchMode(bool active)
    {
        this.IsInSearchMode = active;
        this.lensMask.gameObject.SetActive(active);
        //this.lensCanvasGroup.gameObject.SetActive(!active);

        this.lensTween?.Kill();
        if (active)
        {
            this.lensCanvasGroup.gameObject.SetActive(true);
            this.lensTween = this.lensCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InOutSine);
        }
        else
        {
            this.lensTween = this.lensCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.InOutSine).OnComplete(() => this.lensCanvasGroup.gameObject.SetActive(false));
        }
    }

    protected override void RefreshQuantityDisplay()
    {
        base.RefreshQuantityDisplay();
    }

    public void ScrollInnerWorld()
    {
        if (!this.IsInSearchMode)
            return;
            
        this.innerWorld.localPosition = this.transform.localPosition * -1 * this.scrollSpeed;
    }

    protected override bool CheckGenerationConditions()
    {
        // Only generate when it is not raining and it is morning
        return !WeatherManager.IN.IsRaining && TimeManager.IN.CurrentTimeOfDay.HasFlag(ETimeOfDay.Morning);
    }
}
