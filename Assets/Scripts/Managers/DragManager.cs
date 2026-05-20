using System;
using UnityEngine;
using static GlobalEnums;

public class DragManager : MonoBehaviour
{
    public static DragManager IN;

    public static Vector3 ScreenToWorldCameraDelta { get; private set; }
    public static Vector3 CameraDelta = Vector3.zero;

    public static float UiCanvasScaleFactor => UiManager.IN.UICanvas.transform.localScale.x; // Assuming uniform scaling on the canvas

    public static Canvas DragStartCanvas;
    public static Vector3 OffsetFromCursor;
    public static Vector3 SnapBackStartPos;

    public static bool StartedDragInWorldCanvas => DragStartCanvas == UiManager.IN.WorldCanvas;
    public static bool StartedDragInUICanvas => DragStartCanvas == UiManager.IN.UICanvas;

    public static bool IsDragModeActivated = false;

    //Events
    public static Action<bool> OnDragOverInventoryZoneActiveChanged;
    public static Action<EDecorationType> OnDragStartedWithDecorationType;
    public static Action OnDragStarted;
    public static Action OnDragEnded;
    public static Action<bool> OnDragModeChanged;

    [Space] public Transform WorldDecorationsContainer;

    [SerializeField] private GameObject inventoryOpenTrigger;

    [Space] public Material DragEnabledMaterial;

    // Drag Proxy State
    public RectTransform CurrentDraggedTransform { get; private set; }
    public bool IsDraggingActive { get; private set; }

    public Draggable CurrentDragSource => this.currentDragSource;
    private Draggable currentDragSource;
    private GameObject currentDragProxy;

    private bool hasBrokenFreeOfClamp = false;

    private void Start()
    {
        InputManager.OnDragPress += OnToggleDragMode;
        OnDragOverInventoryZoneActiveChanged += SetDragOverInventoryZoneActive;
        SetDragOverInventoryZoneActive(false);
    }

    private void Update()
    {
        ScreenToWorldCameraDelta = UiManager.IN.WorldCamera.transform.position - UiManager.IN.DragCamera.transform.position;// * UiCanvasScaleFactor;

        // Continue updating drag position autonomously when active
        // This allows dragging to continue after object swap
        if (this.IsDraggingActive && this.CurrentDraggedTransform != null)
        {
            UpdateDrag(Input.mousePosition);
            
            // Detect mouse release to end drag on swapped objects
            if (Input.GetMouseButtonUp(0))
            {
                TriggerEndDragOnCurrentObject();
            }
        }
    }

    private void OnDestroy()
    {
        InputManager.OnDragPress -= OnToggleDragMode;
        OnDragOverInventoryZoneActiveChanged -= SetDragOverInventoryZoneActive;
    }

    private void SetDragOverInventoryZoneActive(bool isActive)
    {
        if (this.inventoryOpenTrigger)
            this.inventoryOpenTrigger.SetActive(isActive);
    }

    //called from Drag UI Button and also Tab hotkey
    public void OnToggleDragMode()
    {
        IsDragModeActivated = !IsDragModeActivated;
        OnDragModeChanged?.Invoke(IsDragModeActivated);
    }

    public void SetDragMode(bool isDragMode)
    {
        IsDragModeActivated = isDragMode;

        OnDragModeChanged?.Invoke(IsDragModeActivated);
    }
    public void InitDrag(RectTransform inRectTrans)
    {
        //set/cache values to be used in drag calculations
        var isChildOfWorld = inRectTrans.IsChildOf(UiManager.IN.WorldCanvas.transform);

        DragStartCanvas = isChildOfWorld ? UiManager.IN.WorldCanvas : UiManager.IN.UICanvas;
        CameraDelta = isChildOfWorld ? ScreenToWorldCameraDelta : Vector3.zero;
    }

