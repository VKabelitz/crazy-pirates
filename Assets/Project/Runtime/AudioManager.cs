using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

/// <summary>
/// The implementation of a AudioManager that can manage sound files.
/// Public access with AudioManager.instance .
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region Variables
    public static AudioManager instance = null;

    [SerializeField]
    private AudioMixer audioMixer;
    public Sound[] sounds;
    private string currentMusic = null;
    private Coroutine musicCoroutine = null;
    private List<(Coroutine, string)> soundCoroutines = new List<(Coroutine soundCor, string sndN)>();
    /// <summary>
    /// The names of the exposed parameters in the Audio Mixer.
    /// </summary>
    private string[] audioMixerGroupKeys =
    {
        "TotalVolume",
        "MusicVolume",
        "BackgroundVolume",
        "SFXVolume"
    };
    #endregion

    #region Unity Event Functions
    void Awake()
    {
        if (!instance)
        {
            instance = this;
        }

        instance.LoadSounds();
    }

    void Start()
    {
        instance.LoadGroupVolumePrefs();

        // instance.SwitchMusicFade("M_Secrets", 3f);
    }
    #endregion

    #region Basic Functionality

    /// <summary>
    /// Play a soundfile stored in the AudioManager.
    /// </summary>
    /// <param name="soundName">The name of the sound that is assigned in the AudioManager.</param>
    public void PlaySound(string soundName, float volume = -1, float pitch = -1, bool switchLooped = false)
    {
        Sound s = GetSound(soundName, volume, pitch, switchLooped);
        if (s == null)
            return;
        if (s.soundType == Sound.SoundType.Music)
            Debug.LogWarning("Please consider using SwitchMusic or SwitchMusicFade, since it automatically pauses the current track playing.");
        s.source.Play();
    }
    /// <summary>
    /// Play a soundfile stored in the AudioManager without variable parameters, so that it can be called as Event in the Inspector.
    /// </summary>
    /// <param name="soundName">The name of the sound that is assigned in the AudioManager.</param>
    public void PlaySoundStatic(string soundName)
    {
        Sound s = GetSound(soundName);
        if (s == null)
            return;
        if (s.soundType == Sound.SoundType.Music)
            Debug.LogWarning("Please consider using SwitchMusic or SwitchMusicFade, since it automatically pauses the current track playing.");
        s.source.Play();
    }

    /// <summary>
    /// Pause a sound.
    /// </summary>
    /// <param name="soundName">The name of the sound that is assigned in the AudioManager.</param>
    public void PauseSound(string soundName, float volume = -1, float pitch = -1, bool switchLooped = false)
    {
        Sound s = GetSound(soundName, volume, pitch, switchLooped);
        if (s == null)
            return;
        s.source.Pause();
    }
    /// <summary>
    /// Switch the music track.
    /// </summary>
    /// <param name="newSoundName">The name of the new sound.</param>
    public void SwitchMusic(string newSoundName, float volume = -1, float pitch = -1, bool switchLooped = false)
    {
        if (instance.currentMusic != null)
            PauseSound(instance.currentMusic);

        Sound s = GetSound(newSoundName, volume, pitch, switchLooped);
        if (s == null)
            return;

        s.source.Play();
        instance.currentMusic = newSoundName;
    }

    public void PauseAudioListener()
    {
        AudioListener.pause = true;
    }
    public void UnpauseAudioListener()
    {
        AudioListener.pause = false;
    }
    /// <summary>
    /// Helper function to get a sound. Private, because other classes should not be able to edit the Sounds saved in the Audiomanager directly, but only through use of the AudioManager functions.
    /// </summary>
    /// <param name="soundName"></param>
    /// /// <param name="volume"></param>
    /// /// <param name="pitch"></param>
    /// /// <param name="switchLooped"></param>
    /// <returns>The Sound corresponding to the given soundName, null if no sound was found.</returns>

    private Sound GetSound(string soundName, float volume = -1, float pitch = -1, bool switchLooped = false)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return null;
        }
        s.source.volume = (volume == -1) ? s.volume : volume;
        s.source.pitch = (pitch == -1) ? s.pitch : pitch;
        s.source.loop = (!switchLooped) ? s.loop : !s.loop;

        return s;
    }

    #endregion

    #region Fading Sounds
    /// <summary>
    /// Play a sound by fading in.
    /// </summary>
    /// <param name="soundName">The name of the sound that is assigned in the AudioManager</param>
    /// <param name="fadeTime">The time it should take to fade the sound in.</param>
    public void FadeIn(string soundName, float fadeTime = 2f, float volume = -1, float pitch = -1, bool switchLooped = false)
    {

        Sound s = GetSound(soundName, volume, pitch, switchLooped);
        if (s == null)
            return;
        if (s.soundType == Sound.SoundType.Music)
            Debug.LogWarning($"Please consider using SwitchMusicFade for track {soundName}, since it automatically pauses the current track playing.");

        float startVolume = 0f;
        float endVolume = s.source.volume;
        s.source.volume = startVolume;
        s.source.Play();
        StartCoroutine(FadeSound(soundName, fadeTime, startVolume, endVolume));
    }
    /// <summary>
    /// Stop a sound by fading out.
    /// </summary>
    /// <param name="soundName">The name of the sound that is assigned in the AudioManager</param>
    /// <param name="fadeTime">The time it should take to fade the sound in.</param>
    public void FadeOut(string soundName, float fadeTime = 2f)
    {
        Sound s = GetSound(soundName);
        if (s == null)
            return;
        if (s.soundType == Sound.SoundType.Music)
            Debug.LogWarning($"Please consider using SwitchMusicFade for track {soundName}, since it automatically pauses the current track playing.");

        float startVolume = s.source.volume;
        float endVolume = 0f;
        s.source.volume = startVolume;
        foreach (var corItem in soundCoroutines)
        {
            if (corItem.Item2 == soundName)
                StopCoroutine(corItem.Item1);
        }
        soundCoroutines.Add((StartCoroutine(FadeSound(soundName, fadeTime, startVolume, endVolume)), soundName));
    }



    /// <summary>
    /// Switch the music track by fading between the two tracks.
    /// </summary>
    /// <param name="newSoundName">The name of the new sound.</param>
    /// <param name="fadeTime">The time it should take to fade the tracks.</param>
    /// <param name="volume">The volume the sound shall be played with. If no volume given, the volume of the AudioManager Setting is used.</param>
    public void SwitchMusicFade(string newSoundName, float fadeTime = 2f, float volume = -1, float pitch = -1, bool switchLooped = false)
    {
        Sound newSound = GetSound(newSoundName, volume, pitch, switchLooped);
        if (newSound == null)
            return;
        if (musicCoroutine != null)
            StopCoroutine(musicCoroutine);
        musicCoroutine = StartCoroutine(FadeTrack(newSoundName, newSound.source.volume, fadeTime));
    }
    #endregion

    #region Extra Functionality
    public void PlaySoundAfterDelay(string soundName, float delayDuration, float volume = -1, float pitch = -1, bool switchLooped = false)
    {
        Sound newSound = GetSound(soundName, volume, pitch, switchLooped);
        if (newSound == null)
            return;
        StartCoroutine(PlaySoundWithDelay(soundName, delayDuration));
    }

    /// <summary>
    /// Play a random Sound of a given SoundType.
    /// </summary>
    /// <param name="soundType">The SoundType of the sound.</param>
    /// /// <param name="volume">The volume the sound shall be played with. If no volume given, the volume of the AudioManager Setting is used.</param>
    public void PlayRandomSoundOfType(Sound.SoundType soundType, float volume = -1, float pitch = -1, bool switchLooped = false)
    {
        Sound s = GetRandomSoundOfType(soundType, volume, pitch, switchLooped);
        if (s == null)
            return;
        PlaySound(s.name);
    }


    public Sound GetRandomSoundOfType(Sound.SoundType soundType, float volume = -1, float pitch = -1, bool switchLooped = false)
    {
        Sound[] soundsOfType = Array.FindAll(sounds, sound => sound.soundType == soundType);
        if (sounds.Length == 0)
        {
            Debug.LogWarning("No Sounds of type  " + soundType + " where found!");
            return null;
        }
        Sound s = soundsOfType[UnityEngine.Random.Range(0, soundsOfType.Length)];
        s.source.volume = (volume == -1) ? s.volume : volume;
        s.source.pitch = (pitch == -1) ? s.pitch : pitch;
        s.source.loop = (!switchLooped) ? s.loop : !s.loop;
        return s;
    }
    #endregion

    #region Settings
    private void LoadSounds()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.outputAudioMixerGroup = s.audioMixerGroup;
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.ignoreListenerPause = s.ignorePause;
        }
    }
    /// <summary>
    /// Change the Volume of an AdioMixerGroup.
    /// </summary>
    /// <param name="key">The name of the exposed parameter of the Audio Mixer Group (see audioMixerGroupKeys).</param>
    /// <param name="volume">The new volume of the AudioMixerGroup. In the range of -80(silent) and 0(loud).</param>
    public void ChangeGroupVolume(string key, float volume)
    {
        if (!audioMixerGroupKeys.Contains(key))
        {
            Debug.LogWarning($"Key {key} is not in the array of audio mixer group keys!");
            return;
        }
        audioMixer.SetFloat(key, volume);
        PlayerPrefs.SetFloat(key, volume);
    }

    /// <summary>
    /// Set the Volume of the Sound. This changes the default Setting in the AM as well. Only for Setting Purposes.
    /// </summary>
    /// <param name="soundName">The name of the sound.</param>
    /// <param name="volume">Volume in the range [0, 1] inclusive.</param>
    public void SetSoundVolume(string soundName, float volume)
    {
        Sound s = GetSound(soundName);
        if (s == null)
            return;
        s.source.volume = s.volume = volume;
    }

    /// <summary>
    /// Set the Pitch of the Sound. This changes the default Setting in the AM as well. Only for Setting Purposes.
    /// </summary>
    /// <param name="soundName">The name of the sound.</param>
    /// <param name="pitch">Pitch in the range [0.1, 3] inclusive.</param>
    public void SetSoundPitch(string soundName, float pitch)
    {
        Sound s = GetSound(soundName);
        if (s == null)
            return;
        s.source.pitch = s.pitch = pitch;
    }

    /// <summary>
    /// Set the Loop boolean parameter of the Sound. This changes the default Setting in the AM as well. Only for Setting Purposes.
    /// </summary>
    /// <param name="soundName">The name of the sound.</param>
    /// <param name="loop">Loop of type bool. True if the sound shall be looped.</param>
    public void SetSoundLoop(string soundName, bool loop)
    {
        Sound s = GetSound(soundName);
        if (s == null)
            return;
        s.source.loop = s.loop = loop;
    }

    /// <summary>
    /// Load the Volumes of the Groups saved in the Player Prefs.
    /// </summary>
    public void LoadGroupVolumePrefs()
    {
        foreach (string audioMixerGroupKey in audioMixerGroupKeys)
        {
            if (PlayerPrefs.HasKey(audioMixerGroupKey))
            {
                instance.ChangeGroupVolume(
                    audioMixerGroupKey,
                    PlayerPrefs.GetFloat(audioMixerGroupKey)
                );
            }
        }
    }
    #endregion

    #region Coroutines
    private IEnumerator FadeSound(string soundName, float fadeTime, float startVolume, float endVolume)
    {
        Sound s = GetSound(soundName);
        if (endVolume > 0f)
            s.source.Play();

        float elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            s.source.volume = Mathf.Lerp(startVolume, endVolume, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (endVolume == 0f)
            s.source.Pause();

    }
    private IEnumerator FadeTrack(string newSoundName, float newTrackTargetVolume, float fadeTime = 10f)
    {
        Sound newSound = GetSound(newSoundName);
        float newTrackCurrentVolume = 0f;
        if (newSound.source.isPlaying)
            newTrackCurrentVolume = newSound.source.volume;

        Sound oldSound = currentMusic != null ? GetSound(currentMusic) : null;
        float oldTrackVolumeTarget = 0f;
        float oldTrackCurrentVolume = 0f;

        if (oldSound != null)
            oldTrackCurrentVolume = oldSound.source.volume;
        float elapsedTime = 0f;


        newSound.source.volume = newTrackCurrentVolume;
        if (!newSound.source.isPlaying)
            newSound.source.Play();
        instance.currentMusic = newSoundName;
        while (elapsedTime < fadeTime)
        {
            if (oldSound != null)
                oldSound.source.volume = Mathf.Lerp(oldTrackCurrentVolume, oldTrackVolumeTarget, elapsedTime / fadeTime);
            newSound.source.volume = Mathf.Lerp(newTrackCurrentVolume, newTrackTargetVolume, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (oldSound != null)
        {
            PauseSound(oldSound.name);
        }
        musicCoroutine = null;
    }

    private IEnumerator PlaySoundWithDelay(string soundName, float delayDuration)
    {
        Sound s = GetSound(soundName);
        float elapsedTime = 0f;
        while (elapsedTime < delayDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        s.source.Play();
    }
    #endregion
}
