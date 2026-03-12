using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class UiDraggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static HashSet<UiDragTarget> CurrentHighlightedTargets = new();

    [SerializeField] protected bool isDraggingPermanent;
    [SerializeField] protected bool onlyDragToTargets;
    [SerializeField] protected bool limitToParentTargetBounds;
    [Tooltip("Set False for Menus, etc.")]
    [SerializeField] protected bool shouldDetectDropTargets = true;
    [Tooltip("Set True for Menus, etc.")]
    [SerializeField] protected bool shouldReturnToOriginalParent;
    [SerializeField] protected RectTransform targetRectTransform;
    [Tooltip("Optional outline to show when drag mode is enabled")]
    [SerializeField] protected GameObject dragEnabledDisplay;

    protected Vector2 originalLocalPointerPosition;
    protected Vector3 originalLocalPosition;
    protected Vector3 originalWorldPosition;

    protected Transform originalParent;
    protected int originalSiblingIndex;
    protected bool isDragging = false;

    public static void UpdateHighlightedObjects()
    {
        foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
        {
            if (possibleTarget == null)
                continue;

            var dragTarget = possibleTarget.GetComponent<UiDragTarget>();

            if (dragTarget != null)
            {
                CurrentHighlightedTargets.Add(dragTarget);
                dragTarget.SetHighlight(true);
            }
        }

        // Clear highlights from targets no longer under the mouse
        List<UiDragTarget> targetsToClear = new();
        foreach (var highlightedTarget in CurrentHighlightedTargets)
        {
            if (!InputManager.ObjectsUnderMouse.Contains(highlightedTarget.gameObject))
            {
                highlightedTarget.SetHighlight(false);
                targetsToClear.Add(highlightedTarget);
            }
        }

        foreach (var targetToClear in targetsToClear)
        {
            CurrentHighlightedTargets.Remove(targetToClear);
        }
    }

    private void Awake()
    {
        if (this.targetRectTransform == null)
            this.targetRectTransform = GetComponent<RectTransform>();
    }

    protected virtual void Start()
    {
        DragManager.OnDragModeChanged += HandleDragModeChanged;
        HandleDragModeChanged(DragManager.IsDragModeActivated);
    }

    protected virtual void OnDestroy()
    {
        DragManager.OnDragModeChanged -= HandleDragModeChanged;
    }

    protected virtual void HandleDragModeChanged(bool isDragMode)
    {
        if (this.dragEnabledDisplay != null)
        {
            this.dragEnabledDisplay.SetActive(isDragMode);
        }
    }

    protected virtual bool DoOnBeginDrag()
    {
        // Override in subclasses for additional behavior
        return true;
    }

    protected virtual bool DoOnDrag()
    {
        // Override in subclasses for additional behavior
        return true;
    }
    
    protected virtual bool DoOnEndDrag()
    {
        // Override in subclasses for additional behavior
        return true;
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (!DragManager.IsDragModeActivated && !this.isDraggingPermanent)
            return;

        if(!DoOnBeginDrag())
            return;

        this.isDragging = true;

        this.originalParent = this.targetRectTransform.parent;
        this.originalSiblingIndex = this.targetRectTransform.GetSiblingIndex();

        this.targetRectTransform.SetParent(DragManager.IN.DragCanvas, true);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            DragManager.IN.DragCanvas,
            eventData.position,
            eventData.pressEventCamera,
            out this.originalLocalPointerPosition);

        this.originalLocalPosition = this.targetRectTransform.localPosition;
        this.originalWorldPosition = this.targetRectTransform.position;

        // Register with drag proxy
        DragManager.IN.StartDrag(this, this.targetRectTransform, this.originalLocalPointerPosition, eventData.pressEventCamera, this.originalLocalPosition);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!this.isDragging)
            return;

        // Use drag proxy for position updates
        DragManager.IN.UpdateDrag(eventData.position, eventData.pressEventCamera, this.originalLocalPosition);

        if (!this.shouldDetectDropTargets)
            return;

        if (!DoOnDrag())
            return;

        if (this.limitToParentTargetBounds && this.originalParent != null)
        {
            if (this.originalParent.TryGetComponent(out UiDragTarget parentDragTarget))
            {
                Vector3 clampedPosition = this.targetRectTransform.position;
                if (parentDragTarget.BoundsCollider != null)
                {
                    // Clamp position using BoundsCollider by raycasting from the center of the dragged object
                    Vector3 dragCenter = this.targetRectTransform.position;

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
                    RectTransform parentRect = this.originalParent.GetComponent<RectTransform>();
                    if (parentRect != null)
                    {
                        Vector3[] worldCorners = new Vector3[4];
                        parentRect.GetWorldCorners(worldCorners);
                        Vector3 min = worldCorners[0];
                        Vector3 max = worldCorners[2];

                        clampedPosition = this.targetRectTransform.position;
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

                this.targetRectTransform.position = clampedPosition;
            }
        }

        // Highlight potential drop targets
        UpdateHighlightedObjects();
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        this.isDragging = false;

        // Notify drag proxy that drag ended
        DragManager.IN.EndDrag();

        bool flowControl = TryToParentToDropTarget();

        if (!flowControl)
        {
            DoOnEndDrag();
            return;
        }

        DoSnapBack();
    }

    protected virtual void SaveItemPosition()
    {
        //implement in subclasses
    }

    protected virtual bool TryToParentToDropTarget()
    {
        if (this.shouldDetectDropTargets)
        {
            foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
            {
                if (possibleTarget.TryGetComponent<UiDragTarget>(out var dragTarget))
                {
                    dragTarget.SetAsParent(this.targetRectTransform);
                    this.targetRectTransform.SetAsLastSibling();
                    dragTarget.SetHighlight(false);
                    SaveItemPosition();
                    return false;//found drag target, reparent and exit
                }
            }
        }
        
        // If not detecting drop targets, just reparent to original parent or default
        var originalParent = this.shouldReturnToOriginalParent ? this.originalParent : DragManager.IN.DefaultParent;
        this.targetRectTransform.SetParent(originalParent, true);

        if (this.shouldReturnToOriginalParent)
            this.targetRectTransform.SetSiblingIndex(this.originalSiblingIndex);
        else
            this.targetRectTransform.SetAsLastSibling();

        SaveItemPosition();
            
        return true;
    }

    protected virtual void DoSnapBack()
    {
        if (this.originalParent != null)
        {
            if (this.onlyDragToTargets)
            {
                //snap back to original position
                transform.DOMove(this.originalWorldPosition, 0.2f).OnComplete(() =>
                {
                    var originalParent = this.shouldReturnToOriginalParent ? this.originalParent : DragManager.IN.DefaultParent;
                    this.targetRectTransform.SetParent(originalParent, true);

                    if (this.shouldReturnToOriginalParent)
                        this.targetRectTransform.SetSiblingIndex(this.originalSiblingIndex);

                    SaveItemPosition();
                });
            }
        }
    }
}