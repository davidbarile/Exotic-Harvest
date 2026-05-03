using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Sirenix.OdinInspector;
using static GlobalEnums;

[RequireComponent(typeof(RectTransform))]
public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static HashSet<DragTarget> CurrentHighlightedTargets = new();

    [Header("Can drag when Drag Mode is Off")]
    [SerializeField] protected bool isDraggingPermanent;
    [SerializeField] protected bool onlyDragToTargets;
    [SerializeField] protected bool limitToParentTargetBounds;
    [SerializeField] protected bool shouldDetectDropTargets = true;
    public bool HighlightValidTargetsWhenDragged => this.highlightValidTargetsWhenDragged;
    [Header("This is overridden by DecorationData.HighlightValidTargetsWhenDragged if UiDecorationBase")]
    [SerializeField] protected bool highlightValidTargetsWhenDragged;
    [SerializeField] protected bool isMenuPanel;
    [Space, Range(0,50f), SerializeField] protected float padding = 10f;

    [Header("(Defaults to Object Root)")]
    [SerializeField] protected RectTransform targetRectTransform;

    [Header("Drag Offset")]
    [SerializeField] protected bool snapToCursor;
    [SerializeField] protected Vector3 snapToCursorOffset;

    [Header("Drop Offset")]
    [SerializeField] protected Vector3 snapToCenterOffset;
    [SerializeField] private bool autoCalculateSnapToCenterOffset = true;

    [HideIf("IsDraggingPermanent"), Header("Optional outline to show when drag mode is enabled")]
    [SerializeField] protected GameObject dragEnabledDisplay;

    public bool IsDraggingPermanent => this.isDraggingPermanent;
    public bool LimitToParentTargetBounds => this.limitToParentTargetBounds;
    public bool OnlyDragToTargets => this.onlyDragToTargets;
    public bool ShouldDetectDropTargets => this.shouldDetectDropTargets;

    public Transform OriginalParent => this.originalParent;
    public int OriginalSiblingIndex => this.originalSiblingIndex;
    public RectTransform TargetRectTransform => this.targetRectTransform;

    protected Vector3 originalWorldPosition;

    protected Vector3 offsetFromCursor;

    protected Transform originalParent;
    protected int originalSiblingIndex;
    protected bool isDragging = false;

    protected virtual void OnValidate()
    {
        if (this.targetRectTransform == null)
            this.targetRectTransform = GetComponent<RectTransform>();

        if (this.autoCalculateSnapToCenterOffset)
        {
            this.snapToCenterOffset = new Vector3(0, this.targetRectTransform.rect.height * 0.5f);
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }


    public static void UpdateHighlightedObjects()
    {
        EDecorationType decorationType = EDecorationType.All;
        if (DragManager.IN.CurrentDragSource is DecorationBase decoration && decoration.ItemData != null && decoration.ItemData.DecorationData != null)
        {
            decorationType = decoration.ItemData.DecorationData.DecorationType;
        }

        foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
        {
            if (possibleTarget == null)
                continue;

            var dragTarget = possibleTarget.GetComponent<DragTarget>();

            if (dragTarget != null && !dragTarget.transform.IsChildOf(DragManager.IN.CurrentDraggedTransform))
            {
                if (dragTarget.AllowsDecorationType(decorationType))
                {
                    CurrentHighlightedTargets.Add(dragTarget);
                    dragTarget.SetHighlight(true);
                }
            }
        }

        // Clear highlights from targets no longer under the mouse
        List<DragTarget> targetsToClear = new();
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

    protected virtual void Start()
    {
        SetDragEnabledDisplayVisibility(false);

        DragManager.OnDragModeChanged += SetDragEnabledDisplayVisibility;
        SetDragEnabledDisplayVisibility(DragManager.IsDragModeActivated);
    }

    protected virtual void OnDestroy()
    {
        DragManager.OnDragModeChanged -= SetDragEnabledDisplayVisibility;
    }

    protected virtual void SetDragEnabledDisplayVisibility(bool isDragMode)
    {
        if (this.dragEnabledDisplay)
            this.dragEnabledDisplay.SetActive(isDragMode);
    }

    protected virtual bool DoOnBeginDrag()
    {
        // Override in subclasses for additional behavior
        return true;
    }

    public virtual void OnDragUpdate()
    {
        // Called from DragManager Override in subclasses for additional behavior
    }

    protected virtual void DoOnEndDrag()
    {
        // Override in subclasses for additional behavior
    }
    
    protected virtual bool DoNoDropTargetFound()
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

        var dragPos = DragManager.GetPositionValuesForDrag(eventData.position, this.targetRectTransform, out var cursorOffset);

        if(this.snapToCursor)
            this.offsetFromCursor = this.snapToCursorOffset;
        else
            this.offsetFromCursor = cursorOffset;

        this.targetRectTransform.position = dragPos + this.offsetFromCursor;

        // Register with drag proxy
        DragManager.IN.StartDrag(this, this.targetRectTransform, this.offsetFromCursor);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!DragManager.IsDragModeActivated && !this.isDraggingPermanent)
            return;

        if (!this.isDragging)
            return;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (!DragManager.IsDragModeActivated && !this.isDraggingPermanent)
            return;

        this.isDragging = false;

        // Notify drag proxy that drag ended
        DragManager.IN.EndDrag();

        bool flowControl = TryToParentToDropTarget();

        if (!flowControl)
        {
            DoNoDropTargetFound();
            DoOnEndDrag();
            return;
        }

        DoOnEndDrag();

        DoSnapBack();
    }

    protected virtual void SaveItemPosition()
    {
        //implement in subclasses
    }

    protected virtual bool TryToParentToDropTarget()
    {
        if (this.isMenuPanel)
        {
            this.targetRectTransform.SetParent(this.originalParent, true);

            this.targetRectTransform.position = DragManager.GetPositionValuesForDrop(Input.mousePosition, this.targetRectTransform);
            this.targetRectTransform.SetSiblingIndex(this.originalSiblingIndex);

            ClampToScreenBounds();

            SaveItemPosition();
            return false;
        }
        
        if (this.shouldDetectDropTargets)
        {
            EDecorationType decorationType = EDecorationType.All;
            if (this is DecorationBase decoration && decoration.ItemData != null && decoration.ItemData.DecorationData != null)
            {
                decorationType = decoration.ItemData.DecorationData.DecorationType;
            }

            var canvasOffset = Vector3.zero;
            Canvas parentCanvas = null;
        
            foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
            {
                if (possibleTarget.TryGetComponent<DragTarget>(out var dragTarget) && !dragTarget.transform.IsChildOf(this.targetRectTransform))
                {
                    if (!dragTarget.AllowsDecorationType(decorationType))
                        continue;

                    parentCanvas = dragTarget.transform.GetComponentInParent<Canvas>();
                    if (parentCanvas == UiManager.IN.WorldCanvas)
                        canvasOffset = DragManager.ScreenToWorldCameraDelta;

                    dragTarget.SetAsParent(this.targetRectTransform);

                    if (dragTarget.ShouldSnapToCenter)
                        this.targetRectTransform.localPosition += this.snapToCenterOffset - canvasOffset;
                        
                    this.targetRectTransform.SetAsLastSibling();
                    dragTarget.SetHighlight(false);
                    this.targetRectTransform.position = DragManager.GetPositionValuesForDrop(Input.mousePosition, this.targetRectTransform);

                    ClampToScreenBounds();

                    SaveItemPosition();
                    return false;//found drag target, reparent and exit
                }
            }
        }

        var foundTarget = false;
        foreach (var possibleTarget in InputManager.ObjectsUnderMouse)
        {
            if (!possibleTarget.transform.IsChildOf(this.targetRectTransform))
            {
                foundTarget = true;
                break;
            }
        }

        // If not detecting drop targets, just reparent to original parent
        if(!foundTarget)
            this.targetRectTransform.SetParent(DragManager.IN.WorldDecorationsContainer, true);

        this.targetRectTransform.position = DragManager.GetPositionValuesForDrop(Input.mousePosition, this.targetRectTransform);
        this.targetRectTransform.SetAsLastSibling();

        ClampToScreenBounds();

        SaveItemPosition();
            
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
    
    private void ClampToScreenBounds()
    {
        var itemRect = this.targetRectTransform.rect;

        var clampedPosition = this.targetRectTransform.position;
        
        // Calculate offset based on pivot (0 = left/bottom edge, 0.5 = center, 1 = right/top edge)
        var pivotOffsetX = itemRect.width * this.targetRectTransform.pivot.x;
        var pivotOffsetY = itemRect.height * this.targetRectTransform.pivot.y;
        
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, 0 + pivotOffsetX + this.padding, Screen.width - (itemRect.width - pivotOffsetX) - this.padding);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, 0 + pivotOffsetY + this.padding, Screen.height - (itemRect.height - pivotOffsetY) - this.padding);

        this.targetRectTransform.position = clampedPosition;
    }   
}