using UnityEngine;
using DG.Tweening;

/// <summary>
/// Dewdrop collectable - appears in morning, click to collect water
/// UI-based for desktop overlay gameplay
/// </summary>
public class Dewdrop : Collectable
{
    public override int Amount
    {
        get
        {
            var amountBySize = this.amount * this.transform.localScale.x * 4f;
            return Mathf.CeilToInt(amountBySize);
        }
    }

    public override void Spawn()
    {
        //this.resourceType = EResourceType.Dew;
        //this.collectionType = ECollectionMethod.Click;
        this.amount = 1;

        this.canvasGroup.alpha = 0f;

        this.initScale = Random.Range(0.5f, 1f);
        this.transform.localScale = 0.1f * this.initScale * Vector3.one;

        KillTweens();

        //start slow grow/shrink animation
        var sequence = DOTween.Sequence()
            .Append(this.transform.DOScale(this.initScale, this.lifetime * 0.5f))
            .Join(this.canvasGroup.DOFade(1f, 5f))
            .Append(this.transform.DOScale(this.initScale * 0.1f, this.lifetime * 0.5f))
            .Append(this.canvasGroup.DOFade(0f, 5f))
            .OnComplete(() => base.OnCollected());

        base.Spawn();
    }

    protected override void OnCollected()
    {
        // Stop animations
        KillTweens();

        // Collection animation
        var sequence = DOTween.Sequence()
            .Append(this.transform.DOScale(this.initScale * 1.2f, 0.1f))
            .Join(this.canvasGroup.DOFade(0f, 0.2f))
            .Append(this.transform.DOScale(.1f, 0.1f))
            .OnComplete(() => base.OnCollected());
    }
    
    private void KillTweens()
    {
        DOTween.Kill(this.transform);
        DOTween.Kill(this.canvasGroup);
    }
    
    protected override void OnDestroy()
    {
        KillTweens();
        base.OnDestroy();
    }
}