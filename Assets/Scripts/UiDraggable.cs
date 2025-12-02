using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;

public class UiDraggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private bool isDraggingPermanent;
    [SerializeField] private bool onlyDragToTargets;
    [SerializeField] private bool limitToParentTargetBounds;
    [Tooltip("Set False for Menus, etc.")]
    [SerializeField] private bool shouldDetectDropTargets = true;
    [Tooltip("Set True for Menus, etc.")]
    [SerializeField] private bool shouldReturnToOriginalParent;
    [SerializeField] private RectTransform targetRectTransform;
    [Tooltip("Optional outline to show when drag mode is enabled")]
    [SerializeField] private GameObject dragEnabledDisplay;
    private Vector2 originalLocalPointerPosition;
    private Vector3 originalLocalPosition;
    private Vector3 originalWorldPosition;

    private Transform originalParent;
    private int originalSiblingIndex;
    private bool isDragging = false;

    private HashSet<UiDragTarget> currentHighlightedTargets = new();

    private void Start()
    {
        DragManager.OnDragModeChanged += HandleDragModeChanged;
        HandleDragModeChanged(DragManager.IsDragModeActivated);
    }

    private void OnDestroy()
    {
        DragManager.OnDragModeChanged -= HandleDragModeChanged;
    }
    
    private void HandleDragModeChanged(bool isDragMode)
    {
        if (dragEnabledDisplay != null)
        {
            dragEnabledDisplay.SetActive(isDragMode);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!DragManager.IsDragModeActivated && !isDraggingPermanent)
            return;

        isDragging = true;

        originalParent = targetRectTransform.parent;
        originalSiblingIndex = targetRectTransform.GetSiblingIndex();

        targetRectTransform.SetParent(DragManager.IN.DragCanvas, true);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            DragManager.IN.DragCanvas,
            eventData.position,
            eventData.pressEventCamera,
            out originalLocalPointerPosition);
        originalLocalPosition = targetRectTransform.localPosition;
        originalWorldPosition = targetRectTransform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                DragManager.IN.DragCanvas,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPointerPosition))
            {
                Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
                targetRectTransform.localPosition = originalLocalPosition + new Vector3(offsetToOriginal.x, offsetToOriginal.y, 0f);
            }

            if (!shouldDetectDropTargets)
                return;

            if(limitToParentTargetBounds && originalParent != null)
            {
                if (originalParent.TryGetComponent(out UiDragTarget parentDragTarget))
                {
                    Vector3 clampedPosition = targetRectTransform.position;
                    if (parentDragTarget.BoundsCollider != null)
                    {
                        // Clamp position using BoundsCollider by raycasting from the center of the dragged object
                        Vector3 dragCenter = targetRectTransform.position;

                        // Check if the point is inside the 2D collider
                        if (!parentDragTarget.BoundsCollider.OverlapPoint(dragCenter))
                        {
                            // Find the closest point on the collider's bounds
                            Vector2 closestPoint = parentDragTarget.BoundsCollider.ClosestPoint(dragCenter);
                            clampedPosition = new Vector3(closestPoint.x, closestPoint.y, targetRectTransform.position.z);
                        }
                    }
                    else
                    {
                        RectTransform parentRect = originalParent.GetComponent<RectTransform>();
                        if (parentRect != null)
                        {
                            Vector3[] worldCorners = new Vector3[4];
                            parentRect.GetWorldCorners(worldCorners);
                            Vector3 min = worldCorners[0];
                            Vector3 max = worldCorners[2];

                            clampedPosition = targetRectTransform.position;
                            clampedPosition.x = Mathf.Clamp(clampedPosition.x, min.x, max.x);
                            clampedPosition.y = Mathf.Clamp(clampedPosition.y, min.y, max.y);
                        }
                    }

                    if (parentDragTarget.UnsnapRange > -1)
                    {
                        // Distance from drag start point (should be 0 at drag start)
                        float distance = Vector2.Distance(eventData.position, RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, clampedPosition));
                        if (distance > parentDragTarget.UnsnapRange)
                        {
                            // Outside unsnap range, do not clamp
                            return;
                        }
                    }
                    
                    targetRectTransform.position = clampedPosition;
                }   
            }

            // Highlight potential drop targets
            foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
            {
                UiDragTarget dragTarget = possibleTarget.GetComponent<UiDragTarget>();
                if (dragTarget != null)
                {
                    currentHighlightedTargets.Add(dragTarget);
                    dragTarget.SetHighlight(true);
                }
            }
            
            // Clear highlights from targets no longer under the mouse
            List<UiDragTarget> targetsToClear = new();
            foreach (var highlightedTarget in currentHighlightedTargets)
            {
                if (!InputManager.ObjectsUnderMouse.Contains(highlightedTarget.gameObject))
                {
                    highlightedTarget.SetHighlight(false);
                    targetsToClear.Add(highlightedTarget);
                }
            }

            foreach (var targetToClear in targetsToClear)
            {
                currentHighlightedTargets.Remove(targetToClear);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        if(shouldDetectDropTargets)
        {
            foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
            {
                UiDragTarget dragTarget = possibleTarget.GetComponent<UiDragTarget>();
                if (dragTarget != null)
                {
                    dragTarget.SetAsParent(targetRectTransform);
                    dragTarget.SetHighlight(false);
                    return;
                }
            }
        }

        if (originalParent != null)
        {
            var originalParent = shouldReturnToOriginalParent ? this.originalParent : DragManager.IN.DefaultParent;
            targetRectTransform.SetParent(originalParent, true);

            if (shouldReturnToOriginalParent)
                targetRectTransform.SetSiblingIndex(originalSiblingIndex);

            if (onlyDragToTargets)
            {
                transform.DOMove(originalWorldPosition, 0.2f);
            }
        }
    }
}