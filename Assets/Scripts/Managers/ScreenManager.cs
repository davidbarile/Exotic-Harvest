using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager IN;

    public static float WorldToScreenRatio { get; private set; }
    public static RectTransform WorldScreenRectTrans => IN.worldScreenRectTrans;

    public static Action<bool> OnMinimizeMaximizeToggled;
    public static Action<bool> OnGameFocusChanged;

    [SerializeField] private CanvasGroup rootCanvasGroup;
    [SerializeField] private CanvasGroup worldCanvasGroup;
    [Space, SerializeField] private UiFadeTween worldDecorationsFader;
    [SerializeField] private UiFadeTween screenDecorationsFader;
    [Space, SerializeField] private CanvasGroup bgCanvasGroup;

    [Space, SerializeField] private CanvasGroup followMoonCanvasGroup;
    [SerializeField] private CanvasGroup sunCanvasGroup;

    [Space, SerializeField] private _2dxFX_Distortion[] worldDistortionEffects;
    private List<Image> litShaderImages = new();

    [SerializeField] private GameObject maximizePanel;

    public Material DefaultSpriteMaterial => this.defaultSpriteMaterial;
    [SerializeField] private Material defaultSpriteMaterial;
    public Material LitShaderMaterial => this.litShaderMaterial;
    [SerializeField] private Material litShaderMaterial;

    [Header("Items to Hide on Minimize")]
    [SerializeField] private GameObject[] sunAndMoon;
    [SerializeField] private GameObject[] clouds;
    [SerializeField] private GameObject[] mountains;
    [Space, SerializeField] private GameObject[] collidersToDisableOnMinimize;

    [SerializeField] private RectTransform worldScreenRectTrans;

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

    public void Init()
    {
        GatherLitShaderImages();

        var bgAlpha = PlatformManager.IsMobile ? 1f : SaveManager.Data.BgAlpha;

        SetWorldBgAlpha(bgAlpha);

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

    private void Update()
    {
        //Calculate World to Screen ratio
        var bl = UiManager.IN.WorldCamera.ViewportToWorldPoint(new Vector3(0, 0, 1000f));
        var br = UiManager.IN.WorldCamera.ViewportToWorldPoint(new Vector3(1, 0, 1000f));
        var tl = UiManager.IN.WorldCamera.ViewportToWorldPoint(new Vector3(0, 1, 1000f));
        var tr = UiManager.IN.WorldCamera.ViewportToWorldPoint(new Vector3(1, 1, 1000f));

        // position and size the worldScreenRect to match the world camera's viewport in world space, so we can use it as a reference for placing world-space objects that should align with the screen
        this.worldScreenRectTrans.position = (bl + tr) / 2f;
        var worldWidth = Vector3.Distance(bl, br);
        var worldHeight = Vector3.Distance(bl, tl);
        this.worldScreenRectTrans.sizeDelta = new Vector2(worldWidth, worldHeight);

        WorldToScreenRatio = worldWidth / Screen.width;
    }

    public void ToggleRootVisibility()
    {
        SetMinOrMaximized(this.rootCanvasGroup.alpha == 0f);
    }

    private void ToggleBackgroundVisibility()
    {
        SetBgVisibility(this.isClickThrough);
    }

    public void SetBgVisibility(bool inIsVisible)
    {
        if (inIsVisible != this.isClickThrough)
            return;

        this.isClickThrough = !inIsVisible;

        SetCanvasGroupInteractable(this.bgCanvasGroup, inIsVisible);
        SetCollidersEnabled(inIsVisible);

        UiManager.IN.SetDebugText($"SetBgVis({inIsVisible}) App Focus: {this.appHasFocus}\nBg Click-thru: {this.isClickThrough}", true);
    }

    private void ToggleDecorationsVisibility()
    {
        SetDecorationsVisibility(this.isClickThrough);
    }

    public void SetDecorationsVisibility(bool inIsVisible)
    {
        this.worldDecorationsFader.FadeToVisibility(inIsVisible);
        this.screenDecorationsFader.FadeToVisibility(inIsVisible);
    }

    private void SetCollidersEnabled(bool inEnabled)
    {
        foreach (var colGO in this.collidersToDisableOnMinimize)
        {
            colGO.SetActive(inEnabled);
        }
    }

    public void ToggleElementsVisibility(bool inShouldShow)
    {
        //TODO: refactor to enable/disable OnFocusShow components 
        UiManager.IN.TimeWeatherPanel.gameObject.SetActive(inShouldShow || UiManager.IN.SettingsPanel.ShowTimeWeatherPanelToggle.isOn);
        UiManager.IN.UiButtonsPanel.SetActive(inShouldShow || UiManager.IN.SettingsPanel.ShowUiButtonsToggle.isOn);
        UiManager.IN.NotificationsCanvas.enabled = inShouldShow || UiManager.IN.SettingsPanel.ShowNotificationsToggle.isOn;
        
        this.worldDecorationsFader.FadeToVisibility(inShouldShow || UiManager.IN.SettingsPanel.ShowDecorationsToggle.isOn);
        this.screenDecorationsFader.FadeToVisibility(inShouldShow || UiManager.IN.SettingsPanel.ShowDecorationsToggle.isOn);

        foreach (var item in this.sunAndMoon)
        {
            item.SetActive(inShouldShow || UiManager.IN.SettingsPanel.ShowSunAndMoonToggle.isOn);
        }

        foreach (var item in this.clouds)
        {
            item.SetActive(inShouldShow || UiManager.IN.SettingsPanel.ShowCloudsToggle.isOn);
        }

        foreach (var item in this.mountains)
        {
            item.SetActive(inShouldShow || UiManager.IN.SettingsPanel.ShowMountainsToggle.isOn);
        }
    }

    //called by slider in Settings menu
    public void SetWorldBgAlpha(float inAlpha)
    {
        this.bgCanvasGroup.alpha = inAlpha;

        SetWorldEffectsAlpha(inAlpha);
        if (this.bgCanvasGroup.TryGetComponent<OnFocusShow>(out var onFocusShow))
        {
            //onFocusShow.MinAlpha = inAlpha;
            onFocusShow.MaxAlpha = inAlpha;
        }
    }

    public void SetWorldEffectsAlpha(float inAlpha)
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

        UiManager.IN.MinimizePanel.HandleSelectedButtonPress();
    }
    
    public void SetMinOrMaximized(bool inIsMaximized)
    {
        if (inIsMaximized)
            FadeInRoot();
        else
            FadeOutRoot();

        if(Application.isEditor)
            OnApplicationFocus(inIsMaximized);

        OnMinimizeMaximizeToggled?.Invoke(inIsMaximized);
    }
    
    private void FadeInRoot()
    {
        this.maximizePanel.SetActive(false);

        SetCollidersEnabled(true);

        this.rootCanvasGroup.gameObject.SetActive(true);
        var alpha = this.rootCanvasGroup.alpha;
        DOVirtual.Float(alpha, 1f, 0.5f, value =>
        {
            SetWorldEffectsAlpha(value);
        });

        //TODO: break this into many small canvas groups so we can specify which ones to show/hide based on settings
        this.rootCanvasGroup.DOFade(1f, 0.5f).OnComplete(() =>
        {
            SetCanvasGroupInteractable(this.rootCanvasGroup, true, 1f);
        });
        
        //TODO: break this into many small canvas groups so we can specify which ones to show/hide based on settings
        this.worldCanvasGroup.gameObject.SetActive(true);
        
        this.worldCanvasGroup.DOFade(1f, 0.5f).OnComplete(() =>
        {
            SetCanvasGroupInteractable(this.worldCanvasGroup, true, 1f);
        });
    }
    
    private void FadeOutRoot()
    {
        this.maximizePanel.SetActive(true);

        SetCollidersEnabled(false);
        
        var alpha = this.rootCanvasGroup.alpha;
        DOVirtual.Float(alpha, 0f, 0.5f, value =>
        {
            SetWorldEffectsAlpha(value);
        });

        //TODO: break this into many small canvas groups so we can specify which ones to show/hide based on settings
        this.rootCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
        {
            SetCanvasGroupInteractable(this.rootCanvasGroup, false, 0f);
            this.rootCanvasGroup.gameObject.SetActive(false);
        });

        //TODO: break this into many small canvas groups so we can specify which ones to show/hide based on settings
        this.worldCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
        {
            SetCanvasGroupInteractable(this.worldCanvasGroup, false, 0f);
            this.worldCanvasGroup.gameObject.SetActive(false);
        });
    }
    
    public void SwitchToMonitor(int inMonitorIndex)
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

            uniWin.monitorToFit = inMonitorIndex < monitorCount ? inMonitorIndex : 0;

            UiManager.IN.SetDebugText($"Found {monitorCount} monitors. Using monitor {inMonitorIndex} as primary.", true);

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

    private void OnApplicationFocus(bool inHasFocus)
    {
        this.appHasFocus = inHasFocus;
        UiManager.IN.SetDebugText($"App Focus: {this.appHasFocus}\nBg Click-thru: {this.isClickThrough}", true);
        SetBgVisibility(inHasFocus);
        SetDecorationsVisibility(inHasFocus);
        OnGameFocusChanged?.Invoke(inHasFocus);
    }

    private void OnApplicationPause(bool inPauseStatus)
    {
        UiManager.IN.SetDebugText($"App Paused: {inPauseStatus}", true);
    }

    public void HandleQuitButtonClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    public static Vector3 TransformPositionToScreenSpace(Vector3 inPosition)
    {
        if (UiManager.IN == null)
            return Vector3.zero;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(UiManager.IN.DragCanvas, inPosition, UiManager.IN.WorldCamera, out Vector3 outWorldPos))
            inPosition = outWorldPos;

        return inPosition;
    }

    public static Vector3 TransformPositionToWorldSpace(Vector3 inPosition)
    {
        if (UiManager.IN == null)
            return Vector3.zero;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(UiManager.IN.WorldRectTrans, inPosition, UiManager.IN.DragCamera, out Vector3 outWorldPos))
            inPosition = outWorldPos;

        return inPosition;
    }
}