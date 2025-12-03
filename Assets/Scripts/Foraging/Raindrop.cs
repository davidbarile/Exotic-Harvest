using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

/// <summary>
/// Raindrop collectable - appears during rain, collect by dragging bucket
/// UI-based for desktop overlay gameplay
/// </summary>
public class Raindrop : Collectable
{
    [Header("Raindrop Animation")]
    [SerializeField] private float fallDuration = 10f; // Time to fall across screen
    [SerializeField] private float sideWave = 20f; // Horizontal movement
    
    private bool isFalling = true;
    private Tweener fallTween;
    private Tweener waveTween;
    
    protected override void Start()
    {
        resourceType = ResourceType.Water;
        amount = 1;
        collectionMethod = CollectionMethod.Hover;
        lifetime = 10f; // Raindrops fall quickly
        autoDestroy = false; // Will destroy when hitting ground
        
        base.Start();
        StartFallingAnimation();
    }
    
    private void StartFallingAnimation()
    {
        if (rectTransform != null && parentCanvas != null)
        {
            // Get canvas bounds for ground detection
            RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
            float groundY = -canvasRect.rect.height * 0.5f - 50f; // Below canvas
            
            Vector2 startPos = rectTransform.anchoredPosition;
            
            // Falling animation
            fallTween = rectTransform.DOAnchorPosY(groundY, fallDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(HitGround);
            
            // Subtle horizontal wave motion
            waveTween = rectTransform.DOAnchorPosX(startPos.x + sideWave, fallDuration * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
    
    private void HitGround()
    {
        if (!isCollected)
        {
            isFalling = false;
            
            // Splash effect animation
            if (collectableImage != null)
            {
                var splashSequence = DOTween.Sequence()
                    .Append(rectTransform.DOScale(1.3f, 0.1f))
                    .Join(collectableImage.DOFade(0f, 0.2f))
                    .OnComplete(() => Destroy(gameObject));
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (isFalling) // Can only collect while falling
            base.OnPointerEnter(eventData);
    }
    
    protected override void OnCollected()
    {
        isFalling = false;
        
        // Stop falling animations
        fallTween?.Kill();
        waveTween?.Kill();
        
        // Collection effect
        if (rectTransform != null && collectableImage != null)
        {
            var collectSequence = DOTween.Sequence()
                .Append(rectTransform.DOScale(0.8f, 0.1f))
                .Join(collectableImage.DOFade(0.3f, 0.15f))
                .Append(rectTransform.DOScale(0f, 0.05f))
                .OnComplete(() => Destroy(gameObject));
        }
        
        base.OnCollected();
    }
    
    protected override void OnDestroy()
    {
        fallTween?.Kill();
        waveTween?.Kill();
        
        base.OnDestroy();
    }
}