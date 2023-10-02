using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Core
{
    public class AreaBGM : MonoBehaviour
    {
        [SerializeField] AudioClip bgm;
        [SerializeField] float volume = 1f;
        [SerializeField] float pitch = 1f;

        public AudioClip GetAudioClip()
        {
            return bgm;
        }
        public float GetVolume()
        {
            return volume;
        }
        public float GetPitch()
        {
            return pitch;
        }
    }
}
