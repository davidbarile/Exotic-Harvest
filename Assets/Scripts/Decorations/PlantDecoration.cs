using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Plant decoration - purely visual, no resource generation (Phase 1)
/// UI-based for desktop overlay gameplay
/// </summary>
public class PlantDecoration : DecorationBase
{
    [Header("Plant UI Animation")]
    [SerializeField] private float swayAmount = 5f; // UI rotation degrees
    [SerializeField] private float swayDuration = 2f;
    [SerializeField] private RectTransform[] swayingParts; // UI elements that sway
    [SerializeField] private Image plantImage;
    
    private Sequence swaySequence;
    
    protected override void Start()
    {
        this.decorationType = DecorationType.Plant;
        this.decorationName = "Jungle Plant";
        
        if (this.plantImage == null)
            this.plantImage = GetComponent<Image>();
            
        base.Start();
        StartSwayAnimation();
    }
    
    private void StartSwayAnimation()
    {
        // Create gentle swaying motion for the main plant
        if (GetComponent<RectTransform>() != null)
        {
            this.swaySequence = DOTween.Sequence()
                .Append(transform.DORotate(new Vector3(0, 0, this.swayAmount), this.swayDuration).SetEase(Ease.InOutSine))
                .Append(transform.DORotate(new Vector3(0, 0, -this.swayAmount), this.swayDuration).SetEase(Ease.InOutSine))
                .SetLoops(-1, LoopType.Yoyo);
        }
        
        // Animate individual swaying parts if available
        if (this.swayingParts != null)
        {
            for (int i = 0; i < this.swayingParts.Length; i++)
            {
                if (this.swayingParts[i] != null)
                {
                    float delay = i * 0.2f; // Offset each part slightly
                    float partSwayAmount = this.swayAmount * (0.5f + UnityEngine.Random.value * 0.5f);
                    
                    DOTween.Sequence()
                        .SetDelay(delay)
                        .Append(this.swayingParts[i].DORotate(new Vector3(0, 0, partSwayAmount), this.swayDuration).SetEase(Ease.InOutSine))
                        .Append(this.swayingParts[i].DORotate(new Vector3(0, 0, -partSwayAmount), this.swayDuration).SetEase(Ease.InOutSine))
                        .SetLoops(-1, LoopType.Yoyo);
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        this.swaySequence?.Kill();
    }
    
    protected override void OnPlaced()
    {
        // TODO: Add placement effects
    }
}