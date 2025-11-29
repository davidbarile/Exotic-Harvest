using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Kirurobo;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager IN; 

    public static bool IsDragModeActivated = false;

    public static Action<bool> OnDragModeChanged;

    [SerializeField] private CanvasGroup rootCanvasGroup;
    [SerializeField] private CanvasGroup bgCanvasGroup;

    [SerializeField] private GameObject showButton;

    private bool isBgShowing;

    private int monitorIndex = 1;

    public TMP_Text DebugText;

    private void Awake()
    {
        if (IN == null)
        {
            IN = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        isBgShowing = bgCanvasGroup.alpha > 0f;
        showButton.SetActive(false);
        SwitchToMonitor(monitorIndex);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            FadeOutRoot();
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
    
    public void FadeInRoot()
    {
        showButton.SetActive(false);

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
        showButton.SetActive(true);

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
            
            if (DebugText != null)
                DebugText.text = $"Found {monitorCount} monitors. Using monitor {monitorIndex} as primary.";
                
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