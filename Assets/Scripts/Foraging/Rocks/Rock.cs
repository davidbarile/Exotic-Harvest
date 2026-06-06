using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using Lean.Pool;
using static GlobalEnums;

[RequireComponent(typeof(CanvasGroup))]
public class Rock : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public RockPile ParentRockPile { get; set; }

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

    private Vector3 originalScale;

    private Tween fadeTween;
    private Tween fallTween;
    private Tween scaleTween;
    private Tween shakeTween;

    private void Awake()
    {
        if (this.targetRectTransform == null)
            this.targetRectTransform = GetComponent<RectTransform>();

        this.canvasGroup = GetComponent<CanvasGroup>();

        SetShadowActive(false);
    }

    public void SetSprite(Sprite inSprite)
    {
        if (this.rockImage)
        {
            this.rockImage.sprite = inSprite;

            if (this.fillImage)
                this.fillImage.sprite = inSprite;
        }
    }

    public void SetColor(Color inColor)
    {
        this.rockImage.color = inColor;
    }

    public void SetScale(float inScale)
    {
        this.originalScale = Vector3.one * inScale;
        this.targetRectTransform.localScale = this.originalScale;
    }

    public void SetText(string inText)
    {
        if (this.label)
            this.label.text = inText;
    }

    public void SetShadowActive(bool inIsActive)
    {
        if (this.shadow)
            this.shadow.gameObject.SetActive(inIsActive);
    }

    public void SetPosition(Vector3 inPosition)
    {
        this.originalPosition = inPosition;
        this.targetRectTransform.localPosition = inPosition;
        this.targetRectTransform.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-30f, 30f));
            
        this.gameObject.SetActive(true);
    }

    public void Reset()
    {
        this.hasBeenHarvested = false;

        for (int i = 0; i < this.spawnedLoots.Count; i++)
        {
            var loot = this.spawnedLoots[i];
            if (loot != null)
                LeanPool.Despawn(loot.gameObject);
        }

        this.spawnedLoots.Clear();

        if (this.originalParent != null)
        {
            this.targetRectTransform.SetParent(this.originalParent, true);
            this.targetRectTransform.SetSiblingIndex(this.originalSiblingIndex);
        }

        // this.targetRectTransform.SetAsLastSibling();
        // this.originalSiblingIndex = this.targetRectTransform.GetSiblingIndex();
        this.targetRectTransform.localPosition = this.originalPosition;

        this.fadeTween?.Kill();
        this.scaleTween?.Kill();

        if (!this.gameObject.activeSelf)
        {
            this.fadeTween = this.canvasGroup.DOFade(1f, 0.3f).OnComplete(() => this.canvasGroup.interactable = true);
            this.transform.localScale = this.originalScale * 0.1f;
            this.scaleTween = this.targetRectTransform.DOScale(this.originalScale.x, 0.4f).From(0.3f).SetEase(Ease.OutElastic);
        }
        else
        {
            this.canvasGroup.interactable = true;
            this.canvasGroup.alpha = 1f;
        }

        this.gameObject.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        this.shakeTween?.Kill();

        var canSpawn = TrySpawnLoot();//not sure if I want to stop drag here.  Maybe something more subtle
        if (!canSpawn)
        {
            //make sound, shake rock, tint rock red briefly
            this.shakeTween = this.targetRectTransform.DOShakePosition(0.5f, 10f, 40).SetEase(Ease.Linear);

            //TODO maybe move to OnEndDrag
            var rejectMessage = ForagingManager.GetHarvestRejectMessage(EHarvestLocation.Beach, out var rejectTitle);

            if (!string.IsNullOrEmpty(rejectMessage))
            {
                Vector3 offsetPosition = RectTransformUtility.WorldToScreenPoint(UiManager.IN.WorldCamera, this.targetRectTransform.position);
                offsetPosition += new Vector3(25f, 50f, 0f);
                TooltipManager.IN.ShowHarvestRejectMessage(rejectMessage, rejectTitle, offsetPosition);
            }

            return;
        }
        
        this.isDragging = true;

        SetShadowActive(true);

        this.originalParent = this.targetRectTransform.parent;
        this.originalSiblingIndex = this.targetRectTransform.GetSiblingIndex();

        this.targetRectTransform.SetParent(UiManager.IN.DragCanvas, true);
        this.targetRectTransform.localScale *= DragManager.UiCanvasScaleFactor;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!this.isDragging)
            return;

        this.targetRectTransform.position = (Vector3)eventData.position - this.offsetFromCursor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!this.isDragging)
            return;
            
        this.isDragging = false;

        this.targetRectTransform.SetParent(this.originalParent, true);
        this.targetRectTransform.SetAsLastSibling();
        this.originalSiblingIndex = this.targetRectTransform.GetSiblingIndex();

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(this.targetRectTransform, eventData.position, UiManager.IN.WorldCanvas.worldCamera, out Vector3 outWorldPos))
        {
            this.targetRectTransform.position = outWorldPos;
        }

        this.targetRectTransform.localScale /= DragManager.UiCanvasScaleFactor;

        SetShadowActive(false);

        //fade out and hide rock after dragging
        this.canvasGroup.interactable = false;

        this.fadeTween?.Kill();
        this.fadeTween = this.canvasGroup.DOFade(0f, 0.3f).SetDelay(.4f).OnComplete(() => this.gameObject.SetActive(false));
        this.fallTween?.Kill();
        this.fallTween = this.targetRectTransform.DOLocalMoveY(this.targetRectTransform.localPosition.y - 100f, 0.6f).SetDelay(.1f).SetEase(Ease.InQuad);
    }

    public bool TrySpawnLoot()
    {
        if (this.hasBeenHarvested)
            return false;

        if(!this.ParentRockPile.CanSpawnLoot())
            return false;

        this.hasBeenHarvested = true;

        var lootConfig = LootManager.IN.GetRandomLootConfigOfType(ELootType.RockPile, TimeManager.CurrentTimeOfDay);

        var lootDatas = lootConfig.GetRandomLoot(false, 10, 3);

        if (lootDatas.Count == 0)
        {
            //Debug.Log($"<color=red>Rock.TrySpawnLoot()  No loot was returned from LootConfig.GetRandomLoot() for {this.LootConfig.DisplayName}</color>");
            return true;
        }

        this.spawnedLoots.Clear();

        for (int i = 0; i < lootDatas.Count; i++)
        {
            var lootData = lootDatas[i];
            var loot = Pool.Spawn<Loot>("Loot", this.originalParent);
            loot.transform.position = this.targetRectTransform.position;
            loot.transform.SetSiblingIndex(this.originalSiblingIndex + i);
            loot.transform.localScale = this.transform.localScale * .7f;

            if (i > 0)
            {
                // offset each loot spawn so they don't overlap exactly on top of each other
                var distanceFromCenter = 25f * loot.transform.localScale.x;
                var sign = (i % 2 == 0) ? 1 : -1; // alternate left and right
                var sign2 = (i % 4 < 2) ? 1 : -1; // alternate up and down every two loots
                loot.transform.position += new Vector3(sign * UnityEngine.Random.Range(.5f * distanceFromCenter, distanceFromCenter), sign2 * UnityEngine.Random.Range(.5f * distanceFromCenter, distanceFromCenter), 0f);
            }

            //Debug.Log($"<color=#00FF00>Rock.TrySpawnLoot() Spawned loot {lootData.DisplayName}  for {this.LootConfig.DisplayName}</color>");

            loot.Configure(lootData);
            this.spawnedLoots.Add(loot);
        }
        return true;
    }
}