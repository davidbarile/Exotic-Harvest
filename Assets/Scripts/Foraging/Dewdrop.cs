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
        resourceType = ResourceType.Water;
        amount = 1;
        collectionMethod = CollectionMethod.Click;
        lifetime = 60f; // Dewdrops last longer
        
        base.Start();
        
        if (rectTransform != null)
        {
            startAnchoredPosition = rectTransform.anchoredPosition;
            StartBobAnimation();
            StartShimmerAnimation();
        }
    }
    
    private void StartBobAnimation()
    {
        bobSequence = DOTween.Sequence()
            .Append(rectTransform.DOAnchorPosY(startAnchoredPosition.y + bobAmount, 1f / bobSpeed).SetEase(Ease.InOutSine))
            .Append(rectTransform.DOAnchorPosY(startAnchoredPosition.y - bobAmount, 1f / bobSpeed).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    private void StartShimmerAnimation()
    {
        if (collectableImage != null)
        {
            shimmerSequence = DOTween.Sequence()
                .Append(collectableImage.DOFade(0.7f, shimmerInterval * 0.5f))
                .Append(collectableImage.DOFade(1f, shimmerInterval * 0.5f))
                .SetLoops(-1);
        }
    }
    
    protected override void OnCollected()
    {
        // Stop animations
        bobSequence?.Kill();
        shimmerSequence?.Kill();
        
        // Collection animation
        if (rectTransform != null && collectableImage != null)
        {
            var sequence = DOTween.Sequence()
                .Append(rectTransform.DOScale(1.2f, 0.1f))
                .Join(collectableImage.DOFade(0f, 0.2f))
                .Append(rectTransform.DOScale(0f, 0.1f));
        }
        
        base.OnCollected();
    }
    
    protected override void OnDestroy()
    {
        bobSequence?.Kill();
        shimmerSequence?.Kill();

        base.OnDestroy();
    }
}