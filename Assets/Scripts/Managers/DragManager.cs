using System;
using UnityEngine;

public class DragManager : MonoBehaviour
{
    public static DragManager IN;

    public static Action<bool> OnDragOverInventoryZoneActiveChanged;

    public static bool IsDragModeActivated = false;

    public static Action<bool> OnDragModeChanged;

    public RectTransform DragCanvas;

    public Transform DefaultParent;

    [SerializeField] private GameObject inventoryOpenTrigger;

    // Drag Proxy State
    public RectTransform CurrentDraggedTransform { get; private set; }
    public Vector2 DragOffset { get; private set; }
    public bool IsDraggingActive { get; private set; }
    private UiDraggable currentDragSource;
    private Camera dragCamera;
    private Vector3 originalLocalPosition;

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
            UpdateDrag(Input.mousePosition, this.dragCamera, this.originalLocalPosition);
            
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

    public void StartDrag(UiDraggable source, RectTransform draggedTransform, Vector2 dragOffset, Camera camera, Vector3 originalLocalPos)
    {
        this.currentDragSource = source;
        this.CurrentDraggedTransform = draggedTransform;
        this.DragOffset = dragOffset;
        this.dragCamera = camera;
        this.originalLocalPosition = originalLocalPos;
        this.IsDraggingActive = true;
    }

    public void UpdateDrag(Vector2 screenPosition, Camera camera, Vector3 originalLocalPosition)
    {
        if (!this.IsDraggingActive || this.CurrentDraggedTransform == null)
            return;

        if (!UiManager.IN.InventoryPanel.IsShowing && UiWorldItemBase.CheckIfOverInventoryZone())
        {
            //TODO: swap for InventoryItem prefab
            // this.IsDraggingActive = false;
            // return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.DragCanvas,
            screenPosition,
            camera,
            out Vector2 localPointerPosition))
        {
            Vector3 offsetToOriginal = localPointerPosition - this.DragOffset;
            this.CurrentDraggedTransform.localPosition = originalLocalPosition + new Vector3(offsetToOriginal.x, offsetToOriginal.y, 0f);
        }
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
            this.DragOffset = currentLocalPointerPosition;
            this.originalLocalPosition = newDraggedTransform.localPosition;
        }

        // Update the reference to the new transform
        this.CurrentDraggedTransform = newDraggedTransform;

        // Update drag source if the new object has UiDraggable
        if (newDraggedTransform.TryGetComponent<UiDraggable>(out var newDragSource))
        {
            this.currentDragSource = newDragSource;
        }
    }
}
