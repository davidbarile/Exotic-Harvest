using UnityEngine;
using DG.Tweening;
using System;

public class DayNightCycleController : MonoBehaviour
{
    [SerializeField] private Animation dayNightCycleAnim;
    [SerializeField] private RectTransform timeOfDayRectTrans;
    [SerializeField] private float rectTransOffset;
    
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
            this.dayNightCycleAnim[clipName].speed = 60f * (clipLength / secondsPerDay) * TimeManager.IN.TimeScale;
            this.dayNightCycleAnim.CrossFade(clipName, 1f); // Smoothly transition to the new time of day
        }

        SetTimeOfDayTextPosition(normalizedTime);

        //Debug.Log($"currentHour: {currentHour} ({TimeManager.FormatFloatAsTime(currentHour)})   normalizedTime: {normalizedTime}   frame = {frameNum} / {clipLength}   anim speed = {this.dayNightCycleAnim[clipName].speed}");
    }
    
    public void SetTimeOfDayTextPosition(float normalizedTime)
    {
        var rectWidth = this.timeOfDayRectTrans.rect.width;
        var newPosition = new Vector3(normalizedTime * rectWidth + rectTransOffset, 0, 0);

        if (TimeManager.IN.UseRealTime)
        {
            this.timeOfDayRectTrans.anchoredPosition = newPosition;
        }
        else
        {
            var delta = Math.Abs(this.timeOfDayRectTrans.anchoredPosition.x - newPosition.x);
            Debug.Log($"delta: {delta}   newPos: {newPosition}   currentPos: {this.timeOfDayRectTrans.anchoredPosition}");
        
            if (delta > 1000f && normalizedTime < .1f) // If the delta is large, jump to the new position without tweening
            {
                var fakePosition = new Vector3( (normalizedTime + 1) * rectWidth + rectTransOffset, 0, 0);
                this.timeOfDayRectTrans.anchoredPosition = fakePosition + delta * Vector3.right; // Move to the opposite side before tweening
            }
                
            this.timeOfDayRectTrans.DOAnchorPos(newPosition, 1f).SetEase(Ease.Linear);
        }
    }
}