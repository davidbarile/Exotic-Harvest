using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;
using static GlobalEnums;

public class AudioManager : MonoBehaviour
{
    public static AudioManager IN;

    public static AudioClip CurrentMusicClip { get; private set; }
    public static AudioClip CurrentAmbientClip { get; private set; }

    [SerializeField] private bool isAudioEnabled = true;

    [Space]
    [SerializeField] private AudioSource activeMusicSource, inactiveMusicSource;
    [SerializeField] private AudioSource activeAmbientSource, inactiveAmbientSource;

    [Header("Audio Configs -----------------")]
    [SerializeField] private AudioConfig[] musicConfigs;
    [SerializeField] private AudioConfig[] ambientConfigs;

    [Space, SerializeField] private WeightedRandom minMaxMinutesBetweenMusicChanges;
    [Range(0f, 1f), SerializeField] private float musicChangeChance = 0.5f; // Chance to change music on time/weather change, for variety

    [Space, SerializeField] private WeightedRandom minMaxSecondsBetweenAmbientChanges;
    [Range(0f, 1f), SerializeField] private float ambientChangeChance = 0.1f; // Chance to change ambient on time/weather change, for variety

    [Header("Audio Settings -----------------")]
    [Range(0f, 3f), SerializeField] private float audioFadeDuration = 1;

    [Space, Range(0f, 1f),SerializeField] private float buttonVolume = 1;
    [Range(0f, 2f), SerializeField] private float buttonPitch = 1;
    [Space, Range(0f, 1f),SerializeField] private float keyPressVolume = 1;
    [Range(0f, 2f), SerializeField] private float keyPressPitch = 1;

    [Header("Audio Clips -----------------")]
    public AudioClip ButtonClickClip;
    public AudioClip KeyPressClickClip;
    public AudioClip IncrementCounterClip;
    public AudioClip ErrorSoundClip;
    public AudioClip GoldenAppleUseClip;

    [Space()]
    public AudioClip GrasshopperCollectClip;
    public AudioClip GrasshopperUseClip;
    public AudioClip GrasshopperJumpClip;

    private List<AudioSource> audioSources = new List<AudioSource>();
    private int audioSourceIndex, recursionCounter;

    private Tween activeMusicTween = null;
    private Tween inactiveMusicTween = null;
    private Tween activeAmbientTween = null;
    private Tween inactiveAmbientTween = null;

    private bool isMaximized;

    private float minutesBetweenMusicChanges = 5f;//gets set by weighted random on start and after each change
    private float secondsBetweenAmbientChanges = 15f;//gets set by weighted random on start and after each change

    private DateTime lastMusicChangeHour = DateTime.MinValue;
    private DateTime lastAmbientChangeHour = DateTime.MinValue;

    private ETimeOfDay lastTimeOfDay;

    public void Init()
    {
        this.activeMusicSource.volume = SaveManager.Data.MusicVolume;
        this.inactiveMusicSource.volume = 0;
        this.activeMusicSource.ignoreListenerVolume = true;
        this.inactiveMusicSource.ignoreListenerVolume = true;

        this.activeAmbientSource.volume = SaveManager.Data.AmbientVolume;
        this.inactiveAmbientSource.volume = 0;
        this.activeAmbientSource.ignoreListenerVolume = true;
        this.inactiveAmbientSource.ignoreListenerVolume = true;

        this.minutesBetweenMusicChanges = this.minMaxMinutesBetweenMusicChanges.GetWeightedRandomQuantity();
        this.secondsBetweenAmbientChanges = this.minMaxSecondsBetweenAmbientChanges.GetWeightedRandomQuantity();

        SetAudioMode(true);

        //dynamically create audio sources
        for (int i = 0; i < 8; ++i)
        {
            AudioSource a = this.gameObject.AddComponent<AudioSource>();
            a.playOnAwake = false;

            this.audioSources.Add(a);
        }

        TimeManager.OnHourChanged += OnHourChanged; //also update music on hour change for more variety
        WeatherManager.OnWeatherChanged += OnWeatherChanged;
        ScreenManager.OnMinimizeMaximizeToggled += SetAudioMode;
    }

