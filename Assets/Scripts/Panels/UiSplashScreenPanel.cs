using System;
using UnityEngine;
using UnityEngine.UI;

public class UiSplashScreenPanel : UIPanelBase
{
    [SerializeField] private Animation splashAnim;

    public void PlaySplashAnim()
    {
        SetVisible(true, true);
        this.splashAnim.Play();
    }

    public void HandlePlayButtonPress()
    {
        Hide();
        AudioManager.IN.StartGameAudio();
    }

    public void HandleTutorialButtonPress()
    {
        Hide();
        //AudioManager.IN.StartGameAudio();
    }
    
    public void HandleFeedbackButtonPress()
    {
        Hide();
    }
}