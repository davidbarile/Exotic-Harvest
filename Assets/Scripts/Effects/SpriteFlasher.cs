using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Graphic))]
public class SpriteFlasher : MonoBehaviour
{
    [SerializeField] private float flashDuration = 1f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private AnimationCurve animationCurve;
    private Graphic graphic;
    private Material materialInstance;

    private void Awake()
    {
        this.graphic = GetComponent<Graphic>();
    }

    public void Flash()
    {
        if (this.materialInstance == null)
        {
            this.materialInstance = new Material(this.graphic.material);
            this.graphic.material = this.materialInstance;
        }
            
        this.graphic.material.SetColor("_FlashColor", this.flashColor);

        DOVirtual.Float(0, 1f, this.flashDuration, value =>
        {
            var flashIntensity = this.animationCurve.Evaluate(value);
            this.graphic.material.SetFloat("_FlashAmount", flashIntensity);
        });
    }

    private void OnDestroy()
    {
        if (this.materialInstance == null)
            DestroyImmediate(this.materialInstance);
    }
}