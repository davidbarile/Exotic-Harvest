using System;
using UnityEngine;
using static GlobalEnums;

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

    [Serializable]
    public struct NotificationPalette
    {
        public ENotificationType Type;
        public ColorPalette Palette;
        public Sprite Icon;
    }

    [Header("Panel Colors")]
    public Color PanelColor = Color.black;

    [Header("Resource Colors")]
    [SerializeField] private ResourcePalette[] resourceColors;

    [Header("Notification Colors")]
    [SerializeField] private NotificationPalette[] notificationColors;

    [Header("Saturation")]
    public Material SaturateMaterial;
    public Material DesaturateMaterial;


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

    public ColorPalette GetNotificationTypeColors(ENotificationType notificationType)
    {
        foreach (var notificationColor in this.notificationColors)
        {
            if (notificationColor.Type == notificationType)
                return notificationColor.Palette;
        }
        return null;
    }

    public Color GetNotificationTypeColor(ENotificationType notificationType, EColorType colorType)
    {
        var palette = GetNotificationTypeColors(notificationType);
        return palette.GetColorByType(colorType);
    }
}