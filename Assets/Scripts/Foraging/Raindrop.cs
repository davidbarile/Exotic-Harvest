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
        this.resourceType = EResourceType.Rain;
        this.amount = 1;
        this.collectionType = ECollectionMethod.Hover;
        this.lifetime = 10f; // Raindrops fall quickly
        this.autoDestroy = false; // Will destroy when hitting ground
        
        base.Start();
        StartFallingAnimation();
    }
    
    private void StartFallingAnimation()
    {
        if (this.rectTransform != null && this.parentCanvas != null)
        {
            // Get canvas bounds for ground detection;
            float groundY = -ForagingManager.IN.RainParent.rect.height * 0.5f - 50f; // Below canvas
            
            Vector2 startPos = this.rectTransform.anchoredPosition;
            
            // Falling animation
            this.fallTween = this.rectTransform.DOAnchorPosY(groundY, this.fallDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(HitGround);
            
            // Subtle horizontal wave motion
            this.waveTween = this.rectTransform.DOAnchorPosX(startPos.x + this.sideWave, this.fallDuration * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
    
    private void HitGround()
    {
        if (!this.isCollected)
        {
            this.isFalling = false;
            
            // Splash effect animation
            if (this.collectableImage != null)
            {
                var splashSequence = DOTween.Sequence()
                    .Append(this.rectTransform.DOScale(1.3f, 0.1f))
                    .Join(this.collectableImage.DOFade(0f, 0.2f))
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
        if (this.isFalling) // Can only collect while falling
            base.OnPointerEnter(eventData);
    }
    
    protected override void OnCollected()
    {
        this.isFalling = false;
        
        // Stop falling animations
        this.fallTween?.Kill();
        this.waveTween?.Kill();
        
        // Collection effect
        if (this.rectTransform != null && this.collectableImage != null)
        {
            var collectSequence = DOTween.Sequence()
                .Append(this.rectTransform.DOScale(0.8f, 0.1f))
                .Join(this.collectableImage.DOFade(0.3f, 0.15f))
                .Append(this.rectTransform.DOScale(0f, 0.05f))
                .OnComplete(() => Destroy(gameObject));
        }
        
        base.OnCollected();
    }
    
    protected override void OnDestroy()
    {
        this.fallTween?.Kill();
        this.waveTween?.Kill();
        
        base.OnDestroy();
    }
}