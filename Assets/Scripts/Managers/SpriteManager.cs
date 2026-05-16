using UnityEngine;
using UnityEngine.UI;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager IN;

    private void Awake()
    {
        IN = this;
    }

    public static Sprite GetSprite(string spriteName)
    {
        var sprite = Resources.Load<Sprite>($"Sprites/{spriteName}");
        return sprite;
    }

    public static void SetImageSprite(Image inImage, Sprite inSprite, bool shouldFitInRect = true)
    {
        inImage.sprite = inSprite;

        if (!shouldFitInRect) return;

        var spriteDims = new Vector2(inSprite.texture.width, inSprite.texture.height);
        var rectSize = inImage.rectTransform.rect.size;
       
        var spriteDimsRatio = spriteDims.x / spriteDims.y;
        var rectSizeRatio = rectSize.x / rectSize.y;

        var isSpriteWider = spriteDimsRatio > rectSizeRatio;

        float width;
        float height;
        if (isSpriteWider)
        {
            width = inImage.rectTransform.rect.width;
            height = width / spriteDimsRatio;
        }
        else
        {
            height = inImage.rectTransform.rect.height;
            width = height * spriteDimsRatio;
        }

        inImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        inImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
}