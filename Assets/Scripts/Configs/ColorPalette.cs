using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "Exotic Harvest/ColorPalette")]
public class ColorPalette : ScriptableObject
{
    public enum EColorType
    {
        Main,
        Dark,
        Light,
        Disabled
    }

    public Color GetColorByType(EColorType colorType)
    {
        switch (colorType)
        {
            case EColorType.Main:
                return this.mainColor;
            case EColorType.Dark:
                return this.darkColor;
            case EColorType.Light:
                return this.lightColor;
            case EColorType.Disabled:
                return this.disabledColor;
            default:
                return Color.white;
        }
    }

    [Header("Colors")]
    [SerializeField] private Color mainColor = Color.white;
    [SerializeField] private Color darkColor = Color.white;
    [SerializeField] private Color lightColor = Color.white;
    [SerializeField] private Color disabledColor = Color.grey;

    [SerializeField] private bool autoGenerateShades = true;

    private void OnValidate()
    {
        if (this.autoGenerateShades)
        {
            this.darkColor = new Color(this.mainColor.r * 0.7f, this.mainColor.g * 0.7f, this.mainColor.b * 0.7f);
            this.lightColor = new Color(this.mainColor.r * 1.4f, this.mainColor.g * 1.4f, this.mainColor.b * 1.4f);
        }
    }
}