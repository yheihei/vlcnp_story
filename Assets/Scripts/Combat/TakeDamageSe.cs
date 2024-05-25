using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Combat
{
    [RequireComponent(typeof(AudioSource))]
    public class TakeDamageSe : MonoBehaviour, ISe
    {
        AudioSource audioSource;
        [SerializeField] AudioClip audioClip;
        [SerializeField] float volume = 0.5f;
        [SerializeField] float pitch = 1f;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void Play()
        {
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            if (audioSource == null) return;
            if (audioClip == null) return;
            audioSource.Play();
        }
    }    
}
