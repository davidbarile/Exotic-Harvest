using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OnFocusShow : OnFocusModifierBase
{
    private enum EShowMode
    {
        SetActive,
        EnableGraphic,
        Alpha,
        Fade
    }

    [SerializeField] private EShowMode showMode = EShowMode.Alpha;
    [Space, SerializeField] private bool shouldModifyCanvasBlockRaycasts;
    [Space, Range(0, 2f), SerializeField] private float fadeDuration = 1f;
    [Space, Range(0, 1f)] public float MinAlpha = 0f;
    [Range(0, 1f)] public float MaxAlpha = 1f;

    private CanvasGroup canvasGroupFade;
    private Graphic graphicToEnable;

    private Tween fadeTween;

    protected override void Start()
    {
        this.canvasGroupFade = GetComponent<CanvasGroup>();
        this.graphicToEnable = GetComponent<Graphic>();
        base.Start();
    }
    
    protected override void OnGameFocusChanged(bool hasFocus)
    {
        base.OnGameFocusChanged(hasFocus);

        switch (showMode)
        {
            case EShowMode.SetActive:
                this.gameObject.SetActive(hasFocus);
                break;

            case EShowMode.EnableGraphic:
                if (this.graphicToEnable != null)                
                {
                    this.graphicToEnable.enabled = hasFocus;
                }
                break;

            case EShowMode.Alpha:
                if (this.canvasGroupFade != null)
                {
                    this.canvasGroupFade.alpha = hasFocus ? this.MaxAlpha : this.MinAlpha;
                    if(this.shouldModifyCanvasBlockRaycasts)
                    {
                        this.canvasGroupFade.interactable = hasFocus;
                        this.canvasGroupFade.blocksRaycasts = hasFocus;
                    }
                }
                break;

            case EShowMode.Fade:
                
                if (this.canvasGroupFade != null)
                {
                    this.fadeTween?.Kill();

                    this.fadeTween = this.canvasGroupFade.DOFade(hasFocus ? this.MaxAlpha : this.MinAlpha, this.fadeDuration);
                    if(this.shouldModifyCanvasBlockRaycasts)
                    {
                        this.canvasGroupFade.interactable = hasFocus;
                        this.canvasGroupFade.blocksRaycasts = hasFocus;
                    }
                }
                break;
        }
    }
}
