using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Loot : Collectable
{
    [SerializeField] private Image lootImage;
    [SerializeField] private Image shadow;
    [SerializeField] private Image fillImage;

    [SerializeField] private TMP_Text label;

    [SerializeField] private RectTransform targetRectTransform;

    //public InventoryItemData ItemData { get; private set; }

    protected Transform originalParent;
    protected int originalSiblingIndex;

    private void Awake()
    {
        if (this.targetRectTransform == null)
            this.targetRectTransform = GetComponent<RectTransform>();

        SetShadowActive(false);
    }
    
     public void SetSprite(Sprite sprite)
    {
        if (this.lootImage)
        {
            this.lootImage.sprite = sprite;
            
            if (this.fillImage)
                this.fillImage.sprite = sprite;
        }
    }

    public void SetColor(Color color)
    {
        this.lootImage.color = color;
    }

    public void SetText(string text)
    {
        if (this.label)
            this.label.text = text;
    }

    public void SetShadowActive(bool isActive)
    {
        if (this.shadow != null)
            this.shadow.gameObject.SetActive(isActive);
    }
}