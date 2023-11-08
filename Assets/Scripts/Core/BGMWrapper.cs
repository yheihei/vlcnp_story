using System.Collections;
using System.Collections.Generic;
using Nethereum.ENS.PublicResolver.ContractDefinition;
using UnityEngine;

namespace VLCNP.Core
{
    public class BGMWrapper : MonoBehaviour
    {
        StartBGM bgm = null;
        void Start()
        {
            // BGMのtagを持つオブジェクトを探して、そのStartBGMを取得する
            bgm = GameObject.FindWithTag("BGM")?.GetComponent<StartBGM>();
        }

        public void Play(AudioClip clip, float volume, float pitch)
        {
            if (bgm == null) return;
            bgm.Stop();
            bgm.SetAudioClip(clip);
            bgm.SetAudioVolume(volume);
            bgm.SetAudioPitch(pitch);
            bgm.Play();
        }

        // BGMをフェードアウトさせる
        public void FadeOut(float duration)
        {
            if (bgm == null) return;
            StartCoroutine(FadeOutCoroutine(duration));
        }

        IEnumerator FadeOutCoroutine(float duration)
        {
            float startVolume = bgm.GetAudioVolume();
            float startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                float t = (Time.time - startTime) / duration;
                bgm.SetAudioVolume(Mathf.Lerp(startVolume, 0, t));
                yield return null;
            }
        }

        public void ChangeVolume(float volume)
        {
            bgm?.SetAudioVolume(volume);
        }
    }
}
