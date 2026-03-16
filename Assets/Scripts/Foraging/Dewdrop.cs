using UnityEngine;
using DG.Tweening;

/// <summary>
/// Dewdrop collectable - appears in morning, click to collect water
/// UI-based for desktop overlay gameplay
/// </summary>
public class Dewdrop : Collectable
{
    [Header("Dewdrop Animation")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 10f; // UI pixels
    [SerializeField] private float shimmerInterval = 3f;
    
    private Vector2 startAnchoredPosition;
    private Sequence bobSequence;
    private Sequence shimmerSequence;
    
    protected override void Start()
    {
        this.resourceType = EResourceType.Dew;
        this.amount = 1;
        this.collectionType = ECollectionMethod.Click;
        this.lifetime = 60f; // Dewdrops last longer

        this.canvasGroup.alpha = 1f;
        
        base.Start();
        
        this.startAnchoredPosition = this.rectTransform.anchoredPosition;
        StartBobAnimation();
        StartShimmerAnimation();
    }
    
    private void StartBobAnimation()
    {
        this.bobSequence = DOTween.Sequence()
            .Append(this.rectTransform.DOAnchorPosY(this.startAnchoredPosition.y + this.bobAmount, 1f / this.bobSpeed).SetEase(Ease.InOutSine))
            .Append(this.rectTransform.DOAnchorPosY(this.startAnchoredPosition.y - this.bobAmount, 1f / this.bobSpeed).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    private void StartShimmerAnimation()
    {
        this.shimmerSequence = DOTween.Sequence()
            .Append(this.collectableImage.DOFade(0.7f, this.shimmerInterval * 0.5f))
            .Append(this.collectableImage.DOFade(1f, this.shimmerInterval * 0.5f))
            .SetLoops(-1);
    }
    
    protected override void OnCollected()
    {
        // Stop animations
        this.bobSequence?.Kill();
        this.shimmerSequence?.Kill();
        
        // Collection animation
        var sequence = DOTween.Sequence()
            .Append(this.rectTransform.DOScale(1.2f, 0.1f))
            .Join(this.canvasGroup.DOFade(0f, 0.2f))
            .Append(this.rectTransform.DOScale(0f, 0.1f))
            .OnComplete(() => base.OnCollected());
    }
    
    protected override void OnDestroy()
    {
        this.bobSequence?.Kill();
        this.shimmerSequence?.Kill();

        base.OnDestroy();
    }
}