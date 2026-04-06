using UnityEngine;

public class UiDragTarget : MonoBehaviour
{
    [SerializeField] private GameObject highlightObject;
     [SerializeField] private GameObject isValidHighlight;
    [SerializeField] private bool shouldSnapToCenter;

    public EDecorationType AcceptedDecorationTypes => this.acceptedDecorationTypes;
    [SerializeField] private EDecorationType acceptedDecorationTypes = EDecorationType.All;

    [Header("Optional Bounds")]
    public Collider2D BoundsCollider;
    public float UnsnapRange = -1;

    public bool IsDragOutOfInventoryZone;
    public bool IsDragOverOpenInventoryZone;

    private void Awake()
    {
        this.BoundsCollider = this.BoundsCollider == null ? GetComponent<Collider2D>() : this.BoundsCollider;
        SetHighlight(false);
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