    private void OnDestroy()
    {
        TimeManager.OnHourChanged -= OnHourChanged;
        WeatherManager.OnWeatherChanged -= OnWeatherChanged;
        ScreenManager.OnMinimizeMaximizeToggled -= SetAudioMode;
    }

    public void ApplyAudioSettingsFromSaveData()
    {
        SetMusicVolume(SaveManager.Data.MusicVolume);
        SetAmbientVolume(SaveManager.Data.AmbientVolume);
        SetEffectsVolume(SaveManager.Data.EffectsVolume);
        SetMusicVolume_Minimized(SaveManager.Data.MusicVolume_Minimized);
        SetAmbientVolume_Minimized(SaveManager.Data.AmbientVolume_Minimized);
        SetEffectsVolume_Minimized(SaveManager.Data.EffectsVolume_Minimized);

        UiManager.IN.SettingsPanel.MusicVolumeSlider.SetValueWithoutNotify(SaveManager.Data.MusicVolume);
        UiManager.IN.SettingsPanel.AmbientVolumeSlider.SetValueWithoutNotify(SaveManager.Data.AmbientVolume);
        UiManager.IN.SettingsPanel.EffectsVolumeSlider.SetValueWithoutNotify(SaveManager.Data.EffectsVolume);
        UiManager.IN.SettingsPanel.MusicVolumeSlider_Minimized.SetValueWithoutNotify(SaveManager.Data.MusicVolume_Minimized);
        UiManager.IN.SettingsPanel.AmbientVolumeSlider_Minimized.SetValueWithoutNotify(SaveManager.Data.AmbientVolume_Minimized);
        UiManager.IN.SettingsPanel.EffectsVolumeSlider_Minimized.SetValueWithoutNotify(SaveManager.Data.EffectsVolume_Minimized);
    }

    private void OnWeatherChanged(EWeatherType inWeather)
    {
        //only fire on change
        if (WeatherManager.CurrentWeather == WeatherManager.LastWeather)
            return;

        ChangeMusic(true);
        ChangeAmbient(true);
    }

    private void OnHourChanged(float inHour)
    {
        var musicMinutesElapsed = (DateTime.Now - this.lastMusicChangeHour).TotalMinutes;
        musicMinutesElapsed *= TimeManager.IN.TimeScale;

        //if time of day or weather changed, force new music to play
        var isNewTimeOfDay = TimeManager.CurrentTimeOfDay != this.lastTimeOfDay;
        var forceChange = isNewTimeOfDay && WeatherManager.IsClear;

        //UiManager.IN.SetDebugText($"AudioManager.OnHourChanged()   CurrentTimeOfDay: {TimeManager.CurrentTimeOfDay},  LastTimeOfDay: {this.lastTimeOfDay},  CurrentWeather: {WeatherManager.CurrentWeather},  LastWeather: {WeatherManager.LastWeather}", true);
        //UiManager.IN.SetDebugText($"OnHourChanged({inHour})  musicMinutesElapsed: {musicMinutesElapsed} / {this.minutesBetweenMusicChanges}", true);
        if (forceChange || musicMinutesElapsed > this.minutesBetweenMusicChanges)
        {
            this.lastMusicChangeHour = DateTime.Now;

            this.minutesBetweenMusicChanges = this.minMaxMinutesBetweenMusicChanges.GetWeightedRandomQuantity();

            if (forceChange || UnityEngine.Random.value < this.musicChangeChance)
                ChangeMusic(false);
        }

        var ambientSecondsElapsed = (DateTime.Now - this.lastAmbientChangeHour).TotalSeconds;
        ambientSecondsElapsed *= TimeManager.IN.TimeScale;
        //UiManager.IN.SetDebugText($" - ambientSecondsElapsed: {ambientSecondsElapsed} / {this.secondsBetweenAmbientChanges}", true);
        if (ambientSecondsElapsed > this.secondsBetweenAmbientChanges)
        {
            this.lastAmbientChangeHour = DateTime.Now;

            this.secondsBetweenAmbientChanges = this.minMaxSecondsBetweenAmbientChanges.GetWeightedRandomQuantity();

            if (UnityEngine.Random.value < this.ambientChangeChance)
                ChangeAmbient();
        }

        this.lastTimeOfDay = TimeManager.CurrentTimeOfDay;
    }

