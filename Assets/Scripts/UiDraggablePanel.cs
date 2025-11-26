using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UiDraggablePanel : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private Button dragButton;
    [SerializeField] private RectTransform targetRectTransform;
    private Vector2 originalLocalPointerPosition;
    private Vector3 originalPanelLocalPosition;
    private bool isDragging = false;

    private void Start()
    {
        GameManager.OnDragModeChanged += HandleDragModeChanged;
        HandleDragModeChanged(GameManager.IsDragModeActivated);
    }

    private void OnDestroy()
    {
        GameManager.OnDragModeChanged -= HandleDragModeChanged;
    }
    
    private void HandleDragModeChanged(bool isDragMode)
    {
        if (dragButton != null)
        {
            dragButton.gameObject.SetActive(isDragMode);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!GameManager.IsDragModeActivated)
            return;
            
        isDragging = true;
        // Use the parent RectTransform for correct pointer offset
        RectTransform parentRect = targetRectTransform.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out originalLocalPointerPosition);
        originalPanelLocalPosition = targetRectTransform.localPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            RectTransform parentRect = targetRectTransform.parent as RectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPointerPosition))
            {
                Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
                targetRectTransform.localPosition = originalPanelLocalPosition + new Vector3(offsetToOriginal.x, offsetToOriginal.y, 0f);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }
}