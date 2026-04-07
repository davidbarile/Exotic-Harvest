using System;
using UnityEngine;
using static ColorPalette;

public class ColorManager : MonoBehaviour
{
    public static ColorManager IN;

    public static Action<Color> OnPanelColorChanged;

    [Serializable]
    public struct ResourcePalette
    {
        public EResourceCategory Category;
        public ColorPalette Palette;
    }

    [Header("Panel Colors")]
    public Color PanelColor = Color.black;

    [Header("Resource Colors")]
    [SerializeField] private ResourcePalette[] resourceColors;

    private void Start()
    {
        OnPanelColorChanged?.Invoke(this.PanelColor);
    }

    private void Update()
    {
        OnPanelColorChanged?.Invoke(this.PanelColor);
    }

    public ColorPalette GetResourceCategoryColors(EResourceCategory resourceType)
    {
        foreach (var resourceColor in this.resourceColors)
        {
            if (resourceColor.Category == resourceType)
                return resourceColor.Palette;
        }
        return null;
    }

    public Color GetResourceCategoryColor(EResourceCategory resourceType, EColorType colorType)
    {
        var palette = GetResourceCategoryColors(resourceType);
        return palette.GetColorByType(colorType);
    }
    
}