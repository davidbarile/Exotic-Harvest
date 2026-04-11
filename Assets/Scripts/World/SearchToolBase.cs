using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//base for magnifying glass and telescope, mainly for shared functionality like search mode and lens effects. Also serves as a common type for the UI to reference without needing to know the specific decoration type
public class SearchToolBase : DecorationBase
{
    public bool IsInSearchMode { get; private set; }
    [SerializeField] protected RectTransform innerWorld;
    [SerializeField] protected Transform container;
    [Space, SerializeField] protected Mask lensMask;
    [SerializeField] protected CanvasGroup lensCanvasGroup;

    [Space,SerializeField] protected GameObject fillBarDisplay;
    [SerializeField] protected Image fillBarImage;

    [Space, Range(0f, 10f),SerializeField] protected float scrollSpeed = 1f;
    [Range(0f, 2f), SerializeField] protected float lensTweenDuration = .3f;

    [SerializeField] protected bool isMaskEnabled = true; // For debug, allows toggling the mask on/off in the inspector

    protected Tween lensTween;
    protected bool isOverSearchableArea;

    protected int searchableLayerMask = 0;

    protected override void OnValidate()
    {
        base.OnValidate();
        //for debug
        this.lensMask.enabled = this.isMaskEnabled;
        if (!this.isMaskEnabled)
        {
            SetSearchMode(true);
        }
    }
    
    protected override void Awake()
    {
        this.linkedPassiveHarvester = GetComponent<MagnifyingGlass>();
        //set this in override
        //this.searchableLayerMask = LayerMask.GetMask("Searchable");
    }

    protected override void Start()
    {
        base.Start();
        SetFillBarActive(false);
        SetSearchMode(false, true);
    }

    public void SetSearchMode(bool inIsSearchMode, bool inShouldForce = false)
    {
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
        if (!this.IsInSearchMode)
            return;

        this.innerWorld.localPosition = this.transform.localPosition * -1 * this.scrollSpeed;
    }
    
    protected override bool DoOnBeginDrag()
    {
        SetSearchMode(true);
        return true;
    }

    protected override bool DoOnDrag()
    {
        this.isOverSearchableArea = IsOverSearchableArea();

        SetSearchMode(this.isOverSearchableArea);

        if(this.isOverSearchableArea)
        {
            ScrollInnerWorld();
        }

        if (!base.DoOnDrag())
        {
            return false;
        }

        return true;
    }

    protected virtual bool IsOverSearchableArea()
    {
        Collider2D hitCollider = Physics2D.OverlapPoint(this.worldProxy.transform.position, this.searchableLayerMask);

        if (hitCollider != null)
            return true;

        return false;
    }
    
    protected override void DoOnEndDrag()
    {
        SetSearchMode(false, true);
    }
}