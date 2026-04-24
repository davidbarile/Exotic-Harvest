using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager IN;

    public static Action<bool> OnMinimizeMaximizeToggled;

    [SerializeField] private CanvasGroup rootCanvasGroup;
    [SerializeField] private CanvasGroup worldCanvasGroup;
    [SerializeField] private CanvasGroup decorationsCanvasGroup;
    [SerializeField] private CanvasGroup bgCanvasGroup;

    [Space, SerializeField] private CanvasGroup followMoonCanvasGroup;
    [SerializeField] private CanvasGroup sunCanvasGroup;

    [Space, SerializeField] private _2dxFX_Distortion[] worldDistortionEffects;
    private List<Image> litShaderImages = new();

    [SerializeField] private GameObject maximizePanel;

    [SerializeField] private Material litShaderMaterial;

    [Header("Items to Hide on Minimize")]
    [SerializeField] private GameObject timeAndWeatherPanel;
    [SerializeField] private GameObject panelsButtons;
    [SerializeField] private GameObject notifications;
    [SerializeField] private GameObject decorationsPanel;
    [SerializeField] private GameObject[] sunAndMoon;
    [SerializeField] private GameObject[] clouds;
    [SerializeField] private GameObject[] mountains;
    [Space, SerializeField] private GameObject[] collidersToDisableOnMinimize;

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
        GatherLitShaderImages();

        this.bgCanvasGroup.alpha = PlatformManager.IsMobile ? 1f : .5f;

        this.maximizePanel.SetActive(false);
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
        SetMinOrMaximized(this.rootCanvasGroup.alpha == 0f);
    }

    private void ToggleBackgroundVisibility()
    {
        this.isClickThrough = !this.isClickThrough;

        SetCanvasGroupInteractable(this.bgCanvasGroup, !this.isClickThrough);
        SetCanvasGroupInteractable(this.decorationsCanvasGroup, !this.isClickThrough);

        ToggleColliders(!this.isClickThrough);

        UiManager.IN.SetDebugText($"App Focus: {this.appHasFocus}\nBackground Click-thru: {this.isClickThrough}", true);
    }

    private void ToggleColliders(bool enable)
    {
        foreach (var col in this.collidersToDisableOnMinimize)
        {
            col.SetActive(enable);
        }
    }

    private void ToggleElementsVisibility(bool show)
    {
        //TODO: tell the canvas groups to fade in/out or not based on settings
        this.timeAndWeatherPanel.SetActive(show || UiManager.IN.SettingsPanel.ShowTimeWeatherPanelToggle.isOn);
        this.notifications.SetActive(show || UiManager.IN.SettingsPanel.ShowNotificationsToggle.isOn);
        this.panelsButtons.SetActive(show || UiManager.IN.SettingsPanel.ShowPanelsButtonsToggle.isOn);
        this.decorationsPanel.SetActive(show || UiManager.IN.SettingsPanel.ShowDecorationsToggle.isOn);

        foreach (var item in this.sunAndMoon)
        {
            item.SetActive(show || UiManager.IN.SettingsPanel.ShowSunAndMoonToggle.isOn);
        }

        foreach (var item in this.clouds)
        {
            item.SetActive(show || UiManager.IN.SettingsPanel.ShowCloudsToggle.isOn);
        }

        foreach (var item in this.mountains)
        {
            item.SetActive(show || UiManager.IN.SettingsPanel.ShowMountainsToggle.isOn);
        }
    }

    public void SetWorldBgAlpha(float inAlpha)
    {
        this.bgCanvasGroup.alpha = inAlpha;
        SetWordEffectsAlpha(inAlpha);
    }

    public void SetWordEffectsAlpha(float inAlpha)
    {
        foreach (var effect in this.worldDistortionEffects)
        {
            effect._Alpha = inAlpha;
            effect.gameObject.SetActive(inAlpha > 0f);
        }

        this.sunCanvasGroup.alpha = inAlpha;
        this.followMoonCanvasGroup.alpha = inAlpha;

        foreach (var img in this.litShaderImages)
        {
            var color = img.color;
            color.a = inAlpha;
            img.color = color;
        }
    }

    //call this when Settings menu is opened, to ensure we have the correct list of lit shader images in case something changed
    public void GatherLitShaderImages()
    {
        var litImages = this.worldCanvasGroup.GetComponentsInChildren<Image>(true);
        this.litShaderImages.Clear();

        foreach (var img in litImages)
        {
            if (img.material == this.litShaderMaterial)
            {
                this.litShaderImages.Add(img);
            }
        }
    }

    private void ToggleMonitor()
    {
#if UNITY_STANDALONE
        this.monitorIndex++;
        
        var monitorCount = Kirurobo.UniWindowController.GetMonitorCount();

        if (this.monitorIndex >= monitorCount)
        {
            this.monitorIndex = 0;
        }
        SwitchToMonitor(this.monitorIndex);
#endif
    }

    private void HandleEscapeKeyPress()
    {
        if (UIPanelBase.CurrentOpenPanel != null)
            return;

        SetMinOrMaximized(false);
    }

    public void HandleMaximizeButtonClick()
    {
        if (DragManager.IsDragModeActivated)
            return;

        SetMinOrMaximized(true);
    }
    
    public void SetMinOrMaximized(bool inIsMaximized)
    {
        if (inIsMaximized)
            FadeInRoot();
        else
            FadeOutRoot();

        OnMinimizeMaximizeToggled?.Invoke(inIsMaximized);
    }
    
    private void FadeInRoot()
    {
        this.maximizePanel.SetActive(false);

        ToggleElementsVisibility(true);
        ToggleColliders(true);

        this.rootCanvasGroup.gameObject.SetActive(true);
        var alpha = this.rootCanvasGroup.alpha;
        DOVirtual.Float(alpha, 1f, 0.3f, value =>
        {
            SetWordEffectsAlpha(value);
        });

        //TODO: break this into many small canvas groups so we can specify which ones to show/hide based on settings
        this.rootCanvasGroup.DOFade(1f, 0.3f).OnComplete(() =>
        {
            SetCanvasGroupInteractable(this.rootCanvasGroup, true, 1f);
        });
        
        //TODO: break this into many small canvas groups so we can specify which ones to show/hide based on settings
        this.worldCanvasGroup.gameObject.SetActive(true);
        
        this.worldCanvasGroup.DOFade(1f, 0.3f).OnComplete(() =>
        {
            SetCanvasGroupInteractable(this.worldCanvasGroup, true, 1f);
        });
    }
    
    private void FadeOutRoot()
    {
        this.maximizePanel.SetActive(true);

        ToggleElementsVisibility(false);
        ToggleColliders(false);
        
        var alpha = this.rootCanvasGroup.alpha;
        DOVirtual.Float(alpha, 0f, 0.3f, value =>
        {
            SetWordEffectsAlpha(value);
        });

        //TODO: break this into many small canvas groups so we can specify which ones to show/hide based on settings
        this.rootCanvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
        {
            SetCanvasGroupInteractable(this.rootCanvasGroup, false, 0f);
            this.rootCanvasGroup.gameObject.SetActive(false);
        });

        //TODO: break this into many small canvas groups so we can specify which ones to show/hide based on settings
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
            ToggleColliders(true);
        });
    }

    private void FadeOutBackground()
    {
        this.isClickThrough = true;

        this.bgCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
        {
            SetCanvasGroupInteractable(this.bgCanvasGroup, false, 0f);
            ToggleColliders(false);
        });
    }
    
    public void SwitchToMonitor(int monitorIndex)
    {
#if UNITY_STANDALONE
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
#endif
    }

    private void ShowMonitorInfo()
    {
#if UNITY_STANDALONE
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
#endif
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