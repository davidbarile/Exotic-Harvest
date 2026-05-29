using UnityEngine;
using UnityEngine.EventSystems;

public class WorldBgDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [SerializeField] private bool enableOnMobileOnly;
    [Range(0f, 1f), SerializeField] private float dragSensitivity = 1f;
    private float startDragPosX;
    private float scrollerStartDragPosX;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (this.enableOnMobileOnly && !PlatformManager.IsMobile)
            return;
            
        this.startDragPosX = eventData.position.x;
        this.scrollerStartDragPosX = UiManager.IN.DayNightCycleController.CurrentScrollValue;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (this.enableOnMobileOnly && !PlatformManager.IsMobile)
            return;

        var deltaX = eventData.position.x - this.startDragPosX;

        if (Mathf.Abs(deltaX) < 10f)
            return;

        var normalizedDelta = (deltaX / Screen.width) * this.dragSensitivity / ScreenManager.WorldToScreenRatio;
        var clampedDelta = Mathf.Clamp01(this.scrollerStartDragPosX + normalizedDelta);
        UiManager.IN.DayNightCycleController.UserPanCamera(clampedDelta, true);
    }
}
