using UnityEngine;
using UnityEngine.UI;
using static GlobalEnums;

[RequireComponent(typeof(Image))]
public class MaterialSwapper : MonoBehaviour
{
    [SerializeField] private EListernerType eventListenerType;
    
    private Image image;

    private void Start()
    {
        this.image = GetComponent<Image>();

        if(TryGetComponent<OnFocusShow>(out var onFocusShow))
            this.eventListenerType = onFocusShow.EventListenerType;

        switch (this.eventListenerType)
        {
            case EListernerType.OnGameFocusChanged:
                ScreenManager.OnGameFocusChanged += OnMinimizeMaximizeToggled;
                break;
            case EListernerType.OnMinimizeMaximizeToggled:
                ScreenManager.OnMinimizeMaximizeToggled += OnMinimizeMaximizeToggled;
                break;
            case EListernerType.Both:
                ScreenManager.OnGameFocusChanged += OnMinimizeMaximizeToggled;
                ScreenManager.OnMinimizeMaximizeToggled += OnMinimizeMaximizeToggled;
                break;
        }
    }

    private void OnDestroy()
    {
        ScreenManager.OnGameFocusChanged -= OnMinimizeMaximizeToggled;
        ScreenManager.OnMinimizeMaximizeToggled -= OnMinimizeMaximizeToggled;
    }

    private void OnMinimizeMaximizeToggled(bool inIsMaximized)
    {
        if (ScreenManager.IN == null)
        {
            Debug.Log($"[{this.name}] ScreenManager instance is null. Cannot swap materials.", this.gameObject);
            return;
        }

        if (inIsMaximized)
        {
            // Swap to the original material
            this.image.material = ScreenManager.IN.LitShaderMaterial;
        }
        else
        {
            // Swap to the minimized material
            this.image.material = ScreenManager.IN.DefaultSpriteMaterial;
        }
    }
}