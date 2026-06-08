using UnityEngine;
using DG.Tweening;
using Lean.Pool;

public class Stardust : Collectable
{
    [Range(0, 5), SerializeField] private float tweenDuration = 1f;
    
    [SerializeField] private ParticleSystem[] starParticles;
    private float initAlpha = -1f;

    private Tween fadeTween;

    public override void Spawn()
    {
        base.Spawn();

        if (this.initAlpha < 0f)
            this.initAlpha = this.canvasGroup.alpha;
            
        this.gameObject.SetActive(true);

        this.canvasGroup.alpha = 0;

        this.fadeTween?.Kill();
        this.fadeTween = this.canvasGroup.DOFade(this.initAlpha, this.tweenDuration).SetEase(Ease.InOutSine);
        foreach (var particle in this.starParticles)
        {
            particle.Play();
        }
    }

    public override void Expire()
    {
        this.fadeTween?.Kill();
        this.canvasGroup.DOFade(0, this.tweenDuration).SetEase(Ease.InOutSine).OnComplete(() => LeanPool.Despawn(this.gameObject));
        foreach (var particle in this.starParticles)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public override void Collect(bool inShouldAddResourceImmediately = true)
    {
        if (!CanBeCollected())
            return;

        if (inShouldAddResourceImmediately)
            ResourceManager.IN.AddResource(this.ResourceType, this.Amount);

        this.isCollected = true;
        OnCollected();
    }

    protected override void OnCollected()
    {
        Expire();
    }
    
    public override void OnAttracted()
    {
        this.fadeTween?.Kill();
        this.canvasGroup.DOFade(0, this.tweenDuration).SetEase(Ease.InOutSine).OnComplete(() => LeanPool.Despawn(this.gameObject));
    }
}