using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UIPanelBase : MonoBehaviour
{
    public static UIPanelBase CurrentOpenPanel;
    public bool IsShowing { get; private set; }

    [SerializeField] private bool shouldFadeInOut;
    [Range(0f, 1f), SerializeField] private float tweenDuration = 0.3f;

    [SerializeField] private bool canEscapeKeyClose;

    private Canvas Canvas
    {
        get
        {
            if (this.canvas == null)
                this.canvas = this.GetComponent<Canvas>();

            return this.canvas;
        }
    }

    private CanvasGroup RootCanvasGroup
    {
        get
        {
            if (this.rootCanvasGroup == null)
                this.rootCanvasGroup = this.GetComponent<CanvasGroup>();

            return this.rootCanvasGroup;
        }
    }

    private CanvasGroup rootCanvasGroup;

    [Header("Serialized Automatically if null")]
    [SerializeField] private Canvas canvas;

    private void OnEnable()
    {
        if (this.canEscapeKeyClose)
            InputManager.OnEscapePress += HideCurrentPanel;
    }

    private void OnDisable()
    {
        if (this.canEscapeKeyClose)
            InputManager.OnEscapePress -= HideCurrentPanel;
    }

    public virtual void Show()
    {
        SetVisible(true);
    }

    public virtual void Hide()
    {
        SetVisible(false);
    }
    
    public virtual void Toggle()
    {
        if(this.IsShowing)
            Hide();
        else
            Show();
    }

    protected virtual void RegisterEvents() {}

    protected virtual void UnregisterEvents() {}

    private void HideCurrentPanel()
    {
        if (CurrentOpenPanel != null && CurrentOpenPanel == this)
            CurrentOpenPanel.Hide();
    }

    [Tooltip("This avoids calling functionality of overrides")]
    public void SetVisible(bool inIsVisible, bool inSkipFade = false)
    {
        if (inIsVisible == this.IsShowing)
            return;
            
        this.IsShowing = inIsVisible;

        if (this.shouldFadeInOut && !inSkipFade)
        {
            if (inIsVisible)
                FadeIn();
            else
                FadeOut();

            return;
        }

        CanvasShowHide(inIsVisible);
    }
    
    private void CanvasShowHide(bool inIsVisible)
    {
        if(this.canEscapeKeyClose)
        {
            if (inIsVisible)
                CurrentOpenPanel = this;
            else if (CurrentOpenPanel == this)
                CurrentOpenPanel = null;
        }

        if (inIsVisible)
            this.gameObject.SetActive(true);

        if (this.Canvas != null)
            this.Canvas.enabled = inIsVisible;
        else
            this.gameObject.SetActive(inIsVisible);

        if (inIsVisible)
            RegisterEvents();
        else
            UnregisterEvents();
    }

    public void FadeIn()
    {
        CanvasShowHide(true);

        this.RootCanvasGroup.DOFade(1f, this.tweenDuration).OnComplete(() =>
        {
            this.RootCanvasGroup.alpha = 1f;
            this.RootCanvasGroup.interactable = true;
            this.RootCanvasGroup.blocksRaycasts = true;
        });
    }

    public void FadeOut()
    {
        this.RootCanvasGroup.DOFade(0f, this.tweenDuration).OnComplete(() =>
        {
            this.RootCanvasGroup.alpha = 0f;
            this.RootCanvasGroup.interactable = false;
            this.RootCanvasGroup.blocksRaycasts = false;
            CanvasShowHide(false);
        });
    }
}