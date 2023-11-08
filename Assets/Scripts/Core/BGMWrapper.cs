using System.Collections;
using System.Collections.Generic;
using Nethereum.ENS.PublicResolver.ContractDefinition;
using UnityEngine;

namespace VLCNP.Core
{
    public class BGMWrapper : MonoBehaviour
    {
        StartBGM startBGM = null;
        void Start()
        {
            // BGMのtagを持つオブジェクトを探して、そのStartBGMを取得する
            startBGM = GameObject.FindWithTag("BGM")?.GetComponent<StartBGM>();
        }

        public void Play(AudioClip clip, float volume, float pitch)
        {
            if (startBGM == null) return;
            startBGM.Stop();
            startBGM.SetAudioClip(clip);
            startBGM.SetAudioVolume(volume);
            startBGM.SetAudioPitch(pitch);
            startBGM.Play();
        }

        public void ChangeVolume(float volume)
        {
            startBGM?.SetAudioVolume(volume);
        }
    }
}
