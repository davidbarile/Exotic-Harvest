using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

//base for magnifying glass and telescope, mainly for shared functionality like search mode and lens effects. Also serves as a common type for the UI to reference without needing to know the specific decoration type
public class SearchToolBase : DecorationBase
{
    public bool IsInSearchMode { get; private set; }

    [Space, SerializeField] protected RectTransform innerWorld;
    [Space, SerializeField] protected Transform rayCastOrigin;
    [Space, SerializeField] protected Mask lensMask;
    [SerializeField] protected CanvasGroup lensCanvasGroup;

    [Space,SerializeField] protected GameObject fillBarDisplay;
    [SerializeField] protected Image fillBarImage;

    [Space, Range(0f, 10f),SerializeField] protected float scrollSpeed = 1f;
    [Range(0f, 2f), SerializeField] protected float lensTweenDuration = .3f;

    [Space, Range(0f, 5f), SerializeField] protected float timeToActivateHover = 1f;//how long the player needs to hover over a searchable object before it "activates"

    [SerializeField] protected bool isMaskEnabled = true; // For debug, allows toggling the mask on/off in the inspector

    protected Tween lensTween;
    protected bool isOverSearchableArea;

    protected int searchAreaLayerMask = 0;
    protected int searchObjectLayerMask = 0;

    protected Searchable activeSearchable = null;

    private DateTime startActiveObjectHoverTime;

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
        this.linkedPassiveHarvester = GetComponent<MagnifyingGlass>();

        this.searchObjectLayerMask = LayerMask.GetMask("Default");
        //set this in override
        //this.searchAreaLayerMask = LayerMask.GetMask("MeadowSearchArea");
    }

    protected override void Start()
    {
        base.Start();
        SetFillBarActive(false);
        SetSearchMode(false, true);
    }

    public void SetSearchMode(bool inIsSearchMode, bool inShouldForce = false)
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
        }
        else
        {
            this.lensCanvasGroup.gameObject.SetActive(true);
            this.lensTween = this.lensCanvasGroup.DOFade(1f, this.lensTweenDuration).SetEase(Ease.InOutSine).OnComplete(() => this.lensMask.gameObject.SetActive(false));
        }
    }

    public void SetFillAmount(float fillAmount)
    {
        if (this.fillBarDisplay)
        {
            SetFillBarActive(true);
            if (this.fillBarImage)
            {
                this.fillBarImage.fillAmount = fillAmount;
            }
        }
    }

    public void SetFillBarActive(bool isActive)
    {
        if (this.fillBarDisplay)
        {
            this.fillBarDisplay.SetActive(isActive);
        }
    }

    public void ScrollInnerWorld()
    {
        if (!this.IsInSearchMode && this.isMaskEnabled)
            return;

        this.innerWorld.localPosition = this.transform.localPosition * -1 * this.scrollSpeed;

        var wasSearchObjectNull = this.activeSearchable == null;

        this.activeSearchable = GetActiveSearchObject();
        if (this.activeSearchable != null)
        {
            if(wasSearchObjectNull)
            {
                this.startActiveObjectHoverTime = DateTime.Now;
                SetFillBarActive(true);
                this.linkedPassiveHarvester.SetText($"Found: {this.activeSearchable.name}!");
            }
            else
            {
                var hoverDuration = (DateTime.Now - this.startActiveObjectHoverTime).TotalSeconds;
                var percent = Mathf.Clamp01((float)(hoverDuration / this.timeToActivateHover));
                SetFillAmount(percent);

                if (hoverDuration >= this.timeToActivateHover)
                {
                    this.activeSearchable.Collect();
                    this.activeSearchable = null;
                    SetFillBarActive(false);
                    this.linkedPassiveHarvester.SetText(string.Empty);
                }
            }
        }
        else
        {
            SetFillBarActive(false);
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

    protected override bool DoOnBeginDrag()
    {
        //override in derived classes
        //SetLootFieldParent(ForagingManager.IN.MeadowLootField);

        SetSearchMode(true);
        return true;
    }

    protected void SetLootFieldParent(Transform inLootField)
    {
        if (inLootField == this.innerWorld)
            return;
            
        this.lootField = inLootField;
        this.originalLootFieldParent = inLootField.parent;

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
            
        this.lootField.SetParent(this.originalLootFieldParent);
        this.lootField.localPosition = Vector3.zero;
        this.lootField.localScale = Vector3.one;
        this.lootField.localRotation = Quaternion.identity;
        this.lootField = null;
    }

    public override void OnDragUpdate()
    {
        this.isOverSearchableArea = IsOverSearchableArea();

        SetSearchMode(this.isOverSearchableArea);

        if(this.isOverSearchableArea || !this.isMaskEnabled)
        {
            ScrollInnerWorld();
        }
    }

    protected virtual bool IsOverSearchableArea()
    {
        Collider2D hitCollider = Physics2D.OverlapPoint(this.worldProxy.transform.position, this.searchAreaLayerMask);

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