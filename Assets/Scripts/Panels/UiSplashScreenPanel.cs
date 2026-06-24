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
        GameManager.IN.StartGame();
    }

    public void HandleTutorialButtonPress()
    {
        Hide();
        AudioManager.IN.StartGameAudio();
        TutorialManager.IN.SetTutorialMode(true);
        GameManager.IN.StartGame();//maybe not yet...
    }
    
    public void HandleFeedbackButtonPress()
    {
        Hide();
    }
}