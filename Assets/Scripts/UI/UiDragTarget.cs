using UnityEngine;

public class UiDragTarget : MonoBehaviour
{
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private bool shouldSnapToCenter;

    [Header("Optional Bounds")]
    public Collider2D BoundsCollider;
    public float UnsnapRange = -1;

    private void Awake()
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(isHighlighted);
        }
    }

    public void SetAsParent(Transform childObject)
    {
        childObject.SetParent(transform, true);

        if(shouldSnapToCenter)
        {
            childObject.localPosition = Vector3.zero;
        }
    }
}