    private void PlayOrCrossfadeMusic(AudioClip inNewClip)
    {
        //UiManager.IN.SetDebugText($"AudioManager.PlayOrCrossfadeMusic({inNewClip})   CurrentMusicClip: {CurrentMusicClip?.name}. Time = {TimeManager.CurrentTimeOfDay}", true);

        if (inNewClip == null || inNewClip == CurrentMusicClip)
            return;

        AudioManager.CurrentMusicClip = inNewClip;

        //swap active/inactive
        var temp = this.activeMusicSource;
        this.activeMusicSource = this.inactiveMusicSource;
        this.inactiveMusicSource = temp;

        var musicVolume = this.isMaximized ? SaveManager.Data.MusicVolume : SaveManager.Data.MusicVolume_Minimized;

        //if nothing is playing, just play new clip without fading
        if (!this.inactiveMusicSource.isPlaying)
        {
            this.activeMusicSource.clip = inNewClip;
            this.activeMusicSource.volume = musicVolume;
            this.activeMusicSource.Play();
            return;
        }

        KillMusicTweens();

        //fade out old
        this.inactiveMusicTween = this.inactiveMusicSource.DOFade(0, this.audioFadeDuration).OnComplete(() => { this.inactiveMusicSource.Pause(); });

        //fade in new
        this.activeMusicSource.Stop();//just in case
        this.activeMusicSource.clip = inNewClip;
        this.activeMusicSource.volume = 0;
        this.activeMusicSource.Play();

        if (musicVolume > 0)
            this.activeMusicTween = this.activeMusicSource.DOFade(musicVolume, this.audioFadeDuration);
    }
    
    private void PlayOrCrossfadeAmbient(AudioClip inNewClip)
    {
        if (inNewClip == null || inNewClip == CurrentAmbientClip)
            return;

        AudioManager.CurrentAmbientClip = inNewClip;

        var ambientVolume = this.isMaximized ? SaveManager.Data.AmbientVolume : SaveManager.Data.AmbientVolume_Minimized;

        //swap active/inactive
        var temp = this.activeAmbientSource;
        this.activeAmbientSource = this.inactiveAmbientSource;
        this.inactiveAmbientSource = temp;

        //if nothing is playing, just play new clip without fading
        if (!this.inactiveAmbientSource.isPlaying)
        {
            this.activeAmbientSource.clip = inNewClip;
            this.activeAmbientSource.volume = ambientVolume;
            this.activeAmbientSource.Play();
            return;
        }

        KillAmbientTweens();

        //fade out old
        this.inactiveAmbientTween = this.inactiveAmbientSource.DOFade(0, this.audioFadeDuration).OnComplete(() => { this.inactiveAmbientSource.Pause(); });

        //fade in new
        this.activeAmbientSource.Stop();//just in case
        this.activeAmbientSource.clip = inNewClip;
        this.activeAmbientSource.volume = 0;
        this.activeAmbientSource.Play();

        if(ambientVolume > 0)
            this.activeAmbientTween = this.activeAmbientSource.DOFade(ambientVolume, this.audioFadeDuration);
    }

