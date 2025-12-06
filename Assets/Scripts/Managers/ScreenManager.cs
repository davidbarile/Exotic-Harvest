using System;
using UnityEngine;
using DG.Tweening;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager IN; 

    [SerializeField] private CanvasGroup rootCanvasGroup;
    [SerializeField] private CanvasGroup bgCanvasGroup;

    [SerializeField] private GameObject maximizeButton;

    private bool doesBgBlockClicks;
    private bool appHasFocus = true;

    private int monitorIndex = 1;

    private void Start()
    {
        this.maximizeButton.SetActive(false);
        SwitchToMonitor(this.monitorIndex);

        InputManager.OnEscapePress += HandleEscapeKeyPress;
        InputManager.OnTabPress += ToggleBackgroundVisibility;
        InputManager.OnF1Press += ToggleMonitor;
        InputManager.OnMPress += ShowMonitorInfo;
    }

    private void OnDestroy()
    {
        InputManager.OnEscapePress -= HandleEscapeKeyPress;
        InputManager.OnTabPress -= ToggleBackgroundVisibility;
        InputManager.OnF1Press -= ToggleMonitor;
        InputManager.OnMPress -= ShowMonitorInfo;
    }

    public void ToggleRootVisibility()
    {
        if (rootCanvasGroup.alpha > 0f)
        {
            FadeOutRoot();
        }
        else
        {
            FadeInRoot();
        }
    }

    private void ToggleBackgroundVisibility()
    {
        this.doesBgBlockClicks = !this.doesBgBlockClicks;
        this.bgCanvasGroup.interactable = this.doesBgBlockClicks;
        this.bgCanvasGroup.blocksRaycasts = this.doesBgBlockClicks;

        UiManager.IN.SetDebugText($"App Focus: {this.appHasFocus}\nBackground Click-thru: {!this.doesBgBlockClicks}");
            
        // if (isBgShowing)
        // {
        //     FadeOutBackground();

        // }
        // else
        // {
        //     FadeInBackground();
        // }
    }

    private void ToggleMonitor()
    {
        this.monitorIndex++;
        int monitorCount = Kirurobo.UniWindowController.GetMonitorCount();
        if (this.monitorIndex >= monitorCount)
        {
            this.monitorIndex = 0;
        }
        SwitchToMonitor(this.monitorIndex);
    }
    
    public void FadeInRoot()
    {
        this.maximizeButton.SetActive(false);

        this.rootCanvasGroup.gameObject.SetActive(true);
        
        this.rootCanvasGroup.DOFade(1f, 0.3f).OnComplete(() =>
        {
            this.rootCanvasGroup.alpha = 1f;
            this.rootCanvasGroup.interactable = true;
            this.rootCanvasGroup.blocksRaycasts = true;
        });
    }

    private void HandleEscapeKeyPress()
    {
        if (UIPanelBase.CurrentOpenPanel != null)
            return;

        FadeOutRoot();
    }
    
    public void FadeOutRoot()
    {            
        this.maximizeButton.SetActive(true);

        this.rootCanvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
        {
            this.rootCanvasGroup.alpha = 0f;
            this.rootCanvasGroup.interactable = false;
            this.rootCanvasGroup.blocksRaycasts = false;
            this.rootCanvasGroup.gameObject.SetActive(false);
        });
    }

    private void FadeInBackground()
    {
        this.doesBgBlockClicks = true;
        
        this.bgCanvasGroup.DOFade(1f, 0.5f).OnComplete(() =>
        {
            this.bgCanvasGroup.alpha = 1f;
            this.bgCanvasGroup.interactable = true;
            this.bgCanvasGroup.blocksRaycasts = true;
        });
    }

    private void FadeOutBackground()
    {
        this.doesBgBlockClicks = false;

        this.bgCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
        {
            this.bgCanvasGroup.alpha = 0f;
            this.bgCanvasGroup.interactable = false;
            this.bgCanvasGroup.blocksRaycasts = false;
        });
    }
    
    public void SwitchToMonitor(int monitorIndex)
    {
        // Get the UniWindowController instance
        var uniWin = Kirurobo.UniWindowController.current;
        if (uniWin != null)
        {
            // Disable fitting to prevent automatic monitor switching
            uniWin.shouldFitMonitor = false;

            // On macOS, monitor 0 might not be primary, so let's find the primary monitor
            int monitorCount = Kirurobo.UniWindowController.GetMonitorCount();

            uniWin.monitorToFit = monitorIndex < monitorCount ? monitorIndex : 0;
            
            UiManager.IN.SetDebugText($"Found {monitorCount} monitors. Using monitor {monitorIndex} as primary.");
                
            uniWin.shouldFitMonitor = true;
        }
    }

    private void ShowMonitorInfo()
    {
        var uniWin = Kirurobo.UniWindowController.current;
        if (uniWin != null)
        {
            int monitorCount = Kirurobo.UniWindowController.GetMonitorCount();
            string info = $"Monitors: {monitorCount}\n";

            for (int i = 0; i < monitorCount; i++)
            {
                var rect = Kirurobo.UniWindowController.GetMonitorRect(i);
                info += $"Monitor {i}: {rect.width}x{rect.height} at ({rect.x}, {rect.y})\n";
            }

            info += $"Current: Monitor {uniWin.monitorToFit}, Fit: {uniWin.shouldFitMonitor}";

            UiManager.IN.SetDebugText(info);
            Debug.Log(info);
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        this.appHasFocus = hasFocus;
        UiManager.IN.SetDebugText($"App Focus: {this.appHasFocus}\nBackground Click-thru: {!this.doesBgBlockClicks}");
    }

    void OnApplicationPause(bool pauseStatus)
    {
        UiManager.IN.SetDebugText($"App Paused: {pauseStatus}");
    }

    public void HandleQuitButtonClick()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}