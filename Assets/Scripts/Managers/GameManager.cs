using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Kirurobo;

public class GameManager : MonoBehaviour
{
    public static GameManager IN; 

    public static bool IsDragModeActivated = false;

    public static Action<bool> OnDragModeChanged;

    [SerializeField] private CanvasGroup bgCanvasGroup;

    private bool isBgShowing;

    private int monitorIndex = 1;

    public TMP_Text DebugText;

    private void Awake()
    {
        if (IN == null)
        {
            IN = this;
            DontDestroyOnLoad(gameObject);
            
            SwitchToMonitor(this.monitorIndex);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleQuitButtonClick();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isBgShowing)
                FadeOutBackground();
            else
                FadeInBackground();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            IsDragModeActivated = !IsDragModeActivated;
            OnDragModeChanged?.Invoke(IsDragModeActivated);
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            int monitorCount = Kirurobo.UniWindowController.GetMonitorCount();
            this.monitorIndex = (this.monitorIndex + 1) % monitorCount;
            SwitchToMonitor(this.monitorIndex);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (DebugText != null)
                DebugText.text = $"Frame Count: {Time.frameCount}";
        }
        
        // Debug key to show monitor information
        if (Input.GetKeyDown(KeyCode.M))
        {
            ShowMonitorInfo();
        }
    }

    private void FadeInBackground()
    {
        isBgShowing = true;
        
        bgCanvasGroup.DOFade(1f, 0.5f).OnComplete(() =>
        {
            bgCanvasGroup.alpha = 1f;
            bgCanvasGroup.interactable = true;
            bgCanvasGroup.blocksRaycasts = true;
        });
    }

    private void FadeOutBackground()
    {
        isBgShowing = false;

        bgCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
        {
            bgCanvasGroup.alpha = 0f;
            bgCanvasGroup.interactable = false;
            bgCanvasGroup.blocksRaycasts = false;
        });
    }
    
    public void SwitchToMonitor(int monitorIndex = 0)
    {
        // Get the UniWindowController instance
        var uniWin = Kirurobo.UniWindowController.current;
        if (uniWin != null)
        {
            // Disable fitting to prevent automatic monitor switching
            uniWin.shouldFitMonitor = false;
            uniWin.monitorToFit = 0; // Try monitor 0 first (usually primary)

            // On macOS, monitor 0 might not be primary, so let's find the primary monitor
            int monitorCount = Kirurobo.UniWindowController.GetMonitorCount();
            
            if(monitorCount > monitorIndex)
            {
                uniWin.monitorToFit = monitorIndex;
            }
            
            if (DebugText != null)
                DebugText.text = $"Found {monitorCount} monitors. Using monitor {monitorIndex} as primary.";
                
            uniWin.monitorToFit = monitorIndex;
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

            if (DebugText != null)
                DebugText.text = info;

            Debug.Log(info);
        }
    }
    
     public void HandleQuitButtonClick()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if(DebugText != null)
            DebugText.text = "App Focus: " + hasFocus;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (DebugText != null)
            DebugText.text = "App Paused: " + pauseStatus;
    }
}