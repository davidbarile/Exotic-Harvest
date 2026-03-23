using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine.UI;

public class DayNightCycleController : MonoBehaviour
{
    [SerializeField] private Animation dayNightCycleAnim;
    [SerializeField] private RectTransform timeOfDayRectTrans;
    [SerializeField] private float rectTransOffset;

    private DateTime lastTimeSliderMoved;
    
    private void OnEnable()
    {
        TimeManager.OnHourChanged += HandleHourChanged;
    }

    private void OnDisable()
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

        SetTimeOfDayRectPosition(normalizedTime);

        //Debug.Log($"currentHour: {currentHour} ({TimeManager.FormatFloatAsTime(currentHour)})   normalizedTime: {normalizedTime}   frame = {frameNum} / {clipLength}   anim speed = {this.dayNightCycleAnim[clipName].speed}");
    }

    // This method can be called by a UI slider to allow the user to pan world rect trans
    public void UserPanWorld(float normalizedValue)
    {
        var rectWidth = this.timeOfDayRectTrans.rect.width;
        var scrollValue = normalizedValue * rectWidth + this.rectTransOffset;
        UiManager.IN.Compass.SetDirection(normalizedValue);
        this.timeOfDayRectTrans.localPosition = new Vector3(scrollValue, 0, 0);

        this.lastTimeSliderMoved = DateTime.Now;
    }

    private void SetTimeOfDayRectPosition(float normalizedTime)
    {
        if (DateTime.Now - this.lastTimeSliderMoved < TimeSpan.FromSeconds(2))
            return; // Don't update the position if the user has recently moved the slider

        UiManager.IN.Compass.SetDirection(normalizedTime);

        var rectWidth = this.timeOfDayRectTrans.rect.width;
        var newPosition = new Vector3(normalizedTime * rectWidth + this.rectTransOffset, 0, 0);

        if (TimeManager.IN.UseRealTime || Time.frameCount < 100)
        {
            this.timeOfDayRectTrans.localPosition = newPosition;
            return;
        }
        
        var delta = Math.Abs(this.timeOfDayRectTrans.localPosition.x - newPosition.x);
        if (delta > 1000f && this.timeOfDayRectTrans.localPosition.x > rectWidth * 0.5f) // If the delta is large, jump to the new position without tweening
        {
            var fakePosition = new Vector3((normalizedTime + 1) * rectWidth + this.rectTransOffset, 0, 0);
            var fakeDelta = Math.Abs(this.timeOfDayRectTrans.localPosition.x - fakePosition.x);
            this.timeOfDayRectTrans.localPosition = new Vector3(newPosition.x - fakeDelta, this.timeOfDayRectTrans.localPosition.y, this.timeOfDayRectTrans.localPosition.z); // Move to the opposite side before tweening
            StartCoroutine(DelayedSetTimeOfDayRectPosition(newPosition));
            return;
        }

        this.timeOfDayRectTrans.DOLocalMoveX(newPosition.x, 1f).SetEase(Ease.Linear);
    }
    
    private IEnumerator DelayedSetTimeOfDayRectPosition(Vector3 newPosition)
    {
        yield return new WaitForSeconds(0.05f); // Wait a short time before tweening to the new position    
        this.timeOfDayRectTrans.DOLocalMoveX(newPosition.x, 0.95f).SetEase(Ease.Linear);
    }
}