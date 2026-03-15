using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class Rock : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private Image rockImage;
    [SerializeField] private Image shadow;
    [SerializeField] private Image fillImage;

    [SerializeField] private TMP_Text label;

    [SerializeField] protected RectTransform targetRectTransform;

    public InventoryItemData ItemData { get; private set; }

    protected Vector2 originalLocalPointerPosition;
    protected Vector3 originalLocalPosition;
    protected Vector3 originalWorldPosition;

    protected Transform originalParent;
    protected int originalSiblingIndex;
    protected bool isDragging = false;

    protected Loot spawnedLoot;

    // public void Configure(InventoryItemData inItemData)
    // {
    //     this.ItemData = inItemData;

    //     if (this.rockImage != null)
    //     {
    //         var sprite = SpriteManager.GetSprite(inItemData.IconSpriteName);
    //         this.rockImage.sprite = sprite;
    //         this.fillImage.sprite = sprite;
    //     }
    // }

    private void Awake()
    {
        if (this.targetRectTransform == null)
            this.targetRectTransform = GetComponent<RectTransform>();

        SetShadowActive(false);
    }

    public void SetSprite(Sprite sprite)
    {
        if (this.rockImage)
        {
            this.rockImage.sprite = sprite;

            if (this.fillImage)
                this.fillImage.sprite = sprite;
        }
    }

    public void SetColor(Color color)
    {
        this.rockImage.color = color;
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        this.isDragging = true;

        SetShadowActive(true);

        this.originalParent = this.targetRectTransform.parent;
        this.originalSiblingIndex = this.targetRectTransform.GetSiblingIndex();

        SpawnLoot();

        this.targetRectTransform.SetParent(DragManager.IN.DragCanvas, true);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            DragManager.IN.DragCanvas,
            eventData.position,
            eventData.pressEventCamera,
            out this.originalLocalPointerPosition);

        this.originalLocalPosition = this.targetRectTransform.localPosition;
        this.originalWorldPosition = this.targetRectTransform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!this.isDragging)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            DragManager.IN.DragCanvas,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPointerPosition))
        {
            Vector3 offsetToOriginal = localPointerPosition - this.originalLocalPointerPosition;
            this.targetRectTransform.localPosition = originalLocalPosition + new Vector3(offsetToOriginal.x, offsetToOriginal.y, 0f);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.isDragging = false;

        this.targetRectTransform.SetParent(this.originalParent, true);
        this.targetRectTransform.SetAsLastSibling();
        this.originalSiblingIndex = this.targetRectTransform.GetSiblingIndex();
        SetShadowActive(false);
    }

    public void SpawnLoot()
    {
        this.spawnedLoot = PrefabManager.IN.SpawnPrefab<Loot>("Loot", this.originalParent);
        this.spawnedLoot.transform.position = this.targetRectTransform.position;
        this.spawnedLoot.transform.SetSiblingIndex(this.originalSiblingIndex);

        this.spawnedLoot.SetSprite(this.rockImage.sprite);
        this.spawnedLoot.SetColor(this.rockImage.color);
        this.spawnedLoot.SetText(this.label.text);
    }
}