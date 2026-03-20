using System;
using UnityEngine;

public class DayNightCycleController : MonoBehaviour
{
    [SerializeField] private Animation dayNightCycleAnim;
    
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

        //Debug.Log($"currentHour: {currentHour} ({TimeManager.FormatFloatAsTime(currentHour)})   normalizedTime: {normalizedTime}   frame = {frameNum} / {clipLength}   anim speed = {this.dayNightCycleAnim[clipName].speed}");
    }
}