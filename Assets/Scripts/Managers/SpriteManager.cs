using UnityEngine;
using UnityEngine.UI;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager IN;

    [SerializeField] private Sprite[] itemSprites;

    private void Awake()
    {
        IN = this;
    }

    public Sprite GetItemSpriteByIndex(int index)
    {
        if (index >= 0 && index < this.itemSprites.Length)
        {
            return this.itemSprites[index];
        }
        return null;
    }

    public static Sprite GetSprite(string spriteName)
    {
        var sprite = Resources.Load<Sprite>($"Sprites/{spriteName}");
        return sprite;
    }

     public static void SetImageSprite(Image inImage, Sprite inSprite, bool shouldFitInRect = true)
    {
        inImage.sprite = inSprite;

        if(!shouldFitInRect) return;

        var rectSize = inImage.rectTransform.sizeDelta;
        var spriteDims = inImage.sprite.bounds.size;
        var spriteDimsRatio = spriteDims.x / spriteDims.y;
        var rectSizeRatio = rectSize.x / rectSize.y;
        var isWider = spriteDimsRatio > rectSizeRatio;

        float minWidth;
        float minHeight;
        if (isWider)
        {
            minWidth = inImage.rectTransform.sizeDelta.x;
            minHeight = minWidth / spriteDimsRatio;
        }
        else
        {
            minHeight = inImage.rectTransform.sizeDelta.y;
            minWidth = minHeight * spriteDimsRatio;
        }

        inImage.rectTransform.sizeDelta = new Vector2(
            minWidth,
            minHeight
        );
    }
}