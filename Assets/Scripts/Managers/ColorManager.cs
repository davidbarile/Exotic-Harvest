using System;
using UnityEngine;
using static ColorPalette;

public class ColorManager : MonoBehaviour
{
    public static ColorManager IN;

    [Serializable]
    public struct ResourcePalette
    {
        public EResourceCategory Category;
        public ColorPalette Palette;
    }

    [Header("Resource Colors")]
    [SerializeField] private ResourcePalette[] resourceColors;

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