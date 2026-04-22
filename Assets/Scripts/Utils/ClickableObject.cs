using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

public class ClickableObject : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent OnLeftClick;
    public UnityEvent OnMiddleClick;
    public UnityEvent OnRightClick;

    [Space]
    public UnityEvent OnPointerDownEvent;
    public UnityEvent OnPointerUpEvent;

    [Space]
    public UnityEvent OnLongPressEvent;
    public UnityEvent OnDoubleClickEvent;

    [Space, Range(0, 3f), SerializeField] private float longPressThreshold = 1f;
    [Space, Range(0, .5f), SerializeField] private float doubleClickThreshold = 0.2f;

    private DateTime lastClickTime = DateTime.MinValue;
    private Vector3 pointerDownPosition;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            OnLeftClick?.Invoke();
        else if (eventData.button == PointerEventData.InputButton.Middle)
            OnMiddleClick?.Invoke();
        else if (eventData.button == PointerEventData.InputButton.Right)
            OnRightClick?.Invoke();

        if(DateTime.Now - this.lastClickTime < TimeSpan.FromSeconds(this.doubleClickThreshold))
        {
            this.lastClickTime = DateTime.MinValue;
            OnDoubleClickEvent?.Invoke();
        }
        else
        {
            this.lastClickTime = DateTime.Now;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CancelInvoke();
        Invoke(nameof(HandleLongPress), this.longPressThreshold);

        this.pointerDownPosition = eventData.position;

        this.OnPointerDownEvent?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelInvoke();
        this.OnPointerUpEvent?.Invoke();//if long press, cancel this
    }

    public void HandleLongPress()
    {
        if (Vector3.Distance(this.pointerDownPosition, Input.mousePosition) > 10f)
            return;

        OnLongPressEvent?.Invoke();
    }
}