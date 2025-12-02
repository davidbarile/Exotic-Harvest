using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// UI-based draggable component for decorations in the desktop overlay
/// </summary>
public class UIDraggableDecoration : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Drag Settings")]
    [SerializeField] private bool isDraggable = true;
    [SerializeField] private bool returnToOriginalPosition = false;
    [SerializeField] private float snapBackDuration = 0.3f;
    [SerializeField] private Canvas dragCanvas; // Canvas for dragging (should be overlay)
    
    [Header("Visual Feedback")]
    [SerializeField] private float dragScale = 1.1f;
    [SerializeField] private float dragAlpha = 0.8f;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Vector2 lastValidPosition;
    private bool isDragging = false;
    private Transform originalParent;
    private int originalSiblingIndex;
    private DecorationBase decoration;
    
    // Events
    public System.Action<UIDraggableDecoration> OnDragStarted;
    public System.Action<UIDraggableDecoration> OnDragEnded;
    public System.Action<UIDraggableDecoration, Vector2> OnPositionChanged;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        decoration = GetComponent<DecorationBase>();
        
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        if (dragCanvas == null)
            dragCanvas = GetComponentInParent<Canvas>();
            
        originalPosition = rectTransform.anchoredPosition;
        lastValidPosition = originalPosition;
    }
    
    private void Start()
    {
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Check if we're in decoration drag mode
        if (DragManager.IsDragModeActivated)
        {
            // Allow dragging in drag mode, but also handle clicks for interaction
            if (decoration != null && !isDragging)
            {
                // Interact with decoration (collect resources, etc.)
                Debug.Log($"Interacting with decoration: {decoration.name}");
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable || !CanDrag())
            return;
            
        isDragging = true;
        originalPosition = rectTransform.anchoredPosition;
        lastValidPosition = originalPosition;
        
        // Visual feedback
        transform.DOScale(dragScale, 0.1f);
        canvasGroup.alpha = dragAlpha;
        
        // Move to drag canvas for proper layering
        if (dragCanvas != null && dragCanvas != GetComponentInParent<Canvas>())
        {
            transform.SetParent(dragCanvas.transform);
        }
        
        // Bring to front
        transform.SetAsLastSibling();
        
        OnDragStarted?.Invoke(this);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;
            
        if (rectTransform != null)
        {
            Vector2 newPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out newPosition
            );
            
            // Constrain to canvas bounds
            Vector2 constrainedPosition = ConstrainToCanvas(newPosition);
            rectTransform.anchoredPosition = constrainedPosition;
            
            // Check if position is valid for decoration placement
            if (IsValidPosition(constrainedPosition))
            {
                lastValidPosition = constrainedPosition;
                // Visual feedback for valid position
                canvasGroup.alpha = dragAlpha;
            }
            else
            {
                // Visual feedback for invalid position
                canvasGroup.alpha = dragAlpha * 0.5f;
            }
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;
            
        isDragging = false;
        
        // Return to original parent
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
        }
        
        // Determine final position
        Vector2 finalPosition;
        if (IsValidPosition(rectTransform.anchoredPosition))
        {
            finalPosition = rectTransform.anchoredPosition;
        }
        else if (returnToOriginalPosition)
        {
            finalPosition = originalPosition;
        }
        else
        {
            finalPosition = lastValidPosition;
        }
        
        // Animate to final position
        var sequence = DOTween.Sequence()
            .Append(rectTransform.DOAnchorPos(finalPosition, snapBackDuration).SetEase(Ease.OutBack))
            .Join(transform.DOScale(1f, snapBackDuration))
            .Join(canvasGroup.DOFade(1f, snapBackDuration))
            .OnComplete(() => {
                OnPositionChanged?.Invoke(this, finalPosition);
                
                // Update decoration position if component exists
                if (decoration != null)
                {
                    // Position updated via RectTransform automatically
                    Debug.Log($"Decoration {decoration.name} moved to {finalPosition}");
                }
            });
            
        OnDragEnded?.Invoke(this);
    }
    
    private bool CanDrag()
    {
        // Only allow dragging when in drag mode
        return DragManager.IsDragModeActivated;
    }
    
    private Vector2 ConstrainToCanvas(Vector2 position)
    {
        if (dragCanvas == null)
            return position;
            
        RectTransform canvasRect = dragCanvas.GetComponent<RectTransform>();
        if (canvasRect == null)
            return position;
            
        Rect canvasBounds = canvasRect.rect;
        
        // Add some padding to keep decorations fully visible
        float padding = 50f;
        
        position.x = Mathf.Clamp(position.x, 
            canvasBounds.xMin + padding, 
            canvasBounds.xMax - padding);
        position.y = Mathf.Clamp(position.y, 
            canvasBounds.yMin + padding, 
            canvasBounds.yMax - padding);
            
        return position;
    }
    
    private bool IsValidPosition(Vector2 position)
    {
        // Use DecorationManager to validate position
        if (DecorationManager.IN != null)
        {
            // This would need to be exposed from DecorationManager
            // For now, just check canvas bounds
            return true;
        }
        
        return true;
    }
    
    // Public methods for external control
    public void SetDraggable(bool draggable)
    {
        isDraggable = draggable;
    }
    
    public void SetPosition(Vector2 position)
    {
        rectTransform.anchoredPosition = position;
        originalPosition = position;
        lastValidPosition = position;
    }
    
    public Vector2 GetPosition()
    {
        return rectTransform.anchoredPosition;
    }
    
    // Animation helpers
    public void AnimateToPosition(Vector2 targetPosition, float duration = 0.5f)
    {
        rectTransform.DOAnchorPos(targetPosition, duration).SetEase(Ease.OutQuad);
    }
    
    public void PulseScale(float scale = 1.2f, float duration = 0.3f)
    {
        transform.DOPunchScale(Vector3.one * (scale - 1f), duration, 1, 0.5f);
    }
}