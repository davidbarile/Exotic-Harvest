using UnityEngine;
using Lean.Pool;
using Sirenix.OdinInspector;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleHelper : MonoBehaviour
{
    [SerializeField] private bool shouldDespawnOnComplete;
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

    [ShowInInspector, ReadOnly] public bool IsPlaying { get; private set; }

    public void Play()
    {
        this.Particle.Play();

        this.IsPlaying = true;

        Invoke(nameof(OnComplete), this.Particle.main.duration + this.Particle.main.startLifetime.constantMax + 0.3f);
    }

    public void Stop()
    {
        this.Particle.Stop();

        this.IsPlaying = false;
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

    public void SetStartColor(Color inColor)
    {
        var mainModule = this.Particle.main;
        mainModule.startColor = inColor;
    }

    public void SetStartColors(Color inColor1, Color inColor2)
    {
        var mainModule = this.Particle.main;
        mainModule.startColor = new ParticleSystem.MinMaxGradient(inColor1, inColor2);
    }

    public void SetSpeed(float inValue)
    {
        var mainModule = this.Particle.main;
        mainModule.startSpeed = inValue;
    }

    public void SetSpeed(float inMinValue, float inMaxValue)
    {
        var mainModule = this.Particle.main;
        mainModule.startSpeed = new ParticleSystem.MinMaxCurve(inMinValue, inMaxValue);
    }

    private void OnComplete()
    {
        if(this.shouldDespawnOnComplete)
            LeanPool.Despawn(this.gameObject);
    }
}