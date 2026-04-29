using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class OnFocusModifierBase : MonoBehaviour
{
    [SerializeField] private bool shouldAppendObjectName;
    private string originalName;

    protected virtual void Start()
    {
        this.originalName = this.gameObject.name;
        ScreenManager.OnGameFocusChanged += OnGameFocusChanged;
    }

    protected virtual void OnDestroy()
    {
        ScreenManager.OnGameFocusChanged -= OnGameFocusChanged;
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