using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static GlobalEnums;

//base for magnifying glass and telescope, mainly for shared functionality like search mode and lens effects.
//Also serves as a common type for the UI to reference without needing to know the specific decoration type
public class SearchToolBase : DecorationBase //Draggable
{
    public bool IsInSearchMode { get; protected set; }

    [Space, SerializeField] protected RectTransform innerWorld;
    [Space, SerializeField] protected Transform rayCastOrigin;
    [Space, SerializeField] protected Mask lensMask;
    [SerializeField] protected CanvasGroup lensCanvasGroup;

    [Space,SerializeField] protected GameObject fillBarDisplay;
    [SerializeField] protected Image fillBarImage;

    [Space, Range(0f, 10f), SerializeField] protected float scrollSpeed = 1f;
    [SerializeField] protected Vector2 scrollOffset;
    [Range(0f, 2f), SerializeField] protected float lensTweenDuration = .3f;

    [Space, Range(0f, 5f), SerializeField] protected float timeToActivateHover = 1f;//how long the player needs to hover over a searchable object before it "activates"

    [SerializeField] protected bool shouldTrackWithWorldCamera;

    [SerializeField] protected bool isMaskEnabled = true; // For debug, allows toggling the mask on/off in the inspector

    protected Tween lensTween;
    protected bool isOverSearchableArea;

    protected int searchAreaLayerMask = 0;
    protected int searchObjectLayerMask = 0;

    protected Searchable activeSearchable = null;

    private DateTime startActiveObjectHoverTime = DateTime.MinValue;

    private Transform lootField;
    private Transform originalLootFieldParent;

    protected override void OnValidate()
    {
        base.OnValidate();
        //for debug
        this.lensMask.enabled = this.isMaskEnabled;
        if (!this.isMaskEnabled)
        {
            this.lensMask.gameObject.SetActive(true);
            this.lensCanvasGroup.gameObject.SetActive(false);
        }
    }
    
    protected override void Awake()
    {
        base.Awake();//call before, because the next code overrwrites the base

        this.linkedPassiveHarvester = GetComponent<PassiveHarvester>();
        this.searchObjectLayerMask = LayerMask.GetMask("Default");
        //set this in override
        //this.searchAreaLayerMask = LayerMask.GetMask("MeadowSearchArea");
    }

    protected override void Start()
    {
        base.Start();
        SetFillAmount(0f);
        SetSearchMode(false, true);
    }

    public virtual void SetSearchMode(bool inIsSearchMode, bool inShouldForce = false)
    {
        if (!this.isMaskEnabled)
            return;

        if (this.IsInSearchMode == inIsSearchMode && !inShouldForce)
            return;
        
        this.IsInSearchMode = inIsSearchMode;

        this.lensTween?.Kill();
        if (inIsSearchMode)
        {
            this.lensMask.gameObject.SetActive(true);

            this.lensCanvasGroup.gameObject.SetActive(true);
            this.lensTween = this.lensCanvasGroup.DOFade(0f, this.lensTweenDuration).SetEase(Ease.InOutSine);

            if(this.harvestRejectMessage)
            {
                var rejectMessage = ForagingManager.GetHarvestRejectMessage(this.harvestLocation, out var rejectTitle);

                if(string.IsNullOrEmpty(rejectMessage))
                    this.harvestRejectMessage.Hide();
                else
                    this.harvestRejectMessage.Show(rejectMessage, rejectTitle);
            }
        }
        else
        {
            this.lensCanvasGroup.gameObject.SetActive(true);
            this.lensTween = this.lensCanvasGroup.DOFade(1f, this.lensTweenDuration).SetEase(Ease.InOutSine).OnComplete(() => this.lensMask.gameObject.SetActive(false));

            if(this.harvestRejectMessage)
                this.harvestRejectMessage.Hide();
        }
    }

    public void SetFillAmount(float fillAmount)
    {
        if (this.fillBarDisplay)
        {
            this.fillBarDisplay.SetActive(fillAmount > 0f);

            if (this.fillBarImage)
                this.fillBarImage.fillAmount = fillAmount;
        }
    }

