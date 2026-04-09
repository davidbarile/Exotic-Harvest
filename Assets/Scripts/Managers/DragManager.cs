using System;
using UnityEngine;

public class DragManager : MonoBehaviour
{
    public static DragManager IN;
    public static Vector3 ScreenToWorldCameraDelta { get; private set; }
    public static Vector3 CameraDelta = Vector3.zero;

    public static Action<bool> OnDragOverInventoryZoneActiveChanged;

    public static Action<EDecorationType> OnDragStartedWithDecorationType;
    public static Action OnDragEnded;

    public static bool IsDragModeActivated = false;

    public static Action<bool> OnDragModeChanged;

    public RectTransform DragCanvas;
    public RectTransform WorldRectTrans;

    [Space] public Transform WorldDecorationsContainer;

    public Camera DragCamera => this.dragCamera;
    public Camera WorldCamera => this.worldCamera;

    [Space, SerializeField] private Camera dragCamera;
    [SerializeField] private Camera worldCamera;

    [SerializeField] private GameObject inventoryOpenTrigger;

    // Drag Proxy State
    public RectTransform CurrentDraggedTransform { get; private set; }
    public Vector3 OffsetFromCursor { get; private set; }
    public bool IsDraggingActive { get; private set; }

    public UiDraggable CurrentDragSource => this.currentDragSource;
    private UiDraggable currentDragSource;
    private GameObject currentDragProxy;

    private void Start()
    {
        InputManager.OnDragPress += HandleDragModeChanged;
        OnDragOverInventoryZoneActiveChanged += SetDragOverInventoryZoneActive;
        SetDragOverInventoryZoneActive(false);
    }

    private void Update()
    {
        DragManager.ScreenToWorldCameraDelta = this.worldCamera.transform.position - this.dragCamera.transform.position;

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
        InputManager.OnDragPress -= HandleDragModeChanged;
        OnDragOverInventoryZoneActiveChanged -= SetDragOverInventoryZoneActive;
    }

    private void SetDragOverInventoryZoneActive(bool isActive)
    {
        if (this.inventoryOpenTrigger)
            this.inventoryOpenTrigger.SetActive(isActive);
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

        inDraggedTransform.SetParent(this.DragCanvas, true);

        var dragDecoration = this.currentDragSource as UiDecorationBase;

        if(inSource.HighlightValidTargetsWhenDragged)
            OnDragStartedWithDecorationType?.Invoke(dragDecoration != null ? dragDecoration.ItemData.DecorationData.DecorationType : EDecorationType.None);

        if (dragDecoration != null && dragDecoration.WorldProxy != null)
        {
            this.currentDragProxy = dragDecoration.WorldProxy.gameObject;

            var shouldShow = true; //CameraDelta != Vector3.zero;
            this.currentDragProxy.SetActive(shouldShow);
            // this.currentDragProxy.transform.localPosition = CameraDelta / this.currentDragSource.transform.localScale.x;
            this.currentDragProxy.transform.localPosition = ScreenToWorldCameraDelta / this.currentDragSource.transform.localScale.x;
        }
    }

    public void UpdateDrag(Vector2 mousePos)
    {
        if (!this.IsDraggingActive || this.CurrentDraggedTransform == null)
            return;

        if (!UiManager.IN.InventoryPanel.IsShowing && UiDecorationBase.CheckIfOverInventoryZone())
        {
            //TODO: swap for InventoryItem prefab
            // this.IsDraggingActive = false;
            // return;
        }

        if( this.currentDragProxy)
        {
            //this.currentDragProxy.transform.localPosition = CameraDelta / this.currentDragSource.transform.localScale.x;
            this.currentDragProxy.transform.localPosition = ScreenToWorldCameraDelta / this.currentDragSource.transform.localScale.x;
            this.currentDragProxy.SetActive(!UiManager.IN.InventoryPanel.IsShowing);
        }

        if (this.currentDragSource.ShouldDetectDropTargets)
            UiDraggable.UpdateHighlightedObjects();

        if(IsClampedToDragTargetBounds(mousePos))
            return;

        var dragPos = GetPositionInSpace(mousePos);
        this.CurrentDraggedTransform.position = dragPos + this.OffsetFromCursor;
    }
    
