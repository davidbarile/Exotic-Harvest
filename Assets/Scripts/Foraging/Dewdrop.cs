using UnityEngine;
using DG.Tweening;

/// <summary>
/// Dewdrop collectable - appears in morning, click to collect water
/// UI-based for desktop overlay gameplay
/// </summary>
public class Dewdrop : Collectable
{
    public override void Spawn()
    {
        this.resourceType = EResourceType.Dew;
        this.amount = 1;
        this.collectionType = ECollectionMethod.Click;
        this.lifetime = 60f; // Dewdrops last longer

        this.canvasGroup.alpha = 1f;
        
        base.Spawn();
    }
    
    protected override void OnCollected()
    {
        // Stop animations
        
        // Collection animation
        var sequence = DOTween.Sequence()
            .Append(this.transform.DOScale(1.2f, 0.1f))
            .Join(this.canvasGroup.DOFade(0f, 0.2f))
            .Append(this.transform.DOScale(0f, 0.1f))
            .OnComplete(() => base.OnCollected());
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}