    public void ScrollInnerWorld()
    {
        if (!this.IsInSearchMode && this.isMaskEnabled)
            return;

        var worldCameraOffset = this.shouldTrackWithWorldCamera ? new Vector3(UiManager.IN.WorldCamera.transform.position.x, 0f, 0f) : Vector3.zero;

        this.innerWorld.localPosition = (this.transform.localPosition + worldCameraOffset) * -1 * this.scrollSpeed + (Vector3)this.scrollOffset;

        var wasSearchObjectNull = this.activeSearchable == null;
        this.activeSearchable = GetActiveSearchObject();

        if (this.activeSearchable != null)
        {
            if(wasSearchObjectNull)
            {
                if (this.startActiveObjectHoverTime > DateTime.Now)
                    return;

                this.startActiveObjectHoverTime = DateTime.Now;
                SetFillAmount(0f);
                this.linkedPassiveHarvester.SetText($"Found: {this.activeSearchable.SearchableName}!");
            }
            else
            {
                var hoverDuration = (DateTime.Now - this.startActiveObjectHoverTime).TotalSeconds;
                var percent = Mathf.Clamp01((float)(hoverDuration / this.timeToActivateHover));
                SetFillAmount(percent);
                if (hoverDuration >= this.timeToActivateHover && this.activeSearchable != null)
                {
                    this.activeSearchable.Collect();
                    this.activeSearchable = null;
                    SetFillAmount(0f);
                    this.startActiveObjectHoverTime = DateTime.Now + TimeSpan.FromSeconds(1); //reset hover time to prevent immediate re-activation
                    this.linkedPassiveHarvester.SetText(string.Empty);
                }
            }
        }
        else
        {
            SetFillAmount(0f);
            this.linkedPassiveHarvester.SetText(string.Empty);
        }
    }

    private Searchable GetActiveSearchObject()
    {
        Collider2D hitCollider = Physics2D.OverlapPoint(this.rayCastOrigin.position, this.searchObjectLayerMask);

        if (hitCollider != null && hitCollider.TryGetComponent<Searchable>(out var searchable))
        {
            return searchable;
        }

        return null;
    }

    public override void ConfigureFromDrag(InventoryItemData inItemData, Transform inOriginalParent = null, int inOriginalSiblingIndex = -1)
    {
        base.ConfigureFromDrag(inItemData, inOriginalParent, inOriginalSiblingIndex);
        DoOnBeginDrag();//calls SetLootFieldParent() in MagnifyingGlass and Telescope overrides
    }

    protected override bool DoOnBeginDrag()
    {
        //override in derived classes
        //SetLootFieldParent(ForagingManager.IN.MeadowLootField);

        //SetSearchMode(true);
        return true;
    }

    protected void SetLootFieldParent(Transform inLootField)
    {
        if (!ForagingManager.IsInitialized)
            return;

        //Debug.Log($"Setting loot field parent to {inLootField.name} for {this.name}. this.innerWorld = {this.innerWorld.name}");
        if (inLootField == this.innerWorld)
            return;
            
        this.lootField = inLootField;
        this.originalLootFieldParent = inLootField.parent;

        // Debug.Log($"SUCCESS. Setting loot field parent to {inLootField.name} for {this.name}. this.innerWorld = {this.innerWorld.name}. this.originalLootFieldParent = {this.originalLootFieldParent.name}");

        inLootField.SetParent(this.innerWorld);
        inLootField.localPosition = Vector3.zero;
        inLootField.localScale = Vector3.one;
        inLootField.localRotation = Quaternion.identity;
    }
    
    private void ReturnLootFieldToOriginalParent()
    {
        if (this.IsInSearchMode)
        {
            CancelInvoke();
            return;
        }

        // Debug.Log($"this.lootField = {this.lootField}");
        // Debug.Log($"this.originalLootFieldParent = {this.originalLootFieldParent}");
            
        this.lootField.SetParent(this.originalLootFieldParent);
        this.lootField.localPosition = Vector3.zero;
        this.lootField.localScale = Vector3.one;
        this.lootField.localRotation = Quaternion.identity;
        this.lootField = null;
    }

    public override void OnDragUpdate()
    {
        this.isOverSearchableArea = IsOverSearchableArea();

        if (UiManager.IN.InventoryPanel.IsShowing)
        {
            foreach (var obj in InputManager.ObjectsUnderMouse)
            {
                if(obj.transform.IsChildOf(UiManager.IN.InventoryPanel.transform))
                {
                    this.isOverSearchableArea = false;
                    break;
                }
            }
        }

        SetSearchMode(this.isOverSearchableArea);

        if(this.isOverSearchableArea || !this.isMaskEnabled)
        {
            ScrollInnerWorld();
        }
    }

    protected virtual bool IsOverSearchableArea()
    {
        var hitCollider = Physics2D.OverlapPoint(this.worldProxy.transform.position, this.searchAreaLayerMask);

        if (hitCollider != null)
            return true;

        return false;
    }
    
    protected override void DoOnEndDrag()
    {
        SetSearchMode(false, true);
        CancelInvoke();
        Invoke(nameof(ReturnLootFieldToOriginalParent), 2f);
    }
}