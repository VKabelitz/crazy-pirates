using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

[System.Serializable]
public class Sound
{
    public enum SoundType
    {
        //Put sound categories in here
        Default = 0,
        Music = 1,
        SFX = 2
    }
    public string name;
    public AudioClip clip;
    public AudioMixerGroup audioMixerGroup;

    [Range(0f, 1f)]
    public float volume;

    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;
    public SoundType soundType;
    public bool ignorePause = false;

    [HideInInspector]
    public AudioSource source;
}
