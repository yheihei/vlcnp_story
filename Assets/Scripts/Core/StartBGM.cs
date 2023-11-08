using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Core;

public class StartBGM : MonoBehaviour
{
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        Play();
    }

    public void Stop()
    {
        if (audioSource.clip == null) return;
        audioSource.Stop();
    }

    public void SetAudioClip(AudioClip clip)
    {
        audioSource.clip = clip;
    }

    public void SetAudioVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public float GetAudioVolume()
    {
        return audioSource.volume;
    }

    public void SetAudioPitch(float pitch)
    {
        audioSource.pitch = pitch;
    }

    public void Play()
    {
        // audioSourceにclipが設定されていなければ、AreaBGMを探して再生
        if (audioSource.clip == null)
        {
            AreaBGM areaBGM = GameObject.FindWithTag("AreaBGM").GetComponent<AreaBGM>();
            if (areaBGM != null)
            {
                SetAudioClip(areaBGM.GetAudioClip());
                SetAudioVolume(areaBGM.GetVolume());
                SetAudioPitch(areaBGM.GetPitch());
            }
        }
        audioSource.Play();
    }
}
