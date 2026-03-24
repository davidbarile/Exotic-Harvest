using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;

public class DayNightCycleController : MonoBehaviour
{
     [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animation dayNightCycleAnim;
    [SerializeField] private RectTransform timeOfDayRectTrans;
    [SerializeField] private float cameraOffset = -1920;

    private Vector3 initCameraPos;

    private DateTime lastTimeSliderMoved;
    
    private void Start()
    {
        this.initCameraPos = this.cameraTransform.localPosition;
        TimeManager.OnHourChanged += HandleHourChanged;
    }

    private void OnDestroy()
    {
        TimeManager.OnHourChanged -= HandleHourChanged;
    }

    private void HandleHourChanged(float currentHour)
    {
        // Handle the time of day change
        var normalizedTime = currentHour / 24f; // Normalize to 0-1
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

        //Debug.Log($"currentHour: {currentHour} ({TimeManager.FormatFloatAsTime(currentHour)})   normalizedTime: {normalizedTime}   frame = {frameNum} / {clipLength}   anim speed = {this.dayNightCycleAnim[clipName].speed}");
    }

    // This method can be called by a UI slider to allow the user to pan world rect trans
    public void UserPanCamera(float normalizedValue)
    {
        var rectWidth = this.timeOfDayRectTrans.rect.width;
        var scrollValue = -1 * ((normalizedValue * rectWidth) + this.cameraOffset);
        UiManager.IN.Compass.SetDirection(normalizedValue);
        this.cameraTransform.localPosition = new Vector3(scrollValue, this.initCameraPos.y, this.initCameraPos.z);

        this.lastTimeSliderMoved = DateTime.Now;
    }

    private void SetCameraPosition(float normalizedTime)
    {
        if (DateTime.Now - this.lastTimeSliderMoved < TimeSpan.FromSeconds(2))
            return; // Don't update the position if the user has recently moved the slider

        UiManager.IN.Compass.SetDirection(normalizedTime);

        var rectWidth = this.timeOfDayRectTrans.rect.width;
        var newPosition = new Vector3(-1 * ((normalizedTime * rectWidth) + this.cameraOffset), this.initCameraPos.y, this.initCameraPos.z);

        if (TimeManager.IN.UseRealTime || Time.frameCount < 100)
        {
            this.cameraTransform.localPosition = newPosition;
            return;
        }
        
        var delta = Math.Abs(this.cameraTransform.localPosition.x - newPosition.x);
        if (delta > 1000f && this.cameraTransform.localPosition.x > rectWidth * 0.5f) // If the delta is large, jump to the new position without tweening
        {
            var fakePosition = new Vector3((normalizedTime * rectWidth) + this.cameraOffset, this.initCameraPos.y, this.initCameraPos.z);
            var fakeDelta = Math.Abs(this.cameraTransform.localPosition.x - fakePosition.x);
            this.cameraTransform.localPosition = new Vector3(newPosition.x - fakeDelta, this.cameraTransform.localPosition.y, this.cameraTransform.localPosition.z); // Move to the opposite side before tweening
            StartCoroutine(DelayedSetTimeOfDayRectPosition(newPosition));
            return;
        }

        this.cameraTransform.DOLocalMoveX(newPosition.x, 1f).SetEase(Ease.Linear);
    }
    
    private IEnumerator DelayedSetTimeOfDayRectPosition(Vector3 newPosition)
    {
        yield return new WaitForSeconds(0.05f); // Wait a short time before tweening to the new position    
        this.cameraTransform.DOLocalMoveX(newPosition.x, 0.95f).SetEase(Ease.Linear);
    }
}