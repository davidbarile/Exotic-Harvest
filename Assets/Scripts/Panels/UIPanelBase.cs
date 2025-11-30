using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UIPanelBase : MonoBehaviour
{
    public static UIPanelBase CurrentOpenPanel;
    public bool IsShowing { get; private set; }

    [SerializeField] private bool shouldFadeInOut;
    [Range(0f, 1f), SerializeField] private float tweenDuration = 0.3f;

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

    public virtual void Show()
    {
        InputManager.OnEscapePress += SetCurrentPanel;
        SetVisible(true);
    }

    public virtual void Hide()
    {
        SetVisible(false);
        InputManager.OnEscapePress -= SetCurrentPanel; 
    }

    private void SetCurrentPanel()
    {
        if (CurrentOpenPanel != null && CurrentOpenPanel == this)
            CurrentOpenPanel.Hide();
    }

    [Tooltip("This avoids calling functionality of overrides")]
    public void SetVisible(bool inIsVisible, bool inSkipFade = false)
    {
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
        if (inIsVisible)
            CurrentOpenPanel = this;
        else if (CurrentOpenPanel == this)
            CurrentOpenPanel = null;
   
        if (inIsVisible)
            this.gameObject.SetActive(true);

        if (this.Canvas != null)
            this.Canvas.enabled = inIsVisible;
        else
            this.gameObject.SetActive(inIsVisible);
    }

    public void FadeIn()
    {
        CanvasShowHide(true);

        RootCanvasGroup.DOFade(1f, tweenDuration).OnComplete(() =>
        {
            RootCanvasGroup.alpha = 1f;
            RootCanvasGroup.interactable = true;
            RootCanvasGroup.blocksRaycasts = true;
        });
    }

    public void FadeOut()
    {
        RootCanvasGroup.DOFade(0f, tweenDuration).OnComplete(() =>
        {
            RootCanvasGroup.alpha = 0f;
            RootCanvasGroup.interactable = false;
            RootCanvasGroup.blocksRaycasts = false;
            CanvasShowHide(false);
        });
    }
}