    private bool IsClampedToDragTargetBounds(Vector2 mousePos)
    {
        if (!this.currentDragSource.LimitToParentTargetBounds || this.currentDragSource.OriginalParent == null)
            return false;

        if (this.currentDragSource.OriginalParent.TryGetComponent(out UiDragTarget parentDragTarget))
        {
            if (parentDragTarget.UnsnapRange == -1)
                return false;
                
            //if the parent has a bounds collider, clamp to that.  Otherwise clamp to the parent's rect transform bounds
            Vector3 clampedPosition = GetPositionInSpace(mousePos);
            if (parentDragTarget.BoundsCollider && parentDragTarget.BoundsCollider.enabled)
            {
                var offsetClampedPosition = clampedPosition + CameraDelta + this.OffsetFromCursor;
                // Check if the point is inside the 2D collider
                if (!parentDragTarget.BoundsCollider.OverlapPoint(offsetClampedPosition))
                {
                    // Find the closest point on the collider's bounds
                    var closestPoint = parentDragTarget.BoundsCollider.ClosestPoint(offsetClampedPosition) - (Vector2)this.OffsetFromCursor - (Vector2)CameraDelta;
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
                    Vector3 min = worldCorners[0] - this.OffsetFromCursor - CameraDelta;
                    Vector3 max = worldCorners[2] - this.OffsetFromCursor - CameraDelta;

                    clampedPosition.x = Mathf.Clamp(clampedPosition.x, min.x, max.x);
                    clampedPosition.y = Mathf.Clamp(clampedPosition.y, min.y, max.y);
                }
            }

            if (parentDragTarget.UnsnapRange > -1)
            {
                // Distance from drag start point (should be 0 at drag start)
                float distance = Vector2.Distance(mousePos, RectTransformUtility.WorldToScreenPoint(this.dragCamera, clampedPosition));
                if (distance > parentDragTarget.UnsnapRange)
                {
                    // Outside unsnap range, do not clamp
                    return false;
                }
            }

            this.CurrentDraggedTransform.position = clampedPosition + this.OffsetFromCursor;
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
        OnDragEnded?.Invoke();
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
        newDraggedTransform.SetParent(this.DragCanvas, true);

        // Update the reference to the new transform
        this.CurrentDraggedTransform = newDraggedTransform;

        // Update drag source if the new object has UiDraggable
        if (newDraggedTransform.TryGetComponent<UiDraggable>(out var newDragSource))
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

    public Vector3 GetPositionInSpace(Vector3 inWorldPosition)
    {
        var screenPoint = this.dragCamera.WorldToScreenPoint(inWorldPosition);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(this.DragCanvas, screenPoint, this.dragCamera, out Vector2 localPoint))
            return this.DragCanvas.TransformPoint(localPoint);

        return inWorldPosition;
    }

    public static Vector3 GetPositionValuesForDrag(Vector2 inMousePosition, Transform inObject, out Vector3 outOffsetFromCursor)
    {
        //figure out which canvas this object is a child of to determine drag space
        var parentCanvas = inObject.GetComponentInParent<Canvas>();
        CameraDelta = parentCanvas.renderMode == RenderMode.WorldSpace ? DragManager.ScreenToWorldCameraDelta : Vector3.zero;

        var objectPos = inObject.transform.position - CameraDelta;
        outOffsetFromCursor = objectPos - (Vector3)inMousePosition;

        return DragManager.IN.GetPositionInSpace(objectPos);
    }

    public static Vector3 GetPositionValuesForDrop(Vector2 inMousePosition, Transform inObject)
    {
        //figure out which canvas this object is a child of to determine drag space
        var parentCanvas = inObject.GetComponentInParent<Canvas>();
        var cameraDelta = parentCanvas.renderMode == RenderMode.WorldSpace ? DragManager.ScreenToWorldCameraDelta : Vector3.zero;

        var objectPos = inObject.transform.position + cameraDelta;

        return DragManager.IN.GetPositionInSpace(objectPos);
    }
}