    private void ChangeMusic(bool isWeatherChange = false)
    {
        if (!this.isAudioEnabled)
            return;

        var newClip = GetMusicClipForCurrentConditions();

        UiManager.IN.SetDebugText($"AudioManager.ChangeMusic({TimeManager.CurrentTimeOfDay}, {WeatherManager.CurrentWeather})   NewMusicClip: {newClip?.name}", true);

        if (newClip != null)
        {
            PlayOrCrossfadeMusic(newClip);
        }

        AudioClip GetMusicClipForCurrentConditions()
        {
            var matchingClips = new List<AudioClip>();

            if (WeatherManager.IsClear)
            {
                //clear day
                foreach (var audioConfig in this.musicConfigs)
                {
                    if (audioConfig.TimeOfDay.HasFlag(TimeManager.CurrentTimeOfDay) && audioConfig.WeatherType == EWeatherType.Clear)
                    {
                        matchingClips.AddRange(audioConfig.AudioClips);
                    }
                }
            }
            else
            {
                //rain, storm, wind, snow, foggy
                foreach (var audioConfig in this.musicConfigs)
                {
                    if (audioConfig.WeatherType.HasFlag(WeatherManager.CurrentWeather))
                    {
                        matchingClips.AddRange(audioConfig.AudioClips);
                    }
                }
            }

            if(CurrentMusicClip != null && matchingClips.Contains(CurrentMusicClip) && matchingClips.Count > 1)
            {
                matchingClips.Remove(CurrentMusicClip); // Don't pick the same clip again for variety
            }

            if (matchingClips.Count > 0)
            {
                return matchingClips[UnityEngine.Random.Range(0, matchingClips.Count)];
            }

            Debug.Log($"<color=red>AudioManager.GetMusicClipForCurrentConditions()   No matching AudioConfig found for {TimeManager.CurrentTimeOfDay} and {WeatherManager.CurrentWeather}! Returning null.</color>");
            return null;
        }
    }

    private void ChangeAmbient(bool isWeatherChange = false)
    {
        if (!this.isAudioEnabled)
            return;
            
        var newClip = GetAmbientClipForCurrentConditions();

        //UiManager.IN.SetDebugText($"AudioManager.ChangeAmbient({TimeManager.CurrentTimeOfDay}, {WeatherManager.CurrentWeather})   NewAmbientClip: {newClip?.name}", true);

        if (newClip != null)
        {
            PlayOrCrossfadeAmbient(newClip);
        }

        AudioClip GetAmbientClipForCurrentConditions()
        {
            var matchingClips = new List<AudioClip>();

            if (WeatherManager.IsClear || WeatherManager.IsFoggy)
            {
                //clear day
                foreach (var audioConfig in this.ambientConfigs)
                {
                    if (audioConfig.TimeOfDay.HasFlag(TimeManager.CurrentTimeOfDay) && audioConfig.WeatherType == EWeatherType.Clear)
                    {
                        matchingClips.AddRange(audioConfig.AudioClips);
                    }
                }
            }
            else
            {
                //rain, storm, wind, snow, foggy
                foreach (var audioConfig in this.ambientConfigs)
                {
                    if (audioConfig.WeatherType.HasFlag(WeatherManager.CurrentWeather))
                    {
                        matchingClips.AddRange(audioConfig.AudioClips);
                    }
                }
            }

            if(CurrentAmbientClip != null && matchingClips.Contains(CurrentAmbientClip) && matchingClips.Count > 1)
            {
                matchingClips.Remove(CurrentAmbientClip); // Don't pick the same clip again for variety
            }

            if (matchingClips.Count > 0)
            {
                return matchingClips[UnityEngine.Random.Range(0, matchingClips.Count)];
            }

            Debug.Log($"<color=red>AudioManager.GetAmbientClipForCurrentConditions()   No matching AudioConfig found for {TimeManager.CurrentTimeOfDay} and {WeatherManager.CurrentWeather}! Returning null.</color>");
            return null;
        }
    }

