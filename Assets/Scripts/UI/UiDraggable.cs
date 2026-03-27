using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using static DragManager;

[RequireComponent(typeof(RectTransform))]
public class UiDraggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static HashSet<UiDragTarget> CurrentHighlightedTargets = new();

    [Header("Can drag when Drag Mode is Off")]
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

    public bool IsDraggingPermanent => this.isDraggingPermanent;
    public bool LimitToParentTargetBounds => this.limitToParentTargetBounds;
    public bool OnlyDragToTargets => this.onlyDragToTargets;
    public bool ShouldDetectDropTargets => this.shouldDetectDropTargets;
    public bool ShouldReturnToOriginalParent => this.shouldReturnToOriginalParent;
    public Transform OriginalParent => this.originalParent;
    public int OriginalSiblingIndex => this.originalSiblingIndex;
    public RectTransform TargetRectTransform => this.targetRectTransform;

    protected Vector3 originalWorldPosition;

    public Vector3 OffsetFromCursor;

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

        var dragSpace = EDragSpace.World;
        var parentCanvas = this.GetComponentInParent<Canvas>();
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            dragSpace = EDragSpace.Screen;
        
        var dragPos = DragManager.IN.GetPositionInSpace(this.targetRectTransform.position, dragSpace);
        this.targetRectTransform.position = dragPos;

        this.originalWorldPosition = dragPos;
        this.OffsetFromCursor = dragPos - this.targetRectTransform.position;

        // RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //     DragManager.IN.DragCanvas,
        //     eventData.position,
        //     eventData.pressEventCamera,
        //     out var originalLocalPointerPosition);

        // this.originalWorldPosition = this.targetRectTransform.position;
        // this.OffsetFromCursor = (Vector3)eventData.position - this.targetRectTransform.position;

        // Register with drag proxy
        DragManager.IN.StartDrag(this, this.targetRectTransform, this.OffsetFromCursor);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!this.isDragging)
            return;
            
        if(!DoOnDrag())
            return;

        // Use drag proxy for position updates
        DragManager.IN.UpdateDrag(eventData.position);
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
            Debug.Log($"UiDraggable.TryToParentToDropTarget(). {InputManager.ObjectsUnderMouse.Count} objects under mouse");
            foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
            {
                if (possibleTarget.TryGetComponent<UiDragTarget>(out var dragTarget))
                {
                    dragTarget.SetAsParent(this.targetRectTransform);
                    this.targetRectTransform.SetAsLastSibling();
                    dragTarget.SetHighlight(false);
                    this.targetRectTransform.position -= this.OffsetFromCursor;
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