using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UiFadeTween : MonoBehaviour
{
    [SerializeField] private bool fadeInOnStart = true;
    [Range(0, 2f),SerializeField] private float fadeDuration = 1f;
    public CanvasGroup CanvasGroup { get;  private set; }
    private Tweener tweener;

    private void Awake()
    {
        this.CanvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (this.fadeInOnStart)
        {
            SetAlpha(0f);
            FadeIn();
        }
    }

    public void SetAlpha(float inAlpha)
    {
        this.CanvasGroup.alpha = inAlpha;
    }

    public void FadeIn()
    {
        this.tweener?.Kill();
        this.gameObject.SetActive(true);
        this.tweener = this.CanvasGroup.DOFade(1f, this.fadeDuration).OnComplete(() =>
        {
            SetCanvasGroupInteractable(true);
        });
    }

    public void FadeToVisibility(bool inIsVisible)
    {
        if (inIsVisible)
            FadeIn();
        else
            FadeOut();
    }

    public void FadeOut()
    {
        SetCanvasGroupInteractable(false);

        this.tweener?.Kill();
        this.tweener = this.CanvasGroup.DOFade(0f, this.fadeDuration).OnComplete(() =>
        {
            this.gameObject.SetActive(false);
        });
    }

    public void SetCanvasGroupInteractable(bool inIsInteractable)
    {
        this.CanvasGroup.interactable = inIsInteractable;
        this.CanvasGroup.blocksRaycasts = inIsInteractable;
    }
}