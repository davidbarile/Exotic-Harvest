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

    private _2dxFX_Distortion[] worldDistortionEffects;

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

        if (this.invertLogic)
            hasFocus = !hasFocus;

        var alphaValue = hasFocus ? this.MaxAlpha : this.MinAlpha;

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
                    this.canvasGroupFade.alpha = alphaValue;
                    if (this.shouldModifyCanvasBlockRaycasts)
                    {
                        this.canvasGroupFade.interactable = hasFocus;
                        this.canvasGroupFade.blocksRaycasts = hasFocus;
                    }
                }

                this.worldDistortionEffects ??= GetComponentsInChildren<_2dxFX_Distortion>(true);
                
                foreach (var effect in this.worldDistortionEffects)
                {
                    effect._Alpha = alphaValue;
                    effect.gameObject.SetActive(alphaValue > 0f);
                }
                break;

            case EShowMode.Fade:

                if (this.canvasGroupFade != null)
                {
                    this.fadeTween?.Kill();

                    this.fadeTween = this.canvasGroupFade.DOFade(alphaValue, this.fadeDuration);
                    if (this.shouldModifyCanvasBlockRaycasts)
                    {
                        this.canvasGroupFade.interactable = hasFocus;
                        this.canvasGroupFade.blocksRaycasts = hasFocus;
                    }
                }
                
                this.worldDistortionEffects ??= GetComponentsInChildren<_2dxFX_Distortion>(true);
                
                foreach (var effect in this.worldDistortionEffects)
                {
                    DOTween.To(() => effect._Alpha, x => effect._Alpha = x, alphaValue, this.fadeDuration).OnStart(() =>
                    {
                        if (alphaValue > 0f)
                            effect.gameObject.SetActive(true);
                    }).OnComplete(() =>
                    {
                        if (alphaValue <= 0f)
                            effect.gameObject.SetActive(false);
                    });
                }
                break;
        }
    }
}
