using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class EditModeMaterialSwapper : MonoBehaviour
{
    [SerializeField] private Color outlineColor = Color.white;

    private Image image;
    private Material originalMaterial;

    private void Start()
    {
        this.image = GetComponent<Image>();
        this.originalMaterial = this.image.material;
        SetEditModeMaterial(false);

        DragManager.OnEditModeChanged += SetEditModeMaterial;
        SetEditModeMaterial(DragManager.IsEditModeActivated);
    }

    private void OnDestroy()
    {
        DragManager.OnEditModeChanged -= SetEditModeMaterial;
    }

    private void SetEditModeMaterial(bool inIsEditModeEnabled)
    {
        this.image.material = inIsEditModeEnabled ? DragManager.IN.DragEnabledMaterial : this.originalMaterial;

        if(inIsEditModeEnabled)
            this.image.material.SetColor("_OutlineColor", this.outlineColor);
    }
}