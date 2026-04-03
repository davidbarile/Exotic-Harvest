using UnityEngine;
using DG.Tweening;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager IN;

    [SerializeField] private CanvasGroup rootCanvasGroup;
     [SerializeField] private CanvasGroup worldCanvasGroup;
    [SerializeField] private CanvasGroup decorationsCanvasGroup;
    [SerializeField] private CanvasGroup bgCanvasGroup;

    [SerializeField] private GameObject maximizeButton;

    private bool isClickThrough;
    private bool appHasFocus = true;

    private int monitorIndex = 1;

    public static void SetCanvasGroupInteractable(CanvasGroup canvasGroup, bool isInteractable, float alpha = -1f)
    {
        canvasGroup.interactable = isInteractable;
        canvasGroup.blocksRaycasts = isInteractable;

        if (alpha >= 0f)
            canvasGroup.alpha = alpha;
    }

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
        if (this.rootCanvasGroup.alpha > 0f)
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
        this.isClickThrough = !this.isClickThrough;

        SetCanvasGroupInteractable(this.bgCanvasGroup, !this.isClickThrough);
        SetCanvasGroupInteractable(this.decorationsCanvasGroup, !this.isClickThrough);

        UiManager.IN.SetDebugText($"App Focus: {this.appHasFocus}\nBackground Click-thru: {this.isClickThrough}", true);

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

    private void HandleEscapeKeyPress()
    {
        if (UIPanelBase.CurrentOpenPanel != null)
            return;

        FadeOutRoot();
    }
    
    public void FadeInRoot()
    {
        this.maximizeButton.SetActive(false);

        this.rootCanvasGroup.gameObject.SetActive(true);

        this.rootCanvasGroup.DOFade(1f, 0.3f).OnComplete(() =>
        {
            SetCanvasGroupInteractable(this.rootCanvasGroup, true, 1f);
        });
        
        this.worldCanvasGroup.gameObject.SetActive(true);
        
        this.worldCanvasGroup.DOFade(1f, 0.3f).OnComplete(() =>
        {
            SetCanvasGroupInteractable(this.worldCanvasGroup, true, 1f);
        });
    }
    
    public void FadeOutRoot()
    {            
        this.maximizeButton.SetActive(true);

        this.rootCanvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
        {
            SetCanvasGroupInteractable(this.rootCanvasGroup, false, 0f);
            this.rootCanvasGroup.gameObject.SetActive(false);
        });

        this.worldCanvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
        {
            SetCanvasGroupInteractable(this.worldCanvasGroup, false, 0f);
            this.worldCanvasGroup.gameObject.SetActive(false);
        });
    }

    private void FadeInBackground()
    {
        this.isClickThrough = false;
        
        this.bgCanvasGroup.DOFade(1f, 0.5f).OnComplete(() =>
        {
            SetCanvasGroupInteractable(this.bgCanvasGroup, true, 1f);
        });
    }

    private void FadeOutBackground()
    {
        this.isClickThrough = true;

        this.bgCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
        {
            SetCanvasGroupInteractable(this.bgCanvasGroup, false, 0f);
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
            
            UiManager.IN.SetDebugText($"Found {monitorCount} monitors. Using monitor {monitorIndex} as primary.", true);
                
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

            UiManager.IN.SetDebugText(info, true);
            Debug.Log(info);
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        this.appHasFocus = hasFocus;
        UiManager.IN.SetDebugText($"App Focus: {this.appHasFocus}\nBackground Click-thru: {this.isClickThrough}", true);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        UiManager.IN.SetDebugText($"App Paused: {pauseStatus}", true);
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