    public void StartDrag(Draggable inSource, RectTransform inDraggedTransform)
    {
        this.currentDragSource = inSource;
        this.CurrentDraggedTransform = inDraggedTransform;
        this.IsDraggingActive = true;
        this.hasBrokenFreeOfClamp = false;

        OnDragStarted?.Invoke();

        inDraggedTransform.SetParent(UiManager.IN.DragCanvas, true);

        UpdateDrag(Input.mousePosition);//call once to set position in DragCanvas before caching
        SnapBackStartPos = inDraggedTransform.position;

        Debug.Log($"SnapBackStartPos = {SnapBackStartPos}, parent = {inDraggedTransform.parent.name}", inDraggedTransform);

        var dragDecoration = this.currentDragSource as DecorationBase;

        if(inSource.HighlightValidTargetsWhenDragged)
        {
            var decorationType = EDecorationType.None;
            
            if (dragDecoration != null)
                decorationType = dragDecoration.ItemData.DecorationData.DecorationType;
            else if (inSource.TryGetComponent(out UiInventoryItem uiInventoryItem))
                decorationType = uiInventoryItem.ItemData.DecorationData.DecorationType;

            OnDragStartedWithDecorationType?.Invoke(decorationType);
        }

        if (dragDecoration != null && dragDecoration.WorldProxy != null)
        {
            this.currentDragProxy = dragDecoration.WorldProxy.gameObject;

            var shouldShow = true; //CameraDelta != Vector3.zero;
            this.currentDragProxy.SetActive(shouldShow);
            this.currentDragProxy.transform.localPosition = ScreenToWorldCameraDelta / this.currentDragSource.transform.localScale.x;
        }
    }

    public void UpdateDrag(Vector2 mousePos)
    {
        if (!this.IsDraggingActive || this.CurrentDraggedTransform == null)
            return;

        if (!UiManager.IN.InventoryPanel.IsShowing && DecorationBase.CheckIfOverInventoryZone())
        {
            //TODO: swap for InventoryItem prefab
            // this.IsDraggingActive = false;
            // return;
        }

        if (this.currentDragProxy)
        {
            // Convert mouse position to world position using the WorldCamera
            var mouseScreenPos = new Vector3(mousePos.x, mousePos.y, Mathf.Abs(UiManager.IN.WorldCamera.transform.position.z));
            var worldPos = UiManager.IN.WorldCamera.ScreenToWorldPoint(mouseScreenPos);
            this.currentDragProxy.transform.position = worldPos;
            this.currentDragProxy.SetActive(!UiManager.IN.InventoryPanel.IsShowing);
        }

        if (this.currentDragSource.ShouldDetectDropTargets)
            Draggable.UpdateHighlightedObjects();

        if (!this.hasBrokenFreeOfClamp && IsClampedToDragTargetBounds(mousePos))
            return;

        this.hasBrokenFreeOfClamp = true;

        var dragPos = GetDragPosition(mousePos, this.CurrentDraggedTransform, UiManager.IN.UICanvas);
        this.CurrentDraggedTransform.position = dragPos + OffsetFromCursor;
        this.currentDragSource.OnDragUpdate();
    }
    
