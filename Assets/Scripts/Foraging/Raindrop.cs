using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

/// <summary>
/// Raindrop collectable - appears during rain, collect by dragging bucket
/// UI-based for desktop overlay gameplay
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Raindrop : Collectable
{
    [Header("Raindrop Animation")]
    [SerializeField] private float fallDuration = 5f; // Time to fall across screen
    [SerializeField] private float sideWave = 20f; // Horizontal movement

    private bool isFalling = true;
    private Tweener fallTween;
    private Tweener waveTween;
    
    public override void Spawn()
    {
        this.amount = 1;

        base.Spawn();
        StartFallingAnimation();
    }
    
    private void StartFallingAnimation()
    {
        // Get canvas bounds for ground detection;
        float groundY = -ForagingManager.IN.RainParent.rect.height * 0.5f; // Below canvas
        
        Vector2 startPos = this.transform.localPosition;

        // Falling animation
        this.fallTween = this.transform.DOLocalMoveY(groundY, this.fallDuration)
            .SetEase(Ease.Linear)
            .OnComplete(HitGround);
        
        // Subtle horizontal wave motion
        this.waveTween = this.transform.DOLocalMoveX(startPos.x + this.sideWave, this.fallDuration * 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    private void HitGround()
    {
        if (!this.isCollected)
        {
            this.isFalling = false;

            this.fallTween?.Kill();
            this.waveTween?.Kill();
            
            // Splash effect animation
            if (this.collectableImage != null)
            {
                var splashSequence = DOTween.Sequence()
                    .Append(this.transform.DOScale(2f, 0.15f))
                    .Join(this.collectableImage.DOFade(0f, 0.15f))
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
        var sequence = DOTween.Sequence()
            .Append(this.transform.DOScale(1.2f, 0.1f))
            .Join(this.canvasGroup.DOFade(0f, 0.05f))
            .Append(this.transform.DOScale(0f, 0.05f))
            .OnComplete(() => base.OnCollected());
    }

    protected override void OnDestroy()
    {
        this.fallTween?.Kill();
        this.waveTween?.Kill();

        base.OnDestroy();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") && this.isFalling)
        {
            HitGround();
        }
    }
}