using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class OnFocusModifierBase : MonoBehaviour
{
    private enum EListernerType
    {
        OnGameFocusChanged,
        OnMinimizeMaximizeToggled,
        Both
    }

    [SerializeField] private EListernerType eventListenerType;

    [SerializeField] private bool shouldAppendObjectName;
    private string originalName;

    protected virtual void Start()
    {
        this.originalName = this.gameObject.name;

        switch (this.eventListenerType)
        {
            case EListernerType.OnGameFocusChanged:
                ScreenManager.OnGameFocusChanged += OnGameFocusChanged;
                break;
            case EListernerType.OnMinimizeMaximizeToggled:
                ScreenManager.OnMinimizeMaximizeToggled += OnGameFocusChanged;
                break;
            case EListernerType.Both:
                ScreenManager.OnGameFocusChanged += OnGameFocusChanged;
                ScreenManager.OnMinimizeMaximizeToggled += OnGameFocusChanged;
                break;
        }
    }

    protected virtual void OnDestroy()
    {
        ScreenManager.OnGameFocusChanged -= OnGameFocusChanged;
        ScreenManager.OnMinimizeMaximizeToggled -= OnGameFocusChanged;
    }

    protected virtual void OnGameFocusChanged(bool hasFocus)
    {
        if(this.shouldAppendObjectName)
        {
            if (hasFocus)
                this.name = this.originalName;
            else
                this.name = $"{this.originalName}_IgnoreAnim";
        }
    }
}