    private bool IsClampedToDragTargetBounds(Vector2 mousePos)
    {
        if (!this.currentDragSource.LimitToParentTargetBounds || this.currentDragSource.OriginalParent == null)
            return false;

        if (this.currentDragSource.OriginalParent.TryGetComponent(out DragTarget parentDragTarget))
        {
            if (parentDragTarget.UnsnapRange == -1)
                return false;
                
            //if the parent has a bounds collider, clamp to that.  Otherwise clamp to the parent's rect transform bounds
            Vector3 clampedPosition = GetDragPosition(mousePos, this.CurrentDraggedTransform);
            if (parentDragTarget.BoundsCollider && parentDragTarget.BoundsCollider.enabled)
            {
                var offsetClampedPosition = clampedPosition + CameraDelta + OffsetFromCursor;
                // Check if the point is inside the 2D collider
                if (!parentDragTarget.BoundsCollider.OverlapPoint(offsetClampedPosition))
                {
                    // Find the closest point on the collider's bounds
                    var closestPoint = parentDragTarget.BoundsCollider.ClosestPoint(offsetClampedPosition) - (Vector2)OffsetFromCursor - (Vector2)CameraDelta;
                    clampedPosition = new Vector3(closestPoint.x, closestPoint.y, this.CurrentDraggedTransform.position.z);
                }
            }
            else
            {
                //no collider, clamp to rect transform bounds
                RectTransform parentRect = this.currentDragSource.OriginalParent.GetComponent<RectTransform>();
                if (parentRect != null)
                {
                    var worldCorners = new Vector3[4];
                    parentRect.GetWorldCorners(worldCorners);
                    Vector3 min = worldCorners[0] - OffsetFromCursor - CameraDelta;
                    Vector3 max = worldCorners[2] - OffsetFromCursor - CameraDelta;

                    clampedPosition.x = Mathf.Clamp(clampedPosition.x, min.x, max.x);
                    clampedPosition.y = Mathf.Clamp(clampedPosition.y, min.y, max.y);
                }
            }

            if (parentDragTarget.UnsnapRange > -1)
            {
                // Distance from drag start point (should be 0 at drag start)
                float distance = Vector2.Distance(mousePos, RectTransformUtility.WorldToScreenPoint(UiManager.IN.DragCamera, clampedPosition));
                if (distance > parentDragTarget.UnsnapRange)
                {
                    // Outside unsnap range, do not clamp
                    return false;
                }
            }

            this.CurrentDraggedTransform.position = clampedPosition + OffsetFromCursor;
            return true;
        }
        return false;
    }

    public void EndDrag()
    {
        this.IsDraggingActive = false;
        this.CurrentDraggedTransform = null;
        this.currentDragSource = null;
        this.currentDragProxy = null;
        this.hasBrokenFreeOfClamp = false;
        OnDragEnded?.Invoke();//unhighlight all drag targets
    }

    private void TriggerEndDragOnCurrentObject()
    {
        if (this.CurrentDraggedTransform != null && this.currentDragSource != null)
        {
            // Create simulated PointerEventData
            var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
            {
                position = Input.mousePosition
            };
            
            // Manually trigger OnEndDrag on the current drag source
            this.currentDragSource.OnEndDrag(eventData);
        }
        else
        {
            // Fallback: just clear drag state
            EndDrag();
        }
    }

    public void SwapDraggedObject(RectTransform newDraggedTransform)
    {
        if (!this.IsDraggingActive || this.CurrentDraggedTransform == null)
            return;

        // Copy position from current dragged object to new one
        newDraggedTransform.position = this.CurrentDraggedTransform.position;
        newDraggedTransform.SetParent(UiManager.IN.DragCanvas, true);
        newDraggedTransform.localScale *= UiCanvasScaleFactor;

        //force InitDrag to world
        InitDrag(newDraggedTransform);

        // Update the reference to the new transform
        this.CurrentDraggedTransform = newDraggedTransform;

        // Update drag source if the new object has UiDraggable
        if (newDraggedTransform.TryGetComponent<Draggable>(out var newDragSource))
        {
            this.currentDragSource = newDragSource;

            if(!newDragSource.IsDraggingPermanent)
            {
                Debug.Log($"SetDragMode(true)  Swapped drag to new object [{newDraggedTransform.name}] with source [{newDragSource.name}]", newDraggedTransform);
                //flag to turn off on complete
                DragManager.IN.SetDragMode(true);
            }
        }
    }

    public static Vector3 GetDragPosition(Vector2 inMousePosition, RectTransform inRectTrans, Canvas inDragStartCanvas = null)
    {
        Vector3 result = inRectTrans.position;
        var canvas = inDragStartCanvas ? inDragStartCanvas : DragStartCanvas;
        
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(inRectTrans, inMousePosition, canvas.worldCamera, out Vector3 outWorldPos))
        {
            result = outWorldPos;
        }
        return result;
    }
}