using System;
using UnityEngine;
using DG.Tweening;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager IN; 

    public static bool IsDragModeActivated = false;

    public static Action<bool> OnDragModeChanged;

    [SerializeField] private CanvasGroup rootCanvasGroup;
    [SerializeField] private CanvasGroup bgCanvasGroup;

    [SerializeField] private GameObject maximizeButton;

    private bool doesBgBlockClicks;
    private bool appHasFocus = true;

    private int monitorIndex = 1;

    private void Start()
    {
        maximizeButton.SetActive(false);
        SwitchToMonitor(monitorIndex);

        InputManager.OnEscapePress += FadeOutRoot;
        InputManager.OnTabPress += ToggleBackgroundVisibility;
        InputManager.OnDragPress += HandleDragModeChanged;
        InputManager.OnF1Press += ToggleMonitor;
        InputManager.OnMPress += ShowMonitorInfo;
    }

    private void OnDestroy()
    {
        InputManager.OnEscapePress -= FadeOutRoot;
        InputManager.OnTabPress -= ToggleBackgroundVisibility;
        InputManager.OnDragPress -= HandleDragModeChanged;
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
        doesBgBlockClicks = !doesBgBlockClicks;
        bgCanvasGroup.interactable = doesBgBlockClicks;
        bgCanvasGroup.blocksRaycasts = doesBgBlockClicks;

        UiManager.IN.SetDebugText($"App Focus: {appHasFocus}\nBackground Click-thru: {!doesBgBlockClicks}");
            
        // if (isBgShowing)
        // {
        //     FadeOutBackground();

        // }
        // else
        // {
        //     FadeInBackground();
        // }
    }

    private void HandleDragModeChanged()
    {
        IsDragModeActivated = !IsDragModeActivated;
        OnDragModeChanged?.Invoke(IsDragModeActivated);
    }

    private void ToggleMonitor()
    {
        monitorIndex++;
        int monitorCount = Kirurobo.UniWindowController.GetMonitorCount();
        if (monitorIndex >= monitorCount)
        {
            monitorIndex = 0;
        }
        SwitchToMonitor(monitorIndex);
    }
    
    public void FadeInRoot()
    {
        maximizeButton.SetActive(false);

        rootCanvasGroup.gameObject.SetActive(true);
        
        rootCanvasGroup.DOFade(1f, 0.3f).OnComplete(() =>
        {
            rootCanvasGroup.alpha = 1f;
            rootCanvasGroup.interactable = true;
            rootCanvasGroup.blocksRaycasts = true;
        });
    }

    public void FadeOutRoot()
    {
        if (UIPanelBase.CurrentOpenPanel != null)
            return;
            
        maximizeButton.SetActive(true);

        rootCanvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
        {
            rootCanvasGroup.alpha = 0f;
            rootCanvasGroup.interactable = false;
            rootCanvasGroup.blocksRaycasts = false;
            rootCanvasGroup.gameObject.SetActive(false);
        });
    }

    private void FadeInBackground()
    {
        doesBgBlockClicks = true;
        
        bgCanvasGroup.DOFade(1f, 0.5f).OnComplete(() =>
        {
            bgCanvasGroup.alpha = 1f;
            bgCanvasGroup.interactable = true;
            bgCanvasGroup.blocksRaycasts = true;
        });
    }

    private void FadeOutBackground()
    {
        doesBgBlockClicks = false;

        bgCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
        {
            bgCanvasGroup.alpha = 0f;
            bgCanvasGroup.interactable = false;
            bgCanvasGroup.blocksRaycasts = false;
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

    public void SetDragMode(bool isDragMode)
    {
        IsDragModeActivated = isDragMode;
        OnDragModeChanged?.Invoke(IsDragModeActivated);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        appHasFocus = hasFocus;
        UiManager.IN.SetDebugText($"App Focus: {appHasFocus}\nBackground Click-thru: {!doesBgBlockClicks}");
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