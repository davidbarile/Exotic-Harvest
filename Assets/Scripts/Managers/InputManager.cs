using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public static InputManager IN;

    public static bool IsInputBlocked;

    public static List<GameObject> ObjectsUnderMouse = new();

    public static Action OnEscapePress;
    public static Action OnTabPress;
    public static Action OnSpacePress;
    public static Action OnDragPress;
    public static Action OnMPress;
    public static Action OnSettingsPress;
    public static Action OnShopPress;
    public static Action OnF1Press;
    public static Action OnF2Press;
    public static Action OnF3Press;

    public bool IsShiftPressed => this.isShiftPressed;
    private bool isShiftPressed;

    private bool isMouseOverUiThisFrame;

    public void Update()
    {
        if (IsInputBlocked) return;

        this.isMouseOverUiThisFrame = IsMouseOverUI();

        if (Input.GetKeyDown(KeyCode.Tab)) OnTabPress?.Invoke();
        if (Input.GetKeyDown(KeyCode.Escape)) OnEscapePress?.Invoke();
        if (Input.GetKeyDown(KeyCode.Space)) OnSpacePress?.Invoke();
        if (Input.GetKeyDown(KeyCode.D)) OnDragPress?.Invoke();
        if (Input.GetKeyDown(KeyCode.F1)) OnF1Press?.Invoke();
        if (Input.GetKeyDown(KeyCode.F2)) OnF2Press?.Invoke();
        if (Input.GetKeyDown(KeyCode.F3)) OnF3Press?.Invoke();
        if (Input.GetKeyDown(KeyCode.M)) OnMPress?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnSettingsPress?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha2)) OnShopPress?.Invoke();
    }
    
    public void SecondTick()
    {
        this.isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private bool IsMouseOverUI()
    {
#if !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBGL
        return false;
#endif

        var eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };

        var results = new List<RaycastResult>();

        if (EventSystem.current)
        {
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            ObjectsUnderMouse.Clear();

            foreach (var result in results)
            {
                ObjectsUnderMouse.Add(result.gameObject);
            }

            foreach (var result in results)
            {
                if (result.gameObject.layer == 5)
                    return true;
            }
        }

        return false;
    }
}