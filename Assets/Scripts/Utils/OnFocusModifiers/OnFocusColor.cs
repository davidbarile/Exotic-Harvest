using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class OnFocusColor : OnFocusModifierBase
{
    private Graphic graphicToColor;

    private Tween fadeTween;

    private Color originalColor;

    [SerializeField] private Color unFocusedColor = Color.white;

    [Space, SerializeField] private bool shouldFade;
    [Range(0, 2f), SerializeField] private float fadeDuration = 1f;

    protected override void Start()
    {
        base.Start();

        this.graphicToColor = GetComponent<Graphic>();
        
        if (this.graphicToColor == null)
            return;

        this.originalColor = this.graphicToColor.color;
    }

    protected override void OnGameFocusChanged(bool hasFocus)
    {
        base.OnGameFocusChanged(hasFocus);

        if (this.graphicToColor == null)
            return;

        if (this.shouldFade)
        {
            this.fadeTween?.Kill();
            this.fadeTween = this.graphicToColor.DOColor(hasFocus ? this.originalColor : this.unFocusedColor, this.fadeDuration);
        }
        else
        {
            this.graphicToColor.color = hasFocus ? this.originalColor : this.unFocusedColor;
        }
    }
}
