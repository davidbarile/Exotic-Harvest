using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//base for magnifying glass and telescope, mainly for shared functionality like search mode and lens effects. Also serves as a common type for the UI to reference without needing to know the specific decoration type
public class SearchToolBase : DecorationBase
{
    public bool IsInSearchMode { get; private set; }
    [SerializeField] private RectTransform innerWorld;
    [SerializeField] private Transform container;
    [SerializeField] private Mask lensMask;
    [SerializeField] private CanvasGroup lensCanvasGroup;
    [SerializeField] private float scrollSpeed = 1f;

    [SerializeField] private bool isMaskEnabled = true; // For debug, allows toggling the mask on/off in the inspector

    private Tween lensTween;

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
    }

    protected override void Start()
    {
        base.Start();
        SetSearchMode(false);
    }

    public void SetSearchMode(bool active)
    {
        this.IsInSearchMode = active;

        this.lensTween?.Kill();
        if (active)
        {
            this.lensMask.gameObject.SetActive(true);

            this.lensCanvasGroup.gameObject.SetActive(true);
            this.lensTween = this.lensCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InOutSine);
        }
        else
        {
            this.lensCanvasGroup.gameObject.SetActive(true);
            this.lensTween = this.lensCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.InOutSine).OnComplete(() => this.lensMask.gameObject.SetActive(false));
        }
    }

    public void ScrollInnerWorld()
    {
        if (!this.IsInSearchMode)
            return;
            
        this.innerWorld.localPosition = this.transform.localPosition * -1 * this.scrollSpeed;
    }

    protected override bool DoOnDrag()
    {
        ScrollInnerWorld();

        if (!base.DoOnDrag())
        {
            return false;
        }

        return true;
    }
}