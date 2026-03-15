using UnityEngine;

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
}