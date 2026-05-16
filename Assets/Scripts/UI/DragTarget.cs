using UnityEngine;
using static GlobalEnums;

public class DragTarget : MonoBehaviour
{
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
    public float UnsnapRange = -1;

    public bool IsDragOutOfInventoryZone;
    public bool IsDragOverOpenInventoryZone;

    private void Awake()
    {
        this.BoundsCollider = this.BoundsCollider == null ? GetComponent<Collider2D>() : this.BoundsCollider;
        SetHighlight(false);
        SetIsValidHighlight(false);

        DragManager.OnDragStartedWithDecorationType += OnDragStartedWithDecorationType;
        DragManager.OnDragEnded += OnDragEnded;
    }

    private void OnDestroy()
    {
        DragManager.OnDragStartedWithDecorationType -= OnDragStartedWithDecorationType;
        DragManager.OnDragEnded -= OnDragEnded;
    }

    private void OnDragStartedWithDecorationType(EDecorationType decorationType)
    {
        SetIsValidHighlight(AllowsDecorationType(decorationType));
    }

    private void OnDragEnded()
    {
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
        Debug.Log($"Setting valid highlight to {isHighlighted} for {gameObject.name}");
        if (this.isValidHighlight)
            this.isValidHighlight.SetActive(isHighlighted);
    }

    public void SetAsParent(Transform inChildObject)
    {
        inChildObject.SetParent(transform, true);

        if(this.shouldSnapToCenter)
            inChildObject.localPosition = Vector3.zero;
    }
}