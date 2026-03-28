using System;
using UnityEngine;

public class DragManager : MonoBehaviour
{
    public static DragManager IN;

    public static Action<bool> OnDragOverInventoryZoneActiveChanged;

    public static bool IsDragModeActivated = false;

    public static Action<bool> OnDragModeChanged;

    public static EDragSpace CurrentDragSpace { get; private set; }
    public static Camera CurrentDragCamera { get; private set; }
    public static RectTransform CurrentDragCanvas { get; private set; }

    public enum EDragSpace
    {
        Custom,
        Screen,
        World
    }

    public RectTransform DragCanvas;
    public RectTransform WorldRectTrans;

    public Transform DefaultParent;

    public Camera DragCamera => this.dragCamera;
    [SerializeField] private Camera dragCamera;

    public Camera WorldCamera => this.worldCamera;
    [SerializeField] private Camera worldCamera;

    [SerializeField] private GameObject inventoryOpenTrigger;

    [Header("Debug")]
    [SerializeField] private Transform worldDragObj;

    [SerializeField] private Transform worldRef;
    [SerializeField] private Transform screenRef;

    [SerializeField] private Vector3 screenToWorldDelta;
    [SerializeField] private Vector3 screenToWorldCameraDelta;
    [Space, SerializeField] private Vector3 screenToWorldLocalDelta;
    [SerializeField] private Vector3 screenToWorldLocalCameraDelta;

    [Space, SerializeField] private Vector3 convertWorldToScreenPoint;
    [SerializeField] private Vector3 convertWorldToScreenPos;

    [Space, SerializeField] private Vector3 convertUiToScreenPos;
    [SerializeField] private Vector3 convertUiToScreenPoint;

    [SerializeField] private bool refresh;

    // Drag Proxy State
    public RectTransform CurrentDraggedTransform { get; private set; }
    public Vector2 OffsetFromCursor { get; private set; }
    public bool IsDraggingActive { get; private set; }
    private UiDraggable currentDragSource;

    // private Vector3 originalLocalPosition;

    private void OnValidate()
    {
        this.screenToWorldDelta = worldRef.position - screenRef.position;
        this.screenToWorldCameraDelta = worldCamera.transform.position - dragCamera.transform.position;

        this.screenToWorldLocalDelta = worldRef.localPosition - screenRef.localPosition;
        this.screenToWorldLocalCameraDelta = worldCamera.transform.localPosition - dragCamera.transform.localPosition;

        convertWorldToScreenPoint = this.dragCamera.WorldToScreenPoint(worldRef.position);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(DragCanvas, convertWorldToScreenPoint, dragCamera, out Vector2 localPoint))
        {
            convertWorldToScreenPos = DragCanvas.TransformPoint(localPoint);
        }

