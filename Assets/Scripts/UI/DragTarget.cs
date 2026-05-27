using UnityEngine;
using UnityEngine.UI;
using static GlobalEnums;

public class DragTarget : MonoBehaviour
{
    [Header("If null, will use this transform")]
    [SerializeField] private Transform itemContainer;
    [Header("Leave null to not highlight on valid drag over")]
    [SerializeField] private GameObject highlightObject;
    [Header("Leave null to not highlight on valid drag start")]
    [SerializeField] private GameObject isValidHighlight;

    public EDecorationType AcceptedDecorationTypes => this.acceptedDecorationTypes;
    [Space, SerializeField] private EDecorationType acceptedDecorationTypes = EDecorationType.All;

    public bool ShouldSnapToCenter => this.shouldSnapToCenter;
    [Space, SerializeField] private bool shouldSnapToCenter;

    [Header("Optional Bounds")]
    public Collider2D BoundsCollider;
    [Tooltip("-1 = Disabled")]
    public float UnsnapRange = -1;

    [Space]
    public bool IsDragOutOfInventoryZone;
    public bool IsDragOverOpenInventoryZone;

    private Image baseImage;

    private void Awake()
    {
        if (this.itemContainer == null)
            this.itemContainer = this.transform;

        this.BoundsCollider = this.BoundsCollider == null ? GetComponent<Collider2D>() : this.BoundsCollider;

        this.baseImage = GetComponent<Image>();
        if(this.baseImage != null)
            this.baseImage.raycastTarget = false;

        SetHighlight(false);
        SetIsValidHighlight(false);

        DragManager.OnDragStartedWithDecorationType += OnDragStartedWithDecorationType;
        DragManager.OnDragStarted += OnDragStarted;
        DragManager.OnDragEnded += OnDragEnded;
    }

    private void OnDestroy()
    {
        DragManager.OnDragStartedWithDecorationType -= OnDragStartedWithDecorationType;
        DragManager.OnDragStarted -= OnDragStarted;
        DragManager.OnDragEnded -= OnDragEnded;
    }

    private void OnDragStartedWithDecorationType(EDecorationType decorationType)
    {
        var isValid = AllowsDecorationType(decorationType);
        SetIsValidHighlight(isValid);
    }
    
    private void OnDragStarted()
    {
        if (this.baseImage != null)
            this.baseImage.raycastTarget = true;
    }

    private void OnDragEnded()
    {
        if (this.baseImage != null)
            this.baseImage.raycastTarget = false;
            
        SetHighlight(false);
        SetIsValidHighlight(false);
    }

    public bool AllowsDecorationType(EDecorationType decorationType)
    {
        return this.AcceptedDecorationTypes == EDecorationType.All ||
            decorationType == EDecorationType.All ||
            (this.AcceptedDecorationTypes & decorationType) != 0;
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (this.highlightObject)
            this.highlightObject.SetActive(isHighlighted);
    }

    public void SetIsValidHighlight(bool isHighlighted)
    {
        //Debug.Log($"Setting valid highlight to {isHighlighted} for {gameObject.name}");
        if (this.isValidHighlight)
            this.isValidHighlight.SetActive(isHighlighted);
    }

    public void SetAsParent(Transform inChildObject)
    {
        inChildObject.SetParent(this.itemContainer, true);

        if (this.shouldSnapToCenter)
            inChildObject.localPosition = Vector3.zero;
    }
}