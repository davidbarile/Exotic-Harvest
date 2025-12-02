using System;
using UnityEngine;

public class DragManager : MonoBehaviour
{
    public static DragManager IN;

    public static bool IsDragModeActivated = false;

    public static Action<bool> OnDragModeChanged;

    public RectTransform DragCanvas;

    public Transform DefaultParent;

     private void Start()
    {
        InputManager.OnDragPress += HandleDragModeChanged;
    }

    private void OnDestroy()
    {
        InputManager.OnDragPress -= HandleDragModeChanged;
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
}
