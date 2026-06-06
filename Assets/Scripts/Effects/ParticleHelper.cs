using UnityEngine;
using Lean.Pool;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleHelper : MonoBehaviour
{
    private ParticleSystem particle;

    public ParticleSystem Particle
    {
        get
        {
            if (this.particle == null)
                this.particle = GetComponent<ParticleSystem>();

            return this.particle;
        }
    }

    public void Play()
    {
        this.Particle.Play();

        Invoke(nameof(OnComplete), this.Particle.main.duration + this.Particle.main.startLifetime.constantMax + 0.3f);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(OnComplete));
        OnComplete();
    }

    public void SetEmissionRate(float rate)
    {
        var emission = this.Particle.emission;
        emission.rateOverTime = rate;
    }

    public void SetStartColor(Color color)
    {
        var mainModule = this.Particle.main;
        mainModule.startColor = color;
    }

    private void OnComplete()
    {
        LeanPool.Despawn(this.gameObject);
    }
}