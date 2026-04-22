using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public static AudioManager IN;

    [SerializeField] private AudioSource[] musicSources;
    [SerializeField] private AudioSource[] ambientSources;
    [SerializeField] private float audioFadeDuration = 1;

    [Header("Audio Clips")]
    public AudioClip ButtonClickClip;
    public AudioClip KeyPressClickClip;
    public AudioClip IncrementCounterClip;
    public AudioClip ErrorSoundClip;
    public AudioClip GoldenAppleUseClip;

    [Space()]
    public AudioClip GrasshopperCollectClip;
    public AudioClip GrasshopperUseClip;
    public AudioClip GrasshopperJumpClip;

    [Space()]
    [SerializeField] private float buttonVolume = 1;
    [SerializeField] private float buttonPitch = 1;
    [Space()]
    [SerializeField] private float keyPressVolume = 1;
    [SerializeField] private float keyPressPitch = 1;

    private List<AudioSource> audioSources = new List<AudioSource>();
    private int audioSourceIndex;
    private int recursionCounter;

    private Tween[] musicTweens = { null, null };
    private Tween[] ambientTweens = { null, null };

    private bool isMinimized;

    public void Init()
    {
        this.musicSources[0].volume = SaveManager.Data.MusicVolume;
        this.musicSources[1].volume = 0;
        this.musicSources[0].ignoreListenerVolume = true;
        this.musicSources[1].ignoreListenerVolume = true;

        this.ambientSources[0].volume = SaveManager.Data.AmbientVolume;
        this.ambientSources[1].volume = 0;
        this.ambientSources[0].ignoreListenerVolume = true;
        this.ambientSources[1].ignoreListenerVolume = true;

        SetAudioMode(true);

        //dynamically create audio sources
        for (int i = 0; i < 8; ++i)
        {
            AudioSource a = this.gameObject.AddComponent<AudioSource>();
            a.playOnAwake = false;

            this.audioSources.Add(a);
        }
    }

    public void SetAudioMode(bool inIsMinimized)
    {
        if (inIsMinimized == this.isMinimized)
            return;

        this.isMinimized = inIsMinimized;

        KillAudioTweens();

        if (inIsMinimized)
        {
            this.musicSources[0].Play();
            this.ambientSources[0].Play();

            this.musicTweens[0] = this.musicSources[0].DOFade(SaveManager.Data.MusicVolume, this.audioFadeDuration);
            this.musicTweens[1] = this.musicSources[1].DOFade(0, this.audioFadeDuration);
            this.musicTweens[1].onComplete = () => { this.musicSources[1].Pause(); };

            this.ambientTweens[0] = this.ambientSources[0].DOFade(SaveManager.Data.AmbientVolume, this.audioFadeDuration);
            this.ambientTweens[1] = this.ambientSources[1].DOFade(0, this.audioFadeDuration);
            this.ambientTweens[1].onComplete = () => { this.ambientSources[1].Pause(); };
        }
        else
        {
            this.musicSources[1].Play();
            this.ambientSources[1].Play();

            this.musicTweens[0] = this.musicSources[0].DOFade(0, this.audioFadeDuration);
            this.musicTweens[0].onComplete = () => { this.musicSources[0].Pause(); };
            this.musicTweens[1] = this.musicSources[1].DOFade(SaveManager.Data.MusicVolume, this.audioFadeDuration);

            this.ambientTweens[0] = this.ambientSources[0].DOFade(0, this.audioFadeDuration);
            this.ambientTweens[0].onComplete = () => { this.ambientSources[0].Pause(); };
            this.ambientTweens[1] = this.ambientSources[1].DOFade(SaveManager.Data.AmbientVolume, this.audioFadeDuration);
        }
    }

    public void KillAudioTweens()
    {
        this.musicTweens[0]?.Kill();
        this.musicTweens[1]?.Kill();
        this.ambientTweens[0]?.Kill();
        this.ambientTweens[1]?.Kill();
    }

    public void StopMusicForQuit()
    {
        KillAudioTweens();

        this.musicSources[0].volume = 0;
        this.musicSources[1].volume = 0;

        this.ambientSources[0].volume = 0;
        this.ambientSources[1].volume = 0;

        AudioListener.volume = 0;
    }

    public AudioSource PlayClip(AudioClip inClip)
    {
        return PlayClip(inClip, 1, 1, 0);
    }

    public AudioSource PlayClip(AudioClip inClip, float inVolume)
    {
        return PlayClip(inClip, inVolume, 1, 0);
    }

    public AudioSource PlayClip(AudioClip inClip, float inVolume, float inPitch)
    {
        return PlayClip(inClip, inVolume, inPitch, 0);
    }

    public AudioSource PlayClip(AudioClip inClip, float inVolume, float inPitch, float inDelay)
    {
        if (inClip != null)
        {
            AudioSource audioSource = this.audioSources[this.audioSourceIndex];

            if (!audioSource.isPlaying)
            {
                audioSource.clip = inClip;
                audioSource.volume = inVolume;
                audioSource.pitch = inPitch;
                audioSource.PlayDelayed(inDelay);

                ++this.audioSourceIndex;
                this.audioSourceIndex %= this.audioSources.Count;

                this.recursionCounter = 0;

                return audioSource;
            }
            else
            {
                if (this.recursionCounter < this.audioSources.Count) //find next in list
                {
                    ++this.recursionCounter;

                    ++this.audioSourceIndex;
                    this.audioSourceIndex %= this.audioSources.Count;
                }
                else //entire list is exhausted, so add a new one and play it
                {
                    audioSource = gameObject.AddComponent<AudioSource>();

                    this.audioSources.Add(audioSource);

                    this.audioSourceIndex = this.audioSources.Count - 1;//select it


                }

                PlayClip(inClip, inVolume, inPitch, inDelay);

                return audioSource;
            }
        }
        else
        {
            Debug.Log("AudioManager.PlayClip()   inClip is null!");
            return null;
        }
    }

    public void PlayButtonSound()
    {
        PlayClip(this.ButtonClickClip, this.buttonVolume, this.buttonPitch);
    }

    public void PlayKeyPressSound()
    {
        PlayClip(this.KeyPressClickClip, this.keyPressVolume, this.keyPressPitch);
    }

    public void PlayHoneyCombFullSound()
    {
        var rndPitch = UnityEngine.Random.Range(1.07f, 1.1f);
        PlayClip(this.ErrorSoundClip, 1, rndPitch);
    }

    public void PlayNoBeesSound()
    {
        var rndPitch = UnityEngine.Random.Range(1.17f, 1.2f);
        PlayClip(this.ErrorSoundClip, 1, rndPitch);
    }

    public void SetEffectsVolume(float inValue)
    {
        SaveManager.Data.EffectsVolume = inValue;
        AudioListener.volume = inValue;
    }

    public void SetMusicVolume(float inValue)
    {
        SaveManager.Data.MusicVolume = inValue;
        this.musicSources[0].volume = inValue;
        this.musicSources[1].volume = inValue;
    }

    public void SetAmbientVolume(float inValue)
    {
        SaveManager.Data.AmbientVolume = inValue;
        this.ambientSources[0].volume = inValue;
        this.ambientSources[1].volume = inValue;
    }

     public void SetEffectsVolume_Minimized(float inValue)
    {
        SaveManager.Data.EffectsVolume = inValue;
        AudioListener.volume = inValue;
    }

    public void SetMusicVolume_Minimized(float inValue)
    {
        SaveManager.Data.MusicVolume = inValue;
        this.musicSources[0].volume = inValue;
        this.musicSources[1].volume = inValue;
    }

    public void SetAmbientVolume_Minimized(float inValue)
    {
        SaveManager.Data.AmbientVolume = inValue;
        this.ambientSources[0].volume = inValue;
        this.ambientSources[1].volume = inValue;
    }
}
