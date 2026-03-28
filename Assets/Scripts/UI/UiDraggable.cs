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

    [Header("Set False for Menus, etc.")]
    [SerializeField] protected bool shouldDetectDropTargets = true;

    [Header("(Defaults to Object Root)")]
    [SerializeField] protected RectTransform targetRectTransform;

    [Header("Optional outline to show when drag mode is enabled")]
    [SerializeField] protected GameObject dragEnabledDisplay;

    public bool IsDraggingPermanent => this.isDraggingPermanent;
    public bool LimitToParentTargetBounds => this.limitToParentTargetBounds;
    public bool OnlyDragToTargets => this.onlyDragToTargets;
    public bool ShouldDetectDropTargets => this.shouldDetectDropTargets;

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

        if (!DoOnBeginDrag())
            return;

        this.isDragging = true;

        this.originalParent = this.targetRectTransform.parent;
        this.originalSiblingIndex = this.targetRectTransform.GetSiblingIndex();
        this.originalWorldPosition = this.targetRectTransform.position;

        this.targetRectTransform.SetParent(DragManager.IN.DragCanvas, true);

        //figure out which canvas this object is a child of to determine drag space
        var dragSpace = EDragSpace.World;
        var parentCanvas = this.GetComponentInParent<Canvas>();
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            dragSpace = EDragSpace.Screen;

        var dragPos = DragManager.IN.GetPositionInSpace(this.targetRectTransform.position, dragSpace);
        var screenStartPos = DragManager.IN.GetPositionInSpace(this.targetRectTransform.position, EDragSpace.Screen);

        Vector3 cameraDelta = Vector3.zero;
        if (dragSpace == EDragSpace.World)
        {
            //var screenPoint = DragManager.IN.DragCamera.WorldToScreenPoint(this.originalWorldPosition);
            cameraDelta = DragManager.IN.DragCamera.transform.position - DragManager.IN.WorldCamera.transform.position;
        }

        //temp until I can figure out how to set OffsetFromCursor correctly in world space
        this.OffsetFromCursor = (Vector3)eventData.position - screenStartPos + cameraDelta;

        this.targetRectTransform.position = this.originalWorldPosition - this.OffsetFromCursor;

        Debug.Log($"OnBeginDrag: dragSpace = {dragSpace}  cameraDelta = {cameraDelta}  dragPos = {dragPos}. screenStartPos = {screenStartPos}  originalWorldPosition = {this.originalWorldPosition}  eventData = {eventData.position}    OffsetFromCursor = {this.OffsetFromCursor}");

        // Register with drag proxy
        DragManager.IN.StartDrag(this, this.targetRectTransform, this.OffsetFromCursor);
    }

    public virtual void OnDrag(PointerEventData eventData){}

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
        var destinationSpace = EDragSpace.Screen;
        var dropPosition = Vector3.zero;

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

                    if (dragTarget.GetComponentInParent<Canvas>()?.renderMode == RenderMode.WorldSpace)
                        destinationSpace = EDragSpace.World;

                    dropPosition = DragManager.IN.GetPositionInSpace(this.targetRectTransform.position, destinationSpace);
                    this.targetRectTransform.position = dropPosition;// - this.OffsetFromCursor;
                    SaveItemPosition();
                    return false;//found drag target, reparent and exit
                }
            }
        }

        // If not detecting drop targets, just reparent to original parent
        this.targetRectTransform.SetParent(this.originalParent, true);
      
        if (this.originalParent.GetComponentInParent<Canvas>()?.renderMode == RenderMode.WorldSpace)
            destinationSpace = EDragSpace.World;

        dropPosition = DragManager.IN.GetPositionInSpace(this.targetRectTransform.position, destinationSpace);

        this.targetRectTransform.position = dropPosition;// - this.OffsetFromCursor;
        this.targetRectTransform.SetSiblingIndex(this.originalSiblingIndex);

        SaveItemPosition();//delete?
            
        return true;
    }

    protected virtual void DoSnapBack()
    {
        if (this.originalParent == null || !this.onlyDragToTargets)
            return;
        
        //snap back to original position
        transform.DOMove(this.originalWorldPosition, 0.2f).OnComplete(() =>
        {
            this.targetRectTransform.SetParent(this.originalParent, true);
            this.targetRectTransform.SetSiblingIndex(this.originalSiblingIndex);

            SaveItemPosition();//delete?
        });
    }
}