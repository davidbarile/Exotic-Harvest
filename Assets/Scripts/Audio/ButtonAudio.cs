using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonAudio : AudioComponentBase
{
    private Button button;
    
    private void Start()
    {
        this.button = GetComponent<Button>();
        this.button.onClick.AddListener(PlayAudio);
    }

    public override void PlayAudio()
    {
        if (this.audioClip == null)
            AudioManager.IN.PlayButtonSound();
        else
            base.PlayAudio();
    }

    private void OnDestroy()
    {
        if (this.button == null) return;
        this.button.onClick.RemoveListener(PlayAudio);
    }
}