    public void SetAudioMode(bool inIsMaximized)
    {
        if (!this.isAudioEnabled)
            return;
            
        if (inIsMaximized == this.isMaximized)
            return;

        this.isMaximized = inIsMaximized;

        //UiManager.IN.SetDebugText($"<color=yellow>AudioManager.SetAudioMode()   isMaximized: {this.isMaximized}</color>", true);

        KillAudioTweens();

        if (inIsMaximized)
        {
            if (this.activeMusicSource.isPlaying)
                this.activeMusicTween = this.activeMusicSource.DOFade(SaveManager.Data.MusicVolume, this.audioFadeDuration);
            else
                this.activeMusicSource.volume = SaveManager.Data.MusicVolume;
                
            if(this.inactiveMusicSource.isPlaying)
                this.inactiveMusicTween = this.inactiveMusicSource.DOFade(SaveManager.Data.MusicVolume, this.audioFadeDuration);
            else
                this.inactiveMusicSource.volume = SaveManager.Data.MusicVolume;

            AudioListener.volume = SaveManager.Data.EffectsVolume;
        }
        else
        {
            if (this.activeMusicSource.isPlaying)
                this.activeMusicTween = this.activeMusicSource.DOFade(SaveManager.Data.MusicVolume_Minimized, this.audioFadeDuration);
            else
                this.activeMusicSource.volume = SaveManager.Data.MusicVolume_Minimized;
                
            if(this.inactiveMusicSource.isPlaying)
                this.inactiveMusicTween = this.inactiveMusicSource.DOFade(SaveManager.Data.MusicVolume_Minimized, this.audioFadeDuration);
            else
                this.inactiveMusicSource.volume = SaveManager.Data.MusicVolume_Minimized;

            AudioListener.volume = SaveManager.Data.EffectsVolume_Minimized;
        }
    }

    private void KillAudioTweens()
    {
        KillMusicTweens();
        KillAmbientTweens();
    }

    private void KillMusicTweens()
    {
        this.activeMusicTween?.Kill();
        this.inactiveMusicTween?.Kill();
    }

    private void KillAmbientTweens()
    {
        this.activeAmbientTween?.Kill();
        this.inactiveAmbientTween?.Kill();
    }

    public void StopMusicForQuit()
    {
        KillAudioTweens();

        this.activeMusicSource.volume = 0;
        this.inactiveMusicSource.volume = 0;

        this.activeAmbientSource.volume = 0;
        this.inactiveAmbientSource.volume = 0;

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
        if (!this.isAudioEnabled)
            return null;

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

    public void PlayErrorSound()
    {
        var rndPitch = UnityEngine.Random.Range(.95f, 1.05f);
        PlayClip(this.ErrorSoundClip, 1, rndPitch);
    }

#region Volume Setters
    public void SetMusicVolume(float inValue)
    {
        SaveManager.Data.MusicVolume = inValue;

        if (this.isMaximized)
        {
            this.activeMusicSource.volume = inValue;
            this.inactiveMusicSource.volume = inValue;
        }
    }

    public void SetAmbientVolume(float inValue)
    {
        SaveManager.Data.AmbientVolume = inValue;

        if (this.isMaximized)
        {
            this.activeAmbientSource.volume = inValue;
            this.inactiveAmbientSource.volume = inValue;
        }
    }
    
    public void SetEffectsVolume(float inValue)
    {
        SaveManager.Data.EffectsVolume = inValue;

        if(this.isMaximized)
            AudioListener.volume = inValue;
    }

     public void SetEffectsVolume_Minimized(float inValue)
    {
        SaveManager.Data.EffectsVolume_Minimized = inValue;

        if(!this.isMaximized)
            AudioListener.volume = inValue;
    }

    public void SetMusicVolume_Minimized(float inValue)
    {
        SaveManager.Data.MusicVolume_Minimized = inValue;

        if(!this.isMaximized)
        {
            this.activeMusicSource.volume = inValue;
            this.inactiveMusicSource.volume = inValue;
        }
    }

    public void SetAmbientVolume_Minimized(float inValue)
    {
        SaveManager.Data.AmbientVolume_Minimized = inValue;

        if (!this.isMaximized)
        {
            this.activeAmbientSource.volume = inValue;
            this.inactiveAmbientSource.volume = inValue;
        }
    }
#endregion
}