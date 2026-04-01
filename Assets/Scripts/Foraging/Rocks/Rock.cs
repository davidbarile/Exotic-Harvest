using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Rock : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private Image rockImage;
    [SerializeField] private Image shadow;
    [SerializeField] private Image fillImage;

    [SerializeField] private TMP_Text label;

    [SerializeField] protected RectTransform targetRectTransform;

    private Vector3 OffsetFromCursor;

    public LootConfig LootConfig { get; private set; }

    protected Transform originalParent;
    protected int originalSiblingIndex;
    protected bool isDragging = false;

    protected List<Loot> spawnedLoots = new();

    private bool hasBeenHarvested = false;

    public void Configure(LootConfig inLootConfig)
    {
        this.LootConfig = inLootConfig;

        // if (this.rockImage != null)
        // {
        //     var sprite = SpriteManager.GetSprite(derp);
        //     this.rockImage.sprite = sprite;
        //     this.fillImage.sprite = sprite;
        // }
    }

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

        TrySpawnLoot();

        var dragPos = DragManager.GetPositionValuesForDrag(eventData.position, this.targetRectTransform, out this.OffsetFromCursor);
        this.targetRectTransform.position = dragPos;// + this.OffsetFromCursor; //TODO: fix offset from cursor

        this.targetRectTransform.SetParent(DragManager.IN.DragCanvas, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!this.isDragging)
            return;

        var dragPos = DragManager.IN.GetPositionInSpace(eventData.position);
        this.targetRectTransform.position = dragPos;// + this.OffsetFromCursor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.isDragging = false;

        this.targetRectTransform.SetParent(this.originalParent, true);
        this.targetRectTransform.SetAsLastSibling();
        this.originalSiblingIndex = this.targetRectTransform.GetSiblingIndex();
        this.targetRectTransform.position += DragManager.ScreenToWorldCameraDelta;

        SetShadowActive(false);
    }

    public void TrySpawnLoot()
    {
        if (this.hasBeenHarvested)
            return;

        this.hasBeenHarvested = true;

        var lootDatas = this.LootConfig.GetRandomLoot(false, 10, 3);

        if (lootDatas.Count == 0)
        {
            //Debug.Log($"<color=red>Rock.TrySpawnLoot()  No loot was returned from LootConfig.GetRandomLoot() for {this.LootConfig.DisplayName}</color>");
            return;
        }

        this.spawnedLoots.Clear();

        for(int i = 0; i < lootDatas.Count; i++)
        {
            var lootData = lootDatas[i];
            var loot = PrefabManager.IN.SpawnPrefab<Loot>("Loot", this.originalParent);
            loot.transform.position = this.targetRectTransform.position;
            loot.transform.SetSiblingIndex(this.originalSiblingIndex + i);
            loot.transform.localScale = this.transform.localScale * .7f;

            if(i > 0)
            {
                // offset each loot spawn so they don't overlap exactly on top of each other
                var distanceFromCenter = 25f * loot.transform.localScale.x;
                var sign = (i % 2 == 0) ? 1 : -1; // alternate left and right
                var sign2 = (i % 4 < 2) ? 1 : -1; // alternate up and down every two loots
                loot.transform.position += new Vector3(sign * Random.Range(.5f * distanceFromCenter, distanceFromCenter), sign2 * Random.Range(.5f * distanceFromCenter, distanceFromCenter), 0f);
            }

            //Debug.Log($"<color=#00FF00>Rock.TrySpawnLoot() Spawned loot {lootData.DisplayName}  for {this.LootConfig.DisplayName}</color>");

            loot.Configure(lootData);
            this.spawnedLoots.Add(loot);
        }
    }
}