using System;
using UnityEngine;
using UnityEngine.UI;

public class ColorizePanel : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image outline;
    [SerializeField] private Image shadow;
    private void Start()
    {
        ColorManager.OnPanelColorChanged += HandleMenuColorChanged;
    }

    private void OnDestroy()
    {
        ColorManager.OnPanelColorChanged -= HandleMenuColorChanged;
    }

    private void HandleMenuColorChanged(Color inColor)
    {
        if (this.background)
            this.background.color = inColor;

        if (this.outline)
        {
            if(inColor == Color.black)
                this.outline.color = Color.white;
            else
                this.outline.color = Color.Lerp(inColor, Color.black, 0.2f);
        }

        if (this.shadow)
            this.shadow.gameObject.SetActive(inColor.a > 0.9f);
    }
}
