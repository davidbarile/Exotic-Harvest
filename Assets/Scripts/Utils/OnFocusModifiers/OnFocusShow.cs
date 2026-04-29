using UnityEngine;
using DG.Tweening;

public class OnFocusShow : OnFocusModifierBase
{
    private enum EShowMode
    {
        Enable,
        Alpha,
        Fade
    }

    [SerializeField] private EShowMode showMode = EShowMode.Alpha;
    [Range(0, 2f), SerializeField] private float fadeDuration = 1f;
    [Range(0, 1f), SerializeField] private float minAlpha = 0f;
    [Range(0, 1f), SerializeField] private float maxAlpha = 1f;

    private CanvasGroup canvasGroupFade;

    private Tween fadeTween;

    protected override void Start()
    {
        this.canvasGroupFade = GetComponent<CanvasGroup>();
        base.Start();
    }
    
    protected override void OnGameFocusChanged(bool hasFocus)
    {
        switch (showMode)
        {
            case EShowMode.Enable:
                this.gameObject.SetActive(hasFocus);
                break;

            case EShowMode.Alpha:
                if (this.canvasGroupFade != null)
                {
                    this.canvasGroupFade.alpha = hasFocus ? this.maxAlpha : this.minAlpha;
                    this.canvasGroupFade.interactable = hasFocus;
                    this.canvasGroupFade.blocksRaycasts = hasFocus;
                }
                break;

            case EShowMode.Fade:
                
                if (this.canvasGroupFade != null)
                {
                    this.fadeTween?.Kill();

                    this.fadeTween = this.canvasGroupFade.DOFade(hasFocus ? this.maxAlpha : this.minAlpha, this.fadeDuration);
                    this.canvasGroupFade.interactable = hasFocus;
                    this.canvasGroupFade.blocksRaycasts = hasFocus;
                }
                break;
        }
    }
}