        convertUiToScreenPoint = this.dragCamera.WorldToScreenPoint(screenRef.position);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(DragCanvas, convertUiToScreenPoint, dragCamera, out Vector2 localPoint2))
        {
            convertUiToScreenPos = DragCanvas.TransformPoint(localPoint2);
        }

        this.worldDragObj.position = this.screenRef.position + GetScreenToWorldCameraDelta();
    }

    public Vector3 GetScreenToWorldDelta() => this.screenToWorldDelta;
    public Vector3 GetScreenToWorldCameraDelta() => this.screenToWorldCameraDelta;
    public Vector3 GetScreenToWorldLocalDelta() => this.screenToWorldLocalDelta;
    public Vector3 GetScreenToWorldLocalCameraDelta() => this.screenToWorldLocalCameraDelta;

    private void Start()
    {
        InputManager.OnDragPress += HandleDragModeChanged;
        OnDragOverInventoryZoneActiveChanged += SetDragOverInventoryZoneActive;
        SetDragOverInventoryZoneActive(false);
    }

    private void Update()
    {
        // Continue updating drag position autonomously when active
        // This allows dragging to continue after object swap
        if (this.IsDraggingActive && this.CurrentDraggedTransform != null)
        {
            UpdateDrag(Input.mousePosition);

            OnValidate();
            
            // Detect mouse release to end drag on swapped objects
            if (Input.GetMouseButtonUp(0))
            {
                TriggerEndDragOnCurrentObject();
            }
        }
    }

    private void OnDestroy()
    {
        InputManager.OnDragPress -= HandleDragModeChanged;
        OnDragOverInventoryZoneActiveChanged -= SetDragOverInventoryZoneActive;
    }

    private void SetDragOverInventoryZoneActive(bool isActive)
    {
        if (this.inventoryOpenTrigger != null)
        {
            this.inventoryOpenTrigger.SetActive(isActive);
        }
    }

    private void HandleDragModeChanged()
    {
        IsDragModeActivated = !IsDragModeActivated;
        OnDragModeChanged?.Invoke(IsDragModeActivated);
    }

    public void SetDragMode(bool isDragMode)
    {
        IsDragModeActivated = isDragMode;
        OnDragModeChanged?.Invoke(IsDragModeActivated);
    }

    public void StartDrag(UiDraggable inSource, RectTransform inDraggedTransform, Vector2 inOffsetFromCursor)
    {
        this.currentDragSource = inSource;
        this.CurrentDraggedTransform = inDraggedTransform;
        this.OffsetFromCursor = inOffsetFromCursor;
        this.IsDraggingActive = true;
    }

    public void UpdateDrag(Vector2 mousePosition)
    {
        if (!this.IsDraggingActive || this.CurrentDraggedTransform == null)
            return;

        if (!UiManager.IN.InventoryPanel.IsShowing && UiDecorationBase.CheckIfOverInventoryZone())
        {
            //TODO: swap for InventoryItem prefab
            // this.IsDraggingActive = false;
            // return;
        }

        if(this.currentDragSource.ShouldDetectDropTargets)
           UiDraggable.UpdateHighlightedObjects();

        //this code is quite broken.  Need to factor in screen space, etc.
        if (this.currentDragSource.LimitToParentTargetBounds && this.currentDragSource.OriginalParent != null)
        {
            if (this.currentDragSource.OriginalParent.TryGetComponent(out UiDragTarget parentDragTarget))
            {
                Vector3 clampedPosition = this.CurrentDraggedTransform.position;
                if (parentDragTarget.BoundsCollider != null)
                {
                    // Clamp position using BoundsCollider by raycasting from the center of the dragged object
                    Vector3 dragCenter = this.CurrentDraggedTransform.position;

                    // Check if the point is inside the 2D collider
                    if (!parentDragTarget.BoundsCollider.OverlapPoint(dragCenter))
                    {
                        // Find the closest point on the collider's bounds
                        Vector2 closestPoint = parentDragTarget.BoundsCollider.ClosestPoint(dragCenter);
                        clampedPosition = new Vector3(closestPoint.x, closestPoint.y, this.CurrentDraggedTransform.position.z);
                    }
                }
                else
                {
                    RectTransform parentRect = this.currentDragSource.OriginalParent.GetComponent<RectTransform>();
                    if (parentRect != null)
                    {
                        Vector3[] worldCorners = new Vector3[4];
                        parentRect.GetWorldCorners(worldCorners);
                        Vector3 min = worldCorners[0];
                        Vector3 max = worldCorners[2];

                        clampedPosition = this.CurrentDraggedTransform.position;
                        clampedPosition.x = Mathf.Clamp(clampedPosition.x, min.x, max.x);
                        clampedPosition.y = Mathf.Clamp(clampedPosition.y, min.y, max.y);

                        //print($"B. clampedPosition: {clampedPosition}");
                    }
                }

                if (parentDragTarget.UnsnapRange > -1)
                {
                    // Distance from drag start point (should be 0 at drag start)
                    float distance = Vector2.Distance(mousePosition, RectTransformUtility.WorldToScreenPoint(this.dragCamera, clampedPosition));
                    if (distance > parentDragTarget.UnsnapRange)
                    {
                        // Outside unsnap range, do not clamp
                        //print($"Return");
                        return;
                    }
                }

                this.CurrentDraggedTransform.position = clampedPosition;

                //print($"C. clampedPosition: {clampedPosition}");
            }
        }

        var dragPos = GetPositionInSpace(mousePosition, CurrentDragSpace);
        this.CurrentDraggedTransform.position = dragPos - (Vector3)this.OffsetFromCursor;
    }

    public void EndDrag()
    {
        this.IsDraggingActive = false;
        this.CurrentDraggedTransform = null;
        this.currentDragSource = null;
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
        {
            Debug.LogWarning("SwapDraggedObject called but no drag is active");
            return;
        }

        // Copy position from current dragged object to new one
        newDraggedTransform.position = this.CurrentDraggedTransform.position;
        newDraggedTransform.SetParent(this.DragCanvas, true);

        // Recalculate drag offset to maintain cursor-to-object relationship
        // Get current mouse position in local space
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.DragCanvas,
            Input.mousePosition,
            this.dragCamera,
            out Vector2 currentLocalPointerPosition))
        {
            // Set DragOffset to current pointer position
            // This makes originalLocalPosition = current position, maintaining visual offset
            //this.OffsetFromCursor = currentLocalPointerPosition;
        }

        // Update the reference to the new transform
        this.CurrentDraggedTransform = newDraggedTransform;

        // Update drag source if the new object has UiDraggable
        if (newDraggedTransform.TryGetComponent<UiDraggable>(out var newDragSource))
        {
            this.currentDragSource = newDragSource;
        }
    }

    public Vector3 GetPositionInSpace(Vector3 inWorldPosition, EDragSpace inDragSpace = EDragSpace.Custom, RectTransform inCustomRectTrans = null)
    {
        var screenPoint = this.dragCamera.WorldToScreenPoint(inWorldPosition);

        CurrentDragSpace = inDragSpace;
        var isScreenSpace = inDragSpace == EDragSpace.Screen;
        var isWorldSpace = inDragSpace == EDragSpace.World;
        CurrentDragCanvas = isScreenSpace ? this.DragCanvas : (isWorldSpace ? this.WorldRectTrans : inCustomRectTrans);
        CurrentDragCamera = isScreenSpace ? this.dragCamera : (isWorldSpace ? this.worldCamera : this.dragCamera);

        //Vector3 screenPoint = CurrentDragCamera.WorldToScreenPoint(inWorldPosition);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(CurrentDragCanvas, screenPoint, CurrentDragCamera, out Vector2 localPoint))
        {
            //Debug.Log($"GetPositionInSpace: ({inDragSpace}) rectTrans = {inCustomRectTrans?.name}   worldPosition = {inWorldPosition}    screenPoint = {screenPoint}    localPoint = {localPoint}");
            return CurrentDragCanvas.TransformPoint(localPoint);
        }

        return inWorldPosition;
    }
}
