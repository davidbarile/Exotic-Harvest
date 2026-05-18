using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using static GlobalEnums;

[RequireComponent(typeof(CanvasGroup))]
public class Rock : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private Image rockImage;
    [SerializeField] private Image shadow;
    [SerializeField] private Image fillImage;

    [SerializeField] private TMP_Text label;

    [SerializeField] protected RectTransform targetRectTransform;

    protected CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Vector3 offsetFromCursor;

    private Transform originalParent;
    private int originalSiblingIndex;
    private bool isDragging = false;

    private List<Loot> spawnedLoots = new();

    private bool hasBeenHarvested = false;

    private Tween fadeTween;

    private void Awake()
    {
        if (this.targetRectTransform == null)
            this.targetRectTransform = GetComponent<RectTransform>();

        this.canvasGroup = GetComponent<CanvasGroup>();

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

    public void SetPosition(Vector3 position)
    {
        this.originalPosition = position;
        this.targetRectTransform.localPosition = position;
        this.targetRectTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-30f, 30f));
            
        this.gameObject.SetActive(true);
    }

    public void Reset()
    {
        this.hasBeenHarvested = false;

        for (int i = 0; i < this.spawnedLoots.Count; i++)
        {
            var loot = this.spawnedLoots[i];
            if (loot != null)
                Destroy(loot.gameObject);
        }

        this.spawnedLoots.Clear();

        if(this.originalParent != null)
        {
            this.targetRectTransform.SetParent(this.originalParent, true);
            this.targetRectTransform.SetSiblingIndex(this.originalSiblingIndex);
        }

        // this.targetRectTransform.SetAsLastSibling();
        // this.originalSiblingIndex = this.targetRectTransform.GetSiblingIndex();
        this.targetRectTransform.localPosition = this.originalPosition;
        
        this.fadeTween?.Kill();

        if (!this.gameObject.activeSelf)
            this.fadeTween = this.canvasGroup.DOFade(1f, 0.3f).OnComplete(() => this.canvasGroup.interactable = true);
        else
        {
            this.canvasGroup.interactable = true;
            this.canvasGroup.alpha = 1f;
        }
         
        this.gameObject.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        this.isDragging = true;

        SetShadowActive(true);

        this.originalParent = this.targetRectTransform.parent;
        this.originalSiblingIndex = this.targetRectTransform.GetSiblingIndex();

        TrySpawnLoot();

        var dragPos = DragManager.GetPositionValuesForDrag(eventData.position, this.targetRectTransform, out this.offsetFromCursor);
        this.targetRectTransform.position = dragPos;// + this.OffsetFromCursor; //TODO: fix offset from cursor

        this.targetRectTransform.SetParent(UiManager.IN.DragCanvas, true);

        this.transform.localScale *= DragManager.UiCanvasScaleFactor;
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

        this.transform.localScale /= DragManager.UiCanvasScaleFactor;

        SetShadowActive(false);

        //fade out and hide rock after dragging
        this.canvasGroup.interactable = false;

        this.fadeTween?.Kill();
        this.fadeTween = this.canvasGroup.DOFade(0f, 0.75f).SetDelay(1f).OnComplete(() => this.gameObject.SetActive(false));
    }

    public void TrySpawnLoot()
    {
        if (this.hasBeenHarvested)
            return;

        if (!TimeManager.CurrentTimeOfDay.HasFlag(ETimeOfDay.Afternoon) && !TimeManager.CurrentTimeOfDay.HasFlag(ETimeOfDay.Evening))
        {
            Debug.Log($"<color=yellow>Rock.TrySpawnLoot() It's not afternoon, skipping loot spawn {name}</color>");
            return;
        }

        this.hasBeenHarvested = true;

        var lootConfig = LootManager.IN.GetRandomLootConfigOfType(ELootType.RockPile, TimeManager.CurrentTimeOfDay);

        var lootDatas = lootConfig.GetRandomLoot(false, 10, 3);

        if (lootDatas.Count == 0)
        {
            //Debug.Log($"<color=red>Rock.TrySpawnLoot()  No loot was returned from LootConfig.GetRandomLoot() for {this.LootConfig.DisplayName}</color>");
            return;
        }

        this.spawnedLoots.Clear();

        for (int i = 0; i < lootDatas.Count; i++)
        {
            var lootData = lootDatas[i];
            var loot = PrefabManager.IN.SpawnPrefab<Loot>("Loot", this.originalParent);
            loot.transform.position = this.targetRectTransform.position;
            loot.transform.SetSiblingIndex(this.originalSiblingIndex + i);
            loot.transform.localScale = this.transform.localScale * .7f;

            if (i > 0)
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