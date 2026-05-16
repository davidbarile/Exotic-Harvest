using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DragModeMaterialSwapper : MonoBehaviour
{
    [SerializeField] private Color outlineColor = Color.white;

    private Image image;
    private Material originalMaterial;

    private void Start()
    {
        this.image = GetComponent<Image>();
        this.originalMaterial = this.image.material;
        SetDragModeMaterial(false);

        DragManager.OnDragModeChanged += SetDragModeMaterial;
        SetDragModeMaterial(DragManager.IsDragModeActivated);
    }

    private void OnDestroy()
    {
        DragManager.OnDragModeChanged -= SetDragModeMaterial;
    }

    private void SetDragModeMaterial(bool isDragModeEnabled)
    {
        this.image.material = isDragModeEnabled ? DragManager.IN.DragEnabledMaterial : this.originalMaterial;

        if(isDragModeEnabled)
            this.image.material.SetColor("_OutlineColor", this.outlineColor);
    }
}