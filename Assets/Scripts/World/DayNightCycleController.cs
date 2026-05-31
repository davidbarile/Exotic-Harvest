using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DayNightCycleController : MonoBehaviour
{
    public float CurrentScrollValue { get; private set; }
    
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animation dayNightCycleAnim;
    [SerializeField] private RectTransform timeOfDayRectTrans;
    [SerializeField] private float cameraOffset = -1920;
    [SerializeField] private Slider worldPanSlider;
    [SerializeField] private bool shouldInvertSliderValue;
    [SerializeField] private float delayToResetCameraPosition = 3f;

    private Vector3 initCameraPos;

    private DateTime lastTimeSliderMoved;
    private float lastNormalizedTime;

    private Tween cameraTween;
    private Tween sliderTween;
    
    private void Start()
    {
        this.initCameraPos = this.cameraTransform.localPosition;
        TimeManager.OnHourChanged += HandleHourChanged;
    }

    private void OnDestroy()
    {
        TimeManager.OnHourChanged -= HandleHourChanged;
    }

    private void HandleHourChanged(float inCurrentHour)
    {
        // Handle the time of day change
        var normalizedTime = inCurrentHour / 24f; // Normalize to 0-1
        var clipLength = this.dayNightCycleAnim.clip.length;
        var clipName = this.dayNightCycleAnim.clip.name;
        var frameNum = normalizedTime * clipLength;

        this.dayNightCycleAnim[clipName].time = frameNum;

        if (TimeManager.IN.UseRealTime)
        {
            this.dayNightCycleAnim.Play(clipName);
            this.dayNightCycleAnim[clipName].speed = 0;
            this.dayNightCycleAnim.Sample();
        }
        else
        {
            var secondsPerDay = 24f * 60f * 60f;
            this.dayNightCycleAnim[clipName].speed = 60f * (clipLength / secondsPerDay) * TimeManager.IN.TimeScale * TimeManager.IN.HoursToSecondsRatio;
            this.dayNightCycleAnim.CrossFade(clipName, 1f); // Smoothly transition to the new time of day
        }

        SetCameraPosition(normalizedTime);
        this.lastNormalizedTime = normalizedTime;

        //Debug.Log($"currentHour: {inCurrentHour} ({TimeManager.FormatFloatAsTime(inCurrentHour)})   normalizedTime: {normalizedTime}   frame = {frameNum} / {clipLength}   anim speed = {this.dayNightCycleAnim[clipName].speed}");
    }

    //called by a UI slider to allow the user to pan world rect trans
    public void HandleWorldPanSliderDrag(float inNormalizedValue)
    {
        var sliderValue = this.shouldInvertSliderValue ? 1 - inNormalizedValue : inNormalizedValue;
        UserPanCamera(sliderValue, false);
    }
    
    //called by the WorldBgDrag script when the user drags on the world background, allowing them to pan the camera
    public void UserPanCamera(float inNormalizedValue, bool inShouldUpdateSlider)
    {
        var rectWidth = this.timeOfDayRectTrans.rect.width;
        var scrollValue = -1 * ((inNormalizedValue * rectWidth) + this.cameraOffset);
        UiManager.IN.Compass.SetDirection(inNormalizedValue);
        this.cameraTransform.localPosition = new Vector3(scrollValue, this.initCameraPos.y, this.initCameraPos.z);
        this.CurrentScrollValue = inNormalizedValue;

        if(inShouldUpdateSlider)
        {
            var sliderValue = this.shouldInvertSliderValue ? 1 - inNormalizedValue : inNormalizedValue;
            this.worldPanSlider.SetValueWithoutNotify(sliderValue);
        }

        this.lastTimeSliderMoved = DateTime.Now;
    }

    private void SetCameraPosition(float inNormalizedTime)
    {
        if (this.delayToResetCameraPosition == -1 || DateTime.Now - this.lastTimeSliderMoved < TimeSpan.FromSeconds(this.delayToResetCameraPosition))
            return; // Don't update the position if the user has recently moved the slider

        UiManager.IN.Compass.SetDirection(inNormalizedTime);

        this.CurrentScrollValue = inNormalizedTime;

        var rectWidth = this.timeOfDayRectTrans.rect.width;
        var newPosition = new Vector3(-1 * ((inNormalizedTime * rectWidth) + this.cameraOffset), this.initCameraPos.y, this.initCameraPos.z);

        if (TimeManager.IN.UseRealTime || Time.frameCount < 100)
        {
            this.cameraTransform.localPosition = newPosition;
            this.worldPanSlider.SetValueWithoutNotify(inNormalizedTime);
            return;
        }

        if (inNormalizedTime < this.lastNormalizedTime && this.lastNormalizedTime - inNormalizedTime > 0.5f) // looped around to zero, jump to the new position without tweening
        {
            if (this.cameraTween != null && this.cameraTween.IsActive())
                this.cameraTween.Kill();

            if(this.sliderTween != null && this.sliderTween.IsActive())
                this.sliderTween.Kill();
            
            this.lastNormalizedTime = inNormalizedTime;
            var fakePosition = new Vector3(-1 * (((inNormalizedTime + 1) * rectWidth) + this.cameraOffset), this.initCameraPos.y, this.initCameraPos.z);
            var fakeDelta = Math.Abs(this.cameraTransform.localPosition.x - fakePosition.x);

            this.cameraTransform.localPosition = new Vector3(newPosition.x + fakeDelta, this.cameraTransform.localPosition.y, this.cameraTransform.localPosition.z); // Move to the opposite side before tweening

            this.cameraTween = this.cameraTransform.DOLocalMoveX(newPosition.x, 1f).SetEase(Ease.Linear);
            this.worldPanSlider.SetValueWithoutNotify(inNormalizedTime);
            return;
        }

        this.cameraTween = this.cameraTransform.DOLocalMoveX(newPosition.x, 1f).SetEase(Ease.Linear);
        
        this.sliderTween = DOVirtual.Float(this.worldPanSlider.value, inNormalizedTime, 1f, value =>
        {
            this.worldPanSlider.SetValueWithoutNotify(value);
            UiManager.IN.Compass.SetDirection(value);
        }).SetEase(Ease.Linear);
    }
}