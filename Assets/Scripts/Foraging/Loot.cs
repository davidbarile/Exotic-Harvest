using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Loot : Collectable
{
    [SerializeField] private Image shadow;
    [SerializeField] private Image fillImage;

    [SerializeField] private TMP_Text label;

    [SerializeField] private RectTransform targetRectTransform;

    public LootData LootData { get; private set; }

    protected Transform originalParent;
    protected int originalSiblingIndex;

    protected override void Awake()
    {        
        base.Awake();
        SetShadowActive(false);
    }

    public virtual void Configure(LootData inLootData, Action inOnCollected = null, int maxLootQuantity = 0)
    {
        this.LootData = inLootData;
        this.resourceType = inLootData.ResourceType;
        this.onCollected = inOnCollected;

        if (string.IsNullOrWhiteSpace(inLootData.OverrideSpriteName))
        {
            var resource = ResourceManager.IN.Database.GetResource(inLootData.ResourceType);
            SetSprite(resource.Icon);
            return;
        }
        else
        {
            var sprite = SpriteManager.GetSprite(inLootData.OverrideSpriteName);
            SetSprite(sprite);
        }

        this.amount = inLootData.Quantity;
        if(maxLootQuantity > 0)
            this.amount = Mathf.Min(this.amount, maxLootQuantity);

        if (this.amount> 1)
            SetText(this.amount.ToString());
    }
    
    public void SetSprite(Sprite sprite)
    {
        if (this.collectableImage)
        {
            this.collectableImage.sprite = sprite;
            
            if (this.fillImage)
                this.fillImage.sprite = sprite;
        }
    }

    public void SetColor(Color color)
    {
        this.collectableImage.color = color;
    }

    public void SetText(string text)
    {
        if (this.label)
        {
            this.label.text = text;
            this.label.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
        }
    }

    public void SetShadowActive(bool isActive)
    {
        if (this.shadow != null)
            this.shadow.gameObject.SetActive(isActive);
    }

    protected override void OnCollected()
    {
        var initScale = this.transform.localScale.x;
        // Collection effect
        var sequence = DOTween.Sequence()
            .Append(this.transform.DOScale(1.2f * initScale, 0.1f))
            .Join(this.canvasGroup.DOFade(0f, 0.3f))
            .Append(this.transform.DOScale(0f, 0.1f))
            .OnComplete(() => base.OnCollected());

        var isWorld = this.transform.IsChildOf(UiManager.IN.WorldCanvas.transform);
        var parentContainer = isWorld ? UiManager.IN.WorldParticlesContainer : UiManager.IN.ScreenParticlesContainer;

        var particle = Pool.Spawn<ParticleHelper>("Loot_Particle", parentContainer, this.transform.position, Quaternion.identity);
        particle.transform.localScale = Vector3.one;
        particle.Play();
    }
}