using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

//base for magnifying glass and telescope, mainly for shared functionality like search mode and lens effects. Also serves as a common type for the UI to reference without needing to know the specific decoration type
public class SearchToolBase : DecorationBase
{
    public bool IsInSearchMode { get; private set; }

    [Space, SerializeField] protected RectTransform innerWorld;
    [SerializeField] protected Transform container;
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

    protected GameObject activeSearchObject = null;

    private DateTime startActiveObjectHoverTime;

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

        this.searchObjectLayerMask = LayerMask.GetMask("UI");
        //set this in override
        //this.searchAreaLayerMask = LayerMask.GetMask("Searchable");
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

        var wasSearchObjectNull = this.activeSearchObject == null;

        this.activeSearchObject = GetActiveSearchObject();
        if (this.activeSearchObject != null)
        {
            if(wasSearchObjectNull)
            {
                this.startActiveObjectHoverTime = DateTime.Now;
                SetFillBarActive(true);
                this.linkedPassiveHarvester.SetText($"Found: {this.activeSearchObject.name}!");
            }
            else
            {
                var hoverDuration = (DateTime.Now - this.startActiveObjectHoverTime).TotalSeconds;
                var percent = Mathf.Clamp01((float)(hoverDuration / this.timeToActivateHover));
                SetFillAmount(percent);

                if (hoverDuration >= this.timeToActivateHover)
                {
                    Destroy(this.activeSearchObject);
                    this.activeSearchObject = null;
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

    private GameObject GetActiveSearchObject()
    {
        Collider2D hitCollider = Physics2D.OverlapPoint(this.rayCastOrigin.position, this.searchObjectLayerMask);

        if (hitCollider != null && hitCollider.gameObject != this.gameObject && hitCollider.CompareTag("Searchable"))
        {
            return hitCollider.gameObject;
        }

        return null;
    }
    
    protected override bool DoOnBeginDrag()
    {
        SetSearchMode(true);
        return true;
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
    }
}