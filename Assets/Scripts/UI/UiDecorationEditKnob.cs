using UnityEngine;
using UnityEngine.EventSystems;

public class UiDecorationEditKnob : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform editRT;
    [SerializeField] private Transform corner;
    [SerializeField] private GameObject rect;

    [Header("Scale Settings")]
    [Range(0, 1), SerializeField] private float minScale = 0.5f;
    [Range(0, 5), SerializeField] private float maxScale = 3.0f;

    private Vector3 initialSize, initialScale, initialRotation;
    private float initialDistance;
    private float initialAngle;
    private Canvas canvas;

    private void Start()
    {
        this.initialSize = this.editRT.sizeDelta;
        this.initialScale = this.editRT.localScale;
        this.initialRotation = this.editRT.localEulerAngles;

        // Get canvas for screen space calculations
        this.canvas = this.editRT.GetComponentInParent<Canvas>();

        // Calculate initial distance and angle from Icon center to Corner (represents scale 1.0 and rotation 0)
        this.initialDistance = CalculateDistanceFromIconCenter(this.corner.position);
        this.initialAngle = CalculateAngleFromIconCenter(this.corner.position);

        DragManager.OnDragModeChanged += OnDragModeChanged;
        this.transform.position = this.corner.position;

        if (this.rect)
            this.rect.SetActive(DragManager.IsDragModeActivated);
            
        this.gameObject.SetActive(DragManager.IsDragModeActivated);
    }

    private void OnDestroy()
    {
        DragManager.OnDragModeChanged -= OnDragModeChanged;
    }

    private void OnDragModeChanged(bool inIsActive)
    {
        this.gameObject.SetActive(inIsActive);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Recalculate initial distance and angle in case Icon was already transformed
        this.initialDistance = CalculateDistanceFromIconCenter(this.corner.position);
        this.initialAngle = CalculateAngleFromIconCenter(this.corner.position);

         if (this.rect)
            this.rect.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Get Icon center in screen space
        Vector2 iconCenter = RectTransformUtility.WorldToScreenPoint(this.canvas.worldCamera, this.editRT.position);

        // Calculate vector from Icon center to drag position
        Vector2 delta = eventData.position - iconCenter;
        
        // Calculate distance for scale
        float currentDistance = delta.magnitude;
        float scaleFactor = currentDistance / this.initialDistance;
        scaleFactor = Mathf.Clamp(scaleFactor, this.minScale, this.maxScale);
        
        // Calculate current angle and subtract initial angle to get rotation delta
        float currentAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        float rotationAngle = currentAngle - this.initialAngle;
        
        // Apply uniform scale
        this.editRT.localScale = Vector3.one * scaleFactor;
        
        // Apply rotation (Z-axis only for 2D)
        this.editRT.localEulerAngles = new Vector3(0, 0, rotationAngle);
        
        // Position knob at the Corner's world position (lower-left of Icon after transforms)
        this.transform.position = this.corner.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this.rect)
            this.rect.SetActive(false);
            
        //Store final scale and rotation values
        var decoration = this.editRT.GetComponentInParent<DecorationBase>();
        decoration.ItemData.DecorationData.WorldSaveData.Scale = this.editRT.localScale.x / decoration.ItemData.Scale;
        decoration.ItemData.DecorationData.WorldSaveData.Rotation = this.editRT.localEulerAngles.z;
    }

    private float CalculateDistanceFromIconCenter(Vector3 worldPosition)
    {
        Vector2 iconCenter = RectTransformUtility.WorldToScreenPoint(this.canvas.worldCamera, this.editRT.position);
        Vector2 targetScreen = RectTransformUtility.WorldToScreenPoint(this.canvas.worldCamera, worldPosition);
        
        return Vector2.Distance(iconCenter, targetScreen);
    }
    
    private float CalculateAngleFromIconCenter(Vector3 worldPosition)
    {
        Vector2 iconCenter = RectTransformUtility.WorldToScreenPoint(this.canvas.worldCamera, this.editRT.position);
        Vector2 targetScreen = RectTransformUtility.WorldToScreenPoint(this.canvas.worldCamera, worldPosition);
        
        Vector2 delta = targetScreen - iconCenter;
        return Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
    }
}