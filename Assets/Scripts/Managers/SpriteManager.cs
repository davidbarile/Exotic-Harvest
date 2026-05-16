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
        if (inImage == null)
            return;

        // if sprite is null, set alpha to 0 to hide the image (instead of having a missing sprite icon show up)
        var color = inImage.color;
        var imgAlpha =  color.a == 0 ? 1 : color.a;
        color.a = inSprite == null ? 0 : imgAlpha;
        inImage.color = color;
            
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