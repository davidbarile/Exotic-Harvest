using UnityEngine;

public class UiDragTarget : MonoBehaviour
{
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private bool shouldSnapToCenter;

    [Header("Optional Bounds")]
    public Collider2D BoundsCollider;
    public float UnsnapRange = -1;

    public bool IsDragOutOfInventoryZone;
     public bool IsDragOverOpenInventoryZone;

    private void Awake()
    {
        SetHighlight(false);
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (this.highlightObject)
            this.highlightObject.SetActive(isHighlighted);
    }

    public void SetAsParent(Transform inChildObject)
    {
        inChildObject.SetParent(transform, true);

        if(this.shouldSnapToCenter)
            inChildObject.localPosition = Vector3.zero;
    }
}