using UnityEngine;

public class AudioComponentBase : MonoBehaviour
{
    [Header("Leave Blank for Default")]
    [SerializeField] protected AudioClip audioClip;
    [Space, SerializeField] protected WeightedRandom volumeMinMax;
    [SerializeField] protected WeightedRandom pitchMinMax;
    [SerializeField] protected WeightedRandom delayMinMax;

    public virtual void PlayAudio()
    {
        var volume = this.volumeMinMax.GetWeightedRandomQuantity();
        var pitch = this.pitchMinMax.GetWeightedRandomQuantity();
        var delay = this.delayMinMax.GetWeightedRandomQuantity();

        AudioManager.IN.PlayClip(this.audioClip, volume, pitch, delay);
    }
}