using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UiDecorationEditKnob : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private enum ECorner
    {
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft
    }

    [SerializeField] private ECorner cornerType;
    [Space, SerializeField] private DecorationBase decoration;
    [SerializeField] private RectTransform editRT;
    [SerializeField] private GameObject rect;

    [Header("Scale Settings")]
    [Range(0, 1), SerializeField] private float minScale = 0.5f;
    [Range(0, 5), SerializeField] private float maxScale = 3.0f;

    [Header("Sister Knobs (Optional)")]
    [SerializeField] private UiDecorationEditKnob[] sisterKnobs;

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
        this.initialDistance = CalculateDistanceFromIconCenter(GetCornerPosition());
        this.initialAngle = CalculateAngleFromIconCenter(GetCornerPosition());

        DragManager.OnDragModeChanged += OnDragModeChanged;
        this.transform.position = GetCornerPosition();

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
        this.initialDistance = CalculateDistanceFromIconCenter(GetCornerPosition());
        this.initialAngle = CalculateAngleFromIconCenter(GetCornerPosition());

         if (this.rect)
            this.rect.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Get Icon center in screen space
        Vector2 iconCenter = RectTransformUtility.WorldToScreenPoint(this.canvas.worldCamera, this.editRT.position);

        // Calculate vector from Icon center to cursor position
        Vector2 delta = eventData.position - iconCenter;
        float dragDistance = delta.magnitude;
        
        // Calculate desired scale factor from cursor distance
        float desiredScale = dragDistance / this.initialDistance;
        float scaleFactor = Mathf.Clamp(desiredScale, this.minScale, this.maxScale);
        
        // Apply the scale factor to the initial local scale
        // The initialDistance measurement includes all parent scales, so the ratio accounts for them
        float localScaleFactor = this.initialScale.x * scaleFactor;
        
        // Calculate angle from cursor position
        float currentAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        float rotationAngle = currentAngle - this.initialAngle;
        
        // Apply uniform scale and rotation to Icon
        // This will move the Corner to the correct position
        this.editRT.localScale = Vector3.one * localScaleFactor;
        this.editRT.localEulerAngles = new Vector3(0, 0, rotationAngle);
        
        // Force transform hierarchy update so Corner position is current
        Canvas.ForceUpdateCanvases();

        // Now position the knob at the Corner's world position
        // The Corner has moved to where it should be based on the scale/rotation
        this.transform.position = GetCornerPosition();
        
        foreach (var sister in this.sisterKnobs)
        {
            if (sister != null)
                sister.transform.position = sister.GetCornerPosition();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this.rect)
            this.rect.SetActive(false);
            
        //Store final scale and rotation values
        this.decoration.ItemData.DecorationData.WorldSaveData.Scale = this.editRT.localScale.x;
        this.decoration.ItemData.DecorationData.WorldSaveData.Rotation = this.editRT.localEulerAngles.z;
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
    
    public Vector3 GetCornerPosition()
    {
        Vector3 localPos = Vector3.zero;
        switch (this.cornerType)
        {
            case ECorner.TopRight:
                localPos = new Vector3(this.initialSize.x / 2, this.initialSize.y / 2, 0);
                break;
            case ECorner.TopLeft:
                localPos = new Vector3(-this.initialSize.x / 2, this.initialSize.y / 2, 0);
                break;
            case ECorner.BottomRight:
                localPos = new Vector3(this.initialSize.x / 2, -this.initialSize.y / 2, 0);
                break;
            case ECorner.BottomLeft:
                localPos = new Vector3(-this.initialSize.x / 2, -this.initialSize.y / 2, 0);
                break;
        }
        return this.editRT.TransformPoint(localPos);
    }
}